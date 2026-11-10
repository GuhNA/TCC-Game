# Centro — entrega da planta e escala

Etapa 1 concluída e aprovada pelo usuário. A planta orienta uma cópia isolada
para prototipagem; não representa arte final aprovada nem substitui a cena Centro.

Revisão: [planta interativa e régua](CentroLayoutStageOneReview.html).

Pranchas estáticas: [planta](CentroLayoutMap.png) e
[comparação de proporções](CentroScaleBoard.png). As linhas tracejadas em torno
das bases na planta estática mostram a folga geométrica de 0,38 u.

## Medidas obtidas no Unity

| Referência atual | Medida |
|---|---:|
| Altura visual do jogador (H) | 1,996 u |
| Largura visual do jogador | 1,353 u |
| Largura de colisão (W) | 0,760 u |
| Velocidade configurada | 5,02 u/s |
| Retângulo do terreno atual | 17,762 × 9,997 u |
| Área livre geométrica aproximada nesse terreno | 103,79 u² |

A altura visual usa o quadro atual do sprite do jogador, não sua altura de
colisão. A estimativa de área consulta os colisores sólidos atuais, excluindo
o jogador, em uma grade de 0,05 u; não representa uma medição de acessibilidade
completa. Os dados vieram da cena salva, sem entrar em modo de jogo ou ler o save.

## Planta proposta

- Envelope de circulação de 26 × 20 u, centrado na praça, com eixos para os quatro
  sentinelas. O enquadramento deixa de definir a fronteira física do jogador.
- Ruas principais com 5 u livres (6,58 W), secundárias com 2,4 u (3,16 W) e praça
  de raio 3,5 u. A rua inferior conecta a praça à fachada sul da Taverna.
- Cinco lotes próprios: Quartel e Arqueiros a oeste; Mercado e Taverna a leste;
  Escola ao norte, deslocada da saída e conectada por uma passagem secundária.
- Coletor diante do Mercado, fora do acesso de gerenciamento. Spawns e sentinelas
  são pontos separados, deixando espaço para chegar sem acionar uma saída.
- Ponto de chegada geral em (0; −0,6). As coordenadas são referências de projeto
  e podem ser refinadas ao testar o corpo e a câmera reais.
- Margem decorativa inicial de 6 u além do envelope; conferir com a câmera nos
  extremos e a altura da torre para dimensionar o cenário sem áreas vazias.

O piso de circulação desenhado soma aproximadamente **230,18 u²**, descontando
as pegadas sólidas, cerca de **2,22 vezes** a estimativa livre do terreno atual.
Isso é uma comparação preliminar entre geometrias de projeto; ainda não é uma
afirmação de área acessível no jogo. O protótipo verificará conectividade, folga
do colisor completo, limites, espaços adicionais de grama e oclusão pelas fachadas.

## Escala das novas construções

| Construção | Largura × altura visual da arte | Altura em H | Pegada física |
|---|---|---:|---|
| Quartel | 6,4 × 6,4 u | 3,21 H | 6 × 2,8 u |
| Estande de Arqueiros | 6 × 5,2 u | 2,61 H | 6 × 4 u |
| Mercado Local | 6,6 × 5,8 u | 2,91 H | 6,4 × 2,8 u |
| Taverna | 6,4 × 6,6 u | 3,31 H | 6,4 × 4 u |
| Escola de Magia | 4,4 × 8 u | 4,01 H | 3,1 × 2,1 u |

Porta de referência: **2,7 u de altura × 1,35 u de largura** (1,35 H × 1,78 W).
As elevações da prancha são volumes esquemáticos, com o mesmo jogador real ao
lado de cada uma. No estande aberto, a porta desenhada é somente uma régua de
passagem; o exterior final terá uma entrada aberta para o pátio.

A pegada física delimita o piso ocupado, não a projeção total da arte. Telhados
e paredes altas podem se projetar acima dela. Os sprites terão apoio no chão,
escala uniforme e densidade visual comum, a validar primeiro na fachada piloto
da Taverna. O jogador atual tem PPU 700; seu tamanho não será alterado para
compensar diferenças de arte. O PPU das novas exportações deve preservar as
dimensões físicas propostas, considerando o tamanho e a ampliação do arquivo.

## Taverna: três pontos distintos

| Ação | Coordenada proposta |
|---|---|
| Soleira da fachada sul | (8; −7,2) |
| Entrar, diante da porta | (8; −7,85) |
| Gerenciar, na placa lateral | (10,7; −8,45) |
| Retorno do interior | (8; −9,25) |

A separação entre entrada e gestão é de aproximadamente 2,77 u. O retorno fica
a 1,4 u do centro proposto de entrada. Dimensões e prioridade dos gatilhos serão
validadas no protótipo para evitar sobreposição de ações ou nova entrada ao sair.
A construção no nível 0 mantém o gerenciamento disponível e a entrada bloqueada,
seguindo a regra existente. A arte externa substituirá a imagem de interior atual.

## Conferência desta entrega

- As cinco rotas do esquema não atravessam as pegadas físicas ampliadas por um
  raio de 0,38 u (meia largura do colisor). As cinco pegadas não se sobrepõem.
- O esquema exporta suas coordenadas em JSON, evitando redesenhar posições na
  implementação. A galeria permite destacar cada rota e mostrar a folga geométrica.
- A régua usa o sprite real com seu recorte e a mesma altura em todas as elevações.
- As duas pranchas foram renderizadas localmente em PNG e inspecionadas.
  A abertura automatizada da página local foi bloqueada pela política da
  ferramenta de navegação; a revisão por arquivo permanece disponível.
- Nenhum teste funcional de jogo foi executado nesta etapa de planta.

Ainda precisam de confirmação no Unity: colisão vertical real, alcance das
interações, portas, mudança e retorno de cena, ordenação dos telhados, câmera e
legibilidade em 1920 × 1080 e 1366 × 768.

## Arquivos de referência

- `CentroLayoutV1.json`: coordenadas, ruas, medidas e identidade dos lotes.
- `CentroCurrentMeasurements.json`: levantamento da cena atual.
- `CentroLayoutGeometryReport.json`: resultado da conferência geométrica da planta.
- `Assets/Scripts/Editor/CentroLayoutSurvey.cs`: levantamento somente leitura.
- `tools/Build-CentroLayoutReview.py`: geração da prancha e conferência geométrica.
- `tools/Render-CentroLayoutBoards.cjs`: renderização local das pranchas SVG em PNG.
