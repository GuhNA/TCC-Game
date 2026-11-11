# PROCESSOS DE DESENVOLVIMENTO DE JOGOS INDIES

**Autores:** Gustavo Henrique Nascimento de Almeida; Marcos Vinicius Barbieri Alves

**Instituição:** Centro Universitário da Fundação Hermínio Ometto

**Curso:** Engenharia da Computação

**Orientador:** Prof. Dr. Maurício Acconcia Dias

**E-mails institucionais:** [preencher]

## Resumo

Este trabalho descreve como dois estudantes de Engenharia da Computação desenvolveram, em Unity, um jogo independente 2D de gestão e defesa de uma vila. O jogador usa o ouro dos impostos para melhorar construções, recrutar tropas ou contratar aventureiros. As expedições rendem materiais para novas melhorias, enquanto a defesa exige que o jogador distribua tropas entre os portões antes de cada ataque. A renda pode acumular durante a ausência, o que permite jogar em sessões separadas. Para reconstruir o desenvolvimento, o estudo de caso reúne registros das tarefas, documentos do projeto e alterações no código. A divisão por cenas permitiu trabalhar em paralelo, mas o uso do mesmo saldo e inventário exigiu regras em comum. Nos cenários controlados, o jogador pôde recolher a renda do intervalo elegível ao voltar e encontrou o tempo de preparação da próxima onda preservado. O caso mostra como a equipe organizou as entregas e tratou as dependências entre as mecânicas de um jogo *idle*.

**Palavras-chave:** desenvolvimento independente, jogos idle, design de jogos

## 1 Introdução

### 1.1 Contexto e proposta do jogo

Dois estudantes de Engenharia da Computação desenvolveram o protótipo a partir de uma ideia de jogo que pudesse ser retomado em sessões curtas. O jogador cuida de uma vila e prepara sua defesa. Ao voltar, recupera o progresso salvo e pode recolher o ouro acumulado para decidir o que fazer naquela sessão.

A partida começa no Centro, onde o jogador recolhe os impostos do Mercado Local. Com esse ouro, ele pode melhorar uma construção, comprar tropas para os quatro portões ou contratar aventureiros na Taverna. Os aventureiros exploram regiões que fornecem materiais para novas melhorias. Quando uma onda se aproxima, investir na vila significa ter menos ouro disponível para reforçar a defesa naquele momento.

Alharthi et al. (2018) observam que jogos *idle* permitem algum avanço com pouca interação e dão espaço ao planejamento. No protótipo, o jogador pode esperar pela renda, pela reposição de recrutas ou pela chegada de uma expedição. Quando volta à partida, precisa escolher como usar o que ficou disponível. Na defesa, essa escolha inclui decidir para qual portão enviar tropas antes do ataque.

### 1.2 Problema de pesquisa

Quando alguém fecha um jogo *idle* e volta horas depois, precisa reconhecer o que mudou e o que ainda depende de uma decisão sua. Na vila, faz sentido encontrar novos impostos a recolher. O mesmo intervalo de ausência, se também fosse descontado da campanha, poderia consumir o tempo que o jogador tinha para preparar a defesa. O projeto adotou regras de tempo diferentes para a renda, o recrutamento, a exploração e as ondas.

Essas regras afetam a ordem do trabalho. Uma alteração no saldo muda as compras no Portão e na Taverna. Uma mudança no horário da próxima onda altera o tempo disponível para recrutar e distribuir tropas. A equipe precisa coordenar essas entregas e verificar situações que vão de alguns minutos de preparação a horas fora do jogo. Para isso, a avaliação seleciona momentos do ciclo que podem ser reproduzidos sem aguardar toda a duração de uma partida.

Como uma equipe pequena pode organizar o desenvolvimento, integrar as mecânicas e verificar a progressão de um jogo *idle* pensado para várias sessões?

### 1.3 Justificativa

A produção independente amplia a variedade de jogos que chega ao público. Uma equipe pequena pode combinar gêneros, construir um protótipo e mudar a proposta à medida que entende melhor o que funciona. O estudo de Kultima, Ojanen e Nylund (2024) acompanha esse percurso em *Noita*, da ideia inicial às mudanças feitas ao longo da produção.

