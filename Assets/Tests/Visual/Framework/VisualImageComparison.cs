using System;
using System.IO;
using UnityEngine;

namespace TCCGame.Tests.Visual
{
    public static class VisualImageComparison
    {
        public const float MeanErrorLimit = 0.015f;
        public const float ChangedPixelLimit = 0.01f;
        private const float ChangedPixelThreshold = 0.08f;

        public static bool Compare(
            string baselinePath,
            string candidatePath,
            string diffPath,
            out float meanError,
            out float changedPixelRatio
        )
        {
            Texture2D baseline = Load(baselinePath);
            Texture2D candidate = Load(candidatePath);

            try
            {
                if (baseline.width != candidate.width ||
                    baseline.height != candidate.height)
                {
                    meanError = 1f;
                    changedPixelRatio = 1f;
                    SaveSolidDiff(
                        diffPath,
                        candidate.width,
                        candidate.height,
                        Color.magenta
                    );
                    return false;
                }

                Color32[] expected = baseline.GetPixels32();
                Color32[] actual = candidate.GetPixels32();
                Color32[] diff = new Color32[actual.Length];
                double totalError = 0d;
                int changed = 0;

                for (int i = 0; i < actual.Length; i++)
                {
                    float red = Mathf.Abs(expected[i].r - actual[i].r) / 255f;
                    float green = Mathf.Abs(expected[i].g - actual[i].g) / 255f;
                    float blue = Mathf.Abs(expected[i].b - actual[i].b) / 255f;
                    float pixelError = (red + green + blue) / 3f;
                    totalError += pixelError;

                    if (pixelError > ChangedPixelThreshold)
                    {
                        changed++;
                    }

                    byte intensity = (byte)Mathf.Clamp(
                        Mathf.RoundToInt(pixelError * 510f),
                        0,
                        255
                    );
                    diff[i] = new Color32(intensity, 0, 0, 255);
                }

                meanError = (float)(totalError / actual.Length);
                changedPixelRatio = (float)changed / actual.Length;
                Save(diffPath, candidate.width, candidate.height, diff);
                return meanError <= MeanErrorLimit &&
                    changedPixelRatio <= ChangedPixelLimit;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(baseline);
                UnityEngine.Object.DestroyImmediate(candidate);
            }
        }

        public static Texture2D Load(string path)
        {
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(File.ReadAllBytes(path), false))
            {
                UnityEngine.Object.DestroyImmediate(texture);
                throw new InvalidOperationException($"Could not read image: {path}");
            }

            return texture;
        }

        private static void SaveSolidDiff(
            string path,
            int width,
            int height,
            Color color
        )
        {
            Color32[] pixels = new Color32[width * height];
            Color32 value = color;
            Array.Fill(pixels, value);
            Save(path, width, height, pixels);
        }

        private static void Save(
            string path,
            int width,
            int height,
            Color32[] pixels
        )
        {
            Texture2D texture = new Texture2D(
                width,
                height,
                TextureFormat.RGBA32,
                false
            );
            texture.SetPixels32(pixels);
            texture.Apply();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
        }
    }
}
