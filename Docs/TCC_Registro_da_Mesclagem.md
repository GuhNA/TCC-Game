# Registro da mesclagem do TCC

O Word enviado é a base do documento mesclado. Para identificar alterações independentes, a comparação usou também a versão gerada antes da revisão de linguagem. Foi verificado que o backup em Markdown é idêntico ao manuscrito que originou a versão com revisão de linguagem.

## Pontos para decisão

- **Pergunta de pesquisa (1.2).** O Word havia retirado a pergunta explícita. A mesclagem recupera a pergunta do backup, após a contextualização.

- **Evidência da retomada (4.5).** O Word deixou de apresentar a tabela e o parágrafo dos 23 casos. Foram reinseridos com os oito cenários, os valores esperados e observados e o vínculo ao XML. Vale confirmar se a retirada foi uma decisão editorial.

- **Histórico (3.3 e 4.1).** O Word retirou os identificadores dos commits. Foram recuperados com as datas e o vínculo ao contrato. A redação do backup também recompõe duas frases que ficaram sem sujeito no Word.

- **Redação concorrente.** O resumo e as palavras-chave mantêm a escolha de “indie” do Word. O resumo recupera os resultados simulados do backup. Em escopo, permanecem os avisos por pop-up do Word e a explicação sobre separação entre regras e interface do backup. O carregamento de formatos anteriores foi recuperado em 3.4.

- **Figuras.** As oito imagens do backup têm correspondência visual no Word. O documento mantém as 11 imagens do Word, incluindo combate, mapa e relatório da região. A nova seção 4.3 e a numeração das Figuras 6 a 11 seguem o Word.

## Comparação integral das diferenças de texto

Abaixo estão as redações originais dos blocos divergentes. Isso mantém disponível cada formulação que pode ter sido substituída no corpo do artigo. Figuras e tabelas ficam nos arquivos de origem e no Word mesclado.

### 1. Resumo

**Backup em Markdown**

Este trabalho descreve como dois estudantes de Engenharia da Computação desenvolveram, em Unity, um jogo independente 2D de gestão e defesa de uma vila. O jogador usa o ouro dos impostos para melhorar construções, recrutar tropas ou contratar aventureiros. As expedições rendem materiais para novas melhorias, enquanto a defesa exige que o jogador distribua tropas entre os portões antes de cada ataque. A renda pode acumular durante a ausência, o que permite jogar em sessões separadas. Para reconstruir o desenvolvimento, o estudo de caso reúne registros das tarefas, documentos do projeto e alterações no código. A divisão por cenas permitiu trabalhar em paralelo, mas o uso do mesmo saldo e inventário exigiu regras em comum. Nos cenários controlados, o jogador pôde recolher a renda do intervalo elegível ao voltar e encontrou o tempo de preparação da próxima onda preservado. O caso mostra como a equipe organizou as entregas e tratou as dependências entre as mecânicas de um jogo idle.

Palavras-chave: desenvolvimento independente, jogos idle, design de jogos

**Word enviado**

Este trabalho apresenta o desenvolvimento de um jogo indie 2D em que o jogador administra uma vila e defende de ondas de inimigos. O jogador arrecada ouro, melhora construções, recruta tropas e contrata aventureiros para explorar regiões e obter materiais. Seu recurso idle faz com que a renda acumulada durante a ausência permite retomar essas decisões em momentos diferentes. O estudo de caso acompanha como uma equipe de dois estudantes organizou o trabalho, definiu as regras e integrou as mecânicas em Unity. A divisão por cenas permitiu desenvolver áreas em paralelo, enquanto a integração exigiu que a economia e os demais dados da gameplay fossem compartilhados. O protótipo resultante reúne gestão da vila, defesa e exploração. O caso documenta como uma equipe pequena pode dividir o desenvolvimento de um jogo idle, reconhecer as dependências entre as mecânicas e verificar sua integração.

Palavras-chave: desenvolvimento indie, jogos idle, design de jogos

### 2. 1.1. Contexto e proposta do jogo

**Backup em Markdown**

Dois estudantes de Engenharia da Computação desenvolveram o protótipo a partir de uma ideia de jogo que pudesse ser retomado em sessões curtas. O jogador cuida de uma vila e prepara sua defesa. Ao voltar, recupera o progresso salvo e pode recolher o ouro acumulado para decidir o que fazer naquela sessão.

A partida começa no Centro, onde o jogador recolhe os impostos do Mercado Local. Com esse ouro, ele pode melhorar uma construção, comprar tropas para os quatro portões ou contratar aventureiros na Taverna. Os aventureiros exploram regiões que fornecem materiais para novas melhorias. Quando uma onda se aproxima, investir na vila significa ter menos ouro disponível para reforçar a defesa naquele momento.

Alharthi et al. (2018) observam que jogos idle permitem algum avanço com pouca interação e dão espaço ao planejamento. No protótipo, o jogador pode esperar pela renda, pela reposição de recrutas ou pela chegada de uma expedição. Quando volta à partida, precisa escolher como usar o que ficou disponível. Na defesa, essa escolha inclui decidir para qual portão enviar tropas antes do ataque.

**Word enviado**

O projeto nasceu como um trabalho de Engenharia da Computação desenvolvido por dois estudantes. A proposta foi criar um jogo 2D de gestão e defesa de uma vila que pudesse ser jogado em momentos espaçados. Ao voltar, o jogador encontra a vila no estado em que a deixou e pode usar a renda acumulada para decidir o que fazer a seguir.

A partida começa no Centro. O jogador recolhe impostos do Mercado Local e investe o ouro em construções, tropas ou aventureiros. As tropas defendem os quatro portões da vila. Os aventureiros são contratados na Taverna e enviados a regiões que oferecem materiais para novas melhorias. À medida que as ondas inimigas se aproximam, gastar ouro em uma melhoria ou reservar o recurso para a defesa muda as opções da próxima etapa da campanha.

O protótipo combina momentos de decisão com períodos de espera. Alharthi et al. (2018) observam que jogos idle permitem progresso com pouca interação direta e deslocam parte da atividade do jogador para o planejamento. No projeto, esse princípio aparece na produção de renda, na disponibilidade de recrutas e nas atividades de exploração. A defesa acrescenta uma condição ao ciclo, na preparação para cada onda depende de decisões que o jogador toma enquanto está na partida.

### 3. 1.2. Problema de pesquisa

**Backup em Markdown**

Quando alguém fecha um jogo idle e volta horas depois, precisa reconhecer o que mudou e o que ainda depende de uma decisão sua. Na vila, faz sentido encontrar novos impostos a recolher. O mesmo intervalo de ausência, se também fosse descontado da campanha, poderia consumir o tempo que o jogador tinha para preparar a defesa. O projeto adotou regras de tempo diferentes para a renda, o recrutamento, a exploração e as ondas.

