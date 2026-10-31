# Inventário de interfaces — Centro

Levantamento da cena salva; inclui objetos inativos. Nenhuma alteração foi salva na cena.

| Canvas | Caminho | Ordem | TMP / legado | Botões |
|---|---|---:|---:|---:|
| BuildingPanel | `Canvas/BuildingPanel` | 0 | 7 / 0 | 2 |
| TaxNotification | `Canvas/TaxNotification` | 0 | 0 / 1 | 0 |
| GoldBalanceHUD | `Canvas/GoldBalanceHUD` | 10 | 2 / 1 | 0 |

## Edifícios que compartilham o modal

| Edifício | Limite | Custo 2 → 3 | Definição |
|---|---:|---:|---|
| Mercado Local | 10 | 225 ouro | `Assets/Scriptable/Buildings/LocalMarket.asset` |
| Quartel | 10 | 225 ouro | `Assets/Scriptable/Buildings/Barracks.asset` |
| Escola de Magia | 10 | 225 ouro | `Assets/Scriptable/Buildings/MagicSchool.asset` |
| Estande de Arqueiros | 10 | 225 ouro | `Assets/Scriptable/Buildings/ArcherStand.asset` |
| Taverna | 5 | 450 ouro | `Assets/Scriptable/Buildings/Tavern.asset` |

## Textos e controles do modal atual

- Escala salva do modal: `(1.00, 1.00, 1.00)`; a abertura em runtime restaura escala 1.
- `BuildingNameText`: fonte `LiberationSans SDF`, tamanho 36, texto `Nome Construção`.
- `DescriptionText`: fonte `LiberationSans SDF`, tamanho 18, texto `Descrição do que a construção faz e mais algumas coisas.`.
- `LevelText`: fonte `LiberationSans SDF`, tamanho 24, texto `Level: X`.
- `ProductionText`: fonte `LiberationSans SDF`, tamanho 24, texto `Produz: Y`.
- `UpgradeCostText`: fonte `LiberationSans SDF`, tamanho 24, texto `Próxima Nível: Z`.
- `ButtonText`: fonte `LiberationSans SDF`, tamanho 24, texto `Melhorar`.
- `ButtonText`: fonte `LiberationSans SDF`, tamanho 24, texto `Sair / `.
- Botão `UpgradeButton`: `Melhorar`.
- Botão `CloseButton`: `Sair`.

- Feedback `Canvas/TaxNotification`: duração 1 segundo(s), tema específico ausente.

## Componentes de interação relevantes

- `TavernScenePortal` em `CityRoot/Buildings/Tavern/Tavern Entrance`.
- `CityTravelNpc` em `CityRoot/NPCs/NavigationNPCs/Sentinela Norte`.
- `CityTravelNpc` em `CityRoot/NPCs/NavigationNPCs/Sentinela Leste`.
- `CityTravelNpc` em `CityRoot/NPCs/NavigationNPCs/Sentinela Sul`.
- `CityTravelNpc` em `CityRoot/NPCs/NavigationNPCs/Sentinela Oeste`.
- `TaxCollectorNpc` em `CityRoot/NPCs/TaxCollectorNPC`.
- `CityManager` em `Systems/CityManager`.
- `BuildingPanel` em `Canvas/BuildingPanel`.
