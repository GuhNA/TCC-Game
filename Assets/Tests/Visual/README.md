# Visual review toolkit

This suite is deliberately isolated from EditMode and PlayMode tests. It captures
deterministic UI states, creates a reference-versus-render sheet, and compares
captures with approved baselines when they exist.

## Run

```powershell
.\tools\Test-Visual.ps1 -Scenario recruitment
```

Outputs are written to `Artifacts/VisualTests/<scenario>/`:

- `captures/`: one PNG per deterministic state and resolution;
- `reference-vs-render.png`: approved UX/UI reference on the left and the Unity
  render on the right;
- `diffs/`: red difference images when an approved baseline exists;
- `report.md` and `report.json`: review status and comparison metrics.

## Approval flow

The first capture is a candidate and cannot approve itself. Review the comparison
sheet with a human. Only after explicit approval, run:

```powershell
.\tools\Approve-VisualBaseline.ps1 -Scenario recruitment
```

Later runs compare candidates against the approved images. Baselines detect
regressions; they do not replace art-direction approval.

## Add another HUD

Create an Editor-only class implementing `IVisualReviewScenario`. Declare its
scene, reference image, capture cases, scene preparation, and deterministic state
setup. The runner discovers it automatically.