Essas regras afetam a ordem do trabalho. Uma alteração no saldo muda as compras no Portão e na Taverna. Uma mudança no horário da próxima onda altera o tempo disponível para recrutar e distribuir tropas. A equipe precisa coordenar essas entregas e verificar situações que vão de alguns minutos de preparação a horas fora do jogo. Para isso, a avaliação seleciona momentos do ciclo que podem ser reproduzidos sem aguardar toda a duração de uma partida.

Como uma equipe pequena pode organizar o desenvolvimento, integrar as mecânicas e verificar a progressão de um jogo idle pensado para várias sessões?

**Word enviado**

Em um jogo idle concebido para durar várias sessões, o jogador pode deixar uma tarefa em andamento e voltar horas depois. A equipe precisa definir o que progride nesse intervalo, o que espera pela próxima ação e como o jogo apresenta o estado resultante. Se a renda, o estoque e a defesa seguirem a mesma regra de tempo, uma ausência pode mudar tanto os recursos disponíveis quanto a oportunidade de preparar o portão. O projeto adotou regras próprias para cada sistema.

Esse problema afeta a gestão do desenvolvimento. Uma mudança na economia interfere nas compras do Portão e da Taverna, e uma mudança na agenda de ataques altera o tempo disponível para essas decisões. A equipe precisa ordenar as entregas, combinar regras compartilhadas e verificar um ciclo que pode envolver minutos de preparação e horas de ausência. Repetir toda a duração de cada cenário torna a verificação demorada. É preciso selecionar situações que representem as decisões do jogador ao longo da partida.

### 4. 1.3. Justificativa

**Backup em Markdown**

A produção independente amplia a variedade de jogos que chega ao público. Uma equipe pequena pode combinar gêneros, construir um protótipo e mudar a proposta à medida que entende melhor o que funciona. O estudo de Kultima, Ojanen e Nylund (2024) acompanha esse percurso em Noita, da ideia inicial às mudanças feitas ao longo da produção.

Nos jogos idle, parte do interesse está em decidir o que fazer com os recursos obtidos durante a espera. Essa proposta pode alcançar um público amplo. No lançamento da versão 1.0 de Melvor Idle, a editora Jagex informou que o título já havia recebido mais de um milhão de jogadores entre PC e dispositivos móveis (JAGEX, 2021).

Neste trabalho, a combinação de defesa, economia e exploração permitiu acompanhar decisões que surgiram durante a produção. O registro mostra como duas pessoas dividiram as tarefas, o que precisaram combinar para integrar as cenas e como conferiram o que o jogador encontra ao voltar. Esse material pode servir de referência para outros projetos acadêmicos com mecânicas que avançam entre sessões.

**Word enviado**

Jogos independentes ampliam a variedade de propostas disponíveis na indústria. Equipes pequenas podem experimentar regras e combinações de gêneros e ajustar a ideia à medida que produzem versões jogáveis. O estudo sobre Noita, por exemplo, documenta como protótipos e decisões sucessivas transformaram a ideia de um jogo independente ao longo do desenvolvimento (KULTIMA; OJANEN; NYLUND, 2024).

Os jogos idle exploram uma forma de jogar que alterna planejamento, espera e retorno. Há exemplos de alcance expressivo. No lançamento da versão 1.0 de Melvor Idle, a editora Jagex informou que o jogo já havia recebido mais de um milhão de jogadores entre PC e dispositivos móveis (JAGEX, 2021). Esse caso mostra como a progressão entre sessões pode fazer parte de uma proposta com público além de um único canal de distribuição.

O trabalho tem valor como experiência documentada de desenvolvimento acadêmico. A combinação de economia, defesa e exploração permite observar como a equipe transforma ideias de jogo em tarefas, resolve dependências e verifica o que o jogador encontra ao voltar. O registro dessas decisões pode servir de referência a estudantes e equipes que precisem planejar um protótipo com progressão distribuída ao longo do tempo.

### 5. 1.4. Objetivos

**Backup em Markdown**

O objetivo geral é desenvolver um protótipo independente de gestão e defesa de uma vila e analisar como suas mecânicas foram organizadas e integradas para compor uma experiência idle. Para isso, o projeto tem os seguintes objetivos específicos.

Desenvolver a economia da vila para que o jogador possa arrecadar ouro e investir em construções e melhorias.

Implementar a defesa dos portões, da preparação das guarnições à resolução dos ataques anunciados.

Permitir que o jogador use o ouro da vila para contratar tropas no Portão e aventureiros na Taverna.

Desenvolver a exploração pela Taverna, com expedições em que os aventureiros combatem e obtêm materiais.

Documentar como as entregas foram divididas e integradas e verificar o que o jogador pode consultar e fazer quando volta à partida.

**Word enviado**

O objetivo geral é desenvolver um protótipo independente de gestão e defesa de uma vila e analisar como a equipe organizou e integrou suas mecânicas para compor uma experiência idle. Os objetivos específicos descrevem o que o jogador deve poder fazer e como a equipe examina o ciclo resultante.

Desenvolver a economia da vila, com arrecadação de ouro e investimento em construções e melhorias;

Implementar a defesa dos portões, com avisos de ataque, distribuição de guarnições e resolução de ondas inimigas;

Criar a contratação de tropas para a defesa e de aventureiros na Taverna, com custos compartilhados pela economia da vila;

Implementar a exploração de regiões pela Taverna, com seleção de aventureiros, expedições, combates e obtenção de materiais;

Documentar a organização e a integração das entregas e verificar aspectos da experiência idle, como o que o jogador encontra e pode fazer ao retomar a partida.

### 6. 1.5. Escopo e qualidades do protótipo

**Backup em Markdown**

O protótipo é local e para um jogador. A partida passa pelo Centro, pelo Portão, pela Taverna e pela cena de combate de exploração. No Centro, o jogador acompanha o saldo, recolhe impostos e melhora construções. No Portão, compra tropas conforme o estoque e pode mantê-las na reserva ou transferi-las para as guarnições. A campanha avisa sobre os ataques, registra as consequências das batalhas e prevê até dez ondas contra os quatro portões.

Na Taverna, o jogador contrata aventureiros e abre o mapa de exploração. Uma expedição passa por deslocamento e combate. Depois de conquistar uma região, é possível alocar exploradores e guardas, coletar materiais e transportá-los para a vila. O salvamento guarda os aventureiros, os itens e o progresso das regiões.

Ao trocar de cena, o jogador precisa encontrar o mesmo saldo e usar os mesmos comandos de movimento e interação. Ao retomar uma partida, precisa recuperar o progresso, inclusive quando o salvamento foi criado em uma versão anterior do formato. A interface deve mostrar o ouro, o estoque de tropas e o tempo até a próxima ameaça. Separar as regras da apresentação permite ajustar os parâmetros e conferir o comportamento do jogo sem depender da interface de cada cena.

O artigo parte da literatura sobre produção independente e progressão em jogos idle. Em seguida, acompanha como a equipe organizou as entregas e apresenta os sistemas integrados e os resultados da verificação da retomada.

**Word enviado**

