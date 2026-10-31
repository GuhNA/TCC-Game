using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

namespace TCCGame.Tests.Visual
{
    /// <summary>Candidate review: retain the previously approved recruitment baseline.</summary>
    public sealed class RecruitmentPortraitRevisionVisualScenario : IVisualReviewScenario
    {
        private readonly RecruitmentVisualScenario source = new RecruitmentVisualScenario();
        public string Id => "recruitment-portraits-v2";
        public string DisplayName => "Recrutamento — cabeçalho e retratos novos";
        public string ScenePath => source.ScenePath;
        public string ReferenceImagePath => "Assets/Tests/Visual/Baselines/recruitment/normal-1920x1080.png";
        public IReadOnlyList<VisualReviewCase> Cases => source.Cases;
        public VisualReviewContext PrepareScene()
        {
            VisualReviewContext context = source.PrepareScene();
            Rect Bounds(string name)
            {
                RectTransform rect = context.TargetRoot.GetComponentsInChildren<RectTransform>(true)
                    .Single(item => item.name == name);
                Rect bounds = rect.rect;
                bounds.position += rect.anchoredPosition;
                return bounds;
            }
            Rect gold = Bounds("Gold Plate");
            if (Bounds("Right Header Drape").Overlaps(gold) || gold.Overlaps(Bounds("Close")))
                throw new InvalidOperationException("Recruitment header decorations overlap the balance or close control.");
            Rect subtitle = Bounds("Subtitle");
            Rect plate = Bounds("Subtitle Plate");
            if (!plate.Contains(subtitle.min) || !plate.Contains(subtitle.max))
                throw new InvalidOperationException("Recruitment subtitle must fit inside its own plate.");
            return context;
        }
        public void ConfigureCase(VisualReviewContext context, VisualReviewCase reviewCase) => source.ConfigureCase(context, reviewCase);
    }
}
