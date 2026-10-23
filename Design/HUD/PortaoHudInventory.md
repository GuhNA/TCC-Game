# Inventário de HUDs — mapa Portão

Gerado automaticamente a partir da cena e dos prefabs de runtime.

## Canvases presentes na cena

| Canvas | Caminho | Ativo | Ordem | Textos | Imagens | Botões |
|---|---|---:|---:|---:|---:|---:|
| Canvas | `Canvas` | True | 0 | 0 | 1 | 0 |
| GoldBalanceHUD | `GoldBalanceHUD` | True | 10 | 1 | 0 | 0 |
| Recruitment Prototype Canvas | `Recruitment Prototype Canvas` | True | 5000 | 48 | 229 | 22 |

## Prefabs relacionados ao Portão

| Interface | Asset | Canvas | Textos | Imagens | Botões |
|---|---|---:|---:|---:|---:|
| RecruitmentCanvas | `Assets/Resources/UI/RecruitmentCanvas.prefab` | 1 | 48 | 229 | 22 |
| GoldBalanceHUD | `Assets/Prefabs/UI/GoldBalanceHUD.prefab` | 1 | 1 | 0 | 0 |
| GateContextCanvas | `Assets/Resources/UI/GateContextCanvas.prefab` | 1 | 1 | 0 | 0 |
| InteractionPrompt | `Assets/Resources/UI/InteractionPrompt.prefab` | 1 | 1 | 0 | 0 |
| WaveAlertCanvas | `Assets/Resources/UI/WaveAlertCanvas.prefab` | 1 | 1 | 1 | 0 |
| WaveAlertIndicator | `Assets/Resources/UI/WaveAlertIndicator.prefab` | 1 | 1 | 0 | 0 |
| TravelConversationCanvas | `Assets/Resources/UI/TravelConversationCanvas.prefab` | 1 | 4 | 4 | 2 |
| GarrisonManagementCanvas | `Assets/Resources/UI/GarrisonManagementCanvas.prefab` | 1 | 23 | 17 | 15 |
| SceneFadeCanvas | `Assets/Resources/UI/SceneFadeCanvas.prefab` | 1 | 0 | 1 | 0 |

## Critério de prototipação

O protótipo deve representar HUD persistente, avisos, prompts e modais sem alterar a lógica de jogo. Recrutamento permanece como baseline visual.
