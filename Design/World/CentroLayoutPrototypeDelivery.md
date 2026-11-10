# Centro — protótipo jogável de circulação

> Registro histórico: esta etapa já foi integrada às cenas finais. As cenas de protótipo e seus geradores foram retirados; veja o [registro da limpeza](ApprovedPrototypeCleanup.md). As imagens e aprovações foram preservadas.

Etapa 2 entregue em cenas isoladas, a partir da planta aprovada. Esta entrega
valida espaço e interação com formas provisórias. A criação da nova arte externa
continua na etapa 3, começando pela fachada da Taverna.

Revisão: [galeria do protótipo](CentroLayoutStageTwoReview.html).

**Revisão de escala V2:** o usuário pediu o dobro de área total, mantendo o jogador
na mesma escala. O mapa passa de 26 × 20 u (520 u²) para 36,77 × 28,28 u
(1.040 u²). A base de cada construção também tem área duplicada. O fator linear
é √2, aproximadamente 1,414; portas, alcance de interação, velocidade e zoom da
câmera de jogo permanecem iguais.

Comparação: [antes/depois da escala](CentroLayoutScaleV2Review.html).
Coordenadas atuais: `CentroLayoutV2.json`. A planta V1 e duas capturas anteriores
foram preservadas para comparação.

## Como abrir

1. No Unity, abra `Assets/Scenes/Prototypes/CentroLayoutPrototype.unity`.
2. Entre no modo de jogo e use WASD/setas para andar, E para interagir e Esc
   para fechar os menus dos edifícios.
3. Percorra Quartel, Arqueiros, Mercado, Escola, coletor e quatro sentinelas.
4. Na rua inferior, use a porta para entrar na cópia da Taverna. A placa ao lado
   abre o gerenciamento do edifício.
5. Na cópia interna, use a saída para retornar diante da fachada do Centro.

O protótipo começa com 5.000 de ouro temporário, os edifícios no nível 1 e um
valor de impostos para experimentar o feedback. Cada nova execução inicia outra
sessão temporária. A Taverna nível 0 e sua construção foram exercitadas no teste.

## O que foi aplicado

- Envelope de circulação atual de 36,77 × 28,28 u; ruas principais de 7,07 u e
  secundárias de 3,39 u. As posições dos lotes e ações vêm de `CentroLayoutV2.json`.
- Cinco ocupações físicas distintas, marcadores de entrada/gerenciamento e
  rótulos provisórios. Os volumes mostram a base física; a altura das fachadas
  será aplicada com os sprites da etapa seguinte.
- Colisores sólidos nas bases, zonas de interação independentes e limites do
  mundo. O jogador usa os limites do mapa nesta cópia, em vez do retângulo da tela.
- Câmera acompanhando o jogador, com confinamento à área de cenário e margem
  decorativa. Os quatro sentinelas são marcadores de percurso; não saem do protótipo.
- Modais de construção/melhoria, saldo, prompts e impostos reutilizam os
  componentes existentes com a sessão temporária.
- Cópia do interior em `CentroLayoutTavernPrototype.unity`, com o mesmo personagem
  do Centro. A arte interna existente foi preservada como referência; exploração
  e recrutamento ficam desativados nessa cópia para focar a passagem e o retorno.
- Transição com fade, bloqueio durante a troca e retorno a um ponto diante da
  fachada, fora do gatilho de entrada. A Taverna não construída não permite entrar.

## Refinamentos encontrados ao jogar

- O coletor tem corpo sólido: a aproximação ocorre diante dele, não em sua posição
  central. A interação foi conferida nesse ponto alcançável.
- O personagem da Taverna antiga tinha configuração diferente. A cópia utiliza
  um prefab do personagem do Centro para manter a mesma escala e colisão.
- Na versão inicial, o retorno desenhado em Y = −9,25 ficou próximo demais da borda
  com o colisor real. O gerador ajusta a posição para manter folga de 0,35 u entre
  o corpo e o limite, além de permanecer fora da entrada.
- Um objeto oculto de prévia de animação foi retirado da renderização da cópia
  para evitar a aparência de um segundo jogador. Os rótulos foram afastados do
  personagem e a visão geral de revisão oculta os avisos que cobriam os extremos.
- Um erro de encerramento do menu de exploração da cena antiga apareceu na
  primeira cópia do interior. O menu não participa deste protótipo e foi removido
  somente da cópia; o controlador e a cena de produção não foram alterados.

## Isolamento

Antes de iniciar Play nessas cenas, uma configuração de Editor prepara dados
temporários. `GameSessionProvider` usa `MemoryGameSaveRepository` quando essa
configuração está ativa, inclusive para os serviços iniciados antes dos objetos
da cena. Ao encerrar Play ou iniciar uma cena normal, a configuração é limpa.

As cenas `Centro.unity`, `Taverna.unity` e o arquivo de Build Settings mantiveram
hashes idênticos aos registrados antes da geração. Os protótipos não foram
adicionados à lista de cenas do build. As transições de teste usam as cópias
diretamente no Editor.

A mudança no movimento adiciona um limite de mundo opcional. Ele foi conectado
somente aos personagens das cópias; sem esse campo, os mapas existentes continuam
com o comportamento anterior.

## Validação

**Teste funcional focado: 1/1 passou.** O percurso usa entrada de movimento e
física reais, com verificação de:

- acesso e seleção dos edifícios, abertura/fechamento dos menus e liberação do jogador;
- colisão contra a base do Quartel;
- aproximação e coleta no NPC de impostos;
- quatro saídas e enquadramento da câmera dentro do cenário;
- bloqueio da Taverna não construída, gerenciamento separado e construção;
- entrada no interior, aproximação à saída e retorno diante da fachada;
- ausência de reentrada automática e preservação do estado temporário na ida/volta;
- igualdade dos dados da partida salva antes/depois, incluindo a chave legada.

A revisão V2 também confere numericamente a área de 1.040 u², a base ampliada do
Quartel e a largura inalterada do colisor do jogador. A conferência geométrica
em `CentroLayoutScaleV2Geometry.json` verifica o fator 2 nas cinco bases, portas
sem aumento e rotas sem interseção com os volumes ampliados.

O teste da V2 passou. As cinco capturas externas foram atualizadas e a visão
geral usa fundo uniforme no enquadramento de revisão; as duas capturas do
interior permanecem válidas, pois sua escala não mudou. A revisão V2 e as
referências anteriores estão lado a lado na galeria específica de escala.

Depois do teste, os últimos ajustes foram somente de rótulos, prévia oculta e
composição das capturas. O percurso, os limites, os serviços e a transição não
foram modificados nessa revisão visual.

Foram geradas sete capturas de revisão: cinco do Centro e duas do interior,
em 1920 × 1080 e 1366 × 768. A visão geral usa zoom de revisão; as demais mostram
o enquadramento próximo ao jogador. As imagens continuam candidatas à revisão;
não foi promovida uma baseline automaticamente. Teste funcional não aprova arte.

```powershell
.\tools\Test-CentroLayoutPrototype.ps1
.\tools\Test-Visual.ps1 -Scenario centro-layout-prototype
.\tools\Test-Visual.ps1 -Scenario centro-layout-interior
```

## Próxima etapa

Validar a sensação de espaço e a clareza da porta nesta cópia. Em seguida, criar
a arte piloto da Taverna ao lado do jogador real, usando a altura de porta e a
escala de fachada aprovadas na régua. A aplicação dos cinco exteriores e a
substituição da cena de produção continuam em etapas posteriores.