O protótipo é offline, singleplayer e inclui navegação entre Centro, Portão e Taverna, além da cena de combate de exploração. No Centro, reúne construções, melhorias, arrecadação e acompanhamento do saldo. A campanha inclui quatro portões, reserva e transferência de tropas, avisos, até dez ondas e registro de consequências das batalhas. O recrutamento inclui estoque e renovação de tropas. A Taverna concentra a contratação de aventureiros e o mapa de exploração. As expedições incluem deslocamento, combate, conquista de regiões, alocação de exploradores e guardas, coleta e transporte de materiais.

O projeto também definiu qualidades que atravessam as cenas. O saldo precisa ser consistente quando o jogador circula entre as áreas. O salvamento deve recuperar o progresso e lidar com formatos anteriores. A interface deve tornar visíveis o ouro, o estoque e o tempo até a próxima ameaça. As ações de movimento e interação devem seguir os mesmos controles. Os anúncios visuais por meio de pop-ups permitem a noção do jogador a respeito de eventos referentes a outras cenas. 

A avaliação apresentada examina comportamentos do protótipo e o processo usado para integrá-los. A revisão bibliográfica situa essas decisões no desenvolvimento independente. A metodologia descreve como elas foram registradas e verificadas. Os resultados retomam o ciclo do jogo e discutem o que a implementação e as evidências permitem concluir.

### 7. 2.1. Desenvolvimento independente e organização do trabalho

**Backup em Markdown**

Em uma análise de 200 relatos escritos após o desenvolvimento de jogos, Politowski et al. (2021) encontraram dificuldades de gestão e de produção. Ao acompanhar um estúdio independente, Poulsen e Wirman (2024) observaram que a equipe discutia as decisões criativas junto com a organização do trabalho. No projeto da vila, essa relação apareceu quando cenas desenvolvidas em paralelo passaram a usar o mesmo saldo e inventário. Os integrantes precisaram decidir em conjunto como esses recursos seriam atualizados.

Kultima, Ojanen e Nylund (2024) usaram uma linha do tempo para relacionar mudanças no design de Noita a etapas de produção. Aqui, o cronograma de curto prazo era revisto nas reuniões semanais no Discord, e o Trello registrava as tarefas que saíam dessas conversas. O histórico do Git ajuda a localizar o que foi implementado e quando as alterações passaram a funcionar juntas.

**Word enviado**

Politowski et al. (2021) analisaram 200 relatos pós-projeto e identificaram dificuldades tanto de gestão quanto de produção. Em um estúdio independente, Poulsen e Wirman (2024) observaram que a equipe discutia as decisões criativas junto com a maneira de organizar o trabalho. Isso ajuda a explicar por que a divisão de tarefas depende do conteúdo que está sendo construído. A decisão sobre quais áreas podem avançar em paralelo muda quando elas começam a consumir o mesmo saldo ou a depender do mesmo inventário.

Kultima, Ojanen e Nylund (2024) usam uma linha do tempo de Noita para relacionar mudanças de design e etapas de produção. No projeto deste artigo, o cronograma de curto prazo foi revisto nas reuniões semanais. O Discord serviu para discutir as próximas entregas. O Trello registrou tarefas e pendências. O Git permitiu recuperar alterações nas cenas e marcos de integração. Esses registros ajudam a relacionar o que foi combinado com o que entrou no protótipo.

### 8. 2.2. Documentação de design e dependências entre sistemas

**Backup em Markdown**

O Game Design Document (GDD) descreve o jogo que a equipe pretende construir e as regras que precisam orientar a produção. Carrijo et al. (2023) discutem como o gênero e as necessidades do projeto influenciam a organização desse documento. Em uma equipe pequena, a proposta, os cartões e os documentos técnicos podem cumprir essa função em conjunto, desde que os integrantes consigam localizar as regras comuns.

Para descrever a defesa da vila, por exemplo, é preciso dizer quando o ataque é anunciado, o que o jogador pode preparar e como a guarnição afeta a batalha. Já o recrutamento depende de preço, estoque e saldo. Na exploração, entram os aventureiros disponíveis, o estado das regiões e os materiais obtidos. A economia liga essas regras. A interface da Taverna pode ser criada separadamente, mas a compra de um aventureiro precisa usar o ouro que o jogador vê no Centro.

**Word enviado**

O Game Design Document (GDD) registra regras e decisões que orientam a produção. Carrijo et al. (2023) discutem como sua organização pode variar conforme o gênero e as necessidades do projeto. Para uma equipe pequena, a documentação pode distribuir-se entre a proposta, cartões de tarefas e contratos técnicos, desde que as regras compartilhadas permaneçam identificáveis.

Neste jogo, uma descrição da defesa precisa indicar quando o ataque será anunciado, o que o jogador pode preparar e como a batalha usa a guarnição. A contratação depende de preço, disponibilidade e saldo. A exploração depende dos aventureiros contratados, do estado das regiões e do destino dos materiais. A economia reúne essas decisões porque cada compra consome recursos que poderiam ser usados em outra área. Registrar essas relações ajuda a definir a ordem das tarefas. Por exemplo, a Taverna pode desenvolver sua interface separadamente, mas a contratação precisa consultar o mesmo saldo do Centro.

### 9. 2.3. Espera, planejamento e retorno em jogos idle

**Backup em Markdown**

Alharthi et al. (2018) estudaram 66 jogos idle e os compararam com dez jogos de outros tipos. Os autores descrevem como a automação e a espera abrem espaço para planejar o uso dos recursos. A produção pode ocorrer enquanto o jogo está aberto. Em jogos que consideram também o período de ausência, parte do que o jogador pode fazer na sessão seguinte depende de quanto tempo passou.

Cada atividade pode tratar a espera de um jeito. A atualização 1.1.2 de Melvor Idle passou a atualizar a cidade em intervalos de uma hora e a contabilizar até 24 horas fora do jogo (MELVOR IDLE, 2023). No protótipo da vila, a renda cresce de acordo com a produção do Mercado Local, até um limite entre coletas. Para a defesa, a espera tem outra função. Ela dá tempo para o jogador reagir aos avisos e preparar os portões.

**Word enviado**

Alharthi et al. (2018) examinaram 66 jogos idle e os compararam com dez jogos de outros tipos. A taxonomia descreve formas de progressão com pouca interação, nas quais a espera e a automação abrem espaço para decisões de planejamento. A produção de recursos pode ocorrer enquanto o jogo está aberto. Quando o projeto admite progresso durante a ausência, o jogador também pode encontrar novos recursos ou tarefas prontas ao retornar.

As regras variam conforme a atividade. Em Melvor Idle, a atualização 1.1.2 estabeleceu que a cidade avançava em intervalos de uma hora, com até 24 horas contabilizadas durante a ausência (MELVOR IDLE, 2023). No protótipo deste trabalho, a arrecadação usa uma taxa de produção e um limite entre coletas. A defesa usa a espera para dar ao jogador tempo de se preparar. Essa diferença entre atividades é parte do desenho da partida.

### 10. 2.4. Decisões de balanceamento

**Backup em Markdown**

