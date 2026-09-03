# Planejamento do sistema de defesa e ondas

**Status:** Partes 1, 2, 3, 4 e 5 implementadas  
**Versão:** 1.1  
**Escopo da primeira entrega:** vertical slice de 10 ondas

## 1. Visão do produto

O jogador administra a defesa de uma cidade caminhando entre seus distritos e
conversando com NPCs. A cidade gera uma renda inicial por meio de suas
construções. Essa renda permite construir, contratar soldados, deslocar
guarnições e reparar os portões.

Com o avanço da campanha, os ataques tornam-se mais frequentes e mais fortes.
As despesas com baixas e reparos passam a superar a renda urbana, preparando a
necessidade de fontes de renda alternativas que serão adicionadas no futuro.

Todas as ações realizadas no mapa devem acontecer por meio de NPCs. O HUD pode
apresentar informações globais, mas não deve substituir as conversas usadas
para tomar decisões.

## 2. Objetivos da versão

O jogador deve conseguir:

1. Entender quando, onde e com qual força o próximo ataque acontecerá.
2. Investir ouro nas construções da cidade.
3. Contratar guerreiros, arqueiros e magos.
4. Distribuir e deslocar tropas entre quatro portões cardinais.
5. Viajar entre o Centro e cada portão.
6. Acompanhar uma batalha e compreender seu resultado.
7. Receber pouco espólio e visualizar baixas, reparos e saldo líquido.
8. Perceber que a renda da cidade deixa de acompanhar o custo da guerra.

## 3. Princípios de design

### 3.1 Clareza antes de surpresa

Uma onda anunciada não pode mudar de direção, horário, força ou composição
depois de apresentada ao jogador. O investimento feito durante o aviso deve
gerar uma vantagem real.

### 3.2 Ações por NPC, informação pelo mundo

- O HUD global informa ameaças e recursos.
- Batedores, capitães, recrutadores, engenheiros e mestres de obras executam
  as ações.
- Sinos, cornetas, bandeiras, falas e animações reforçam o estado dos portões.
- Cores nunca devem ser o único indicador; textos e ícones também devem
  comunicar o estado.

### 3.3 Pressão econômica gradual

O início da campanha deve permitir erros e experimentação. O déficit surge
gradualmente por meio de baixas e reparos, não por uma taxa de manutenção
invisível.

### 3.4 Escalonamento limitado pelo progresso

O tempo é a principal fonte de crescimento das ondas. O progresso do jogador
produz somente um ajuste limitado, evitando que melhorar a cidade pareça
inútil.

### 3.5 Uma única fonte de verdade

Relógio, portões, tropas e ondas pertencem à sessão persistente. Componentes de
cena apenas apresentam os dados e encaminham comandos, seguindo o padrão já
usado por `GameSession`.

## 4. Loop principal

```text
Batedor anuncia ameaça
        ↓
Jogador coleta renda e investe na cidade
        ↓
Recrutador vende tropas
        ↓
Capitão aloca ou desloca soldados
        ↓
Onda ataca um dos quatro portões
        ↓
Batalha causa baixas, dano e pequeno espólio
        ↓
Relatório mostra o resultado militar e econômico
        ↓
Novo ciclo
```

## 5. NPCs planejados

| NPC | Local | Responsabilidade |
|---|---|---|
| Batedor | Centro | Informar agenda e composição das ondas |
| Coletor de impostos | Centro | Coletar renda e mostrar produção |
| Mestre de obras | Centro | Construir e melhorar edifícios |
| Recrutador | Centro ou Portão | Contratar tropas e mostrar estoque |
| Capitão | Cada portão | Administrar guarnição e deslocamentos |
| Engenheiro | Cada portão | Reparar a estrutura do portão |
| Sentinela | Saídas cardinais | Levar o jogador entre Centro e Portão |

As construções deixam de ser diretamente interativas. Um mestre de obras ou
encarregado posicionado próximo à construção passa a abrir o painel existente.

## 6. Organização das cenas

### 6.1 Proposta

- `Centro`: núcleo econômico e administrativo.
- `Portão`: uma única cena reutilizável.
- `GateId`: identifica Norte, Leste, Sul ou Oeste antes de carregar a cena.

O mapa do Centro já possui quatro estradas cardinais. Cada saída deve ter uma
sentinela. Ao conversar com ela, a sessão registra o portão de destino e
carrega a cena compartilhada de Portão. Arte e interface são compartilhadas;
vida, guarnição, onda e direção vêm do estado persistente correspondente.

### 6.2 Estado das tropas

As tropas deixam de ser apenas um total global e passam a ocupar um dos
seguintes estados:

- Reserva central.
- Guarnição Norte.
- Guarnição Leste.
- Guarnição Sul.
- Guarnição Oeste.
- Em deslocamento.

O deslocamento inicialmente deve durar entre 15 e 25 segundos estratégicos.

## 7. Comunicação das ondas

### 7.1 HUD global

O HUD de ameaças é permanente e somente informativo. Cada portão apresenta:

- Direção.
- Estado: seguro, observado, ataque iminente ou em combate.
- Contagem regressiva.
- Força prevista da onda.
- Força da guarnição.
- Prognóstico: favorável, arriscado ou desfavorável.

Exemplo:

```text
NORTE   01:42   Ameaça 18   Defesa 12   DESFAVORÁVEL
LESTE   Seguro             Defesa 8
SUL     03:10   Ameaça 9    Defesa 11   FAVORÁVEL
OESTE   Observação         Defesa 6
```

### 7.2 Estágios de aviso

- 90 segundos: anúncio do batedor.
- 30 segundos: sino ou corneta e guardas em alerta.
- 10 segundos: destaque forte e contagem regressiva.
- 0 segundos: portão entra em combate.
- Final: relatório militar e econômico.

## 8. Escalonamento inicial

O valor inicial proposto para testes é:

```text
forçaBase = arredondar(10 × 1,22 ^ (númeroDaOnda - 1))
forçaFinal = arredondar(forçaBase × fatorDeProgresso)
fatorDeProgresso entre 0,90 e 1,15
```

Força base aproximada das dez primeiras ondas:

```text
10, 12, 15, 18, 22, 27, 33, 40, 49, 60
```

Intervalos iniciais propostos:

| Ondas | Intervalo após o ataque anterior |
|---|---:|
| 1–3 | 150–240 segundos |
| 4–7 | 100–180 segundos |
| 8–10 | 75–140 segundos |

Regras de justiça:

- O primeiro ataque deve conceder pelo menos 180 segundos de preparação.
- Um portão não pode ser escolhido mais de duas vezes consecutivas.
- Uma onda anunciada é imutável.
- O sorteio é determinístico e persistido no save.
- A adaptação usa o progresso existente quando a onda é criada, nunca quando
  ela chega.

## 9. Economia inicial

Os valores atuais são mantidos durante o primeiro teste de balanceamento:

- Guerreiro: 3 de ouro.
- Arqueiro: 5 de ouro.
- Mago: 8 de ouro.
- Mercado nível 1: 10 de ouro por minuto.
- Mercado recebe mais 5 de produção por nível.

O relógio de seis horas reais do recrutamento não atende ao loop ativo. A
proposta é convertê-lo posteriormente em um ciclo estratégico de
aproximadamente seis minutos, preservando a fórmula atual de capacidade.

O espólio deve cobrir somente entre 10% e 25% do custo médio provocado por
baixas e reparos. O relatório precisa apresentar separadamente:

- Espólio recebido.
- Valor estimado das baixas.
- Custo do reparo.
- Resultado líquido da batalha.
- Renda estimada até o próximo ataque.

Enquanto as fontes alternativas de renda não forem implementadas, a campanha
será encerrada após a décima onda. As últimas ondas devem tornar negativa a
tendência do caixa, sem exigir que o jogador resolva um problema ainda sem
solução no jogo.

## 10. Combate da primeira versão

O combate recomendado é automático e visível, com duração aproximada de 20 a
30 segundos.

- Guerreiros sustentam a linha de frente.
- Arqueiros são eficientes contra grupos leves.
- Magos são caros e eficientes contra inimigos resistentes.
- Tropas desprotegidas recebem penalidades.
- Inimigos restantes após romper a guarnição causam dano ao portão.
- O capitão apresenta uma previsão antes da batalha.

O controle individual de soldados não faz parte deste vertical slice.

### 10.1 Vantagens entre soldados e inimigos

Cada tipo de soldado possui uma especialização contra um arquétipo inimigo:

| Soldado | Vantagem contra | Justificativa |
|---|---|---|
| Guerreiro | Saqueador | Contém o avanço de tropas leves em combate próximo |
| Arqueiro | Xamã | Consegue eliminar unidades frágeis de suporte à distância |
| Mago | Bruto | Ataques mágicos atravessam a resistência física do inimigo |

Valor inicial de balanceamento:

```text
multiplicadorComVantagem = 1,50
multiplicadorNormal = 1,00
```

Nesta versão não existe penalidade explícita contra os demais inimigos. Uma
composição adequada recebe um bônus, enquanto uma composição inadequada apenas
deixa de recebê-lo. Isso reduz o risco de uma escolha errada provocar uma
derrota desproporcional.

O prognóstico apresentado pelo capitão deve considerar essas vantagens e
explicar sua influência. Exemplo:

```text
Prognóstico: arriscado
Seus magos possuem vantagem contra os 3 Brutos desta onda.
Faltam arqueiros para responder aos 2 Xamãs.
```

As relações e seus multiplicadores devem vir de uma configuração de
balanceamento, não de condicionais espalhadas pela interface ou pelo sistema
de combate.

## 11. Ordem geral de implementação

1. Relógio estratégico, quatro portões e agenda persistente de ondas.
2. Navegação Centro–Portão e prompts de interação.
3. Reserva, guarnições e deslocamento de tropas.
4. HUD global de ameaças e avisos audiovisuais.
5. Resolução de combate, dano, baixas e espólio.
6. Relatório econômico e balanceamento das dez ondas.
7. Conversão das construções para interação por NPC.

---

# Especificação da Parte 1 — Fundação da campanha

## 12. Objetivo

Criar uma camada de domínio persistente e independente de cena que represente:

1. O tempo ativo da campanha.
2. Os quatro portões cardinais.
3. A agenda de ondas futuras.
4. A transição de uma onda anunciada para uma onda pronta para combate.

Esta parte não inclui interface, navegação, movimentação de tropas nem
resolução de combate.

## 13. Escopo funcional

### 13.1 Relógio estratégico

O relógio estratégico representa apenas o tempo ativo da campanha.

Requisitos:

- Começa em zero em uma campanha nova.
- Avança usando o tempo de execução do jogo, não a hora do sistema.
- Não avança durante pausa ou conversa modal.
- Não avança enquanto o jogo está fechado.
- É persistido em segundos, aceitando frações internamente.
- Ao recarregar o save, continua do mesmo instante.
- Pode receber múltiplos pedidos de pausa simultâneos sem ser liberado antes
  que todos os solicitantes tenham encerrado a pausa.

O `IClock` atual continua sendo usado temporariamente pelos sistemas legados de
impostos e recrutamento. A migração desses sistemas para tempo estratégico será
uma mudança separada, evitando alterar duas regras econômicas durante a criação
do diretor de ondas.

### 13.2 Estado persistente dos portões

Deve existir exatamente um estado para cada valor de `GateId`:

```text
North
East
South
West
```

Cada portão começa com:

- Integridade atual: 100.
- Integridade máxima: 100.
- Guarnição vazia.
- Nenhum ataque anterior.

O exército presente no save atual permanece como reserva central. A migração
não distribui tropas automaticamente e não altera as quantidades já compradas.

Modelo persistente proposto:

```text
GateSaveData
    gateId
    currentHealth
    warriors
    archers
    mages
    lastResolvedWaveSequence
```

A integridade máxima deve vir de uma definição de balanceamento e não precisa
ser duplicada em cada save enquanto permanecer fixa.

### 13.3 Estado persistente da campanha

Modelo proposto:

```text
CampaignSaveData
    initialized
    elapsedStrategicSeconds
    randomSeed
    nextWaveSequence
    scheduledWaves[]
```

O estado runtime de pausa não é salvo. Toda sessão carregada começa sem donos
de pausa registrados.

### 13.4 Modelo persistente de onda

```text
WaveSaveData
    sequence
    gateId
    arrivalStrategicSeconds
    threatPower
    status
    raiders
    brutes
    shamans
    warningStage
```

Estados inicialmente necessários:

```text
Scheduled → AwaitingResolution → Resolved
```

Na Parte 1, o diretor encerra sua responsabilidade em
`AwaitingResolution`. Um serviço de combate futuro consumirá a onda e marcará
o resultado.

### 13.5 Composição provisória

Para permitir persistência e testes antes do combate, são definidos três
arquétipos provisórios:

| Inimigo | Custo de ameaça | Disponibilidade |
|---|---:|---|
| Saqueador | 1 | Desde a onda 1 |
| Bruto | 3 | Desde a onda 3 |
| Xamã | 4 | Desde a onda 5 |

Os nomes e multiplicadores poderão mudar quando o combate for especificado,
mas uma composição já anunciada nunca será recalculada em um save existente.

A composição persistida também permite que o futuro sistema de combate aplique
as vantagens já aprovadas:

```text
Guerreiro → Saqueador × 1,50
Arqueiro  → Xamã      × 1,50
Mago      → Bruto     × 1,50
```

Essa afinidade não altera retroativamente a força anunciada da onda. Ela é
usada para calcular a força efetiva da guarnição e o prognóstico do confronto.

## 14. Cálculo do progresso do jogador

O fator adaptativo inicial considera somente poder militar e níveis das três
construções militares:

```text
poderDasTropas = guerreiros × 1,00
               + arqueiros × 1,25
               + magos × 2,00

progressoMilitar = poderDasTropas
                 + 2 × (nívelQuartel
                      + nívelEstandeArqueiros
                      + nívelEscolaMagia)

progressoEsperado = 20 + 4 × (númeroDaOnda - 1)

fatorDeProgresso = limitar(
    0,90 + 0,10 × progressoMilitar / progressoEsperado,
    mínimo: 0,90,
    máximo: 1,15
)
```

O cálculo considera todas as tropas: reserva, guarnições e deslocamentos. O
valor é capturado no momento em que a onda é agendada.

## 15. Seleção do portão

Para a primeira versão:

1. O diretor cria uma lista dos quatro portões.
2. Se as duas últimas ondas atacaram o mesmo portão, ele é removido da lista.
3. Um portão é sorteado uniformemente entre os restantes.
4. O resultado é persistido; o sorteio é derivado da semente da campanha e
   do número da onda.

O diretor não favorece deliberadamente o portão mais fraco nesta etapa. Essa
regra poderá ser adicionada depois de existirem dados de balanceamento.

## 16. Agenda e avisos

O diretor deve manter pelo menos uma onda futura agendada. Ondas adicionais
podem ser suportadas pelo modelo, mas o primeiro corte não precisa criar
ataques simultâneos.

Transições:

```text
tempo restante > 90 s       → Scheduled / sem aviso urgente
tempo restante ≤ 90 s       → aviso inicial
tempo restante ≤ 30 s       → aviso iminente
tempo restante ≤ 10 s       → aviso crítico
tempo restante ≤ 0 s        → AwaitingResolution
```

Cada estágio é emitido uma única vez. O estágio alcançado também é persistido
para que carregar um save não reproduza repetidamente sinos ou notificações.

Eventos de domínio planejados:

```text
WaveScheduled
WaveWarningChanged
WaveBecameDue
CampaignPauseChanged
```

## 17. Geração determinística

O diretor não deve usar diretamente o estado global de `UnityEngine.Random`.
Ele deve possuir um gerador determinístico baseado na semente persistida.

Dado o mesmo estado de save, duas execuções devem gerar:

- O mesmo portão.
- O mesmo intervalo.
- A mesma força.
- A mesma composição.

Depois de criada, a onda inteira é salva; ela não depende de uma nova geração
ao carregar o jogo.

## 18. Persistência e migração

O save deve avançar da versão 3 para a próxima versão de forma aditiva.

Na migração:

- Ouro é preservado.
- Construções são preservadas.
- Impostos são preservados.
- Estoque de recrutamento é preservado.
- Tropas compradas permanecem na reserva central.
- Aventureiros e itens são preservados.
- Quatro portões íntegros e vazios são adicionados.
- O relógio estratégico começa em zero.
- A primeira onda é agendada com 180 segundos de preparação quando a sessão
  inicializa a campanha migrada.

Normalização obrigatória:

- Remover ou consolidar portões duplicados.
- Criar qualquer portão cardinal ausente.
- Impedir vida e quantidade de tropas negativas.
- Ignorar entradas de onda inválidas sem perder o restante do save.

## 19. Integração com Unity

O domínio permanece em classes C# sem dependência de cena. Um único componente
de runtime encaminha `deltaTime` para a sessão.

Responsabilidades propostas:

```text
CampaignRuntimeDriver (MonoBehaviour)
    lê deltaTime
    solicita avanço da campanha
    salva checkpoints periódicos
    encaminha pausa/retomada da aplicação

CampaignService
    controla tempo e pausas
    expõe o estado dos portões

WaveDirectorService
    agenda ondas
    atualiza estágios de aviso
    entrega ondas prontas ao futuro sistema de combate
```

O driver deve salvar um checkpoint:

- Ao cruzar um estágio de aviso.
- Quando uma onda fica pronta.
- Ao pausar ou fechar a aplicação.
- Periodicamente, no máximo a cada 10 segundos ativos.

Não deve ocorrer gravação a cada frame.

## 20. Critérios de aceitação da Parte 1

### 20.1 Relógio

- Avança com tempo ativo.
- Para com um dono de pausa.
- Continua parado enquanto qualquer dono de pausa permanecer.
- Retoma somente após a liberação de todos os donos.
- Mantém o valor após salvar e recarregar.

### 20.2 Portões

- Uma campanha nova possui exatamente quatro portões únicos.
- Todos começam com 100 de integridade e guarnição vazia.
- Modificações persistem após recarregar.
- Tropas existentes antes da migração continuam na reserva.

### 20.3 Ondas

- A primeira onda chega após 180 segundos estratégicos.
- A mesma semente e o mesmo estado geram a mesma onda.
- Nenhum portão recebe mais de dois ataques consecutivos.
- Força e composição não mudam depois do agendamento.
- Avisos de 90, 30 e 10 segundos acontecem uma única vez.
- Ao chegar a zero, a onda fica aguardando resolução.
- O relógio parado não aproxima a onda.

### 20.4 Regressão

- Todos os testes atuais de economia, construções, impostos, recrutamento,
  inventário e cenas continuam passando.
- Saves da versão 3 são migrados sem perda de estado.

## 21. Fora do escopo da Parte 1

- HUD de ameaças.
- NPC batedor e capitães.
- Transição entre cenas.
- Alocação e deslocamento de tropas.
- Combate e animações.
- Dano efetivo aos portões.
- Espólio e relatório econômico.
- Mudança do relógio de impostos e recrutamento.

## 22. Decisões confirmadas

1. Conversas modais pausam completamente o relógio estratégico.
2. Os inimigos provisórios são Saqueador, Bruto e Xamã.
3. O primeiro ataque concede 180 segundos de preparação.
4. A Parte 1 trabalha com uma onda por vez; sobreposição fica para uma etapa
   posterior.
5. Os pesos iniciais são: guerreiro 1, arqueiro 1,25 e mago 2.
6. Cada soldado recebe multiplicador 1,50 contra seu inimigo favorecido, sem
   penalidade explícita nos demais confrontos.
7. A relação inicial é Guerreiro contra Saqueador, Arqueiro contra Xamã e Mago
   contra Bruto.

---

# Plano técnico de execução da Parte 1

## 23. Resultado esperado

Ao concluir esta parte, qualquer cena carregada terá acesso a uma campanha
persistente capaz de:

- Manter tempo estratégico ativo.
- Pausar e retomar o tempo com múltiplos solicitantes.
- Consultar os quatro portões e suas guarnições.
- Gerar uma onda determinística e imutável.
- Emitir os avisos de 90, 30 e 10 segundos.
- Colocar a onda em espera quando o momento do ataque chegar.
- Salvar e recuperar todo esse estado sem perder dados da versão atual.

Não haverá HUD nem combate nesta entrega. A correção será demonstrada por
testes de domínio, persistência e integração runtime.

## 24. Arquitetura proposta

```text
CampaignRuntimeDriver
        │ Advance(deltaTime) / Checkpoint()
        ▼
GameSession.Campaign
        ├── relógio e donos de pausa
        ├── estado dos quatro portões
        └── WaveDirectorService
                ├── CampaignRules
                ├── CampaignRandom
                └── WaveGenerator
                         │
                         ▼
                    GameSaveData v4
                         │
                         ▼
                 GameSaveRepository
```

### 24.1 Responsabilidades

`CampaignService` será a única porta pública da fundação. Ele controla o
relógio, os pedidos de pausa e a consulta dos portões. O serviço possui um
`WaveDirectorService` e o atualiza depois de avançar o relógio.

`WaveDirectorService` controla agenda, estágios de aviso e entrega da onda. Ele
não conhece cenas, HUD, áudio ou prefabs.

`WaveGenerator` é uma operação pura: recebe regras, save e sequência e devolve
uma onda completa. Depois disso, nenhum atributo anunciado pode ser
recalculado.

`CampaignRuntimeDriver` é a única ponte com o ciclo de vida da Unity. Ele não
contém regras de negócio.

### 24.2 Fluxo de uma atualização

```text
Driver recebe deltaTime
    ↓
CampaignService verifica se existe algum dono de pausa
    ↓
Relógio avança
    ↓
WaveDirector calcula tempo restante
    ↓
Se cruzou um limite: persiste o estágio e publica evento
    ↓
Se chegou a zero: marca AwaitingResolution e publica WaveBecameDue
```

Em mudanças semânticas, a ordem deve ser: validar, alterar estado, persistir e
somente então publicar o evento. Isso segue o comportamento dos serviços
atuais e impede que a interface observe um estado ainda não salvo.

## 25. Estrutura de arquivos

### 25.1 Novos arquivos de runtime

