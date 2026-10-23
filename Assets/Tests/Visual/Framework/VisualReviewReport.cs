using System;
using System.Collections.Generic;

namespace TCCGame.Tests.Visual
{
    [Serializable]
    public sealed class VisualReviewReport
    {
        public string scenarioId;
        public string scenarioName;
        public string generatedAtUtc;
        public string referenceImage;
        public string comparisonSheet;
        public string status;
        public string summary;
        public List<VisualReviewCaseReport> captures = new();
    }

    [Serializable]
    public sealed class VisualReviewCaseReport
    {
        public string id;
        public string displayName;
        public int width;
        public int height;
        public string candidateImage;
        public string baselineImage;
        public string diffImage;
        public string status;
        public float meanAbsoluteError;
        public float changedPixelRatio;
    }
}