Pecorella (2016) discute como a relação entre o ritmo de produção e o custo das melhorias afeta o avanço em jogos incrementais. Na vila, usar o ouro para aumentar a renda futura reduz o que sobra para tropas e aventureiros naquela sessão. Conforme as construções sobem de nível, as melhorias ficam mais caras. Algumas exigem também materiais, o que dá ao jogador um motivo para investir em expedições.

O saldo e o estoque limitam quantas tropas podem ser compradas de uma vez. Ao longo da campanha, as ondas ficam mais fortes e os intervalos de preparação diminuem. Os avisos dão ao jogador a chance de reorganizar as tropas antes do ataque. Na economia, o limite de 24 horas estabelece quanto tempo pode ser convertido em renda entre duas coletas. Juntas, essas regras definem os recursos disponíveis no retorno e o tempo para decidir como gastá-los.

**Word enviado**

Pecorella (2016) descreve a relação entre o ritmo de produção e o custo das melhorias em jogos incrementais. No protótipo, o ouro acumulado permite escolher entre investir na renda futura, reforçar a defesa ou ampliar a exploração. O aumento do custo das construções conforme o nível e a exigência de materiais em algumas melhorias conectam a economia do Centro às expedições da Taverna.

O recrutamento limita as compras pelo estoque e pelo saldo disponível. A campanha, por sua vez, aumenta a força das ondas e reduz os intervalos de preparação nas etapas posteriores. Os avisos permitem reorganizar as guarnições antes do ataque. O limite de 24 horas de arrecadação restringe o ganho que pode ser acumulado entre duas coletas. São decisões que regulam quanto o jogador pode fazer em cada retorno e quanto precisa reservar para as próximas etapas.

No desenvolvimento do protótipo, o balanceamento foi a atividade mais trabalhosa para a equipe. A dificuldade esteve em conciliar o ritmo de geração de ouro, os custos de melhorias e contratações e o avanço da força das ondas. Como esses elementos são interdependentes, o ajuste de um parâmetro exige considerar seus efeitos sobre a economia, a defesa e a exploração ao longo da partida.

### 11. 2.5. Continuidade percebida e atualização ao retornar

**Backup em Markdown**

Ao abrir a partida e encontrar ouro a recolher, o jogador tem a impressão de que a vila continuou produzindo. O jogo cria esse efeito a partir do salvamento. Na retomada, compara o horário atual com a referência gravada e calcula o que corresponde ao intervalo. Os impostos podem ser calculados de uma vez, com a taxa de produção e o tempo que ainda cabe no limite. Na exploração, os horários salvos permitem identificar chegadas e ciclos de coleta que já deveriam ter ocorrido.

Para comparar horários entre sessões, o jogo consulta o relógio do dispositivo. O .NET permite expressar esses instantes em segundos desde 1º de janeiro de 1970, em UTC (MICROSOFT, [s.d.]). Na campanha, a contagem depende do tempo em que a partida está em execução. A Unity fornece o intervalo entre quadros com Time.unscaledDeltaTime, que independe da escala de tempo do motor (UNITY TECHNOLOGIES, [s.d.]d). Assim, o retorno pode trazer mais ouro sem descontar o período de ausência da preparação para o ataque.

Um retorno coerente também depende de o jogo lembrar o que o jogador já fez. Se ele recolher os impostos e abrir a partida novamente, o crédito precisa aparecer no saldo uma única vez. Para reproduzir situações como essa, o relógio dos testes pode avançar por um intervalo conhecido, sem que a equipe espere horas entre as execuções. A documentação da Microsoft (2026) exemplifica essa técnica.

**Word enviado**

A sensação de que a vila continuou produzindo vem da diferença entre o que o jogador deixou e o que encontra ao voltar. Para calcular essa diferença, o jogo grava os dados necessários e consulta o tempo decorrido quando a partida volta a ser executada. No caso dos impostos, uma taxa conhecida e um intervalo elegível permitem calcular a renda de uma vez. Na exploração, datas salvas permitem reconhecer chegadas e ciclos de coleta que venceram no intervalo.

O relógio do dispositivo fornece uma referência comparável entre sessões. O .NET permite representar esses instantes como segundos decorridos desde 1º de janeiro de 1970 em UTC (MICROSOFT, [s.d.]). A campanha usa o tempo de execução, medido pelos intervalos entre quadros. A propriedade Time.unscaledDeltaTime fornece esse intervalo independentemente da escala de tempo do motor (UNITY TECHNOLOGIES, [s.d.]d). A separação permite que o jogador encontre renda ao voltar e ainda tenha a oportunidade de reagir à próxima onda.

Essa percepção de continuidade depende da consistência do retorno. O estado apresentado precisa corresponder às regras de cada atividade, e uma nova abertura não deve repetir um crédito já recebido. Um relógio controlável permite representar durações diferentes sem aguardar todo o intervalo, como exemplifica a documentação da Microsoft (2026). A metodologia aplica essa ideia a situações que o jogador encontra durante o ciclo da vila.

### 12. 3.1. Caracterização e fontes

**Backup em Markdown**

Esta pesquisa aplicada acompanha o desenvolvimento de um jogo por dois estudantes. O estudo de caso abrange as regras do jogo, a distribuição das tarefas, a integração das cenas e a verificação do protótipo. O retorno após uma ausência recebe atenção especial porque as regras de economia e campanha precisam funcionar em conjunto com o salvamento.

A proposta inicial, os documentos técnicos, os cartões do Trello e o histórico do Git permitem recuperar as etapas do trabalho. A análise seleciona registros que mostram a criação de uma área, a definição de uma regra ou a integração de sistemas. Em cada episódio, o artigo relaciona a tarefa ao que precisava estar pronto e à mudança que aparece no código. Na verificação da retomada, o código e o relatório dos testes registram as condições e os resultados. Essa forma de ligar a análise às fontes segue a orientação de Runeson e Höst (2009) para estudos de caso em Engenharia de Software.

**Word enviado**

A pesquisa é aplicada e examina o desenvolvimento de um jogo independente por uma equipe de dois estudantes. O caso inclui a definição das mecânicas, a divisão das entregas, a integração entre cenas e a verificação de comportamentos do protótipo. A progressão durante a ausência é um dos desafios analisados, pois exige decisões coordenadas entre economia, campanha e persistência.

A reconstrução do desenvolvimento usa a proposta, os documentos de regras, o quadro de tarefas e o histórico do Git. Foram selecionados registros que indicam a construção de uma área, a introdução de uma regra ou uma integração entre sistemas. A descrição de cada episódio relaciona o que a equipe precisava entregar, a dependência identificada e a alteração registrada. Para a retomada, o código e o relatório de testes documentam as condições da execução e os resultados. Esse encadeamento entre registros e conclusões segue a orientação de Runeson e Höst (2009) para estudos de caso em Engenharia de Software.

### 13. 3.2. Definição das regras e ordem de implementação

**Backup em Markdown**