Nos jogos *idle*, parte do interesse está em decidir o que fazer com os recursos obtidos durante a espera. Essa proposta pode alcançar um público amplo. No lançamento da versão 1.0 de *Melvor Idle*, a editora Jagex informou que o título já havia recebido mais de um milhão de jogadores entre PC e dispositivos móveis (JAGEX, 2021).

Neste trabalho, a combinação de defesa, economia e exploração permitiu acompanhar decisões que surgiram durante a produção. O registro mostra como duas pessoas dividiram as tarefas, o que precisaram combinar para integrar as cenas e como conferiram o que o jogador encontra ao voltar. Esse material pode servir de referência para outros projetos acadêmicos com mecânicas que avançam entre sessões.

### 1.4 Objetivos

O objetivo geral é desenvolver um protótipo independente de gestão e defesa de uma vila e analisar como suas mecânicas foram organizadas e integradas para compor uma experiência *idle*. Para isso, o projeto tem os seguintes objetivos específicos.

1. Desenvolver a economia da vila para que o jogador possa arrecadar ouro e investir em construções e melhorias.
2. Implementar a defesa dos portões, da preparação das guarnições à resolução dos ataques anunciados.
3. Permitir que o jogador use o ouro da vila para contratar tropas no Portão e aventureiros na Taverna.
4. Desenvolver a exploração pela Taverna, com expedições em que os aventureiros combatem e obtêm materiais.
5. Documentar como as entregas foram divididas e integradas e verificar o que o jogador pode consultar e fazer quando volta à partida.

### 1.5 Escopo e qualidades do protótipo

O protótipo é local e para um jogador. A partida passa pelo Centro, pelo Portão, pela Taverna e pela cena de combate de exploração. No Centro, o jogador acompanha o saldo, recolhe impostos e melhora construções. No Portão, compra tropas conforme o estoque e pode mantê-las na reserva ou transferi-las para as guarnições. A campanha avisa sobre os ataques, registra as consequências das batalhas e prevê até dez ondas contra os quatro portões.

Na Taverna, o jogador contrata aventureiros e abre o mapa de exploração. Uma expedição passa por deslocamento e combate. Depois de conquistar uma região, é possível alocar exploradores e guardas, coletar materiais e transportá-los para a vila. O salvamento guarda os aventureiros, os itens e o progresso das regiões.

Ao trocar de cena, o jogador precisa encontrar o mesmo saldo e usar os mesmos comandos de movimento e interação. Ao retomar uma partida, precisa recuperar o progresso, inclusive quando o salvamento foi criado em uma versão anterior do formato. A interface deve mostrar o ouro, o estoque de tropas e o tempo até a próxima ameaça. Separar as regras da apresentação permite ajustar os parâmetros e conferir o comportamento do jogo sem depender da interface de cada cena.

O artigo parte da literatura sobre produção independente e progressão em jogos *idle*. Em seguida, acompanha como a equipe organizou as entregas e apresenta os sistemas integrados e os resultados da verificação da retomada.

## 2 Revisão bibliográfica

### 2.1 Desenvolvimento independente e organização do trabalho

Em uma análise de 200 relatos escritos após o desenvolvimento de jogos, Politowski et al. (2021) encontraram dificuldades de gestão e de produção. Ao acompanhar um estúdio independente, Poulsen e Wirman (2024) observaram que a equipe discutia as decisões criativas junto com a organização do trabalho. No projeto da vila, essa relação apareceu quando cenas desenvolvidas em paralelo passaram a usar o mesmo saldo e inventário. Os integrantes precisaram decidir em conjunto como esses recursos seriam atualizados.

Kultima, Ojanen e Nylund (2024) usaram uma linha do tempo para relacionar mudanças no design de *Noita* a etapas de produção. Aqui, o cronograma de curto prazo era revisto nas reuniões semanais no Discord, e o Trello registrava as tarefas que saíam dessas conversas. O histórico do Git ajuda a localizar o que foi implementado e quando as alterações passaram a funcionar juntas.

### 2.2 Documentação de design e dependências entre sistemas

O *Game Design Document* (GDD) descreve o jogo que a equipe pretende construir e as regras que precisam orientar a produção. Carrijo et al. (2023) discutem como o gênero e as necessidades do projeto influenciam a organização desse documento. Em uma equipe pequena, a proposta, os cartões e os documentos técnicos podem cumprir essa função em conjunto, desde que os integrantes consigam localizar as regras comuns.

