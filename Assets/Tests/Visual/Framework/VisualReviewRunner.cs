using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TCCGame.Tests.Visual
{
    public static class VisualReviewRunner
    {
        private const string ScenarioArgument = "-visualReviewScenario";

        [MenuItem("TCC Game/Visual Review/Capture All")]
        public static void CaptureAllFromMenu()
        {
            int exitCode = Capture(GetArgument(ScenarioArgument));
            Debug.Log(exitCode == 0
                ? "Visual review completed."
                : "Visual review found baseline regressions.");
        }

        public static void RunFromCommandLine()
        {
            int exitCode;
            try
            {
                exitCode = Capture(GetArgument(ScenarioArgument));
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                exitCode = 3;
            }

            EditorApplication.Exit(exitCode);
        }

        private static int Capture(string selectedScenario)
        {
            IReadOnlyList<IVisualReviewScenario> scenarios =
                DiscoverScenarios(selectedScenario);
            bool regressionFound = false;

            foreach (IVisualReviewScenario scenario in scenarios)
            {
                VisualReviewReport report = CaptureScenario(scenario);
                regressionFound |= report.captures.Any(
                    capture => capture.status == "Failed"
                );
            }

            return regressionFound ? 2 : 0;
        }

        private static VisualReviewReport CaptureScenario(
            IVisualReviewScenario scenario
        )
        {
            EditorSceneManager.OpenScene(
                scenario.ScenePath,
                OpenSceneMode.Single
            );
            VisualReviewContext context = scenario.PrepareScene();

            string projectRoot = ProjectRoot;
            string outputDirectory = Path.Combine(
                projectRoot,
                "Artifacts",
                "VisualTests",
                scenario.Id
            );
            string captureDirectory = Path.Combine(
                outputDirectory,
                "captures"
            );
            string diffDirectory = Path.Combine(outputDirectory, "diffs");
            string baselineDirectory = Path.Combine(
                projectRoot,
                "Assets",
                "Tests",
                "Visual",
                "Baselines",
                scenario.Id
            );
            Directory.CreateDirectory(captureDirectory);
            Directory.CreateDirectory(diffDirectory);

            VisualReviewReport report = new VisualReviewReport
            {
                scenarioId = scenario.Id,
                scenarioName = scenario.DisplayName,
                generatedAtUtc = DateTime.UtcNow.ToString("O"),
                referenceImage = scenario.ReferenceImagePath
            };

            foreach (VisualReviewCase reviewCase in scenario.Cases)
            {
                scenario.ConfigureCase(context, reviewCase);
                string candidatePath = Path.Combine(
                    captureDirectory,
                    reviewCase.FileName
                );
                CaptureImage(context, reviewCase, candidatePath);

                string baselinePath = Path.Combine(
                    baselineDirectory,
                    reviewCase.FileName
                );
                string diffPath = Path.Combine(
                    diffDirectory,
                    reviewCase.FileName
                );
                VisualReviewCaseReport caseReport = new VisualReviewCaseReport
                {
                    id = reviewCase.Id,
                    displayName = reviewCase.DisplayName,
                    width = reviewCase.Width,
                    height = reviewCase.Height,
                    candidateImage = RelativeToProject(candidatePath),
                    baselineImage = RelativeToProject(baselinePath),
                    diffImage = RelativeToProject(diffPath)
                };

                if (!File.Exists(baselinePath))
                {
                    caseReport.status = "Candidate";
                }
                else
                {
                    bool passed = VisualImageComparison.Compare(
                        baselinePath,
                        candidatePath,
                        diffPath,
                        out float meanError,
                        out float changedPixelRatio
                    );
                    caseReport.status = passed ? "Passed" : "Failed";
                    caseReport.meanAbsoluteError = meanError;
                    caseReport.changedPixelRatio = changedPixelRatio;
                }

                report.captures.Add(caseReport);
            }

            string referencePath = Path.Combine(
                projectRoot,
                scenario.ReferenceImagePath
            );
            string primaryCandidate = Path.Combine(
                captureDirectory,
                scenario.Cases[0].FileName
            );
            string comparisonPath = Path.Combine(
                outputDirectory,
                "reference-vs-render.png"
            );
            if (File.Exists(referencePath))
            {
                CreateComparisonSheet(
                    referencePath,
                    primaryCandidate,
                    comparisonPath
                );
                report.comparisonSheet = RelativeToProject(comparisonPath);
            }

            bool hasFailure = report.captures.Any(item => item.status == "Failed");
            bool needsApproval = report.captures.Any(
                item => item.status == "Candidate"
            );
            report.status = hasFailure
                ? "Failed"
                : needsApproval
                    ? "NeedsHumanApproval"
                    : "Passed";
            report.summary = hasFailure
                ? "One or more approved baselines changed."
                : needsApproval
                    ? "No approved baseline exists yet. Review the comparison sheet before approval."
                    : "All captures match their approved baselines.";

            File.WriteAllText(
                Path.Combine(outputDirectory, "report.json"),
                JsonUtility.ToJson(report, true)
            );
            File.WriteAllText(
                Path.Combine(outputDirectory, "report.md"),
                BuildMarkdown(report)
            );
            Debug.Log($"Visual review {scenario.Id}: {report.status}");
            return report;
        }

        private static void CaptureImage(
            VisualReviewContext context,
            VisualReviewCase reviewCase,
            string outputPath
        )
        {
            Camera camera = context.Camera;
            Canvas canvas = context.Canvas;
            context.TargetRoot.SetActive(true);
            canvas.enabled = true;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = Mathf.Max(
                camera.nearClipPlane + 0.1f,
                1f
            );
            canvas.sortingOrder = 5000;

            RenderTexture renderTexture = new RenderTexture(
                reviewCase.Width,
                reviewCase.Height,
                24,
                RenderTextureFormat.ARGB32
            );
            renderTexture.antiAliasing = 1;
            renderTexture.filterMode = FilterMode.Point;
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture previousTarget = camera.targetTexture;
            float previousAspect = camera.aspect;

            try
            {
                camera.aspect = (float)reviewCase.Width / reviewCase.Height;
                camera.targetTexture = renderTexture;
                Canvas.ForceUpdateCanvases();
                camera.Render();

                RenderTexture.active = renderTexture;
                Texture2D capture = new Texture2D(
                    reviewCase.Width,
                    reviewCase.Height,
                    TextureFormat.RGBA32,
                    false
                );
                capture.ReadPixels(
                    new Rect(0, 0, reviewCase.Width, reviewCase.Height),
                    0,
                    0
                );
                capture.Apply();
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                File.WriteAllBytes(outputPath, capture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(capture);
            }
            finally
            {
                camera.targetTexture = previousTarget;
                camera.aspect = previousAspect;
                RenderTexture.active = previousActive;
                renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }
        }

        private static void CreateComparisonSheet(
            string referencePath,
            string candidatePath,
            string outputPath
        )
        {
            const int panelWidth = 900;
            const int panelHeight = 600;
            const int margin = 30;
            const int gap = 30;
            int sheetWidth = (panelWidth * 2) + (margin * 2) + gap;
            int sheetHeight = panelHeight + (margin * 2);

            Texture2D reference = VisualImageComparison.Load(referencePath);
            Texture2D candidate = VisualImageComparison.Load(candidatePath);
            Texture2D referenceScaled = ScaleToFit(
                reference,
                panelWidth,
                panelHeight
            );
            Texture2D candidateScaled = ScaleToFit(
                candidate,
                panelWidth,
                panelHeight
            );
            Texture2D sheet = new Texture2D(
                sheetWidth,
                sheetHeight,
                TextureFormat.RGBA32,
                false
            );

            Color32[] background = new Color32[sheetWidth * sheetHeight];
            for (int i = 0; i < background.Length; i++)
            {
                background[i] = new Color32(22, 24, 28, 255);
            }
            sheet.SetPixels32(background);
            PlaceCentered(sheet, referenceScaled, margin, panelWidth, panelHeight);
            PlaceCentered(
                sheet,
                candidateScaled,
                margin + panelWidth + gap,
                panelWidth,
                panelHeight
            );
            sheet.Apply();
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            File.WriteAllBytes(outputPath, sheet.EncodeToPNG());

            UnityEngine.Object.DestroyImmediate(reference);
            UnityEngine.Object.DestroyImmediate(candidate);
            UnityEngine.Object.DestroyImmediate(referenceScaled);
            UnityEngine.Object.DestroyImmediate(candidateScaled);
            UnityEngine.Object.DestroyImmediate(sheet);
        }

        private static Texture2D ScaleToFit(
            Texture2D source,
            int maximumWidth,
            int maximumHeight
        )
        {
            float scale = Mathf.Min(
                (float)maximumWidth / source.width,
                (float)maximumHeight / source.height
            );
            int width = Mathf.Max(1, Mathf.RoundToInt(source.width * scale));
            int height = Mathf.Max(1, Mathf.RoundToInt(source.height * scale));
            RenderTexture temporary = RenderTexture.GetTemporary(
                width,
                height,
                0,
                RenderTextureFormat.ARGB32
            );
            temporary.filterMode = FilterMode.Point;
            source.filterMode = FilterMode.Point;
            RenderTexture previous = RenderTexture.active;
            Graphics.Blit(source, temporary);
            RenderTexture.active = temporary;

            Texture2D result = new Texture2D(
                width,
                height,
                TextureFormat.RGBA32,
                false
            );
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(temporary);
            return result;
        }

        private static void PlaceCentered(
            Texture2D destination,
            Texture2D source,
            int panelX,
            int panelWidth,
            int panelHeight
        )
        {
            int x = panelX + ((panelWidth - source.width) / 2);
            int y = 30 + ((panelHeight - source.height) / 2);
            destination.SetPixels32(
                x,
                y,
                source.width,
                source.height,
                source.GetPixels32()
            );
        }

        private static IReadOnlyList<IVisualReviewScenario> DiscoverScenarios(
            string selectedScenario
        )
        {
            List<IVisualReviewScenario> scenarios = TypeCache
                .GetTypesDerivedFrom<IVisualReviewScenario>()
                .Where(type => !type.IsAbstract && type.GetConstructor(Type.EmptyTypes) != null)
                .Select(type => (IVisualReviewScenario)Activator.CreateInstance(type))
                .Where(scenario => string.IsNullOrWhiteSpace(selectedScenario) ||
                    string.Equals(
                        scenario.Id,
                        selectedScenario,
                        StringComparison.OrdinalIgnoreCase
                    ))
                .OrderBy(scenario => scenario.Id)
                .ToList();

            if (scenarios.Count == 0)
            {
                throw new InvalidOperationException(
                    $"No visual review scenario matched '{selectedScenario}'."
                );
            }

            return scenarios;
        }

        private static string BuildMarkdown(VisualReviewReport report)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"# Visual review: {report.scenarioName}");
            builder.AppendLine();
            builder.AppendLine($"Status: **{report.status}**");
            builder.AppendLine();
            builder.AppendLine(report.summary);
            builder.AppendLine();
            builder.AppendLine("The comparison sheet shows the approved UX/UI reference on the left and the Unity render on the right.");
            builder.AppendLine();
            builder.AppendLine("| State | Resolution | Status | Mean error | Changed pixels |");
            builder.AppendLine("|---|---:|---|---:|---:|");

            foreach (VisualReviewCaseReport capture in report.captures)
            {
                string meanError = capture.status == "Candidate"
                    ? "—"
                    : capture.meanAbsoluteError.ToString("0.0000");
                string changedPixels = capture.status == "Candidate"
                    ? "—"
                    : capture.changedPixelRatio.ToString("P2");
                builder.AppendLine(
                    $"| {capture.displayName} | {capture.width}x{capture.height} | " +
                    $"{capture.status} | {meanError} | {changedPixels} |"
                );
            }

            return builder.ToString();
        }

        private static string GetArgument(string name)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length - 1; i++)
            {
                if (string.Equals(arguments[i], name, StringComparison.OrdinalIgnoreCase))
                {
                    return arguments[i + 1];
                }
            }

            return null;
        }

        private static string RelativeToProject(string path)
        {
            return Path.GetRelativePath(ProjectRoot, path).Replace('\\', '/');
        }

        private static string ProjectRoot => Path.GetFullPath(
            Path.Combine(Application.dataPath, "..")
        );
    }
}