A primeira proposta era uma vila 2D com progressão automática. A equipe dividiu o trabalho em entregas que pudessem ser conferidas à medida que ficavam prontas. O Centro precisava de construções, uma fonte de renda e um saldo visível. No Portão, o jogo precisava conferir o ouro e o estoque antes de enviar guerreiros, arqueiros ou magos para a reserva. A partir dessas tropas, o jogador poderia responder aos avisos e montar as guarnições para as ondas.

Quando Centro e Portão foram integrados, a equipe precisou definir qual saldo valia nas duas cenas e preservar as regras de arrecadação e compra. A Taverna passou a usar o mesmo ouro para contratar aventureiros. A exploração acrescentou uma ligação no sentido inverso. Os aventureiros saem da Taverna para combater, as regiões conquistadas podem receber exploradores e guardas, e os materiais transportados voltam para o inventário. Alguns desses materiais são usados nas construções do Centro.

Com as mecânicas ligadas, a equipe precisou decidir como cada uma trataria o tempo. Os impostos acumulam até o limite disponível entre coletas. As tropas têm um horário de reposição, e a exploração registra prazos de deslocamento, retorno e coleta. Já o tempo de preparação da campanha volta a correr a partir do ponto salvo. O planejamento da defesa e o contrato de unificação registram regras usadas para implementar essas decisões e escolher os cenários da avaliação.

**Word enviado**

A proposta inicial definiu uma vila 2D com progressão automática. A equipe organizou o trabalho em entregas que permitiam verificar partes do ciclo. No Centro, a primeira dependência era ter construções, produção de renda e um saldo que pudesse ser consultado. No Portão, a compra de guerreiros, arqueiros e magos precisava verificar estoque e ouro antes de transferir as unidades para a reserva. A defesa foi desenvolvida em torno dos avisos, da alocação de tropas e da resolução das ondas nos quatro portões.

A integração entre Centro e Portão exigiu definir qual saldo valia nas duas cenas e quais regras de compra e arrecadação deveriam ser preservadas. A Taverna passou a usar essa economia para contratar aventureiros. O mapa de exploração reúne a seleção da equipe, o deslocamento e o combate. As regiões conquistadas podem receber exploradores e guardas. Os materiais transportados entram no inventário e podem ser exigidos para melhorar construções.

Com essas dependências identificadas, a equipe atribuiu a cada atividade uma regra de tempo. Os impostos consideram o intervalo desde o último cálculo, limitado ao tempo restante entre coletas. O estoque de tropas tem um horário de reposição. As expedições registram prazos de deslocamento e retorno, e regiões com exploradores têm ciclos de coleta. A campanha conserva o tempo de preparação salvo e volta a avançar durante a sessão. O planejamento da defesa e o contrato de unificação registram parte dessas regras e serviram de base para a implementação e a seleção dos cenários.

### 14. 3.3. Planejamento das entregas, divisão de tarefas e integração

**Backup em Markdown**

As reuniões semanais no Discord serviam para escolher as próximas tarefas, combinar quem faria cada uma e criar cartões quando surgiam pendências. Durante a semana, cada integrante avançava nas suas entregas e tirava dúvidas por mensagem. No Trello, as listas A Fazer, Fazendo, Revisando e Arquivado mostravam em que ponto cada tarefa estava. A equipe atualizava tanto a lista quanto os itens do cartão. A Figura 1 mostra o quadro usado no projeto.

**Word enviado**

A equipe se reunia semanalmente no Discord para discutir as próximas tarefas, dividir responsabilidades e registrar novas pendências. Entre as reuniões, o trabalho seguia de forma assíncrona e as dúvidas eram tratadas por mensagens. O quadro do Trello funcionava como o cronograma de curto prazo. Os integrantes acompanhavam as entregas pelas listas A Fazer, Fazendo, Revisando e Arquivado e atualizavam os itens dos cartões à medida que avançavam. A Figura 1 apresenta essa organização.

### 15. 3.3. Planejamento das entregas, divisão de tarefas e integração

**Backup em Markdown**

No cartão do Portão Norte, a tarefa foi descrita em termos do que precisava aparecer na cena. O mapa seria pequeno, a partida começaria com ouro e o recrutamento seria feito por meio de um personagem. A Figura 2 mostra esses critérios em um cartão que já estava na lista Revisando.

**Word enviado**

O cartão do Portão Norte exemplifica como uma reunião se transformava em uma tarefa verificável. O escopo incluía um mapa pequeno, ouro inicial e um personagem responsável pelo recrutamento. A Figura 2 registra os comportamentos esperados para essa entrega e a posição do cartão na lista Revisando.

### 16. 3.3. Planejamento das entregas, divisão de tarefas e integração

**Backup em Markdown**

Em uma das reuniões, Marcos ficou com o Centro e Gustavo com a Taverna. Como as tarefas envolviam cenas diferentes, os dois puderam trabalhar em paralelo e conferir as alterações um do outro antes de reuni-las. O histórico registra trabalhos nessas frentes nos commits 279c0ec, f6c4a2b e 90726d3. A equipe costumava usar uma branch para cada desenvolvedor e uma principal. Quando duas tarefas mudavam uma regra comum, como a atualização do saldo, era preciso decidir em conjunto como ela funcionaria antes da integração.

**Word enviado**

A divisão também orientava o uso do Git. Em uma das reuniões, Marcos assumiu o Centro e Gustavo ficou com a Taverna. Como cada integrante trabalhava em uma cena diferente, os dois puderam avançar em paralelo e revisar as alterações antes da integração. Os commits registram trabalhos nessas frentes em momentos distintos. Em geral, a equipe mantinha uma branch por desenvolvedor e uma principal. Quando uma tarefa alterava regras comuns, como o saldo, a integração exigia combinar o comportamento antes de reunir as alterações.

### 17. 3.4. Ferramentas e arquitetura do protótipo

**Backup em Markdown**

A equipe já conhecia Unity e C# e encontrou no motor recursos para montar cenas 2D, trabalhar com sprites e criar interfaces. Com isso, foi possível ajustar o cenário e a apresentação enquanto as regras eram escritas em C#. Jogos independentes como Cuphead e Hollow Knight também foram produzidos com Unity (UNITY TECHNOLOGIES, [s.d.]a, [s.d.]b). A versão do editor usada na revisão analisada foi a 6000.3.11f1.

Dividir o projeto em cenas ajudou a distribuir o trabalho. Para o jogador, porém, passar de uma área a outra faz parte da mesma partida. A equipe colocou o saldo, o inventário e o progresso em uma sessão compartilhada, consultada pelas interfaces para exibir o ouro e processar compras. O repositório salva esses dados em JSON com armazenamento local em PlayerPrefs. Ao abrir o jogo, ele recupera a partida e trata formatos anteriores de salvamento.

Os serviços usam esses dados para atualizar o que estiver sujeito à passagem do tempo. A renda, a reposição de tropas e as atividades da exploração consultam o relógio do dispositivo. A campanha segue o tempo da partida em execução. Nos testes, a interface IClock permite informar um horário conhecido e comparar o estado da partida antes e depois do intervalo escolhido.