Para descrever a defesa da vila, por exemplo, é preciso dizer quando o ataque é anunciado, o que o jogador pode preparar e como a guarnição afeta a batalha. Já o recrutamento depende de preço, estoque e saldo. Na exploração, entram os aventureiros disponíveis, o estado das regiões e os materiais obtidos. A economia liga essas regras. A interface da Taverna pode ser criada separadamente, mas a compra de um aventureiro precisa usar o ouro que o jogador vê no Centro.

### 2.3 Espera, planejamento e retorno em jogos idle

Alharthi et al. (2018) estudaram 66 jogos *idle* e os compararam com dez jogos de outros tipos. Os autores descrevem como a automação e a espera abrem espaço para planejar o uso dos recursos. A produção pode ocorrer enquanto o jogo está aberto. Em jogos que consideram também o período de ausência, parte do que o jogador pode fazer na sessão seguinte depende de quanto tempo passou.

Cada atividade pode tratar a espera de um jeito. A atualização 1.1.2 de *Melvor Idle* passou a atualizar a cidade em intervalos de uma hora e a contabilizar até 24 horas fora do jogo (MELVOR IDLE, 2023). No protótipo da vila, a renda cresce de acordo com a produção do Mercado Local, até um limite entre coletas. Para a defesa, a espera tem outra função. Ela dá tempo para o jogador reagir aos avisos e preparar os portões.

### 2.4 Decisões de balanceamento

Pecorella (2016) discute como a relação entre o ritmo de produção e o custo das melhorias afeta o avanço em jogos incrementais. Na vila, usar o ouro para aumentar a renda futura reduz o que sobra para tropas e aventureiros naquela sessão. Conforme as construções sobem de nível, as melhorias ficam mais caras. Algumas exigem também materiais, o que dá ao jogador um motivo para investir em expedições.

O saldo e o estoque limitam quantas tropas podem ser compradas de uma vez. Ao longo da campanha, as ondas ficam mais fortes e os intervalos de preparação diminuem. Os avisos dão ao jogador a chance de reorganizar as tropas antes do ataque. Na economia, o limite de 24 horas estabelece quanto tempo pode ser convertido em renda entre duas coletas. Juntas, essas regras definem os recursos disponíveis no retorno e o tempo para decidir como gastá-los.

### 2.5 Continuidade percebida e atualização ao retornar

Ao abrir a partida e encontrar ouro a recolher, o jogador tem a impressão de que a vila continuou produzindo. O jogo cria esse efeito a partir do salvamento. Na retomada, compara o horário atual com a referência gravada e calcula o que corresponde ao intervalo. Os impostos podem ser calculados de uma vez, com a taxa de produção e o tempo que ainda cabe no limite. Na exploração, os horários salvos permitem identificar chegadas e ciclos de coleta que já deveriam ter ocorrido.

Para comparar horários entre sessões, o jogo consulta o relógio do dispositivo. O .NET permite expressar esses instantes em segundos desde 1º de janeiro de 1970, em UTC (MICROSOFT, [s.d.]). Na campanha, a contagem depende do tempo em que a partida está em execução. A Unity fornece o intervalo entre quadros com `Time.unscaledDeltaTime`, que independe da escala de tempo do motor (UNITY TECHNOLOGIES, [s.d.]d). Assim, o retorno pode trazer mais ouro sem descontar o período de ausência da preparação para o ataque.

Um retorno coerente também depende de o jogo lembrar o que o jogador já fez. Se ele recolher os impostos e abrir a partida novamente, o crédito precisa aparecer no saldo uma única vez. Para reproduzir situações como essa, o relógio dos testes pode avançar por um intervalo conhecido, sem que a equipe espere horas entre as execuções. A documentação da Microsoft (2026) exemplifica essa técnica.

## 3 Metodologia

### 3.1 Caracterização e fontes

Esta pesquisa aplicada acompanha o desenvolvimento de um jogo por dois estudantes. O estudo de caso abrange as regras do jogo, a distribuição das tarefas, a integração das cenas e a verificação do protótipo. O retorno após uma ausência recebe atenção especial porque as regras de economia e campanha precisam funcionar em conjunto com o salvamento.