| Arquivo | Responsabilidade |
|---|---|
| `Assets/Scripts/Core/Persistence/CampaignSaveData.cs` | Dados serializáveis de campanha, portões e ondas |
| `Assets/Scripts/Campaign/CampaignTypes.cs` | `GateId`, tipos de inimigo, estados, avisos e dados públicos de eventos |
| `Assets/Scripts/Campaign/CampaignRules.cs` | Fórmulas, intervalos, pesos, afinidades e limites configuráveis |
| `Assets/Scripts/Campaign/CampaignRandom.cs` | Gerador pseudoaleatório explícito e estável |
| `Assets/Scripts/Campaign/WaveGenerator.cs` | Geração pura de direção, força e composição |
| `Assets/Scripts/Campaign/WaveDirectorService.cs` | Agenda, avisos e transições da onda |
| `Assets/Scripts/Campaign/CampaignService.cs` | Relógio, pausas, portões, checkpoint e orquestração |
| `Assets/Scripts/Campaign/CampaignRuntimeDriver.cs` | Avanço por frame, bootstrap e checkpoints do ciclo de vida |

Namespace proposto para o domínio:

```text
TCCGame.Gameplay.Campaign
```

### 25.2 Arquivos existentes a modificar

| Arquivo | Mudança |
|---|---|
| `Assets/Scripts/Core/Persistence/GameSaveData.cs` | Versão 4 e campo `campaign` |
| `Assets/Scripts/Core/Persistence/GameSaveRepository.cs` | Migração e normalização dos novos dados |
| `Assets/Scripts/Core/GameSession.cs` | Criar e expor `CampaignService` sem alterar os consumidores atuais |

O construtor atual de `GameSession(IGameSaveRepository, IClock)` deve continuar
existindo. Um overload poderá aceitar `CampaignRules` nos testes, enquanto o
construtor existente utiliza as regras padrão.

Não é necessário modificar os `asmdef`, pois todos os novos arquivos ficam na
assembly runtime já existente.

### 25.3 Novos arquivos de teste

| Arquivo | Cobertura principal |
|---|---|
| `Assets/Tests/EditMode/CampaignPersistenceTests.cs` | Default, normalização e migração v3 → v4 |
| `Assets/Tests/EditMode/CampaignServiceTests.cs` | Relógio, pausas, portões e checkpoints |
| `Assets/Tests/EditMode/WaveGenerationTests.cs` | Fórmula, direção, composição e determinismo |
| `Assets/Tests/EditMode/WaveDirectorTests.cs` | Agenda, avisos, chegada e imutabilidade |
| `Assets/Tests/PlayMode/CampaignRuntimeIntegrationTests.cs` | Driver, avanço real, pausa e recarga |

## 26. Contratos técnicos

### 26.1 Persistência

`GameSaveData.CurrentVersion` passa de 3 para 4. A chave de `PlayerPrefs`
continua sendo `tcc.game.save.v2`; ela identifica o local do save, não a versão
interna do objeto.

`GameSaveData` passa a conter:

```text
CampaignSaveData campaign
```

`CampaignSaveData` contém a lista de portões e a agenda. Listas são usadas em
vez de dicionários porque `JsonUtility` não serializa dicionários de forma
nativa.

O exército já existente em `GameSaveData.army` passa a representar a reserva
central. A migração não move nem duplica essas tropas.

### 26.2 Precisão do relógio

- Estado persistente: `double elapsedStrategicSeconds`.
- Entrada do driver: `double`, convertida a partir de `Time.unscaledDeltaTime`.
- Valores negativos, `NaN` e infinito são rejeitados.
- Pausa usa um `HashSet<object>` runtime, sem referências Unity no save.
- A sessão carregada sempre começa sem donos de pausa.

O uso de `unscaledDeltaTime` impede que uma futura animação em câmera lenta
altere silenciosamente o relógio da campanha. A pausa é controlada pelo
serviço, não por `Time.timeScale`.

### 26.3 Aleatoriedade

Não será usado `UnityEngine.Random` nem será assumida a estabilidade de
`System.Random` entre versões de runtime.

`CampaignRandom` implementará um algoritmo pequeno e explícito, como
`xorshift32`. Cada onda deriva seu estado da semente da campanha e de seu número
de sequência. Assim:

- Gerar uma onda não altera sorteios de animação ou outras mecânicas.
- Testes podem usar sementes conhecidas.
- Uma atualização da Unity não muda a sequência silenciosamente.
- A onda completa ainda é persistida após a geração.

### 26.4 Regras de balanceamento

`CampaignRules` é uma classe C# imutável com valores padrão e possibilidade de
injeção em testes. Nesta etapa ela não será um `ScriptableObject`, evitando que
o domínio dependa de um asset carregado por cena.

Ela contém:

- Integridade máxima padrão do portão.
- Preparação da primeira onda.
- Faixas de intervalo por número de onda.
- Fórmula de força base.
- Limites do fator adaptativo.
- Poder base dos soldados.
- Custos dos inimigos.
- Ondas que desbloqueiam Bruto e Xamã.
- Multiplicador de vantagem 1,50.
- Relação entre soldado favorecido e inimigo.

Uma conversão futura para asset de balanceamento poderá acontecer sem alterar
os serviços, desde que continue produzindo um objeto `CampaignRules` válido.

### 26.5 Composição da onda

O gerador usa o `threatPower` como orçamento:

1. Monta a lista de inimigos desbloqueados para a sequência.
2. Sorteia um arquétipo entre os que ainda cabem no orçamento.
3. Deduz seu custo.
4. Repete até consumir todo o orçamento.

Como Saqueador custa 1, todo orçamento válido pode ser preenchido exatamente.
Pesos iniciais depois de cada desbloqueio:

```text
Saqueador: 60%
Bruto:     25%
Xamã:      15%
```

Antes do desbloqueio, os pesos indisponíveis são redistribuídos entre os tipos
válidos. O teste verifica custo total, disponibilidade por onda e
determinismo, sem exigir uma composição específica para toda semente.

### 26.6 Checkpoints

O serviço mantém um indicador runtime de estado sujo. Um commit ocorre:

- Imediatamente ao mudar estágio de aviso.
- Imediatamente quando a onda chega.
- Imediatamente ao concluir uma onda por comando futuro.
- Ao pausar ou encerrar a aplicação.
- A cada 10 segundos ativos se houver alteração ainda não persistida.

Não há gravação por frame.

## 27. Plano de tarefas

| ID | Tarefa | Dependências | Saída verificável |
|---|---|---|---|
| P1-01 | Congelar a regressão atual | Nenhuma | Resultado-base dos testes registrado |
| P1-02 | Criar tipos e modelos persistentes | P1-01 | Objetos clonam corretamente via `JsonUtility` |
| P1-03 | Implementar save v4 e normalização | P1-02 | Save v3 migra com quatro portões e sem perda |
| P1-04 | Implementar `CampaignRules` | P1-02 | Fórmulas e limites testados isoladamente |
| P1-05 | Implementar `CampaignRandom` | P1-04 | Sementes conhecidas repetem a sequência |
| P1-06 | Implementar `WaveGenerator` | P1-03, P1-04, P1-05 | Onda completa, válida e determinística |
| P1-07 | Implementar relógio e pausas | P1-03 | Tempo e múltiplos donos de pausa testados |
| P1-08 | Implementar `WaveDirectorService` | P1-06, P1-07 | Avisos e chegada funcionam com tempo simulado |
| P1-09 | Integrar em `GameSession` | P1-08 | Consumidores antigos continuam compilando |
| P1-10 | Criar driver e bootstrap runtime | P1-09 | Campanha avança em PlayMode sem objeto manual |
| P1-11 | Validar persistência runtime | P1-10 | Pausar/recarregar preserva relógio e onda |
| P1-12 | Executar regressão e revisar documentação | Todas | EditMode e PlayMode aprovados |

## 28. Fases e marcos

### Marco A — Save v4 íntegro

Inclui P1-01 a P1-03.

Critério de saída:

- Save novo possui quatro portões.
- Save v3 mantém todo o estado anterior.
- Entradas duplicadas ou negativas são normalizadas.
- Testes existentes de persistência continuam passando.

### Marco B — Primeira onda reproduzível

Inclui P1-04 a P1-06.

Critério de saída:

- A mesma semente produz a mesma onda.
- A primeira onda tem chegada em 180 segundos.
- Composição respeita desbloqueios e orçamento.
- A força usa crescimento temporal e adaptação limitada.

### Marco C — Campanha funcional em memória

Inclui P1-07 a P1-09.

Critério de saída:

- Relógio, pausa e avisos funcionam sem uma cena.
- Uma onda chega e aguarda resolução.
- A sessão expõe a campanha sem quebrar economia ou recrutamento.

### Marco D — Campanha funcional em PlayMode

Inclui P1-10 a P1-12.

Critério de saída:

- O driver existe uma única vez entre mudanças de cena.
- Não existem gravações a cada frame.
- Pausa e fechamento salvam o checkpoint.
- Toda a suíte de testes passa.

## 29. Ordem de desenvolvimento orientada por testes

Para cada tarefa, o teste deve ser escrito antes da implementação correspondente:

1. Default e clone de `CampaignSaveData`.
2. Migração preservando ouro, construções, impostos, recrutamento, tropas e
   inventário.
3. Normalização de portões duplicados, ausentes e inválidos.
4. Cálculos puros de força e progresso.
5. Sequência fixa do gerador aleatório.
6. Geração e imutabilidade da onda.
7. Avanço e rejeição de deltas inválidos.
8. Um, dois e múltiplos donos de pausa.
9. Avisos ao cruzar 90, 30, 10 e 0 segundos.
10. Salto de tempo que atravessa mais de um estágio.
11. Persistência do estágio para impedir repetição após reload.
12. Integração do driver em PlayMode.

Após cada marco, executar todos os testes EditMode. PlayMode é obrigatório nos
Marcos C e D.

## 30. Casos extremos obrigatórios

- `deltaTime` zero não altera o estado.
- `deltaTime` negativo, infinito ou `NaN` é rejeitado.
- Liberar um dono de pausa desconhecido não retoma indevidamente a campanha.
- O mesmo dono não pode adquirir pausa duas vezes.
- Carregar exatamente em 90, 30, 10 ou 0 segundos produz o estágio correto.
- Um salto de tempo grande não deixa a onda presa em estágio anterior.
- Uma onda aguardando resolução não é substituída por outra.
- Uma onda resolvida agenda a próxima usando o tempo atual como referência.
- O terceiro ataque consecutivo nunca escolhe o mesmo portão.
- Uma composição nunca contém inimigo ainda bloqueado.
- Overflow no cálculo de ondas muito altas é limitado a um máximo seguro.
- Save sem campanha, sem lista de portões ou com listas nulas é recuperado.
- Save com valores desconhecidos de enum não derruba o carregamento.

## 31. Riscos e mitigação

| Risco | Mitigação |
|---|---|
| Novos commits alterarem contagens esperadas nos testes atuais | Construtores não salvam; inicialização é persistida pelo repositório ou próximo checkpoint |
| Duplicação do driver ao trocar cenas | Bootstrap único com `DontDestroyOnLoad` e guarda de instância |
| Diferença aleatória após atualização da Unity | PRNG explícito e onda completa persistida |
| Perda de progresso por salvar pouco | Checkpoint semântico e periódico de 10 segundos |
| Escrita excessiva em `PlayerPrefs` | Estado sujo e proibição de commit por frame |
| Confusão entre relógio UTC e estratégico | Tipos e nomes distintos; ondas usam apenas tempo estratégico |
| Quebra de saves existentes | Migração aditiva, mesma chave e testes com fixtures v2/v3 |
| `JsonUtility` não suportar coleções complexas | Somente classes serializáveis e listas simples |
| Escalonamento ficar impossível de balancear | Todas as constantes concentradas em `CampaignRules` |