**Word enviado**

A equipe adotou Unity e C# pela familiaridade com as ferramentas e pela disponibilidade de recursos para jogos 2D. O editor de cenas, a renderização de sprites e os componentes de interface permitiram trabalhar na apresentação enquanto as mecânicas eram desenvolvidas em C#. Cuphead e Hollow Knight são exemplos de jogos independentes desenvolvidos com a ferramenta (UNITY TECHNOLOGIES, [s.d.]a, [s.d.]b). A revisão analisada usa Unity 6000.3.11f1.

A divisão em cenas ajudou a organizar o trabalho, mas os dados da partida precisavam acompanhar o jogador em suas trocas de cenas. A equipe centralizou o saldo, o inventário e o progresso em uma sessão compartilhada. As interfaces consultam essa sessão ao mostrar o ouro ou ao iniciar uma compra. O repositório grava o estado em JSON com armazenamento local em PlayerPrefs. Isso permite voltar ao jogo tendo os dados salvos ao realizar o carregamento do jogo.

As regras de tempo ficam nos serviços que usam esses dados. A arrecadação, a reposição de tropas e atividades de exploração consultam o relógio do dispositivo. A campanha recebe o tempo de execução para manter a sequência de ataques. A interface IClock permite trocar o relógio real por um relógio controlado nos cenários de verificação. Essa escolha liga a arquitetura ao procedimento de avaliação.

### 18. 3.5. Planejamento e execução da avaliação

**Backup em Markdown**

A avaliação começa com três ações que o jogador precisa conseguir fazer quando volta à vila. Ele consulta a renda disponível, recolhe o ouro uma única vez e confere quanto falta para o próximo ataque. Um erro em qualquer uma delas muda o que o jogador consegue comprar ou o tempo que tem para preparar a defesa.

Para escolher os cenários, a equipe partiu dos pontos em que o resultado deveria mudar. Abrir logo depois de salvar fornece o caso sem tempo de ausência. Seis minutos dão um ganho que pode ser conferido diretamente pela taxa de produção. Os intervalos antes, no limite e depois de 24 horas mostram quando o acúmulo para. Também foi testado o retorno com parte do limite já usada e a abertura depois da coleta, para conferir se o crédito foi preservado sem duplicação. Por fim, um cenário mede quanto tempo ainda falta para a onda após uma ausência longa.

Em 10 de novembro de 2026, os cenários foram executados no Windows com Unity Test Framework 1.6.0, em Edit Mode (UNITY TECHNOLOGIES, [s.d.]c). Cada partida foi gravada com uma chave isolada e carregada em uma nova sessão do mesmo processo. Nos cenários de renda, a partida começava com 1.000 unidades de ouro e Mercado Local no nível 3, que produz 20 unidades por minuto. A verificação comparou o valor mostrado para coleta, o crédito e o saldo. O procedimento para repetir a rodada está no relatório da execução.

**Word enviado**

A avaliação partiu de situações que o jogador encontra em uma partida distribuída ao longo do tempo. Ao retornar, ele precisa conseguir consultar a renda, recolhê-la uma vez e verificar quanto tempo ainda tem para se preparar para um ataque. Esses comportamentos foram escolhidos porque conectam espera, economia, salvamento e defesa. Um resultado incorreto altera as opções de compra ou a oportunidade de preparar a vila.

Os cenários foram definidos a partir das regras de retorno. A abertura imediata indica o que acontece sem tempo elegível. Um intervalo de seis minutos permite verificar um ganho curto. Intervalos imediatamente antes, no limite e depois de 24 horas distinguem a produção do efeito do teto de acúmulo. Um salvamento com parte do limite já consumida verifica se a retomada respeita o saldo de tempo disponível. Uma nova abertura após a coleta confere a persistência do ouro. Um cenário da campanha verifica quanto falta para a onda após uma ausência prolongada.

A rodada de 10 de novembro de 2026 usou o Unity Test Framework 1.6.0 em Edit Mode, no Windows (UNITY TECHNOLOGIES, [s.d.]c). As partidas foram gravadas em chaves isoladas e carregadas em novas sessões do mesmo processo. Nos cenários de arrecadação, o estado inicial tinha 1.000 unidades de ouro e Mercado Local no nível 3, com produção de 20 unidades por minuto. Foram comparados o valor disponível, o crédito e o saldo. O relatório da execução registra os cenários e como repeti-los.

### 19. 4.1. Evolução do protótipo e integração das entregas

**Backup em Markdown**

No início, o Centro e o Portão tinham saldos separados. Quando as cenas foram reunidas, o ouro arrecadado no Centro precisava estar disponível para comprar tropas no Portão. O commit 4fc66a0, de agosto de 2026, registra essa entrega. O contrato definiu um saldo único e especificou o que deveria continuar valendo para produção, acúmulo e compras. A partir daí, qualquer mudança na economia precisava considerar as duas cenas (ver contrato de unificação).

Em setembro, a campanha ganhou quatro portões e uma reserva de tropas, como registra o commit e79782b. O trabalho na Taverna avançou em paralelo com o do Centro, com a construção do ambiente e a seleção de aventureiros. A exploração exigiu depois que os materiais obtidos em uma região fossem reconhecidos pelas construções da vila. O commit 2436b78, de novembro, registra a entrada de itens e do relatório de exploração. Com essas entregas, integrar as cenas passou a incluir também a circulação de recursos entre elas.

**Word enviado**

O Centro e o Portão foram desenvolvidos inicialmente com saldos separados. Quando a equipe reuniu as alterações, o ouro produzido no Centro precisava estar disponível para recrutar tropas. O arquivo de unificação estabeleceu o saldo compartilhado e registrou as regras de produção, acúmulo e compra que deveriam ser preservadas. Assim, duas frentes que podiam avançar separadamente passaram a depender de uma regra comum de atualização do estado.

Registrou a ampliação da campanha para quatro portões e uma reserva de tropas. A divisão entre Centro e Taverna permitiu avançar no ambiente e na seleção de aventureiros. Com a exploração, a equipe precisou ligar o inventário da Taverna às melhorias do Centro.

A introdução de itens e do relatório de exploração. Essa sequência mostra por que a integração passou a envolver tanto a circulação do jogador quanto o uso de recursos produzidos em outras cenas.

### 20. 4.2. Experiência de jogo e sistemas disponíveis

**Backup em Markdown**

O jogador começa no Centro e decide como usar o ouro que o Mercado Local produz. Pode melhorar a vila, comprar tropas ou reservar parte do saldo para contratar aventureiros. A Figura 3 mostra a área em que essas escolhas começam.

**Word enviado**

No Centro, o jogador acompanha o ouro e decide se investe em produção, tropas ou aventureiros. O Mercado Local gera renda, e as construções ampliam as possibilidades da vila. A Figura 3 apresenta a área em que o jogador inicia esse ciclo.

### 21. 4.2. Experiência de jogo e sistemas disponíveis

**Backup em Markdown**