A proposta inicial, os documentos técnicos, os cartões do Trello e o histórico do Git permitem recuperar as etapas do trabalho. A análise seleciona registros que mostram a criação de uma área, a definição de uma regra ou a integração de sistemas. Em cada episódio, o artigo relaciona a tarefa ao que precisava estar pronto e à mudança que aparece no código. Na verificação da retomada, o código e o relatório dos testes registram as condições e os resultados. Essa forma de ligar a análise às fontes segue a orientação de Runeson e Höst (2009) para estudos de caso em Engenharia de Software.

### 3.2 Definição das regras e ordem de implementação

A primeira proposta era uma vila 2D com progressão automática. A equipe dividiu o trabalho em entregas que pudessem ser conferidas à medida que ficavam prontas. O Centro precisava de construções, uma fonte de renda e um saldo visível. No Portão, o jogo precisava conferir o ouro e o estoque antes de enviar guerreiros, arqueiros ou magos para a reserva. A partir dessas tropas, o jogador poderia responder aos avisos e montar as guarnições para as ondas.

Quando Centro e Portão foram integrados, a equipe precisou definir qual saldo valia nas duas cenas e preservar as regras de arrecadação e compra. A Taverna passou a usar o mesmo ouro para contratar aventureiros. A exploração acrescentou uma ligação no sentido inverso. Os aventureiros saem da Taverna para combater, as regiões conquistadas podem receber exploradores e guardas, e os materiais transportados voltam para o inventário. Alguns desses materiais são usados nas construções do Centro.

Com as mecânicas ligadas, a equipe precisou decidir como cada uma trataria o tempo. Os impostos acumulam até o limite disponível entre coletas. As tropas têm um horário de reposição, e a exploração registra prazos de deslocamento, retorno e coleta. Já o tempo de preparação da campanha volta a correr a partir do ponto salvo. O planejamento da defesa e o contrato de unificação registram regras usadas para implementar essas decisões e escolher os cenários da avaliação.

### 3.3 Planejamento das entregas, divisão de tarefas e integração

As reuniões semanais no Discord serviam para escolher as próximas tarefas, combinar quem faria cada uma e criar cartões quando surgiam pendências. Durante a semana, cada integrante avançava nas suas entregas e tirava dúvidas por mensagem. No Trello, as listas A Fazer, Fazendo, Revisando e Arquivado mostravam em que ponto cada tarefa estava. A equipe atualizava tanto a lista quanto os itens do cartão. A Figura 1 mostra o quadro usado no projeto.

**Figura 1 — Quadro do Trello utilizado para acompanhar as tarefas do projeto**

![Quadro Jogo - TCC com as listas A Fazer, Fazendo, Revisando e Arquivado](Evidencias/imagens/trello-quadro-jogo-tcc.png)

Fonte: captura do quadro da equipe, fornecida pelos autores

No cartão do Portão Norte, a tarefa foi descrita em termos do que precisava aparecer na cena. O mapa seria pequeno, a partida começaria com ouro e o recrutamento seria feito por meio de um personagem. A Figura 2 mostra esses critérios em um cartão que já estava na lista Revisando.

**Figura 2 — Cartão do Portão Norte no Trello**

![Cartão do Portão Norte em Revisando, com descrição da cena e registro de criação em A Fazer](Evidencias/imagens/trello-cartao-portao-norte.png)

Fonte: captura do quadro da equipe, fornecida pelos autores

Em uma das reuniões, Marcos ficou com o Centro e Gustavo com a Taverna. Como as tarefas envolviam cenas diferentes, os dois puderam trabalhar em paralelo e conferir as alterações um do outro antes de reuni-las. O histórico registra trabalhos nessas frentes nos commits `279c0ec`, `f6c4a2b` e `90726d3`. A equipe costumava usar uma branch para cada desenvolvedor e uma principal. Quando duas tarefas mudavam uma regra comum, como a atualização do saldo, era preciso decidir em conjunto como ela funcionaria antes da integração.

### 3.4 Ferramentas e arquitetura do protótipo