## 32. Definição de pronto da Parte 1

A Parte 1 estará concluída somente quando:

1. Todos os critérios da seção 20 estiverem automatizados.
2. Os Marcos A, B, C e D estiverem aprovados.
3. Não houver erro de compilação, referência serializada ausente ou exceção em
   PlayMode.
4. EditMode e PlayMode passarem integralmente.
5. Um save v3 real puder ser carregado como v4 sem perda de estado.
6. Uma simulação acelerada demonstrar a primeira onda em 180 segundos e todos
   os estágios de aviso na ordem correta.
7. A gravação periódica acontecer no máximo uma vez a cada 10 segundos ativos.
8. Nenhuma regra de campanha estiver implementada em componentes de UI ou
   cena.
9. O documento for atualizado com qualquer decisão alterada durante a
   implementação.

## 33. Registro da implementação

**Data:** 27 de agosto de 2026  
**Save:** versão 4, mantido em `PlayerPrefs` na chave
`tcc.game.save.v2`

A Parte 1 foi implementada com os seguintes resultados:

- `CampaignSaveData`, quatro portões e ondas passaram a integrar o save v4.
- Saves v3 são normalizados de forma aditiva; ouro, construções, impostos,
  recrutamento, exército e inventário são preservados.
- A primeira onda é criada com 180 segundos de preparação.
- O PRNG `xorshift32` deriva cada onda da semente persistida e da sequência.
- Força, intervalo, portão e composição ficam imutáveis depois do
  agendamento.
- Os avisos de 90, 30 e 10 segundos são persistidos e emitidos uma única vez.
- A onda passa para `AwaitingResolution` ao chegar e somente é substituída
  depois de uma resolução explícita.
- O relógio estratégico usa `unscaledDeltaTime`, não avança fora do jogo e
  realiza checkpoints periódicos e semânticos sem gravar a cada frame.
- O sistema de interação modal do jogador solicita e libera pausas da
  campanha com suporte a múltiplos donos.
- As afinidades aprovadas estão centralizadas em `CampaignRules`, incluindo o
  multiplicador normal 1,00 e o bônus 1,50.

Validação automatizada executada primeiro em uma cópia isolada e repetida no
projeto real depois que o Editor foi encerrado:

- EditMode: 62 testes executados, 62 aprovados.
- PlayMode: 13 testes executados, 13 aprovados.
- Compilação adicional dos scripts e testes C#: 0 erros.

Resultados detalhados:

- `Logs/CampaignEditModeResults.xml`
- `Logs/CampaignPlayModeResults.xml`

Os itens da seção 21 continuam fora do escopo. A próxima parte recomendada é
a navegação Centro–Portão com o contexto persistente de `GateId`, seguida da
reserva e movimentação de tropas.

---

# Especificação da Parte 2 — Navegação Centro–Portão

## 34. Diagnóstico do projeto atual

A Parte 2 parte das seguintes condições verificadas no projeto:

- `Centro.unity` e `Portão.unity` estão no Build Settings e possuem exatamente
  uma instância do prefab `Player`.
- A cena `Portão` já contém o recrutador e pode ser reutilizada para os quatro
  portões.
- Não existe código de troca de cena, sentinela, ponto de entrada ou contexto
  que informe qual `GateId` está sendo visitado.
- `PlayerInteractor` já seleciona o NPC mais próximo e o sistema modal já
  pausa o relógio estratégico.
- `GameSession.Campaign` já fornece os quatro portões e a onda atual.
- `Taverna` ainda é a primeira cena do build; `Centro` precisa se tornar o
  ponto inicial do fluxo da cidade.
- `mapa_geral.unity` não está no Build Settings e não será incorporada a esta
  rota. Sua reorganização fica para uma etapa posterior.
- A regressão inicial desta parte é de 62 testes EditMode e 13 PlayMode
  aprovados.

## 35. Objetivo da Parte 2

Entregar o primeiro percurso espacial completo da campanha:

```text
Centro
  ├── Sentinela Norte  ──→ Portão / contexto Norte
  ├── Sentinela Leste  ──→ Portão / contexto Leste
  ├── Sentinela Sul    ──→ Portão / contexto Sul
  └── Sentinela Oeste  ──→ Portão / contexto Oeste
                                  │
                                  └── Sentinela de retorno ──→ Centro
```

O jogador deve compreender com qual NPC conversar, qual portão está
selecionando, se existe uma onda anunciada para ele e onde retornará ao Centro.
O mesmo arquivo de cena `Portão.unity` deve representar qualquer direção sem
duplicação de lógica ou de estado.

## 36. Decisões adotadas para o plano

1. `Centro` será o núcleo da cidade e a primeira cena do build.
2. Haverá uma única cena `Portão`, parametrizada por `GateId`.
3. Quatro sentinelas serão posicionadas nas saídas cardinais do Centro.
4. A viagem exige conversa e confirmação; encostar no NPC não troca a cena.
5. O diálogo mostra direção, integridade e ameaça conhecida do destino.
6. A cena de portão sempre exibe sua identidade cardinal de forma textual.
7. Um sentinela no portão permite retornar ao Centro.
8. O jogador retorna próximo à saída correspondente, preservando coerência
   espacial.
9. Conversas e carregamentos pausam o relógio estratégico.
10. A viagem do jogador não consome segundos estratégicos nesta versão.
11. O jogador continua sendo um objeto local de cada cena; ele não recebe
    `DontDestroyOnLoad`.
12. A localização passa a integrar o save v5 em `PlayerPrefs`, na mesma chave
    `tcc.game.save.v2`.
13. Fechar o jogo em um portão e reabrir deve restaurar o mesmo portão.
14. Arte definitiva dos sentinelas não bloqueia o slice; nome, fala, posição e
    indicador cardinal não podem ser provisórios ou ambíguos.

## 37. Fluxo do jogador

### 37.1 Centro para um portão

1. O jogador se aproxima de um sentinela.
2. Surge o prompt `[E] Conversar — Sentinela do Norte`.
3. Ao pressionar `E`, abre-se uma conversa modal.
4. O modal pausa a campanha e apresenta os dados do portão.
5. `Viajar` confirma; `Ficar` ou `Esc` cancela.
6. Na confirmação, a localização é salva e a tela escurece.
7. `Portão.unity` é carregada com o `GateId` escolhido.
8. O jogador aparece no ponto de entrada interno do portão.
9. A tela clareia e o relógio volta a avançar.

### 37.2 Portão para o Centro

1. O jogador conversa com o sentinela de retorno.
2. O modal informa `Voltar ao Centro pelo caminho Norte?`.
3. A confirmação salva o Centro como localização atual.
4. `Centro.unity` é carregada.
5. O jogador aparece próximo à saída Norte, e não no centro genérico do mapa.

### 37.3 Troca entre portões

A Parte 2 exige passar pelo Centro:

```text
Portão Norte → Centro → Sentinela Leste → Portão Leste
```

Viagem direta entre portões será considerada depois que existir movimentação
de tropas e uma representação mais completa da malha urbana.

## 38. Contrato de UI/UX

### 38.1 Prompt de interação

O contrato atual de `IInteractable` não contém texto. Será criado um contrato
opcional, sem quebrar os NPCs existentes:

```text
IInteractionPromptProvider
    InteractionPrompt
```

`PlayerInteractor` passa a publicar a mudança do interagível atual. Um único
`InteractionPromptPresenter` mostra o texto do candidato mais próximo. Quando
o candidato não fornecer texto próprio, o fallback será `[E] Interagir`.

O prompt:

- Só aparece dentro do alcance.
- Some durante um modal ou carregamento.
- Não intercepta cliques.
- Usa texto e ícone de tecla; cor não é o único indicador.
- Atualiza quando dois NPCs próximos trocam de prioridade.

### 38.2 Conversa de viagem

Exemplo com onda anunciada:

```text
SENTINELA DO NORTE

Portão Norte
Integridade: 100/100
Ataque previsto: 01:42
Ameaça estimada: 18

[Viajar]  [Ficar]
```

Exemplo sem ataque para o destino:

```text
SENTINELA DO OESTE

Portão Oeste
Integridade: 83/100
Nenhum ataque anunciado para este portão.

[Viajar]  [Ficar]
```

Se a onda já estiver em `AwaitingResolution`, o texto será `Ataque aguardando
defesa`, sem apresentar uma contagem negativa.

O modal deve aceitar clique, `Submit/Enter` e `Cancel/Esc`, usando as ações de
UI que já existem no Input System.

### 38.3 Identidade da cena de portão

A cena compartilhada deve apresentar permanentemente:

- `PORTÃO NORTE`, `PORTÃO LESTE`, `PORTÃO SUL` ou `PORTÃO OESTE`.
- Integridade atual e máxima.
- Estado da onda destinada a esse portão, quando houver.
- Nome do sentinela de retorno coerente com a direção.

Os nomes portugueses ficam centralizados em um formatador:

```text
North → Norte
East  → Leste
South → Sul
West  → Oeste
```

## 39. Persistência e migração v5

### 39.1 Modelo

O save recebe um bloco separado da campanha:

```text
NavigationSaveData
    initialized
    currentArea            // Center ou Gate
    activeGateId           // válido quando currentArea == Gate
    hasCenterEntryGate
    centerEntryGate        // saída usada no último retorno
```

`GameSaveData.CurrentVersion` passa de 4 para 5. A chave de `PlayerPrefs` não
muda.

### 39.2 Migração

Saves v4 ou anteriores recebem:

```text
currentArea = Center
hasCenterEntryGate = false
activeGateId = North       // valor neutro, ainda não considerado ativo
```

Nenhum dado da Parte 1 é recalculado. Ouro, construções, recrutamento, tropas,
portões, relógio e onda atual permanecem inalterados.

### 39.3 Momento de gravação

A localização é gravada somente depois que o jogador confirma a viagem e
antes de ativar a nova cena. Isso produz um comportamento recuperável: se o
jogo for interrompido durante o carregamento, a próxima inicialização tenta
abrir o destino confirmado.

Não serão persistidos nesta parte:

- Coordenadas exatas do jogador.
- Direção do sprite.
- Velocidade do `Rigidbody2D`.
- Modal aberto.
- Progresso parcial do fade.

## 40. Arquitetura proposta

```text
CityTravelNpc
      │ abre conversa / confirma destino
      ▼
GameSession.Navigation
      │ salva NavigationSaveData v5
      ▼
SceneTransitionController
      ├── bloqueia nova transição
      ├── pausa campanha
      ├── fade out
      ├── LoadSceneAsync
      ├── posiciona jogador
      └── fade in / libera pausa
```

### 40.1 `NavigationService`

Serviço de domínio sem dependência de cena. Responsabilidades:

- Expor área e portão atuais.
- Validar `GateId`.
- Registrar entrada em portão.
- Registrar retorno ao Centro e a saída de origem.
- Persistir somente quando o estado realmente mudar.
- Produzir um snapshot imutável para apresentadores e testes.

### 40.2 `SceneTransitionController`

Componente único e persistente criado antes da primeira cena. Ele não contém
regras de portão; apenas executa a transição solicitada.