Quando recebe um aviso de ataque, o jogador confere qual portão está ameaçado. Pode deixar tropas na reserva ou deslocá-las para a guarnição. Quando a onda chega, o jogo considera a defesa disponível e registra o que aconteceu na batalha. O painel usado para preparar as tropas aparece na Figura 4.

**Word enviado**

Quando a campanha anuncia uma ameaça, o jogador consulta o portão previsto e prepara a guarnição. As tropas podem permanecer na reserva ou ser distribuídas entre os acessos da vila. A resolução da onda considera a defesa disponível e registra consequências para a campanha. A Figura 4 mostra a interface de gestão das guarnições.

### 22. 4.2. Experiência de jogo e sistemas disponíveis

**Backup em Markdown**

Na Taverna, o ouro também pode ser usado para contratar aventureiros. O jogador escolhe até três para enviar a uma região e usa o mapa para acompanhar a expedição. Depois de conquistar a área, pode enviar exploradores, definir guardas e transportar materiais para o inventário. Esses recursos podem ser usados nas melhorias do Centro. A Figura 5 mostra o interior da Taverna.

**Word enviado**

A Taverna oferece outra forma de investir o ouro. O jogador contrata aventureiros e seleciona até três para uma expedição. O mapa acompanha o deslocamento, o combate e o progresso nas regiões. Nas áreas conquistadas, é possível alocar exploradores e guardas e transportar recursos para o inventário. Os materiais retornam ao ciclo do Centro quando são usados em melhorias. A Figura 5 apresenta o ambiente da Taverna.

### 23. 4.2. Experiência de jogo e sistemas disponíveis

**Backup em Markdown**

4.3. Retomada da partida

Ao voltar, o jogador pode consultar quanto há para recolher e quanto falta para a próxima onda. Para calcular a renda, o jogo mede o tempo desde a última atualização e considera a parte que ainda cabe nas 24 horas permitidas entre coletas. A produção do Mercado Local nesse intervalo aparece como prévia. Quando o jogador confirma uma coleta com crédito positivo, recebe a parte inteira do valor. A fração fica acumulada, e o limite de tempo começa a contar de novo.

Na campanha, o jogo recupera a agenda de ataques e o contador que estavam salvos. A contagem continua a partir desse ponto enquanto a sessão está aberta. Assim, o jogador pode encontrar renda adicional e ainda ter tempo de usá-la na preparação do portão.

4.4. Verificação dos comportamentos de retorno

Os testes usaram a revisão 9abb425, de 9 de novembro de 2026, com as alterações para isolar os salvamentos e os novos cenários de retorno. Os 23 casos executados foram aprovados. Oito avaliaram a retomada, cinco verificaram a persistência da campanha e dez trataram da recuperação de dados salvos. A Tabela 1 reúne os oito cenários de retomada.

Tabela 1 — Valores esperados e observados nos cenários de retomada

Fonte: elaboração própria a partir do relatório XML do Unity Test Framework

Com 24 ou 48 horas de ausência, o jogador teve o mesmo valor para recolher. O caso com parte do limite já usada mostrou que o restante disponível também era preservado ao trocar de sessão. Depois da coleta, uma nova abertura manteve o saldo sem repetir o crédito. No cenário da campanha, o tempo até a onda continuou igual ao que havia sido registrado.

A sequência das Figuras 6 a 8 mostra como o cenário de seis minutos aparece na interface. Os valores foram calculados em sessões isoladas, e o procedimento de captura apresentou os três estados na mesma vista do Centro. Antes da retomada, o saldo era de 1.000 unidades de ouro e o valor a recolher era zero (Figura 6).

Figura 6 — Estado salvo antes da retomada simulada

**Word enviado**

4.3. Exploração, combate temporizado e gestão de recursos

A exploração amplia a obtenção de materiais além da economia da vila. O jogador seleciona de um a três aventureiros contratados e disponíveis, escolhe uma região liberada e aguarda o deslocamento. Aventureiros em missão ou na guarda ficam indisponíveis para outro envio, o que exige distribuir a equipe entre novas conquistas e a proteção das áreas já ocupadas.

A Figura 6 apresenta o combate por turnos temporizados. Após o comando Batalhar, as ações ocorrem automaticamente, com intervalo de 0,65 segundo entre elas. A velocidade alimenta a iniciativa e permite que personagens mais rápidos atuem com maior frequência. O dano varia entre os limites de cada combatente e é reduzido pela armadura ou pela resistência mágica do alvo, conforme o tipo de ataque. O jogador acompanha a vida, as rodadas e o histórico de golpes, podendo recuar.

O balanceamento de poder considera a composição das equipes e os atributos dos combatentes. Cada inimigo possui um valor de poder, e cada região define uma faixa para a soma desses valores na equipe sorteada, limitada a três integrantes. As batalhas de defesa usam uma redução configurável dessa faixa. Esses parâmetros permitem ajustar o desafio por região; o resultado também depende da vida, da velocidade, do dano e das resistências dos participantes.

Figura 6 — Combate por turnos temporizados nas Colinas do Vento

Fonte: captura do protótipo, fornecida pelos autores

O mapa da Figura 7 reúne os destinos e permite acompanhar o envio, a chegada e os relatórios das regiões. A vitória conquista a área e libera novos destinos conforme suas dependências. A equipe inicia o retorno, e as recompensas da batalha entram no inventário ao final desse percurso. A região conquistada passa a exigir decisões de coleta e defesa.

Figura 7 — Mapa de exploração e seleção de destinos

Fonte: captura do protótipo, fornecida pelos autores

Os exploradores são trabalhadores dedicados à coleta, enquanto os aventureiros participam dos combates e da guarda. Cada nível da Construção de Exploradores amplia em uma unidade a capacidade compartilhada entre as regiões. Os trabalhadores alocados realizam coletas em ciclos, com recursos e quantidades sorteados conforme o local. Os materiais ficam acumulados na região até o comando Transportar transferi-los para o inventário da vila.

O relatório da Figura 8 mostra os exploradores alocados e disponíveis, a guarda e o tempo até o próximo ataque. Nos recursos, D indica o acumulado para transporte e T, o máximo possível por ciclo, sem representar um limite de armazenamento. O painel permite enviar ou retirar exploradores e definir a guarda. A coleta pausa durante ataques pendentes; uma região atacada sem guarda é perdida, juntamente com os exploradores nela alocados.

Figura 8 — Relatório da região e gestão de exploradores

Fonte: captura do protótipo, fornecida pelos autores

4.4. Retomada da partida

Ao retornar, o jogador pode consultar o ouro disponível para coleta e o tempo de preparação para a próxima onda. Para a renda, o jogo calcula o intervalo desde o último cálculo e usa apenas a parte que cabe no limite restante de 24 horas entre coletas. O serviço multiplica esse intervalo pela produção do Mercado Local. A consulta apresenta uma prévia. Na coleta, o valor inteiro entra no saldo, a fração permanece acumulada e o limite reinicia quando há crédito.