A equipe já conhecia Unity e C# e encontrou no motor recursos para montar cenas 2D, trabalhar com sprites e criar interfaces. Com isso, foi possível ajustar o cenário e a apresentação enquanto as regras eram escritas em C#. Jogos independentes como *Cuphead* e *Hollow Knight* também foram produzidos com Unity (UNITY TECHNOLOGIES, [s.d.]a, [s.d.]b). A versão do editor usada na revisão analisada foi a 6000.3.11f1.

Dividir o projeto em cenas ajudou a distribuir o trabalho. Para o jogador, porém, passar de uma área a outra faz parte da mesma partida. A equipe colocou o saldo, o inventário e o progresso em uma sessão compartilhada, consultada pelas interfaces para exibir o ouro e processar compras. O repositório salva esses dados em JSON com armazenamento local em `PlayerPrefs`. Ao abrir o jogo, ele recupera a partida e trata formatos anteriores de salvamento.

Os serviços usam esses dados para atualizar o que estiver sujeito à passagem do tempo. A renda, a reposição de tropas e as atividades da exploração consultam o relógio do dispositivo. A campanha segue o tempo da partida em execução. Nos testes, a interface `IClock` permite informar um horário conhecido e comparar o estado da partida antes e depois do intervalo escolhido.

### 3.5 Planejamento e execução da avaliação

A avaliação começa com três ações que o jogador precisa conseguir fazer quando volta à vila. Ele consulta a renda disponível, recolhe o ouro uma única vez e confere quanto falta para o próximo ataque. Um erro em qualquer uma delas muda o que o jogador consegue comprar ou o tempo que tem para preparar a defesa.

Para escolher os cenários, a equipe partiu dos pontos em que o resultado deveria mudar. Abrir logo depois de salvar fornece o caso sem tempo de ausência. Seis minutos dão um ganho que pode ser conferido diretamente pela taxa de produção. Os intervalos antes, no limite e depois de 24 horas mostram quando o acúmulo para. Também foi testado o retorno com parte do limite já usada e a abertura depois da coleta, para conferir se o crédito foi preservado sem duplicação. Por fim, um cenário mede quanto tempo ainda falta para a onda após uma ausência longa.

Em 10 de novembro de 2026, os cenários foram executados no Windows com Unity Test Framework 1.6.0, em *Edit Mode* (UNITY TECHNOLOGIES, [s.d.]c). Cada partida foi gravada com uma chave isolada e carregada em uma nova sessão do mesmo processo. Nos cenários de renda, a partida começava com 1.000 unidades de ouro e Mercado Local no nível 3, que produz 20 unidades por minuto. A verificação comparou o valor mostrado para coleta, o crédito e o saldo. O procedimento para repetir a rodada está no [relatório da execução](Evidencias/testes/Resultados_da_retomada_2026-11-10.md).

## 4 Desenvolvimento e resultados

### 4.1 Evolução do protótipo e integração das entregas

No início, o Centro e o Portão tinham saldos separados. Quando as cenas foram reunidas, o ouro arrecadado no Centro precisava estar disponível para comprar tropas no Portão. O commit `4fc66a0`, de agosto de 2026, registra essa entrega. O contrato definiu um saldo único e especificou o que deveria continuar valendo para produção, acúmulo e compras. A partir daí, qualquer mudança na economia precisava considerar as duas cenas. [Contrato de unificação](UnificationBehaviorContract.md)

Em setembro, a campanha ganhou quatro portões e uma reserva de tropas, como registra o commit `e79782b`. O trabalho na Taverna avançou em paralelo com o do Centro, com a construção do ambiente e a seleção de aventureiros. A exploração exigiu depois que os materiais obtidos em uma região fossem reconhecidos pelas construções da vila. O commit `2436b78`, de novembro, registra a entrada de itens e do relatório de exploração. Com essas entregas, integrar as cenas passou a incluir também a circulação de recursos entre elas.

### 4.2 Experiência de jogo e sistemas disponíveis

O jogador começa no Centro e decide como usar o ouro que o Mercado Local produz. Pode melhorar a vila, comprar tropas ou reservar parte do saldo para contratar aventureiros. A Figura 3 mostra a área em que essas escolhas começam.

**Figura 3 — Centro da vila**

![Visão geral da cena Centro com as construções da vila](Evidencias/imagens/centro-visao-geral-2026-11-05.png)