Ordem obrigatória:

1. Rejeitar outra transição em andamento.
2. Adquirir pausa própria na campanha.
3. Bloquear o jogador e o raycast da tela.
4. Persistir o destino confirmado.
5. Fazer fade out com tempo não escalado.
6. Salvar checkpoint da campanha.
7. Carregar a cena por nome constante.
8. Aguardar o `PlayerSpawnController` posicionar o jogador.
9. Fazer fade in.
10. Liberar bloqueio e pausa, mesmo se o objeto de origem tiver sido
    destruído.

### 40.3 Inicialização e restauração

`Centro` passa a ser a cena de índice zero. Durante o primeiro carregamento:

- Se o save indicar `Center`, o Centro permanece ativo.
- Se indicar `Gate`, o overlay começa preto e o controlador carrega `Portão`
  antes de liberar o jogador.
- Contexto inválido é normalizado para o Centro.
- Uma guarda de inicialização impede loop de redirecionamento.

### 40.4 Posicionamento do jogador

Cada cena recebe âncoras explícitas:

```text
Centro
    CenterDefaultSpawn
    CenterFromNorthSpawn
    CenterFromEastSpawn
    CenterFromSouthSpawn
    CenterFromWestSpawn

Portão
    GateEntranceSpawn
```

`PlayerSpawnController` encontra o único jogador da cena, escolhe a âncora a
partir do save, define sua posição e zera a velocidade. O player não atravessa
cenas e, portanto, não pode ser duplicado pelo bootstrap.

## 41. Estrutura de arquivos planejada

### 41.1 Novos arquivos de runtime

| Arquivo | Responsabilidade |
|---|---|
| `Assets/Scripts/Core/Persistence/NavigationSaveData.cs` | Estado serializável da localização |
| `Assets/Scripts/Navigation/NavigationTypes.cs` | Área, destino e snapshots |
| `Assets/Scripts/Navigation/NavigationService.cs` | Regras e persistência da navegação |
| `Assets/Scripts/Navigation/SceneCatalog.cs` | Nomes de cenas em um único local |
| `Assets/Scripts/Navigation/SceneTransitionController.cs` | Fade, pausa e carregamento assíncrono |
| `Assets/Scripts/Navigation/PlayerSpawnController.cs` | Seleção de âncora e posicionamento |
| `Assets/Scripts/Navigation/CityTravelNpc.cs` | Interação de ida e retorno |
| `Assets/Scripts/Navigation/TravelConversationPanel.cs` | Modal de confirmação e dados do destino |
| `Assets/Scripts/Navigation/GateContextPresenter.cs` | Identidade e estado do portão visitado |
| `Assets/Scripts/Core/Input/InteractionPromptPresenter.cs` | Prompt do interagível atual |

### 41.2 Arquivos existentes a modificar

| Arquivo | Mudança |
|---|---|
| `GameSaveData.cs` | Versão 5 e campo `navigation` |
| `GameSaveRepository.cs` | Normalização v5 |
| `GameSession.cs` | Expor `NavigationService` |
| `IInteractable.cs` | Adicionar contrato opcional de prompt |
| `PlayerInteractor.cs` | Expor candidato atual e evento de mudança |
| `Centro.unity` | Quatro sentinelas, cinco spawns, modal e prompt |
| `Portão.unity` | Contexto, sentinela de retorno, spawn, modal e prompt |
| `EditorBuildSettings.asset` | Colocar `Centro` primeiro e preservar `Portão` |

### 41.3 Prefabs planejados

| Prefab | Uso |
|---|---|
| `Assets/Prefabs/NPCs/CitySentinel.prefab` | Base visual e collider dos cinco sentinelas |
| `Assets/Prefabs/UI/TravelConversationPanel.prefab` | Conversa reutilizada nas duas cenas |
| `Assets/Prefabs/UI/InteractionPrompt.prefab` | Prompt global de interação |
| `Assets/Prefabs/UI/SceneTransitionOverlay.prefab` | Fade persistente e bloqueio de raycast |

## 42. Integração com ondas

A Parte 2 não cria novas ondas nem altera sua força. Ela apenas lê:

```text
CampaignService.GetGate(gateId)
CampaignService.Waves.CurrentWave
CampaignService.ElapsedStrategicSeconds
```

O diálogo calcula uma apresentação efêmera:

```text
tempoRestante = max(0, chegada - tempoAtual)
```

Regras:

- Se a onda atual for de outro portão, o destino mostra `Nenhum ataque
  anunciado`.
- Se for do destino e estiver agendada, mostra tempo e ameaça.
- Se estiver aguardando resolução, mostra estado urgente sem tempo negativo.
- Viajar não recalcula direção, força ou composição.
- A cena compartilhada consulta sempre o `GateId` persistido, nunca infere a
  direção por nome de objeto ou posição.

## 43. Critérios de aceitação

### 43.1 Centro

- Existem quatro sentinelas únicos, um para cada `GateId`.
- Eles ocupam as quatro saídas cardinais e possuem prompt legível.
- O diálogo identifica corretamente o destino e sua ameaça.
- Cancelar mantém cena, save e posição.
- Confirmar carrega a única cena `Portão`.

### 43.2 Portão compartilhado

- O Norte e o Leste, visitados em sequência, usam o mesmo arquivo de cena.
- Título, integridade, onda e retorno mudam conforme o contexto.
- Não há leitura acidental do estado de outro portão.
- Existe um sentinela funcional para voltar ao Centro.

### 43.3 Spawn e transição

- Cada cena possui exatamente um jogador.
- O jogador surge no ponto correto e com velocidade zero.
- Retornar pelo Norte posiciona o jogador na saída Norte do Centro; a mesma
  regra vale para Leste, Sul e Oeste.
- Cliques ou confirmações repetidas não iniciam duas cargas.
- O overlay impede interação durante o carregamento.
- O tempo estratégico permanece parado no modal e no carregamento, retomando
  depois do fade.

### 43.4 Persistência

- Saves v4 migram para v5 sem perda.
- Reabrir no Centro mantém o Centro.
- Reabrir no Portão Sul restaura `Portão` com contexto Sul.
- Contexto corrompido volta com segurança ao Centro.
- A chave de `PlayerPrefs` permanece `tcc.game.save.v2`.

### 43.5 Regressão

- Economia, prédios, impostos, recrutamento e inventário continuam
  funcionando depois de qualquer viagem.
- O relógio e a onda mantêm o mesmo estado entre cenas.
- Todos os testes existentes continuam passando.

## 44. Testes planejados

### 44.1 EditMode

- Default e clone de `NavigationSaveData`.
- Migração v4 → v5 preservando toda a campanha.
- Normalização de área e `GateId` inválidos.
- Entrada e retorno persistem uma única vez.
- Tradução cardinal e formatação do diálogo.
- Diálogo com onda do destino, onda de outro portão e onda vencida.
- Build Settings com `Centro` primeiro e `Portão` habilitado.
- Cena Centro com quatro destinos únicos e cinco spawns.
- Cena Portão com contexto, retorno e spawn.
- Referências dos prefabs e painéis sem campos nulos.

### 44.2 PlayMode

- Centro → Norte → Centro com spawns corretos.
- Centro → Leste comprova reutilização da mesma cena com outro contexto.
- Cancelar conversa não troca a cena.
- Modal pausa e fechamento retoma o relógio.
- Carregamento mantém o relógio parado.
- Confirmação duplicada gera somente uma transição.
- Ouro e recrutamento permanecem iguais ao atravessar cenas.
- Onda, estágio de aviso e tempo de chegada permanecem iguais.
- Reset de sessão restaura o portão salvo.
- Fluxo inválido recupera o Centro sem exceção.

## 45. Plano de tarefas

| ID | Tarefa | Dependências | Saída verificável |
|---|---|---|---|
| P2-01 | Congelar regressão e wiring atual | Parte 1 | Baseline 62/13 registrado |
| P2-02 | Criar tipos e save de navegação v5 | P2-01 | Migração e normalização aprovadas |
| P2-03 | Implementar `NavigationService` | P2-02 | Comandos puros testados |
| P2-04 | Integrar navegação em `GameSession` | P2-03 | Sessão expõe localização |
| P2-05 | Implementar prompt de interação | P2-01 | Candidato atual visível |
| P2-06 | Criar conversa de viagem e formatador | P2-03, P2-05 | Modal mostra portão e onda |
| P2-07 | Implementar transição, fade e pausa | P2-04 | Carga única e assíncrona |
| P2-08 | Implementar spawns e restauração | P2-07 | Entrada e retorno coerentes |
| P2-09 | Criar prefab e quatro sentinelas no Centro | P2-06, P2-08 | Quatro destinos funcionais |
| P2-10 | Configurar contexto e retorno no Portão | P2-06, P2-08 | Cena compartilhada funcional |
| P2-11 | Corrigir Build Settings e fluxo inicial | P2-07, P2-08 | Centro inicia; save restaura portão |
| P2-12 | Ampliar testes de cena EditMode | P2-09, P2-10 | Wiring protegido |
| P2-13 | Implementar testes de fluxo PlayMode | P2-11, P2-12 | Percurso completo automatizado |
| P2-14 | Executar regressão e atualizar documento | Todas | EditMode e PlayMode aprovados |

## 46. Marcos

### Marco A — Localização persistente

Inclui P2-01 a P2-04.

Critério de saída:

- Save v5 íntegro.
- Localização e portão atual consultáveis pela sessão.
- Migração não altera dados da campanha.

### Marco B — Transição segura

Inclui P2-05 a P2-08.

Critério de saída:

- Prompt, modal, fade, pausa e spawn funcionam em cenas de teste.
- Uma confirmação produz exatamente uma transição.

### Marco C — Cidade navegável

Inclui P2-09 a P2-11.

Critério de saída:

- As quatro rotas saem do Centro.
- A cena compartilhada identifica qualquer portão.
- O retorno respeita a direção de origem.
- Reiniciar restaura a localização salva.

### Marco D — Parte 2 validada

Inclui P2-12 a P2-14.

Critério de saída:

- Fluxos automatizados para ao menos Norte e Leste.
- Wiring dos quatro destinos protegido em EditMode.
- Regressão integral aprovada.

## 47. Casos extremos obrigatórios

- `GateId` inválido no save.
- Área `Gate` sem portão válido.
- Cena `Portão` aberta diretamente no Editor.
- Interação com duas sentinelas no mesmo alcance.
- `E` pressionado repetidamente antes do modal aparecer.
- Botão de confirmação acionado duas vezes.
- Cancelamento por botão e por `Esc`.
- Jogador destruído pela troca de cena enquanto havia modal aberto.
- Aplicação pausada ou encerrada durante fade.
- Onda cruza um estágio enquanto a conversa começa no mesmo frame.
- Onda aguardando resolução sem contagem negativa.
- Save realizado durante um portão e recarregado a partir do índice zero.
- Cena sem player, com dois players ou sem âncora deve falhar com diagnóstico
  claro em desenvolvimento.
- Ausência acidental de `Portão` no Build Settings deve ser detectada por
  teste, não pelo jogador.

## 48. Riscos e mitigação

