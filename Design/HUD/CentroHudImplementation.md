# Centro — implementação por etapas

Design aprovado pelo usuário em 27/10/2026. As 11 capturas aprovadas estão em
`Assets/Tests/Visual/Baselines/centro-proposals/`, preservadas a partir da galeria.
A aprovação estabelece a referência visual; não significa que a proposta já
esteja integrada à cena ou que seus controles demonstrativos estejam funcionais.

## 1. Modal dos edifícios — concluído em 27/10/2026

Aplicar o template ao Quartel, Estande de Arqueiros, Escola de Magia, Mercado
Local e Taverna. Conectar nível, benefício atual/próximo, custo e saldo reais;
tratar construção inicial, ouro insuficiente e nível máximo. Padronizar Esc,
fechamento e bloqueio do jogador. Preservar as regras econômicas existentes.

Entrega: capturas dos controles de produção comparadas ao protótipo aprovado e
uma verificação funcional focada em construção/melhoria, desconto único,
indisponibilidade, atualização de dados e liberação do jogador ao fechar.

### Resultado da entrega

- `BuildingCanvas.prefab` integrado à cena Centro e conectado ao CityManager;
  apenas o modal legado foi substituído.
- Os cinco edifícios exibem nível, ilustração, benefício atual/próximo, custo,
  saldo após melhoria e estados de construção, falta de ouro e nível máximo.
- Sucesso exibido dentro do modal, com atualização do edifício e do saldo.
  Eventos econômicos durante a transação não redesenham dados intermediários.
- Fechamento por X, Voltar e Esc; bloqueio compartilhado de movimento/interação,
  seleção de um controle disponível e limpeza do edifício selecionado ao fechar.
- Um teste funcional passou, exercitando os cinco edifícios, construção e
  melhoria, desconto único, rejeição sem ouro, atualização após receber dinheiro,
  nível máximo, área clicável e fechamento. A simulação de Esc ignora o foco
  da janela de teste; as configurações de entrada são restauradas ao terminar.
- O teste usa repositório em memória e não grava transações no save do jogador.
- Nove capturas de produção passaram na comparação com o protótipo aprovado,
  incluindo 1920 × 1080 e 1366 × 768. As referências de `centro-buildings`
  foram copiadas das imagens aprovadas de `centro-proposals`, nunca dos renders
  candidatos de produção. Inspeção visual conferiu o modal e a versão compacta.
- A captura do modal legado foi arquivada em `CentroLegacyBuilding.png`; seu
  cenário temporário foi retirado para não sobrescrever o histórico após a integração.

Revisão: [galeria da etapa 1](CentroHudStageOneReview.html).

Verificações reproduzíveis, separadas:

```powershell
.\tools\Test-CentroBuildingSmoke.ps1
.\tools\Test-Visual.ps1 -Scenario centro-buildings
```

## 2. Impostos e interações — concluído em 27/10/2026

Aplicar o aviso compacto de coleta, com tempo de leitura de 3–4 segundos,
saldo atualizado e mensagens claras quando não houver Mercado ou impostos.
Distinguir gestão do edifício e entrada na Taverna. Manter a coleta em uma ação.

A placa de impostos foi apresentada como opção contextual: adotar a exibição
na proximidade do coletor como direção proposta, mantendo essa escolha explícita
na revisão da etapa. A consulta de valores deve ser somente leitura.

Entrega: revisão visual do aviso e da placa; verificação de coleta única,
resultados vazios e respeito à prioridade dos modais.

### Resultado da entrega

- `CityTaxCanvas.prefab` integrado ao coletor do Centro com a placa e o aviso
  do protótipo. O aviso legado da cena foi removido; o feedback de outros mapas
  permanece no componente compartilhado existente.
- A placa é contextual ao coletor selecionado para interação e permanece durante
  a leitura do resultado. Não há um novo saldo global ou um modal de confirmação.
- `TaxService.GetCollectibleAmount()` projeta o valor sem alterar o estado,
  coletar ou gravar a partida. Usa o mesmo limite de 24 horas da coleta e protege
  a consulta contra recuo do relógio.
- Resultado visível por 3,5 segundos de tempo de leitura. Placa e aviso ficam
  ocultos durante um modal e retomam após seu fechamento, sem consumir o tempo
  que o jogador não pôde ler. Nenhum bloqueio de movimento é adquirido pela coleta.
- Prompts distinguem gerenciar os cinco edifícios, entrar na Taverna e consultar
  ou coletar impostos. O valor disponível acompanha o prompt do coletor.
- A revisão revelou uma escala zero salva na instância de ouro do Centro e uma
  cópia decorativa extra no prefab compartilhado. A configuração local da cena
  foi normalizada: canvas raiz próprio e somente o painel ligado ao saldo ativo.
  O prefab compartilhado não foi alterado nesta etapa.
- Teste funcional final: **1/1 passou**, com repositório em memória. Cobre
  consulta repetida sem gravação, crédito único de 120 ouro, saldo atualizado,
  contexto, distinção de prompts, pausa/retomada e expiração do aviso, resultados
  vazios/sem Mercado, bloqueio de coleta durante modal, configuração do HUD de
  ouro, recuo do relógio e equivalência da prévia/coleta no limite de 24 horas.
- Quatro capturas geradas em 1920 × 1080 e 1366 × 768 e inspecionadas visualmente.
  A placa e o aviso seguem o protótipo; ouro e prompt são as peças compartilhadas
  de produção. Após revisão explícita do usuário, as quatro capturas foram
  aprovadas e preservadas em `Assets/Tests/Visual/Baselines/centro-taxes/`,
  com igualdade dos arquivos verificada. O relatório da captura inicial mantém
  seu status histórico de candidato; as próximas execuções usarão essa baseline.

Revisão: [galeria da etapa 2](CentroHudStageTwoReview.html).

```powershell
.\tools\Test-CentroTaxSmoke.ps1
.\tools\Test-Visual.ps1 -Scenario centro-taxes
```

## 3. Revisão do conjunto

Conferir ouro, viagens e alertas compartilhados no Centro, sem duplicar HUDs.
Capturar a composição integrada em 1920 × 1080 e 1366 × 768 e comparar com
as imagens aprovadas. A baseline de produção fica separada da do protótipo;
diferenças de composição devem ser apresentadas para revisão.

A etapa 3 está pendente. Ela confere o conjunto integrado, viagens e alertas
compartilhados antes da revisão final do Centro.