Fonte: captura do protótipo, 5 de novembro de 2026

Quando recebe um aviso de ataque, o jogador confere qual portão está ameaçado. Pode deixar tropas na reserva ou deslocá-las para a guarnição. Quando a onda chega, o jogo considera a defesa disponível e registra o que aconteceu na batalha. O painel usado para preparar as tropas aparece na Figura 4.

**Figura 4 — Painel de guarnição do Portão**

![Painel do Portão com controles de guarnição e informação da próxima ameaça](Evidencias/imagens/portao-painel-guarnicao-2026-10-27.png)

Fonte: captura do protótipo, 27 de outubro de 2026

Na Taverna, o ouro também pode ser usado para contratar aventureiros. O jogador escolhe até três para enviar a uma região e usa o mapa para acompanhar a expedição. Depois de conquistar a área, pode enviar exploradores, definir guardas e transportar materiais para o inventário. Esses recursos podem ser usados nas melhorias do Centro. A Figura 5 mostra o interior da Taverna.

**Figura 5 — Interior da Taverna**

![Cena da Taverna, onde o jogador acessa a contratação de aventureiros e o mapa](Evidencias/imagens/taverna-interior-2026-11-05.png)

Fonte: captura do protótipo, 5 de novembro de 2026

### 4.3 Retomada da partida

Ao voltar, o jogador pode consultar quanto há para recolher e quanto falta para a próxima onda. Para calcular a renda, o jogo mede o tempo desde a última atualização e considera a parte que ainda cabe nas 24 horas permitidas entre coletas. A produção do Mercado Local nesse intervalo aparece como prévia. Quando o jogador confirma uma coleta com crédito positivo, recebe a parte inteira do valor. A fração fica acumulada, e o limite de tempo começa a contar de novo.

Na campanha, o jogo recupera a agenda de ataques e o contador que estavam salvos. A contagem continua a partir desse ponto enquanto a sessão está aberta. Assim, o jogador pode encontrar renda adicional e ainda ter tempo de usá-la na preparação do portão.

### 4.4 Verificação dos comportamentos de retorno

Os testes usaram a revisão `9abb425`, de 9 de novembro de 2026, com as alterações para isolar os salvamentos e os novos cenários de retorno. Os 23 casos executados foram aprovados. Oito avaliaram a retomada, cinco verificaram a persistência da campanha e dez trataram da recuperação de dados salvos. A Tabela 1 reúne os oito cenários de retomada.

**Tabela 1 — Valores esperados e observados nos cenários de retomada**

| Cenário | Esperado | Observado | Resultado |
| --- | --- | --- | --- |
| Retomada imediata | 0 de imposto | 0 de imposto | Aprovado |
| 6 minutos de ausência | 120 de imposto | 120 de imposto | Aprovado |
| 23h59min59s de ausência | 28.799 de imposto | 28.799 de imposto | Aprovado |
| 24 horas de ausência | 28.800 de imposto | 28.800 de imposto | Aprovado |
| 48 horas de ausência | 28.800 de imposto | 28.800 de imposto | Aprovado |
| 2 horas de ausência com 23h30 de acúmulo já usadas | Acréscimo de 600 | Acréscimo de 600 | Aprovado |
| Novo carregamento após a coleta, sem avanço do relógio | Crédito adicional de 0 e saldo de 1.120 | Crédito adicional de 0 e saldo de 1.120 | Aprovado |
| Campanha retomada após 48 horas simuladas | 132,75 s até a onda | 132,75 s até a onda | Aprovado |

Fonte: elaboração própria a partir do [relatório XML do Unity Test Framework](Evidencias/testes/RetomadaEditMode-2026-11-10.xml)

Com 24 ou 48 horas de ausência, o jogador teve o mesmo valor para recolher. O caso com parte do limite já usada mostrou que o restante disponível também era preservado ao trocar de sessão. Depois da coleta, uma nova abertura manteve o saldo sem repetir o crédito. No cenário da campanha, o tempo até a onda continuou igual ao que havia sido registrado.

A sequência das Figuras 6 a 8 mostra como o cenário de seis minutos aparece na interface. Os valores foram calculados em sessões isoladas, e o procedimento de captura apresentou os três estados na mesma vista do Centro. Antes da retomada, o saldo era de 1.000 unidades de ouro e o valor a recolher era zero (Figura 6).