| Risco | Mitigação |
|---|---|
| Nome acentuado de `Portão` divergir entre código e Build Settings | `SceneCatalog` único e teste do Build Settings |
| Duas cargas simultâneas corromperem o contexto | Guarda `IsTransitioning` e botões desativados após confirmar |
| Player persistente duplicar o player da cena | Manter player local e testar contagem igual a um |
| Pausa do modal ser liberada quando o player é destruído | Controlador de transição possui um dono de pausa independente |
| Fade congelar com `timeScale` ou campanha pausada | Animação usa tempo não escalado |
| Portão exibir estado da direção errada | Contexto vem somente de `NavigationService` persistido |
| Retorno colocar jogador sobre collider da sentinela | Âncoras separadas do alcance imediato e teste de posição |
| Startup produzir flash do Centro antes de restaurar portão | Overlay inicia preto até concluir restauração |
| Novo save v5 perder dados v4 | Migração aditiva e fixture completa de regressão |
| UI de viagem virar um HUD de comando | Modal apenas informa e confirma deslocamento; decisões continuam por NPC |

## 49. Fora do escopo da Parte 2

- Alocação de tropas em guarnições.
- Deslocamento temporizado de soldados.
- Combate, dano, baixas e espólio.
- HUD global dos quatro portões.
- Capitão, engenheiro e batedor funcionais.
- Cenas ou artes diferentes para cada direção.
- Tempo ou custo para a viagem pessoal do jogador.
- Viagem direta de um portão para outro.
- Reorganização de `Taverna` e `mapa_geral`.
- Conversão das construções atuais para interação exclusiva por mestre de
  obras.

## 50. Definição de pronto da Parte 2

A Parte 2 estará concluída quando:

1. O jogador puder ir do Centro a qualquer um dos quatro portões por meio de
   um NPC e retornar.
2. O portão visitado for inequívoco antes, durante e depois da transição.
3. O diálogo usar dados reais de integridade e onda.
4. Pausas modais e de carregamento não aproximarem ataques.
5. O retorno posicionar o jogador na saída cardinal correta.
6. Fechar e reabrir restaurar Centro ou portão corretamente via `PlayerPrefs`.
7. Não existirem players, controladores ou eventos duplicados.
8. Save v4 migrar para v5 sem perda de dados.
9. Todos os testes EditMode e PlayMode, antigos e novos, passarem.
10. Nenhum item da seção 49 tiver sido introduzido implicitamente.

Depois desta parte, a próxima especificação deve tratar reserva, guarnições e
deslocamento de soldados entre portões.

## 51. Implementação da Parte 2 — 27/08/2026

Esta etapa foi aplicada com o fluxo mínimo completo de deslocamento:

- O save passou para v5 mantendo a chave `tcc.game.save.v2`. O novo bloco
  `NavigationSaveData` guarda área atual, portão ativo e a saída de retorno
  do Centro; saves anteriores recebem Centro/Norte de forma segura.
- `GameSession.Navigation` concentra as transições de estado. Entrar em um
  portão e retornar ao Centro persistem imediatamente e não duplicam gravações
  quando a ação se repete.
- `Centro` é a primeira cena do Build Settings e contém quatro sentinelas:
  Norte, Leste, Sul e Oeste. Cada um abre uma conversa antes da viagem.
- A cena compartilhada `Portão` recebe seu contexto do save, exibe direção,
  integridade e estado real da onda, e oferece um batedor para voltar ao
  Centro.
- A transição tem fade, bloqueia novos comandos e pausa o tempo estratégico
  durante conversa e carregamento. O jogador reaparece na âncora cardinal
  correspondente e com velocidade zerada.
- Há um prompt de interação para o NPC atualmente alcançável e confirmação por
  botão, Enter ou Esc.

Validação automatizada executada após a implementação:

- EditMode: 67/67 aprovados.
- PlayMode: 10/10 aprovados.

Os testes agora também protegem a migração para v5, a persistência de ida e
volta, a formatação de contagem de onda, a presença dos quatro destinos, as
âncoras e a ordem das cenas no Build Settings.

### Correções pontuais posteriores

- O prompt `[E]` agora consulta `UiMenuState` e desaparece enquanto qualquer
  painel de construção, recrutamento, venda ou viagem estiver aberto.
- O movimento do jogador é limitado ao viewport da câmera ortográfica ativa,
  considerando o tamanho do seu collider para que ele não atravesse a borda
  visível.
- `PlayerSpawnController` passou a executar após o primeiro carregamento e
  após cada troca de cena. Assim, os markers `CenterFrom<Direção>Spawn` são
  aplicados efetivamente no retorno ao Centro.

---

# Especificação e implementação da Parte 3 — Tropas e guarnições

## 52. Objetivo

Transformar o total global de soldados em uma reserva central utilizável e
permitir que o jogador organize as quatro guarnições conversando com o Capitão
do portão visitado.

O fluxo entregue é:

```text
Contratar tropa → Reserva central → Ordem de marcha → Em trânsito → Guarnição
Guarnição atual → Ordem de marcha → Em trânsito → Reserva ou outro portão
```

## 53. Regras de deslocamento

- O envio é confirmado imediatamente e remove as tropas da origem.
- A duração é sorteada deterministicamente entre 15 e 25 segundos
  estratégicos e persistida na ordem.
- Tropas em trânsito contam para o progresso militar usado ao criar ondas,
  mas não contam como defesa de nenhum portão.
- A chegada ocorre somente quando o relógio estratégico alcança o horário
  persistido; menus e transições pausam esse relógio.
- Não é permitido enviar zero, quantidade negativa, mais tropas do que a
  origem possui ou enviar para a própria origem.
- Uma ordem criada não pode ser cancelada nesta versão.
- Toda criação e toda chegada produzem um checkpoint de save.

## 54. Persistência v6

A chave permanece `tcc.game.save.v2`. O save passa a possuir:

```text
CampaignSaveData
    nextTroopTransferSequence
    troopTransfers[]

TroopTransferSaveData
    sequence
    sourceKind / sourceGateId
    destinationKind / destinationGateId
    unitType
    quantity
    departureStrategicSeconds
    arrivalStrategicSeconds
```

Saves v5 recebem uma lista vazia de deslocamentos. A reserva e as quatro
guarnições existentes são preservadas sem redistribuição automática.

## 55. Capitão e interface

A cena compartilhada `Portão` possui um `CaptainNpc`. O portão administrado é
sempre obtido de `NavigationService`, portanto o mesmo Capitão atende Norte,
Leste, Sul e Oeste sem duplicar cenas.

O painel apresenta:

- Quantidades de guerreiros, arqueiros e magos na reserva.
- Quantidades da guarnição local.
- Ordens relacionadas ao portão e segundos restantes.
- Ações rápidas para receber 1 ou 10 tropas da reserva.
- Ações rápidas para enviar 1 ou 10 tropas ao destino selecionado.
- Destino alternável entre reserva e os outros três portões.
- Mensagem explícita de sucesso, tropa insuficiente ou rota inválida.

## 56. Limites desta entrega

- Ainda não existe combate, baixa, dano ao portão ou espólio.
- Ainda não existe prognóstico de batalha na tela do Capitão.
- Não há custo em ouro, limite de estrada ou fila máxima.
- Não há cancelamento nem retorno de uma ordem em andamento.
- O HUD global dos quatro portões continua reservado para a Parte 4.

## 57. Validação

- EditMode: 71/71 aprovados.
- PlayMode: 13/13 aprovados.
- Testes específicos verificam débito na origem, crédito somente na chegada,
  conservação de tropas entre portões, rejeição de ordens inválidas e inclusão
  das tropas em trânsito no escalonamento militar.
- O wiring da cena verifica a presença de exatamente um Capitão no Portão.

---

# Especificação da Parte 4 — HUD global e avisos de ameaça

## 58. Objetivo

Permitir que o jogador compreenda continuamente:

1. Qual portão será atacado.
2. Quanto tempo resta.
3. Qual é a força e a composição conhecida da onda.
4. Qual é a defesa presente no portão.
5. Se a composição atual parece suficiente.

Esta parte é estritamente informativa. Recrutamento, deslocamento e demais
decisões continuam acontecendo por conversa com NPCs.

## 59. Fluxo de informação

```text
WaveDirectorService + CampaignService + TroopDeploymentService
                         │
                         ▼
               ThreatPresentationService
                  ├─ quatro cartões do HUD
                  ├─ banner de mudança de estágio
                  └─ sinal audiovisual
```

O HUD consulta dados existentes e não grava estado próprio. Portanto, a Parte
4 permanece no save v6 e na chave `tcc.game.save.v2`.

## 60. Layout proposto

Quatro cartões compactos ocupam a faixa superior, sem cobrir ouro, diálogos ou
o prompt de interação:

```text
NORTE  SEGURO       LESTE  01:18       SUL  SEGURO       OESTE  SEGURO
Def. 8              Ameaça 14          Def. 5            Def. 0
                     Def. 11 · ARRISCADO
```

Ao visitar a cena `Portão`, o cartão da direção atual recebe uma borda de
contexto, mas apresenta os mesmos dados do Centro. O HUD permanece visível
durante menus; somente banners transitórios deixam de competir visualmente com
um modal.

## 61. Estados visuais

| Estado derivado | Condição | Cor e apresentação |
|---|---|---|
| Seguro | Nenhuma onda conhecida para o portão | Neutro, sem contador |
| Observado | Onda destinada ao portão, acima de 90 s | Azul, sinais distantes |
| Anunciado | Até 90 s | Amarelo, contador e ameaça |
| Iminente | Até 30 s | Laranja, destaque moderado |
| Crítico | Até 10 s | Vermelho, pulso e contador forte |
| Ataque iniciado | `AwaitingResolution` | Vermelho fixo, sem tempo negativo |

O contador usa `CampaignService.ElapsedStrategicSeconds`, nunca tempo da cena
ou relógio real. Pausar a campanha também congela a apresentação.

## 62. Informação revelada

- Acima de 90 segundos: mostra somente direção e `Sinais distantes`.
- Em `Announced`: revela ameaça total e contagem regressiva.
- Em `Imminent`: revela também Saqueadores, Brutos e Xamãs.
- Em `Critical`: mantém toda a informação e reforça urgência.
- Em `AwaitingResolution`: apresenta `Ataque iniciado` até a futura resolução
  de combate.

Isso preserva o papel do batedor e evita que uma onda sorteada seja totalmente
conhecida antes do estágio de anúncio.

## 63. Força defensiva e bônus

A força básica continua usando:

```text
guerreiros × 1,00 + arqueiros × 1,25 + magos × 2,00
```

Para o prognóstico, o bônus de cada tropa é ponderado pela parcela da ameaça
formada pelo inimigo contra o qual ela possui vantagem:

```text
parcelaSaqueador = ameaçaDosSaqueadores / ameaçaTotal
parcelaXamã      = ameaçaDosXamãs / ameaçaTotal
parcelaBruto     = ameaçaDosBrutos / ameaçaTotal

defesaEfetiva =
    poderGuerreiros × (1 + 0,50 × parcelaSaqueador) +
    poderArqueiros  × (1 + 0,50 × parcelaXamã) +
    poderMagos      × (1 + 0,50 × parcelaBruto)
```

A ameaça por arquétipo usa os mesmos custos de geração: Saqueador 1, Bruto 3
e Xamã 4. O cálculo é explicável, determinístico e reutilizável pelo combate
futuro, mas ainda não aplica baixas.

Classificação inicial:

| Relação `defesaEfetiva / ameaça` | Prognóstico |
|---:|---|
| `>= 1,15` | Favorável |
| `>= 0,85 e < 1,15` | Arriscado |
| `< 0,85` | Desfavorável |

Tropas em trânsito não entram na defesa. Quando chegam, o cartão e o
prognóstico mudam automaticamente.

## 64. Avisos graduais

Os eventos já expostos por `WaveDirectorService` serão consumidos por um
controlador persistente:

- 90 s: banner do batedor e sinal curto.
- 30 s: banner laranja e som de corneta.
- 10 s: banner crítico, pulso visual e som urgente.
- 0 s: mensagem `Ataque iniciado no Portão <direção>`.

Regras:

- Um estágio persistido nunca é anunciado novamente ao recarregar.
- Se um avanço grande cruzar vários estágios, os eventos entram em uma fila
  curta e são apresentados em ordem, sem sobreposição.
- Avisos não capturam clique nem teclado.
- O áudio pode ser desligado independentemente no componente de configuração;
  na ausência de assets, a sinalização visual continua funcional.
- O pulso respeita acessibilidade: não pisca mais rápido que três vezes por
  segundo e pode ser desativado.

## 65. Arquitetura prevista

Novos tipos principais:

```text
ThreatForecastService
    CalculateGateDefense(gate)
    CalculateEffectiveDefense(gate, wave)
    Classify(gate, wave)
    ExplainAdvantages(gate, wave)

ThreatPresentationService
    BuildGateCard(gateId)
    BuildAlert(wave, stage)

GlobalThreatHud
    bootstrap persistente
    quatro GateThreatCardView

WaveAlertController
    assina WaveWarningChanged e WaveBecameDue
    controla fila, banner, áudio e pulso

WaveAlertPresentationConfig
    cores, durações, áudio opcional e redução de movimento
```

`GateContextPresenter` deixa de duplicar regras textuais e passa a consumir o
mesmo `ThreatPresentationService` usado pelo HUD.

## 66. Critérios de aceitação

- Os quatro portões aparecem simultaneamente em todas as cenas de jogo.
- Exatamente um cartão identifica a onda atual e sua direção.
- Contagem regressiva nunca fica negativa.
- A chegada de tropas atualiza defesa e prognóstico sem trocar de cena.
- O bônus de cada tipo de soldado altera o prognóstico somente conforme a
  composição inimiga correspondente.
- Reabrir o jogo reconstrói corretamente todos os cartões a partir do save.
- Estágios já persistidos não repetem banners ou sons.
- HUD e banners não bloqueiam movimento, interação ou botões de menus.
- A correção de fechamento de popups permanece funcionando.

## 67. Testes planejados

### EditMode

- Força básica de cada composição de guarnição.
- Bônus Guerreiro→Saqueador, Arqueiro→Xamã e Mago→Bruto.
- Composição mista e ameaça vazia.
- Limites exatos de prognóstico 0,85 e 1,15.
- Estado Seguro, Observado, Anunciado, Iminente, Crítico e Ataque iniciado.
- Formatação de contador em zero e acima de uma hora.
- Tropas em trânsito excluídas e tropas recém-chegadas incluídas.
- Configuração de cores e tempos válida.

### PlayMode

- Singleton do HUD sobrevive Centro → Portão sem duplicar cartões.
- Contador acompanha somente o tempo estratégico.
- Modal congela contador e fechamento retoma.
- Cada estágio gera um aviso uma única vez.
- Salto entre estágios respeita a ordem da fila.
- Chegada de tropa atualiza o cartão do portão correto.
- HUD não intercepta raycasts.

## 68. Plano de tarefas

| ID | Tarefa | Dependências | Saída verificável |
|---|---|---|---|
| P4-01 | Criar snapshots e prognóstico puro | Parte 3 | Fórmulas testadas |
| P4-02 | Criar modelo de apresentação dos cartões | P4-01 | Estados e textos testados |
| P4-03 | Implementar quatro cartões persistentes | P4-02 | HUD em todas as cenas |
| P4-04 | Integrar chegada de tropas e navegação | P4-03 | Atualização ao vivo correta |
| P4-05 | Implementar fila de banners | P4-02 | Estágios não se sobrepõem |
| P4-06 | Adicionar sinais visuais e áudio opcional | P4-05 | Intensidade por estágio |
| P4-07 | Unificar contexto local do Portão | P4-02 | Sem lógica textual duplicada |
| P4-08 | Cobrir PlayMode e regressão | Todas | Suíte integral aprovada |
| P4-09 | Atualizar documentação e balanceamento | P4-08 | Parte 4 registrada |

## 69. Fora do escopo da Parte 4

- Resolver a batalha.
- Causar baixas ou dano ao portão.
- Entregar espólio.
- Permitir comandos diretamente pelos cartões.
- Mostrar ondas posteriores à onda ativa.
- Criar opções completas de áudio no menu principal.
- Encerrar a campanha após a décima onda.

## 70. Definição de pronto

A Parte 4 estará concluída quando direção, tempo, ameaça, defesa e prognóstico
forem legíveis sem conversar com um NPC; os avisos de 90, 30, 10 e 0 segundos
forem únicos e graduais; o HUD refletir deslocamentos e mudanças de cena; e
todos os testes antigos e novos passarem sem regressão dos popups.

## 71. Implementação da Parte 4 — 02/09/2026

A leitura global de ameaças foi implementada com quatro cartões persistentes,
um para cada portão. Os cartões mostram estado, contagem regressiva, ameaça,
defesa disponível e, quando a onda se aproxima, composição e prognóstico. O
portão em que o jogador se encontra recebe destaque sem transformar o HUD em
uma nova interface de comando.

O prognóstico considera a força base das tropas e os bônus de Guerreiro contra
Saqueador, Arqueiro contra Xamã e Mago contra Bruto. Tropas em trânsito não
entram no cálculo até chegarem ao destino. A apresentação progride pelos
estados Seguro, Observado, Anunciado, Iminente, Crítico e Ataque iniciado.

Avisos visuais e sonoros são enfileirados nos marcos de 90, 30, 10 e 0 segundos.
Menus modais ocultam e pausam a exibição do banner; os cartões permanecem
somente informativos e não interceptam interações. O contexto local do portão
passou a usar o mesmo modelo de apresentação do HUD global.

A entrega mantém o formato de save na versão 6, pois todos os novos dados são
derivados do estado já persistido. Validação final: 79 de 79 testes EditMode e
13 de 13 testes PlayMode aprovados.

---

# Parte 5 — Resolução das batalhas, baixas e espólio

## 72. Objetivo

Fechar o primeiro ciclo jogável completo da defesa: uma onda chega, a guarnição
do portão atacado enfrenta a composição inimiga, soldados podem morrer, o
portão pode sofrer dano e uma vitória concede pouco ouro. O resultado precisa
ser imediato no HUD e consultável em detalhes ao conversar com o Capitão.

Esta parte também encerra corretamente o vertical slice: perder um portão
causa derrota e repelir a décima onda causa vitória. Em ambos os casos o tempo
estratégico é interrompido e nenhuma nova onda é agendada.

## 73. Decisões de experiência

1. A batalha é resolvida automaticamente quando o contador chega a zero.
2. O jogador não precisa estar no portão atacado para a guarnição lutar.
3. A resolução não abre um modal à força nem interrompe uma conversa existente.
4. O HUD global exibe um resumo curto do resultado.
5. O cartão do portão mantém `Relatório disponível` até o jogador falar com o
   Capitão daquele portão e confirmar a leitura.
6. O painel do Capitão apresenta forças iniciais, baixas, dano ao portão,
   sobreviventes, espólio e a explicação do bônus de composição aplicado.
7. Ouro só é concedido quando a onda é repelida. Uma brecha não gera espólio.
8. Depois de vitória ou derrota, os cartões mostram o estado final da campanha
   em vez de uma nova contagem regressiva.

A resolução automática evita que uma onda fique indefinidamente congelada
enquanto o jogador atravessa a cidade. A consulta e a confirmação do relatório
continuam sendo uma interação no mapa feita por NPC.

## 74. Ordem de processamento

Em cada atualização do tempo estratégico, a ordem passa a ser:

```text
avançar relógio estratégico
    → concluir deslocamentos cuja chegada <= horário atual
    → atualizar estágios da onda
    → tornar a onda vencida AwaitingResolution
    → resolver exatamente uma vez
    → persistir consequências e próxima onda em uma única transação
    → publicar os eventos de apresentação
```

`CampaignRuntimeDriver` deve processar chegadas antes da resolução da onda. Uma
tropa com chegada exatamente no instante do ataque participa; uma tropa com
chegada posterior não participa. Menus modais continuam pausando o relógio e,
portanto, não disparam uma batalha enquanto estão abertos.

Ao carregar um save que contenha uma onda em `AwaitingResolution`, a resolução
é retomada uma única vez. O relatório persistido impede aplicação duplicada de
baixas, dano ou ouro após recarregar o jogo.

## 75. Modelo determinístico de combate

O cálculo deve ficar em uma classe pura `BattleCalculator`, sem acessar Unity,
PlayerPrefs ou serviços globais. Ela recebe `GateStateSnapshot`, `WaveSnapshot`
e `CampaignRules`, e devolve um `BattleResolution` imutável.

### 75.1 Força efetiva

A força `D` usa exatamente o cálculo de `ThreatForecastService`, garantindo
que previsão e resultado não discordem:

- Guerreiro: força 1,0 e bônus de 1,5 contra Saqueador.
- Arqueiro: força 1,25 e bônus de 1,5 contra Xamã.
- Mago: força 2,0 e bônus de 1,5 contra Bruto.
- `T`: ameaça registrada na onda, com mínimo operacional de 1.
- `R = D / T`: proporção entre defesa efetiva e ameaça.

### 75.2 Baixas

```text
pressão = T / max(D, 1)
taxaDeBaixas = clamp(0,03 + max(0, pressão - 0,75) × 0,35; 0,03; 0,65)
totalDeBaixas = round(totalDeSoldados × taxaDeBaixas)
```

Se houver soldados, uma batalha causa no mínimo uma baixa. O total nunca pode
ultrapassar a guarnição. As baixas são distribuídas entre Guerreiro, Arqueiro e
Mago proporcionalmente às quantidades iniciais, usando o método dos maiores
restos e desempate fixo pela ordem do enum. Assim o resultado é reproduzível e
o total distribuído sempre coincide com `totalDeBaixas`.

O bônus de tipo reduz a pressão total e, consequentemente, as baixas. Nesta
versão não existe sorte, crítico, ferimento, experiência ou prioridade de alvo.

### 75.3 Dano ao portão

```text
se R >= 1: dano = 0
se R < 1:  dano = ceil((1 - R) × (10 + 0,8 × T))
```

O dano é limitado entre 1 e a vida atual quando a defesa é insuficiente. A vida
nunca fica negativa. Se chegar a zero, o resultado é `Breached`; caso contrário
a onda é `Repelled`, inclusive quando a muralha absorve a pressão que restou
após as baixas.

### 75.4 Espólio

```text
espólio = max(1, floor(T × 0,15)) quando Repelled
espólio = 0 quando Breached
```

O valor é deliberadamente menor que o preço de reposição das baixas na maioria
dos confrontos: Guerreiro custa 3, Arqueiro 5 e Mago 8. Isso sustenta a visão de
que a renda urbana deixa de cobrir a defesa no médio prazo e prepara a futura
necessidade de novas fontes de renda.