A defesa conserva o estado em que o jogador deixou a campanha. O salvamento registra o contador e a agenda das ondas. Ao carregar a partida, o contador retoma a contagem a partir do ponto gravado. A arrecadação pode, assim, dar ao jogador novos recursos para decidir como defender o portão antes de o tempo de preparação acabar.

4.5. Verificação dos comportamentos de retorno

A verificação com parte do intervalo já usada mostrou que a regra também se aplica quando o jogador alterna sessões antes de recolher os impostos. A abertura após a coleta preservou o saldo e a campanha manteve o tempo disponível até a onda.

As Figuras 9 a 11 apresentam o cenário de seis minutos do ponto de vista da interface. O serviço calculou os valores em sessões isoladas e um cenário de captura apresentou cada estado na mesma vista do Centro. A sequência começa no estado salvo, com saldo de 1.000 e nenhum imposto disponível.

Figura 9 — Estado salvo antes da retomada simulada

### 24. 4.4. Verificação dos comportamentos de retorno

**Backup em Markdown**

Após seis minutos simulados, a prévia indica 120 unidades de ouro para recolher. O saldo ainda é de 1.000 (Figura 7).

Figura 7 — Impostos disponíveis após seis minutos simulados

**Word enviado**

A Figura 10 mostra 120 unidades disponíveis após a ausência simulada. O saldo continua em 1.000 até o jogador recolher os impostos.

Figura 10 — Impostos disponíveis após seis minutos simulados

### 25. 4.4. Verificação dos comportamentos de retorno

**Backup em Markdown**

Ao confirmar a coleta, o jogador recebe as 120 unidades e passa a ter saldo de 1.120 para gastar na vila (Figura 8).

Figura 8 — Saldo após a coleta de impostos

**Word enviado**

A Figura 11 mostra a confirmação da coleta e o saldo de 1.120. Esse é o valor que fica disponível para a próxima decisão de investimento.

Figura 11 — Saldo após a coleta de impostos

### 26. 4.4. Verificação dos comportamentos de retorno

**Backup em Markdown**

4.5. Discussão e limites da avaliação

A divisão por cenas funcionou para desenvolver interfaces e regras locais. A integração do Centro com o Portão mostrou quando a equipe precisava trabalhar sobre a mesma regra. Recolher impostos, comprar tropas e contratar aventureiros são ações diferentes, mas todas mexem com o mesmo ouro. O contrato registrou as decisões de integração, e a sessão compartilhada deu às três cenas uma fonte comum para consultar e atualizar o saldo. A relação entre escolhas de design e divisão do trabalho também aparece no caso estudado por Poulsen e Wirman (2024).

Uma regra de tempo comum para toda a partida teria efeitos diferentes na renda e na defesa. No protótipo, o ouro considera parte da ausência, e o tempo de preparação da onda fica disponível para a próxima sessão. Os testes reproduziram essas situações e verificaram a persistência dos dados. A avaliação cobre esse trecho da experiência. O percurso completo inclui ainda circular pela vila, recrutar e explorar regiões ao longo da campanha.

A análise de gestão reflete o trabalho de uma única equipe. O Git registra as alterações e o Trello ajuda a identificar o escopo das tarefas. Algumas decisões tomadas em reunião foram reconstituídas pelos próprios desenvolvedores. Os resultados de retorno vêm de sessões simuladas no editor. A confirmação no executável, com encerramento e reabertura do jogo, fica para uma próxima etapa.

**Word enviado**

4.6. Discussão e limites da avaliação

A divisão do trabalho por cenas ajudou a equipe a avançar nas interfaces e mecânicas locais. O caso do saldo mostra onde essa divisão deixou de ser suficiente. A integração precisou estabelecer quais regras pertenciam à partida como um todo. O contrato registrou as decisões, e a sessão compartilhada deu ao Centro, ao Portão e à Taverna a mesma informação para produção, compra e contratação. Essa relação entre decisões de design e organização do trabalho também aparece no estudo de Poulsen e Wirman (2024).

A separação dos relógios resolveu outra dependência. A renda podia considerar a ausência, enquanto a campanha precisava conservar a oportunidade de reação. Os cenários controlados mostraram o comportamento dessas duas regras em novas sessões. A cobertura da rodada se concentra na retomada, na campanha e na persistência. A experiência completa envolve também a navegação, o recrutamento e a exploração ao longo de uma partida.

As conclusões sobre gestão vêm de uma equipe e de seus próprios registros. O histórico documenta alterações, e os cartões ajudam a recuperar o escopo. Parte das decisões de reunião foi reconstruída pelos próprios desenvolvedores. Os resultados da retomada correspondem às condições simuladas no editor. A confirmação do percurso pelo executável, com encerramento e reabertura, é uma etapa posterior.

### 27. 5. Conclusão

**Backup em Markdown**

O protótipo reúne a gestão da economia, a defesa dos portões, a contratação e a exploração. A equipe conseguiu desenvolver cenas em paralelo e usar as reuniões e os cartões para acompanhar as entregas. Quando as mecânicas passaram a disputar o mesmo ouro ou depender dos mesmos itens, foi preciso combinar as regras e centralizar os dados da partida. A integração do saldo é o exemplo mais claro dessa mudança no trabalho.

Para um jogo pensado para várias sessões, definir o que o jogador faz em cada área ajuda a decidir quais dados devem ser compartilhados entre cenas e recuperados quando ele volta. A vila usa a ausência para calcular a renda e guarda o tempo que faltava para preparar a defesa. Os cenários avaliados confirmaram esses dois comportamentos nas condições simuladas.

O estudo registra como essas decisões foram tomadas e onde a divisão de tarefas precisou de coordenação. Uma próxima etapa é acompanhar uma partida pelo executável, da preparação da vila ao encerramento e à reabertura, para observar as mecânicas na sequência em que o jogador as usa.

**Word enviado**

O desenvolvimento reuniu economia, defesa, contratação e exploração em um protótipo idle de gestão de vila. O trabalho pôde avançar em paralelo quando as tarefas pertenciam a cenas diferentes. Nas entregas que envolviam o mesmo ouro ou inventário, a equipe precisou documentar as regras e integrar os sistemas em torno de um estado comum. As reuniões, o quadro de tarefas e o histórico do código permitiram acompanhar essas decisões.

Para organizar um jogo pensado para várias sessões, o caso indica que a equipe pode definir primeiro o que o jogador faz em cada área e depois identificar os dados que precisam atravessar as cenas e os intervalos de ausência. No protótipo, essa abordagem permitiu reunir as decisões do Centro, do Portão e da Taverna. Os cenários de retorno confirmaram que a renda calculada fica disponível para coleta e que o tempo da defesa é preservado nas condições avaliadas.

A contribuição do trabalho é registrar as decisões de desenvolvimento que deram forma a esse ciclo de jogo. Uma próxima etapa é acompanhar o percurso pelo executável, da preparação da vila à reabertura da partida, para observar as mecânicas integradas na sequência em que o jogador as utiliza.