**Figura 6 — Estado salvo antes da retomada simulada**

![Mercado Local com zero de imposto disponível e saldo de 1.000 unidades de ouro](Evidencias/imagens/retomada-01-estado-salvo-2026-11-10.png)

Fonte: elaboração própria, captura da interface do protótipo em cenário controlado

Após seis minutos simulados, a prévia indica 120 unidades de ouro para recolher. O saldo ainda é de 1.000 (Figura 7).

**Figura 7 — Impostos disponíveis após seis minutos simulados**

![Mercado Local com 120 de imposto disponível e saldo de 1.000](Evidencias/imagens/retomada-02-apos-seis-minutos-2026-11-10.png)

Fonte: elaboração própria, captura da interface do protótipo em cenário controlado

Ao confirmar a coleta, o jogador recebe as 120 unidades e passa a ter saldo de 1.120 para gastar na vila (Figura 8).

**Figura 8 — Saldo após a coleta de impostos**

![Confirmação da coleta de 120 unidades de ouro e saldo de 1.120](Evidencias/imagens/retomada-03-apos-coleta-2026-11-10.png)

Fonte: elaboração própria, captura da interface do protótipo em cenário controlado

### 4.5 Discussão e limites da avaliação

A divisão por cenas funcionou para desenvolver interfaces e regras locais. A integração do Centro com o Portão mostrou quando a equipe precisava trabalhar sobre a mesma regra. Recolher impostos, comprar tropas e contratar aventureiros são ações diferentes, mas todas mexem com o mesmo ouro. O contrato registrou as decisões de integração, e a sessão compartilhada deu às três cenas uma fonte comum para consultar e atualizar o saldo. A relação entre escolhas de design e divisão do trabalho também aparece no caso estudado por Poulsen e Wirman (2024).

Uma regra de tempo comum para toda a partida teria efeitos diferentes na renda e na defesa. No protótipo, o ouro considera parte da ausência, e o tempo de preparação da onda fica disponível para a próxima sessão. Os testes reproduziram essas situações e verificaram a persistência dos dados. A avaliação cobre esse trecho da experiência. O percurso completo inclui ainda circular pela vila, recrutar e explorar regiões ao longo da campanha.

A análise de gestão reflete o trabalho de uma única equipe. O Git registra as alterações e o Trello ajuda a identificar o escopo das tarefas. Algumas decisões tomadas em reunião foram reconstituídas pelos próprios desenvolvedores. Os resultados de retorno vêm de sessões simuladas no editor. A confirmação no executável, com encerramento e reabertura do jogo, fica para uma próxima etapa.

## 5 Conclusão

O protótipo reúne a gestão da economia, a defesa dos portões, a contratação e a exploração. A equipe conseguiu desenvolver cenas em paralelo e usar as reuniões e os cartões para acompanhar as entregas. Quando as mecânicas passaram a disputar o mesmo ouro ou depender dos mesmos itens, foi preciso combinar as regras e centralizar os dados da partida. A integração do saldo é o exemplo mais claro dessa mudança no trabalho.

Para um jogo pensado para várias sessões, definir o que o jogador faz em cada área ajuda a decidir quais dados devem ser compartilhados entre cenas e recuperados quando ele volta. A vila usa a ausência para calcular a renda e guarda o tempo que faltava para preparar a defesa. Os cenários avaliados confirmaram esses dois comportamentos nas condições simuladas.

O estudo registra como essas decisões foram tomadas e onde a divisão de tarefas precisou de coordenação. Uma próxima etapa é acompanhar uma partida pelo executável, da preparação da vila ao encerramento e à reabertura, para observar as mecânicas na sequência em que o jogador as usa.

## REFERÊNCIAS

