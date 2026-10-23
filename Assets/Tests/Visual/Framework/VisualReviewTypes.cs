using System.Collections.Generic;
using UnityEngine;

namespace TCCGame.Tests.Visual
{
    public sealed class VisualReviewCase
    {
        public VisualReviewCase(
            string id,
            string displayName,
            int width,
            int height
        )
        {
            Id = id;
            DisplayName = displayName;
            Width = width;
            Height = height;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public int Width { get; }
        public int Height { get; }
        public string FileName => $"{Id}-{Width}x{Height}.png";
    }

    public sealed class VisualReviewContext
    {
        public VisualReviewContext(
            Camera camera,
            Canvas canvas,
            GameObject targetRoot
        )
        {
            Camera = camera;
            Canvas = canvas;
            TargetRoot = targetRoot;
        }

        public Camera Camera { get; }
        public Canvas Canvas { get; }
        public GameObject TargetRoot { get; }
    }

    public interface IVisualReviewScenario
    {
        string Id { get; }
        string DisplayName { get; }
        string ScenePath { get; }
        string ReferenceImagePath { get; }
        IReadOnlyList<VisualReviewCase> Cases { get; }

        VisualReviewContext PrepareScene();
        void ConfigureCase(
            VisualReviewContext context,
            VisualReviewCase reviewCase
        );
    }
}