## 76. Resultados e fim da campanha

Novos estados de domínio:

```text
BattleOutcome
    None
    Repelled
    Breached

CampaignOutcome
    Ongoing
    Victory
    Defeat
```

- `Breached` define `CampaignOutcome.Defeat` imediatamente.
- `Repelled` na onda 10 define `CampaignOutcome.Victory`.
- Uma vitória comum marca a onda como resolvida e agenda a próxima, respeitando
  o intervalo variável já existente.
- Vitória final e derrota não agendam ondas posteriores.
- O relógio estratégico não avança quando a campanha não está `Ongoing`.
- Reiniciar campanha, tela de créditos e metaprogressão ficam fora desta parte.

## 77. Persistência v7

`GameSaveData.CurrentVersion` passa de 6 para 7 sem trocar a chave
`tcc.game.save.v2` do PlayerPrefs. A normalização existente migra saves antigos
no carregamento.

```text
CampaignSaveData
    outcome

WaveSaveData
    battleReport

BattleReportSaveData
    resolved
    outcome
    effectiveDefense
    defenseRatio
    initialGateHealth
    gateDamage
    finalGateHealth
    initialWarriors / lostWarriors / survivingWarriors
    initialArchers / lostArchers / survivingArchers
    initialMages / lostMages / survivingMages
    spoils
    reportAcknowledged
```

Regras de normalização:

- Relatório ausente em onda antiga torna-se um relatório vazio, nunca
  `resolved` implicitamente.
- Quantidades, vida, dano e espólio são limitados a valores não negativos.
- Sobreviventes e baixas não podem exceder os totais iniciais.
- Uma onda `Resolved` sem relatório, possível em um save v6, recebe um relatório
  legado neutro e reconhecido; suas consequências não são reaplicadas.
- `AwaitingResolution` sem relatório permanece pendente e é resolvida ao iniciar
  o runtime.
- `Victory` só é válida se a onda 10 estiver resolvida e não houver brecha.
- `Defeat` exige ao menos um portão com vida zero; estados inconsistentes voltam
  para `Ongoing`.

## 78. Arquitetura prevista

### `BattleCalculator`

- Calcula força, taxa, baixas, dano, resultado e espólio.
- Reutiliza uma única fonte de verdade para a força efetiva.
- Não altera save e não publica eventos.

### `BattleResolutionService`

- Localiza somente a onda atual em `AwaitingResolution`.
- Ignora chamadas repetidas se já existir relatório resolvido.
- Aplica baixas, vida, ouro, relatório e estado da campanha.
- Conclui a onda e agenda a próxima quando permitido.
- Faz um único commit após todas as mutações.
- Restaura o estado anterior se o commit falhar.
- Publica `BattleResolved` e a mudança de ouro somente depois do commit.

### `WaveDirectorService`

- Continua responsável pela agenda e pelos avisos.
- Expõe uma conclusão interna sem commit independente para que a resolução seja
  atômica.
- Limita a agenda às dez ondas e ao estado `Ongoing`.
- `MarkWaveResolved` deixa de ser a API pública de gameplay; testes auxiliares
  devem usar a nova resolução ou uma função explicitamente interna.

### `CampaignRuntimeDriver`

- Conclui chegadas antes de resolver uma onda vencida.
- Solicita no máximo uma resolução por atualização.
- Não contém fórmula de combate nem manipula dados diretamente.

### `ThreatPresentationService` e `GlobalThreatHud`

- Consomem `BattleResolved` para enfileirar o resumo.
- Mostram relatório pendente, campanha vencida ou cidade derrotada.
- Continuam sem interceptar raycasts e sem oferecer comandos.

### `GarrisonManagementPanel`

- Exibe o relatório do portão visitado acima dos controles de deslocamento.
- O botão `Confirmar relatório` registra a leitura por meio do serviço.
- Em campanha encerrada, mostra o resultado final e desabilita novos envios.

## 79. Transação e idempotência

Baixas, dano, espólio, estado da onda, resultado da campanha e eventual próxima
onda formam uma única alteração semântica. Nenhuma dessas partes pode ser salva
isoladamente. O fluxo será:

1. Validar onda e capturar os valores anteriores.
2. Calcular o resultado sem mutação.
3. Aplicar todas as consequências em memória.
4. Normalizar e persistir uma vez.
5. Em falha, restaurar todos os valores capturados e relançar o erro.
6. Após sucesso, publicar eventos de HUD e economia.

Repetir a resolução, recarregar a cena ou reiniciar o jogo não pode conceder
ouro nem causar baixas duas vezes.

## 80. Textos de apresentação

Resumo global de vitória comum:

```text
PORTÃO NORTE — ONDA REPELIDA
Baixas: 2  |  Portão: -8  |  Espólio: +3 ouro
Relatório disponível com o Capitão.
```

Resumo de brecha:

```text
PORTÃO OESTE ROMPIDO
A cidade foi derrotada. Consulte o Capitão para o relatório final.
```

Relatório do Capitão:

```text
ONDA 4 — RELATÓRIO DO PORTÃO LESTE
Defesa efetiva: 31 / Ameaça: 28
Guerreiros: 12 → 11  (-1)
Arqueiros:   8 → 7   (-1)
Magos:       2 → 2   (-0)
Integridade: 86 → 86 (-0)
Espólio recolhido: 4 ouro
Guerreiros exploraram a fraqueza dos Saqueadores.
```

Se mais de um bônus tiver contribuído, a explicação lista todos os pares
relevantes em vez de retornar apenas o primeiro.

## 81. Critérios de aceitação

- A onda vencida é resolvida no mesmo ciclo lógico, sem exigir presença local.
- Tropas que chegam no mesmo horário do ataque participam da batalha.
- Tropas em trânsito com chegada posterior não participam.
- Cada classe aplica seu bônus apenas contra o inimigo correspondente.
- O preview e o cálculo final usam a mesma força efetiva.
- Baixas nunca são negativas nem excedem a guarnição.
- Vida do portão permanece entre zero e o máximo.
- Espólio é pequeno, só existe em vitória e atualiza o HUD de ouro.
- A resolução completa gera exatamente um save e não pode ser repetida.
- O relatório sobrevive a troca de cena e reinício do jogo.
- O relatório só é reconhecido ao interagir com o Capitão correto.
- Uma brecha encerra a campanha em derrota.
- Repelir a onda 10 encerra a campanha em vitória.
- Nenhuma onda 11 é criada.
- Pop-ups, navegação, limites de câmera e movimentação de tropas continuam sem
  regressão.

## 82. Testes planejados

### EditMode — cálculo

- Força favorável reduz baixas em relação à mesma defesa sem bônus.
- Cada par de vantagem funciona isoladamente.
- Defesa suficiente causa zero dano ao portão.
- Defesa insuficiente causa dano crescente e limitado à vida atual.
- Nenhum soldado, um soldado e grandes quantidades.
- Distribuição por maiores restos preserva o total calculado.
- Resultados iguais para entradas iguais.
- Espólio de vitória e zero espólio na brecha.

### EditMode — resolução e persistência

- Resolução muta somente o portão atacado.
- Baixas, ouro, relatório e onda são persistidos juntos.
- Segunda chamada é idempotente.
- Falha de commit restaura todo o estado.
- Save v6 migra para v7 preservando campanha, tropas e ondas.
- `AwaitingResolution` é retomada; `Resolved` legado não é reaplicada.
- Relatório pendente e reconhecimento sobrevivem ao reload.
- Onda 10 vitoriosa e portão rompido encerram corretamente a campanha.

### PlayMode

- Chegada no mesmo instante entra na batalha.
- Evento atualiza cartão, banner, Capitão e ouro.
- Banner aguarda o fechamento de um modal já aberto.
- Troca Centro–Portão não duplica resolução nem HUD.
- Reabrir o Capitão não duplica espólio.
- Estado terminal congela relógio e não agenda outra onda.
- HUD continua sem bloquear raycasts.

## 83. Plano de tarefas

| ID | Tarefa | Dependências | Saída verificável |
|---|---|---|---|
| P5-01 | Extrair cálculo compartilhado de força efetiva | Parte 4 | Preview e batalha idênticos |
| P5-02 | Implementar `BattleCalculator` puro | P5-01 | Fórmulas cobertas por testes |
| P5-03 | Adicionar relatório e estado de campanha ao save v7 | P5-02 | Migração e normalização aprovadas |
| P5-04 | Implementar resolução transacional e idempotente | P5-03 | Uma mutação e um commit |
| P5-05 | Reordenar chegadas, onda e resolução no runtime | P5-04 | Limite temporal correto |
| P5-06 | Limitar campanha a dez ondas e estados terminais | P5-04 | Sem onda 11 |
| P5-07 | Integrar resumo ao HUD global | P5-04 | Feedback imediato e modal-safe |
| P5-08 | Integrar relatório e confirmação ao Capitão | P5-04 | Consulta somente no NPC correto |
| P5-09 | Executar regressão EditMode e PlayMode | Todas | Suítes integrais aprovadas |
| P5-10 | Registrar implementação e parâmetros reais | P5-09 | Documento atualizado |

## 84. Fora do escopo da Parte 5

- Combate animado ou controlado diretamente pelo jogador.
- Posicionamento individual de soldados dentro do portão.
- Aleatoriedade, críticos, feridos, experiência ou promoções.
- Habilidades ativas de aventureiros.
- Reparar o portão; isso será uma interação econômica posterior com um NPC.
- Fontes de renda externas à cidade.
- Reinício de campanha, créditos ou metaprogressão.
- Balanceamento definitivo; as constantes desta parte devem permanecer
  centralizadas em `CampaignRules` para iteração posterior.

## 85. Definição de pronto

A Parte 5 estará concluída quando cada onda for resolvida automaticamente e
uma única vez; bônus, baixas, dano e pequeno espólio forem aplicados e salvos de
forma atômica; o jogador receber um resumo imediato e puder ler o relatório com
o Capitão do portão atacado; e a campanha terminar corretamente em derrota por
brecha ou vitória após a décima onda, sem regressões nas partes anteriores.

## 86. Implementação da Parte 5 — 02/09/2026

A resolução automática e determinística foi implementada usando a mesma força
efetiva exibida no prognóstico. Baixas são distribuídas proporcionalmente, o
portão recebe dano quando a defesa é insuficiente e ondas repelidas concedem o
pequeno espólio definido neste plano.

O runtime agora efetiva chegadas antes de avaliar a onda no mesmo instante e
espera o fechamento de menus modais antes de resolver uma batalha. Toda a
resolução — tropas, portão, ouro, relatório, onda seguinte e resultado da
campanha — é persistida de forma idempotente; falhas de commit restauram o
estado anterior.

O save passou para a versão 7 mantendo a chave `tcc.game.save.v2` do
PlayerPrefs. Relatórios sobrevivem ao reload, aparecem no cartão do portão e
podem ser consultados e confirmados pelo Capitão. O painel bloqueia novos
deslocamentos após vitória ou derrota.

Uma brecha encerra a campanha em derrota. Repelir a décima onda encerra em
vitória e impede a criação da onda 11. O HUD global anuncia o resultado sem
interceptar a interação do jogador.

Validação final: 90 de 90 testes EditMode e 15 de 15 testes PlayMode aprovados.