ALHARTHI, Sultan A. et al. Playing to wait: a taxonomy of idle games. In: CHI CONFERENCE ON HUMAN FACTORS IN COMPUTING SYSTEMS, 2018, Montreal. *Proceedings [...]*. New York: Association for Computing Machinery, 2018. Art. 621. DOI: [10.1145/3173574.3174195](https://doi.org/10.1145/3173574.3174195).

CARRIJO, Gabriel F. et al. Uma revisão sistemática de literatura acerca de Game Design Documents, uma proposta para a criação de um roteiro de jogo otimizado. In: SIMPÓSIO BRASILEIRO DE JOGOS E ENTRETENIMENTO DIGITAL, 22., 2023, Rio Grande. *Anais Estendidos [...]*. Porto Alegre: Sociedade Brasileira de Computação, 2023. p. 43–53. Disponível em: [SBC OpenLib](https://sol.sbc.org.br/index.php/sbgames_estendido/article/download/27796/27606/). Acesso em: 10 nov. 2026.

JAGEX. *Melvor Idle version 1.0 launches on PC and mobile*. Cambridge, 18 nov. 2021. Disponível em: [Comunicado da editora](https://www.jagex.com/news/melvor-idle-version-1-0-launches-on-pc-and-mobile). Acesso em: 10 nov. 2026.

KULTIMA, Annakaisa; OJANEN, Riina; NYLUND, Niklas. Noita: a long journey of a game idea. *Transactions of the Digital Games Research Association*, v. 7, n. 1, p. 155–181, 2024. DOI: [10.26503/todigra.v7i1.2186](https://doi.org/10.26503/todigra.v7i1.2186).

MELVOR IDLE. *Major update: Township rework, v1.1.2*. Steam, 11 abr. 2023. Disponível em: [Notas da atualização](https://steamcommunity.com/games/1267910/announcements/detail/3700311793101067361). Acesso em: 10 nov. 2026.

MICROSOFT. *DateTimeOffset.ToUnixTimeSeconds Method*. Microsoft Learn, [s.d.]. Disponível em: [Documentação da API .NET](https://learn.microsoft.com/en-us/dotnet/api/system.datetimeoffset.tounixtimeseconds). Acesso em: 10 nov. 2026.

MICROSOFT. *Testando com FakeTimeProvider*. Microsoft Learn, 11 mar. 2026. Disponível em: [Documentação do .NET](https://learn.microsoft.com/pt-br/dotnet/core/extensions/timeprovider-testing). Acesso em: 10 nov. 2026.

PECORELLA, Anthony. *The math of idle games, part I*. Game Developer, 13 out. 2016. Disponível em: [Game Developer](https://www.gamedeveloper.com/design/the-math-of-idle-games-part-i). Acesso em: 10 nov. 2026.

POLITOWSKI, Cristiano et al. Game industry problems: an extensive analysis of the gray literature. *Information and Software Technology*, v. 134, art. 106538, 2021. DOI: [10.1016/j.infsof.2021.106538](https://doi.org/10.1016/j.infsof.2021.106538).

POULSEN, Mark Staun; WIRMAN, Hanna. Creative-rational tensions in game development: a Danish case study on team collaboration. *Transactions of the Digital Games Research Association*, v. 7, n. 1, p. 89–124, 2024. DOI: [10.26503/todigra.v7i1.2184](https://doi.org/10.26503/todigra.v7i1.2184).

RUNESON, Per; HÖST, Martin. Guidelines for conducting and reporting case study research in software engineering. *Empirical Software Engineering*, v. 14, p. 131–164, 2009. DOI: [10.1007/s10664-008-9102-8](https://doi.org/10.1007/s10664-008-9102-8).

UNITY TECHNOLOGIES. *Cuphead*. Made with Unity, [s.d.]a. Disponível em: [Estudo de caso](https://unity.com/made-with-unity/cuphead). Acesso em: 10 nov. 2026.

UNITY TECHNOLOGIES. *Hollow Knight*. Made with Unity, [s.d.]b. Disponível em: [Estudo de caso](https://unity.com/made-with-unity/hollow-knight). Acesso em: 10 nov. 2026.

UNITY TECHNOLOGIES. *Test Framework*. Unity 6.3 User Manual, [s.d.]c. Disponível em: [Manual da Unity](https://docs.unity.com/en-us/engine/6000.3/manual/packages-list/packages-all/pack-core/com-unity-test-framework). Acesso em: 10 nov. 2026.

UNITY TECHNOLOGIES. *Time.unscaledDeltaTime*. Unity 6.3 Scripting API, [s.d.]d. Disponível em: [Referência da Unity](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/time/unscaleddeltatime). Acesso em: 10 nov. 2026.
