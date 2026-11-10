# Limpeza dos protótipos aprovados

As cenas finais são a fonte atual do jogo. Foram removidos **74 arquivos**, incluindo os `.meta`, que somavam **5,48 MiB**. A limpeza não altera o layout, a arte usada pelo jogo nem os fluxos aprovados.

## Validação

- Nenhuma referência serializada aos GUIDs removidos permaneceu em Assets, Packages ou ProjectSettings.
- Nenhuma referência de código às classes e cenas retiradas permaneceu nos scripts e ferramentas ativos.
- Centro, Portão, Taverna, ExplorationCombat e EditorBuildSettings mantiveram os hashes registrados antes da limpeza.
- Compilação concluída e [sete testes PlayMode aprovados](../../Logs/ApprovedPrototypeCleanupSmoke.xml) em uma única execução, numa cópia temporária com saves separados. Cobertura: construções, recrutamento, alertas, viagens, ordenação dos personagens e vendedor imóvel.

## Preservado

- As quatro cenas finais, os prefabs de contratação e as configurações de build.
- Baselines aprovadas, pranchas e documentação das decisões visuais.
- Texturas, fachadas, personagens e objetos que ainda são referenciados pelas cenas finais, mesmo em pastas com nomes históricos de protótipo.
- `WorldBuildingView`, `WorldPropOcclusion`, `WorldPrototypeDepthSort` e `TavernFacadePilotView`, usados na apresentação do cenário final.
- Os dois prefabs em `Assets/Resources/UI/Prototypes` e seus construtores: os geradores dos HUDs finais ainda usam essas peças como templates. O construtor `RecruitmentPrototypeSetup` também produz o modal final e permanece.
- O repositório em memória usado pelos testes para não alterar saves reais.
- Os arquivos em `Assets/_Recovery`, cujo conteúdo difere das cenas finais e pode conter trabalho recuperado.

## Ajustes complementares

- O teste visual de edifícios passou a declarar seus próprios casos, sem depender da antiga proposta.
- O provedor de sessão deixou de consultar a chave temporária dos protótipos no Editor. Testes continuam injetando seus saves em memória.
- A asserção que dependia da classe de cena de protótipo foi retirada do smoke de produção; as verificações de serviços, viagens e construções foram mantidas.
- O README visual agora aponta para os cenários de produção. Entradas duplicadas do `.gitignore` foram consolidadas.

## Arquivos removidos

Os `.meta` correspondentes também foram removidos, assim como os diretórios vazios de cenas e prefabs de protótipo.

### Cenas e prefab de protótipo

- `Assets/Prefabs/Prototypes/CentroLayoutPlayer.prefab`
- `Assets/Scenes/Prototypes/CentroLayoutPrototype.unity`
- `Assets/Scenes/Prototypes/CentroLayoutTavernPrototype.unity`

### Geradores de editor antigos

- `Assets/Scripts/Editor/CentroLayoutPrototypeSetup.cs`
- `Assets/Scripts/Editor/CentroProductionLayoutSetup.cs`
- `Assets/Scripts/Editor/CentroPrototypeCollisionSetup.cs`
- `Assets/Scripts/Editor/CentroTerrainDetailSetup.cs`
- `Assets/Scripts/Editor/CentroWorldArtSetup.cs`
- `Assets/Scripts/Editor/TavernFacadePilotSetup.cs`
- `Assets/Scripts/Editor/WorldPrototypeSessionSetup.cs`

### Componentes exclusivos do protótipo

- `Assets/Scripts/Prototypes/WorldPrototypePortal.cs`
- `Assets/Scripts/Prototypes/WorldPrototypeScene.cs`
- `Assets/Scripts/Prototypes/WorldPrototypeTravel.cs`
- `Assets/Scripts/Prototypes/WorldPrototypeWaypoint.cs`

### Testes antigos

- `Assets/Tests/PlayMode/CentroLayoutPrototypeSmokeTests.cs`
- `Assets/Tests/Visual/Scenarios/CentroCollisionVisuals.cs`
- `Assets/Tests/Visual/Scenarios/CentroExteriorSetVisualScenario.cs`
- `Assets/Tests/Visual/Scenarios/CentroHudProposalVisualScenario.cs`
- `Assets/Tests/Visual/Scenarios/CentroLayoutPrototypeVisualScenario.cs`
- `Assets/Tests/Visual/Scenarios/CentroStaticBackgroundVisualScenario.cs`
- `Assets/Tests/Visual/Scenarios/CentroTerrainDetailVisualScenario.cs`
- `Assets/Tests/Visual/Scenarios/PortaoHudPrototypeVisualScenario.cs`
- `Assets/Tests/Visual/Scenarios/TavernFacadePilotVisualScenario.cs`

### Ferramentas de etapas anteriores

- `tools/Build-CentroLayoutReview.py`
- `tools/Render-CentroLayoutBoards.cjs`
- `tools/Revise-CentroScale.py`
- `tools/Test-CentroLayoutPrototype.ps1`

### Arte e arquivos de terreno sem uso

- `Assets/Images/Buildings/CentroSetV1/BarracksExteriorV1.png`
- `Assets/Images/Buildings/CentroSetV1/MagicSchoolExteriorV1.png`
- `Assets/Images/Buildings/CentroSetV1/RoadMesh-10.asset`
- `Assets/Images/Buildings/CentroSetV1/RoadMesh-11.asset`
- `Assets/Images/World/CentroTerrainV2/DirtGround.mat`
- `Assets/Images/World/CentroTerrainV2/EarthPaths.asset`
- `Assets/Images/World/CentroTerrainV2/GrassGround.asset`
- `Assets/Images/World/CentroTerrainV2/GrassGround.mat`
- `Assets/Images/World/CentroTerrainV2/PavingGround.mat`
- `Assets/Images/World/CentroTerrainV2/PixelGround.shader`
- `Assets/Images/World/CentroTerrainV2/StoneStreets.asset`

## Registro técnico

Inventário com hashes, auditoria de referências e cópia de segurança local ficam em `Logs/ApprovedPrototypeCleanupPlan.json`, `Logs/ApprovedPrototypeCleanupAudit.json` e `Logs/ApprovedPrototypeCleanupBackup.zip`. A pasta Logs é ignorada pelo Git.
