# Visual review toolkit

This suite is deliberately isolated from EditMode and PlayMode tests. It captures
deterministic UI states, creates a reference-versus-render sheet, and compares
captures with approved baselines when they exist.

## Run

```powershell
.\tools\Test-Visual.ps1 -Scenario recruitment
```

Pass comma-separated scenario IDs to capture a focused group in one Unity run:

```powershell
.\tools\Test-Visual.ps1 -Scenario 'character-depth-centro,character-depth-gate,character-depth-tavern'
```

The character-depth scenarios capture the player above and below an NPC in each
production scene. The tavern uses a recruitable prefab with the same definition
configuration as the runtime spawner. These captures do not run the PlayMode suite.

The approved Centro/Taverna prototype scenes and their old capture scenarios
have been retired. Use `centro-production`, `centro-tavern-production`, and the
three `character-depth-*` scenarios for the shipped world; use `centro-buildings`
and `centro-taxes` for the current Centro UI. Approved baseline images and design
references remain available. The two UI prototype prefabs are still source
templates for the production HUD builders and must not be deleted as unused.

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
