# Plano de migração das interfaces para Canvas editável

**Data:** 02 de setembro de 2026  
**Escopo:** substituir a construção de UI em runtime por Canvas/prefabs
editáveis no Unity, preservando o fluxo atual de ondas, NPCs, menus e viagem.

## Objetivos

- Permitir edição visual de posição, escala, tipografia, cores, animações e
  navegação diretamente no Inspector/Canvas.
- Remover a dependência de `new GameObject`, `AddComponent<Canvas>` e criação de
  textos/botões nos componentes de apresentação.
- Preservar a lógica de domínio, persistência, pausas, input e eventos já
  validados pelos testes automatizados.
- Falhar de forma explícita quando uma referência obrigatória não estiver
  configurada, em vez de reconstruir uma interface inesperada.

## Interfaces incluídas

| Interface atual | Controlador | Destino editável |
|---|---|---|
| Prompt `[E]` | `InteractionPromptPresenter` | `InteractionPromptCanvas.prefab` |
| Indicador `!` do Capitão | `CaptainNpc` | `WaveAlertIndicator.prefab` com Canvas World Space |
| Notificação transitória de onda | `GlobalThreatHud` | `WaveAlertCanvas.prefab` |
| Contexto do portão | `GateContextPresenter` | `GateContextCanvas.prefab` |
| Guarnição | `GarrisonManagementPanel` | `GarrisonManagementCanvas.prefab` |
| Conversa de viagem | `TravelConversationPanel` | `TravelConversationCanvas.prefab` |
| Fade de cena | `SceneTransitionController` | `SceneFadeCanvas.prefab` |
| Construção | `BuildingPanel` | manter Canvas da cena; corrigir referências/escala |

## Estratégia técnica

1. Criar uma pasta `Assets/Prefabs/UI` e um prefab por interface, com Canvas,
   CanvasScaler, GraphicRaycaster, CanvasGroup, textos, imagens e botões já
   organizados em hierarquia semântica.
2. Expor nos controladores referências `[SerializeField]` para root, textos,
   imagens, botões e CanvasGroup. Os métodos de atualização existentes apenas
   preencherão conteúdo e alternarão visibilidade.
3. Para interfaces persistentes (`DontDestroyOnLoad`), criar um bootstrap de
   cena/prefab único que seja encontrado ou instanciado uma vez. A criação de
   objetos filhos será removida; não haverá fallback silencioso que esconda
   erro de configuração.
4. Para interfaces pertencentes a uma cena, colocar o prefab no Canvas da
   cena e validar a ordem de sorting. O indicador do Capitão será filho do NPC,
   em Canvas World Space, permitindo editar offset e escala sem código.
5. Manter a fila de alertas e os eventos de domínio intactos. O HUD permanente
   de quatro cartões continuará desativado; somente o banner transitório e o
   `!` do NPC serão apresentados.
6. Definir navegação explícita dos botões (Selectables), estado inicial e
   `EventSystem` no prefab. O modal de construção deve selecionar seu botão de
   ação ao abrir.

## Ordem de implementação

### Etapa 1 — fundação e prompt

- Criar `UI` root/prefabs e migrar `InteractionPromptPresenter`.
- Validar troca de cena, ocultação ao abrir qualquer menu e ausência de
  bloqueio de raycast.

### Etapa 2 — alertas de ondas e Capitão

- Criar Canvas World Space editável para o `!`.
- Criar Canvas do banner transitório e ligar textos/cores/áudio por Inspector.
- Remover a criação de cartões permanentes e de seus elementos em código.

### Etapa 3 — portão, guarnição e viagem

- Migrar `GateContextPresenter`, `GarrisonManagementPanel` e
  `TravelConversationPanel` para prefabs.
- Preservar pausa da campanha, bloqueio do jogador, confirmação/cancelamento
  e seleção inicial de botões.

### Etapa 4 — transição e limpeza

- Migrar o fade de `SceneTransitionController`.
- Remover métodos `CreateUi`/`CreateInterface` e helpers de criação que ficaram
  sem uso.
- Atualizar cenas e referências serializadas; não alterar regras de jogo.

## Critérios de aceite

- Todas as interfaces listadas aparecem com layout equivalente ao atual e são
  editáveis sem alterar scripts.
- Nenhuma UI cria Canvas, botão ou texto em runtime.
- Prompt some ao abrir qualquer menu; alertas não ocupam espaço permanente.
- O `!` acompanha o estado da ameaça e abre a conversa do Capitão.
- Botões de construção, guarnição e viagem respondem a mouse, teclado e
  controle.
- EditMode e PlayMode permanecem verdes; adicionar testes de wiring que
  detectem referências nulas e Canvas ausente.
- Executar manualmente M01–M16 do plano rigoroso após a migração.

## Riscos e mitigação

- **Referências nulas:** validação em `Awake` com mensagem clara e testes de
  wiring.
- **Ordem de sorting/raycast:** manter valores atuais e testar abertura de cada
  modal sobre o mundo.
- **Persistência duplicada:** garantir uma única instância dos prefabs
  persistentes e limpar eventos no `OnDestroy`.
- **Diferença de escala/resolução:** usar CanvasScaler com resolução de
  referência comum e revisar em 16:9 e janela menor.

## Progresso

### Etapa 1 — concluída

- Criado `Assets/Resources/UI/InteractionPrompt.prefab` como Canvas editável.
- `InteractionPromptPresenter` agora instancia o prefab e localiza apenas os
  componentes visuais serializados no asset; a hierarquia não é mais montada
  em código.
- Mantidos o ocultamento ao abrir menus, o bloqueio de raycast e o texto
  dinâmico `[E]`.

Validação concluída após remover locks antigos do Asset Database:

- EditMode: 106/106 aprovados (`Logs/UiPrefabEditModeVerified.xml`);
- PlayMode: 15/15 aprovados (`Logs/UiPrefabPlayModeVerified.xml`).

As etapas 2–4 permanecem planejadas para a próxima iteração.

### Etapa 2 — concluída

- Criados `WaveAlertIndicator.prefab` (Canvas World Space do Capitão) e
  `WaveAlertCanvas.prefab` (banner transitório).
- `CaptainNpc` e `GlobalThreatHud` passaram a carregar esses prefabs, sem
  construir textos, Canvas ou imagens de alerta em runtime.
- Os cartões permanentes de ameaça continuam removidos; o aviso persistente é
  o `!` do NPC.
- EditMode: 106/106 aprovados (`Logs/WaveUiEditModeVerified.xml`);
- PlayMode: 15/15 aprovados (`Logs/WaveUiPlayModeVerified.xml`).

### Etapa 3 — concluída

- Criados `GateContextCanvas.prefab`, `TravelConversationCanvas.prefab` e
  `GarrisonManagementCanvas.prefab` em `Assets/Resources/UI`.
- Os três controladores carregam hierarquias editáveis e apenas conectam dados
  e ações aos componentes existentes.
- Adicionados testes de wiring para Canvas, textos, controles obrigatórios e
  quantidade de botões da guarnição.
- EditMode: 114/114 aprovados (`Logs/Stage3UiEditModeFinal.xml`);
- PlayMode: 15/15 aprovados (`Logs/Stage3UiPlayMode2.xml`).

A Etapa 4 permanece pendente: migrar o fade de cena e remover helpers antigos
de construção visual que já não são chamados.

## Plano detalhado da Etapa 4

### 4.1 — Migrar o fade de transição

1. Criar `Assets/Resources/UI/SceneFadeCanvas.prefab` com:
   - Canvas `Screen Space Overlay`, sorting order 1000;
   - CanvasScaler na resolução de referência 1920×1080;
   - imagem preta ocupando a tela inteira;
   - `raycastTarget` controlado pelo estado do fade.
2. Substituir `SceneTransitionController.CreateFadeOverlay()` por carregamento
   do prefab e obtenção da imagem `Fade Image`.
3. Manter inalterados duração, uso de `unscaledDeltaTime`, pausa da campanha,
   checkpoint, carregamento de cena e posicionamento do jogador.
4. Falhar com mensagem explícita e impedir transição caso o prefab ou a imagem
   estejam ausentes.

### 4.2 — Remover construtores visuais legados

Apagar métodos e helpers não chamados que ainda criam UI em código:

- `GarrisonManagementPanel.CreateInterface`, `CreateText` e `CreateButton`;
- `TravelConversationPanel.CreateText` e `CreateButton`, mantendo apenas o
  carregamento do prefab e `SetButtonLabel`;
- `GlobalThreatHud.CreateCard`, `CreateText`, `BuildCardText` e o dicionário de
  cartões permanentes;
- imports e campos que ficarem sem uso após a limpeza.

A criação dos objetos controladores persistentes (`Navigation Runtime`,
`Interaction Prompt`, `Global Threat HUD` e painéis singleton) permanece
permitida nesta etapa: eles não contêm layout visual. Somente Canvas, imagens,
textos e botões devem vir de prefabs editáveis.

### 4.3 — Guardas automatizadas

1. Incluir `SceneFadeCanvas` em `UiPrefabWiringTests`.
2. Validar Canvas, `Fade Image`, cobertura de tela e sorting order 1000.
3. Adicionar teste de arquitetura que procure nos controladores de runtime por
   criação de `Canvas`, `Image`, `Button`, `TextMeshProUGUI` ou métodos antigos
   de montagem visual.
4. Preservar os 114 testes atuais e os 15 testes PlayMode.

### 4.4 — Verificação manual focal

1. Viajar Centro → cada portão → Centro e confirmar fade completo.
2. Tentar clicar durante a tela preta e confirmar que o raycast bloqueia input.
3. Confirmar que o input volta após o fade transparente.
4. Abrir construção, viagem e guarnição após uma troca de cena.
5. Editar cor/posição de um prefab no Inspector e confirmar que a mudança
   aparece sem alterar código.

### Critério de encerramento da migração

- Nenhum controlador de runtime cria hierarquia visual.
- Os sete prefabs UI estão presentes e passam nos testes de wiring.
- EditMode e PlayMode passam integralmente em duas execuções consecutivas.
- O fluxo Centro–Portão, modais e avisos não apresenta bloqueio de raycast,
  duplicação de Canvas ou perda de foco.

### Etapa 4 — concluída

- Criado `Assets/Resources/UI/SceneFadeCanvas.prefab` com Canvas em sorting
  order 1000 e `Fade Image` cobrindo a tela inteira.
- `SceneTransitionController` carrega o prefab e rejeita a transição quando a
  referência visual obrigatória não está disponível.
- Removidos os construtores visuais antigos de `GlobalThreatHud`,
  `GarrisonManagementPanel` e `TravelConversationPanel`.
- Adicionada guarda automatizada contra criação de Canvas, imagens, botões e
  textos nos controladores de runtime migrados.
- EditMode: 116/116 aprovados em duas execuções consecutivas
  (`Logs/Stage4EditMode1.xml` e `Logs/Stage4EditMode2.xml`).
- PlayMode: 15/15 aprovados em duas execuções consecutivas
  (`Logs/Stage4PlayMode1.xml` e `Logs/Stage4PlayMode2.xml`).

A migração técnica está encerrada. A verificação manual focal da seção 4.4
permanece recomendada para validar aparência, foco e sensação das transições no
Editor/build jogável.
