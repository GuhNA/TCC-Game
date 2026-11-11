# Parecer de revisão acadêmica do TCC

Trabalho analisado — Processos de desenvolvimento de jogos indies

Data da revisão — 10 de novembro de 2026

Este parecer examina a versão de 15 páginas do artigo em Word, com cinco figuras no corpo do texto. A análise considera o modelo institucional, o manuscrito, o relatório XML dos testes, os cenários de retomada e o código usado para produzir as capturas. As recomendações abaixo são propostas de revisão. O artigo continua na versão entregue. [Artigo analisado](<D:/TCC-Game/Docs/Processos de desenvolvimento de jogos indies.docx>) · [Modelo institucional](<C:/Users/marcos/Desktop/Modelo de TCC - 2026.docx>)

O trabalho tem um problema técnico delimitável, uma implementação concreta e resultados verificáveis. Minha avaliação é que ele precisa de uma revisão de conteúdo antes da entrega à banca. A principal fragilidade está na ligação entre a gestão do desenvolvimento, as decisões de implementação e os resultados. O texto explica as ferramentas da equipe e demonstra comportamentos de retomada, mas ainda analisa pouco como o processo de trabalho conduziu a essas soluções.

## 1 Prioridades de revisão

| Prioridade | Trecho afetado | Mudança proposta | Resultado esperado |
| --- | --- | --- | --- |
| Alta | Problema, objetivos e conclusão | Alinhar as promessas do estudo ao caso de dois desenvolvedores e às regras de retomada efetivamente avaliadas | Uma pergunta que possa ser respondida pelas evidências apresentadas |
| Alta | Metodologia | Explicar a seleção e a análise dos documentos, cartões, commits e relatos | Permitir que o leitor acompanhe como os autores chegaram às conclusões |
| Alta | Resultados | Acrescentar análise das decisões de gestão e uma discussão com a literatura | Mostrar a contribuição do estudo para o processo de desenvolvimento |
| Alta | Testes | Executar e registrar uma retomada pelo executável, com encerramento real do processo | Verificar a passagem entre funcionamento interno e uso normal do jogo |
| Alta | Evidências | Identificar uma versão reproduzível do código e disponibilizar os registros citados à banca | Tornar os resultados conferíveis fora da pasta local dos autores |
| Média | Organização dos capítulos | Reunir subseções muito curtas e eliminar explicações repetidas | Ganhar espaço para análise e melhorar a continuidade da leitura |
| Média | Referências | Acrescentar fundamentação metodológica e conferir as referências parcialmente verificadas | Sustentar o método e melhorar a precisão bibliográfica |
| Baixa | Apresentação | Preencher e-mails, padronizar legendas e melhorar a leitura das capturas | Concluir os ajustes formais |

## 2 Foco da pesquisa e estrutura do artigo

### 2.1 Delimitar melhor a contribuição

O título é abrangente, enquanto o estudo se concentra no desenvolvimento de um protótipo e na retomada de impostos e campanha. Como o título foi uma escolha dos autores, eu explicitaria esse recorte logo no resumo e no primeiro parágrafo da introdução. Também definiria brevemente em que sentido o projeto é tratado como independente, considerando a equipe, o contexto acadêmico e sua autonomia de produção.

A pergunta atual — como uma equipe pequena pode planejar, implementar e testar o progresso durante a ausência — abre espaço para uma orientação geral. Os dados disponíveis sustentam melhor a análise de como esta equipe trabalhou e do que verificou nesse protótipo.

Uma formulação possível seria “Como uma equipe de dois desenvolvedores organizou a implementação e a verificação da retomada de impostos e do tempo de campanha em um protótipo de jogo idle?”. Essa formulação deve ser acompanhada de resultados sobre planejamento e integração, além dos testes.

A justificativa apresenta cedo o limite de 24 horas e a solução dos dois relógios. Eu reduziria esses detalhes nessa seção e desenvolveria a dificuldade que motivou o estudo. Ao implementar progresso durante a ausência, a equipe precisa conciliar tempo, persistência, integração entre cenas e critérios de aceitação. A contribuição possível é documentar essas decisões e mostrar como foram verificadas.

### 2.2 Ajustar os objetivos ao que será demonstrado

O objetivo geral combina construir o jogo e analisar o processo. Isso é viável, mas os objetivos específicos dão bastante peso à integração de recrutamento, defesa e exploração, enquanto a avaliação apresentada se concentra em impostos e campanha.

Eu reformularia os objetivos para acompanhar o recorte da pesquisa.

1. Identificar os requisitos de retomada e as dependências entre as mecânicas.
2. Analisar a divisão das tarefas e as decisões de integração relacionadas a esses requisitos.
3. Implementar a persistência e as regras de tempo necessárias à retomada.
4. Verificar os comportamentos definidos por meio de cenários de teste.
5. Discutir os resultados e os limites do processo adotado.

Recrutamento e exploração podem continuar como contexto e dependências do protótipo. Caso a integração completa dessas mecânicas permaneça como objetivo avaliado, será necessário apresentar evidências correspondentes.

### 2.3 Reduzir a fragmentação

Há várias subseções com um único parágrafo. A seção 3.5, por exemplo, anuncia uma reconstrução do desenvolvimento, mas explica pouco o procedimento. A conclusão está dividida em três subseções curtas. Essa divisão aumenta a quantidade de títulos e interrompe a leitura.

Eu trabalharia com a seguinte organização, preservando os cinco capítulos previstos no artigo.

| Capítulo | Organização sugerida |
| --- | --- |
| 1 Introdução | Contexto e problema, justificativa, objetivos e delimitação |
| 2 Fundamentação teórica | Desenvolvimento independente e gestão, requisitos de jogos idle, persistência e testes de tempo |
| 3 Metodologia | Desenho do estudo e fontes, organização do desenvolvimento, escolhas técnicas, protocolo de avaliação |
| 4 Resultados e discussão | Evolução do desenvolvimento, solução de retomada, resultados dos testes, discussão e ameaças à validade |
| 5 Conclusão | Texto contínuo com resposta à pergunta, contribuição do caso, limites e próximos passos |

A seção sobre economia pode ser incorporada à discussão das regras de progresso. O artigo reconhece que o balanceamento não foi avaliado, portanto esse tema deve sustentar a delimitação e as escolhas feitas, com espaço proporcional ao seu papel no estudo.

## 3 Metodologia e análise da gestão

### 3.1 Explicar como o estudo foi realizado

A seção 3.1 identifica a pesquisa como aplicada e define a unidade de análise. Também lista fontes pertinentes. Falta explicar como essas fontes foram escolhidas e interpretadas.

Eu acrescentaria o período observado, os critérios para selecionar commits e cartões, os aspectos comparados nos documentos e a forma de registrar os relatos dos autores. Indicaria quais informações foram registradas durante o desenvolvimento e quais foram reconstruídas posteriormente. Isso ajuda a avaliar a confiança na cronologia.

Para cada decisão analisada, os autores podem registrar o problema encontrado, a decisão tomada, sua justificativa, a alteração realizada e a evidência disponível. Esse procedimento deve corresponder à análise efetivamente executada. A classificação como estudo de caso ganharia sustentação com uma referência metodológica, como Runeson e Höst, que discutem desenho do estudo, encadeamento das evidências e ameaças à validade em Engenharia de Software. [Runeson e Höst](https://link.springer.com/article/10.1007/s10664-008-9102-8)

### 3.2 Transformar o relato das ferramentas em resultados de processo

Trello, Discord e Git estão descritos com clareza. Para que a gestão tenha peso na resposta à pergunta de pesquisa, eu analisaria dois ou três episódios concretos.

| Episódio disponível | Análise que falta desenvolver |
| --- | --- |
| Divisão entre Centro e Taverna | Qual dependência foi evitada pela divisão, o que cada integrante entregou e como a revisão ocorreu |
| Unificação do saldo entre Centro e Portão | Como a separação inicial dos saldos foi identificada, quais regras precisaram ser acordadas e que trabalho de integração isso gerou |
| Isolamento do salvamento e revisão dos testes | Qual problema afetava a verificação, que decisão permitiu repetir os cenários e como os resultados foram conferidos |

O exemplo Centro–Taverna mostra paralelização. A unificação Centro–Portão mostra uma dependência que exigiu coordenação. Relacionar os dois episódios permite discutir tanto os benefícios quanto os limites da divisão por cenas.

A frase de que a divisão “evitou conflitos de integração” está atribuída ao relato dos autores, o que é adequado. Ainda assim, a afirmação causal é forte. Eu a ajustaria para informar que os autores relataram uma integração sem conflitos nessa entrega e explicar como a divisão das responsabilidades pode ter contribuído. O histórico de commits, sozinho, não registra todo o esforço de coordenação.

O uso do Discord pode continuar documentado pelo relato já fornecido. Para aumentar sua precisão, basta identificar os participantes, a periodicidade e um exemplo de decisão. O artigo também deve registrar que os próprios desenvolvedores fizeram a análise retrospectiva, sujeita a memória seletiva e preferência por episódios bem-sucedidos.

### 3.3 Justificar as escolhas técnicas com critérios do projeto

A familiaridade com Unity e C# e a disponibilidade de recursos 2D são justificativas pertinentes. Os casos de Cuphead e Hollow Knight demonstram uso do motor em produções publicadas. Eu manteria esses exemplos em uma frase, concentrando a análise nos recursos usados pela equipe e em suas consequências para o desenvolvimento. [Cuphead](https://unity.com/made-with-unity/cuphead) · [Hollow Knight](https://unity.com/made-with-unity/hollow-knight)

Eu explicitaria as razões para usar relógio local, armazenamento em PlayerPrefs com JSON, estado compartilhado entre cenas e interface de relógio substituível nos testes. Para cada escolha, o texto deve indicar o requisito atendido e sua limitação. O relógio local, por exemplo, permite calcular o intervalo entre sessões, mas fica sujeito a alterações no dispositivo.

O limite de 24 horas merece uma justificativa própria. A referência a Melvor Idle serve como exemplo de regra adotada em outra versão de um jogo. Ela não determina o limite adequado ao protótipo. Se 24 horas foi uma escolha inicial de design, isso deve ser explicitado e relacionado à avaliação de balanceamento ainda pendente. A nota de atualização consultada confirma a regra histórica citada no artigo. [Atualização de Melvor Idle](https://store.steampowered.com/news/posts/?enddate=1681275384&feed=steam_community_announcements)

## 4 Resultados e força das evidências

### 4.1 Fechar a verificação pelo uso normal do jogo

O XML confirma 23 casos aprovados, com oito cenários de retomada. A descrição do relatório é coerente com o código examinado. Os cenários criam novas instâncias de GameSession e usam horários controlados no mesmo processo da Unity. Esse resultado sustenta os comportamentos verificados nessas condições. [Relatório XML](D:/TCC-Game/Docs/Evidencias/testes/RetomadaEditMode-2026-11-10.xml) · [Testes de retomada](D:/TCC-Game/Assets/Tests/EditMode/OfflineResumeTests.cs)

Como o tema central envolve o período em que o jogo ficou fechado, eu priorizaria um teste no executável antes da entrega. O registro deve conter a versão usada, o estado inicial, o encerramento normal, o intervalo real de ausência, o estado após reabrir e o saldo após a coleta. Uma nova abertura após a coleta permite conferir também a persistência do crédito.

Os testes simulados continuam apropriados para limites longos, como 24 e 48 horas. A execução real complementa a avaliação ao passar pelo carregamento das cenas, pelo ciclo de salvamento e pela interface usada pelo jogador.

### 4.2 Documentar regras e resultados esperados

A Tabela 1 apresenta valores esperados e observados. Eu acrescentaria uma ligação explícita entre requisito, cenário e resultado, preferencialmente por identificadores curtos de requisito ou uma coluna compacta.

O cálculo pode ser explicado com uma expressão simples, definindo a taxa por minuto, o intervalo elegível, o tempo já consumido do limite e o acúmulo anterior. Um exemplo independente da implementação mostraria que seis minutos a 20 unidades por minuto resultam em 120 unidades. Isso esclarece a origem dos valores esperados.

Na seção 4.4, eu também explicaria o destino da parte fracionária e a condição de reinício do limite. O código examinado preserva o restante fracionário e reinicia o tempo elegível quando há crédito positivo. Esses detalhes importam para interpretar coletas pequenas ou repetidas. [Serviço de impostos](D:/TCC-Game/Assets/Scripts/City/TaxAccrualService.cs)

### 4.3 Completar a cobertura relevante ao recorte

O conjunto relatado cobre valores de fronteira, acúmulo parcial, repetição da coleta e tempo de campanha. Eu verificaria a cobertura das seguintes situações antes de formular uma conclusão mais ampla.

- Relógio anterior ao instante salvo, seguido de retorno ao horário normal.
- Alteração do nível do mercado entre dois períodos de produção.
- Consultas repetidas da prévia, confirmando ausência de mudanças no saldo e no estado persistido.
- Coletas com frações de unidade de ouro e reinício do limite após uma coleta efetiva.

Essas situações não aparecem entre os oito cenários de retomada apresentados. Outros testes do repositório podem cobrir partes dessas regras. O primeiro passo é localizar essa cobertura e vinculá-la ao artigo, acrescentando testes somente para as lacunas relevantes.

A aprovação de 23 casos informa o resultado do conjunto selecionado. Ela não mede cobertura de código, qualidade global do jogo ou balanceamento. O artigo já limita várias dessas interpretações e deve manter a precisão ao atualizar resumo e conclusão.

### 4.4 Explicar as capturas e a recuperação dos testes

As cinco figuras estão nas seções pertinentes. As três capturas de retomada foram produzidas por um cenário que calcula estados em sessões isoladas e os apresenta diretamente na interface. Elas ilustram os estados calculados. Eu acrescentaria essa explicação ao procedimento de captura para delimitar o que as imagens demonstram sobre o fluxo do jogador. [Cenário das capturas](D:/TCC-Game/Docs/Evidencias/fontes/TccEvidenceCaptureScenario.cs)

O relato de que seis testes de migração passaram após atualização das expectativas precisa explicar a causa anterior. Eu indicaria qual expectativa estava desatualizada, por que a nova expectativa corresponde ao requisito e quais verificações dos dados foram mantidas. Atualizar a versão esperada do formato pode ser uma manutenção legítima, mas a banca precisa entender a relação entre requisito, ajuste do teste e comportamento avaliado. [Registro da execução](D:/TCC-Game/Docs/Evidencias/testes/Resultados_da_retomada_2026-11-10.md)

### 4.5 Acrescentar discussão e tornar a execução reproduzível

O capítulo 4 dedica bastante espaço a descrever a solução. Eu acrescentaria uma discussão que relacione os episódios de gestão e os resultados às fontes utilizadas. Ela pode examinar como a definição das regras de retomada orientou a divisão de tarefas, como a integração alterou o planejamento e quais dependências persistiram.

A seção de limitações deve incluir o tamanho da equipe, o caso único, a participação dos desenvolvedores na análise e a seleção dos registros, além das limitações técnicas já apresentadas. Esses fatores delimitam a aplicação das conclusões a outros projetos.

A versão testada aparece como um commit-base acompanhado de alterações. Para reprodução, eu identificaria uma revisão que contenha exatamente o código executado ou disponibilizaria o commit-base junto ao conjunto exato de alterações. Um commit posterior só serve se essa correspondência for demonstrada.

Os vínculos para XML, relatório e contrato de unificação são caminhos relativos à pasta do projeto. Para a banca acessar essas evidências, eu prepararia um pacote suplementar identificado ou um endereço estável permitido para o projeto. A descrição essencial dos resultados deve continuar compreensível no próprio artigo.

## 5 Citações e fundamentação acadêmica

As 12 entradas da bibliografia têm chamadas correspondentes no texto examinado. A presença de documentação técnica, páginas dos desenvolvedores e literatura acadêmica é compatível com o tema. O ponto a melhorar é a função de cada referência na argumentação.

Alharthi et al. sustentam a caracterização dos jogos idle. A página institucional consultada confirma a análise de 66 jogos idle e dez jogos de comparação. Politowski et al. sustentam o contexto de dificuldades de produção e gestão, e o resumo dos autores confirma a análise de 200 postmortems. Eu retomaria essas contribuições na discussão dos resultados do projeto. [Alharthi et al.](https://research.monash.edu/en/publications/playing-to-wait-a-taxonomy-of-idle-games/) · [Politowski et al.](https://arxiv.org/abs/2009.02440)

Poulsen e Wirman oferecem um estudo relacionado à colaboração. Confirmei autoria, título, ano, volume e páginas na publicação. A conferência desta revisão não alcançou a verificação integral do detalhe sobre os oito integrantes. O PDF de Carrijo et al. também apresentou falha de acesso, embora o trabalho e sua autoria estejam registrados na programação oficial do SBGames. Eu concluiria a leitura desses originais antes de fechar as paráfrases e a referência completa dos anais. [Poulsen e Wirman](https://todigra.org/index.php/todigra/article/view/2184) · [Programação do SBGames 2023](https://sbgames.org/sbgames2023/wp-content/uploads/2023/11/Sessoes-AD-2023_v5.pdf)

Pecorella é uma fonte profissional pertinente para discutir produção e custos. Sua data está correta na referência. Microsoft e Unity são fontes primárias adequadas para comportamento das APIs. A data de 11 de março de 2026 na página sobre FakeTimeProvider corresponde à última atualização da documentação. O artigo deve apresentar essa fonte como fundamento para controlar o tempo nos testes, preservando a identificação da implementação própria com IClock. [Pecorella](https://www.gamedeveloper.com/design/the-math-of-idle-games-part-i) · [Microsoft](https://learn.microsoft.com/pt-br/dotnet/core/extensions/timeprovider-testing)

Eu acrescentaria uma referência metodológica diretamente utilizada na descrição da pesquisa. Também explicitaria, em poucas linhas, como a literatura foi localizada e selecionada. A revisão apresentada pode ser tratada como narrativa. Uma revisão sistemática exigiria outro protocolo e não é necessária para resolver as fragilidades identificadas.

Na revisão bibliográfica, alguns parágrafos já explicam como o protótipo deve funcionar. Eu deslocaria esses detalhes para metodologia ou resultados e usaria o espaço para comparar os conceitos das fontes. A contribuição autoral pode aparecer na fundamentação, desde que sua relação com o que foi publicado fique clara.

## 6 Apresentação e exigências do modelo

O documento tem 15 páginas na renderização conferida no Word e está dentro do intervalo institucional de 15 a 20. A versão atual apresenta A4, margens compatíveis com o modelo, corpo em Times New Roman 12, figuras numeradas, fontes abaixo das imagens e referências ao final. A quantidade de páginas é uma condição formal, e a próxima revisão deve aproveitar o espaço disponível para aprofundar método e discussão. [Modelo institucional](<C:/Users/marcos/Desktop/Modelo de TCC - 2026.docx>)

Eu faria os seguintes ajustes de apresentação.

- Preencher os três e-mails institucionais.
- Padronizar o separador das legendas, pois o texto normativo do modelo pede travessão e a versão produzida usa hífen, como alguns exemplos do próprio arquivo.
- Melhorar a leitura dos cartões na Figura 1. A captura ocupa espaço com o fundo do quadro, e os textos ficam pequenos na página. Um enquadramento que priorize as listas e mantenha o contexto pode ajudar.
- Manter as capturas junto às explicações, como solicitado pelos autores, e revisar o tamanho das imagens após a reorganização dos capítulos.
- Uniformizar a apresentação de termos estrangeiros e identificadores técnicos.

A revisão formal deve seguir a orientação institucional vigente. O arquivo de modelo possui diferenças entre algumas instruções e seus exemplos, por isso ajustes de pontuação e convenções bibliográficas devem ser tratados como acabamento, sem desviar a atenção das lacunas de método e discussão.

## 7 Sequência recomendada para a próxima revisão

1. Fechar a pergunta e os objetivos com foco no processo de desenvolvimento da retomada.
2. Escrever o procedimento de análise dos registros e documentar dois ou três episódios de gestão.
3. Definir a versão reproduzível e realizar a verificação pelo executável.
4. Revisar a explicação dos cálculos, a relação entre requisitos e testes e o alcance das capturas.
5. Reorganizar o capítulo 4 para incluir discussão com a literatura e ameaças à validade.
6. Consolidar subseções curtas, atualizar resumo e conclusão e concluir a conferência bibliográfica.
7. Ajustar a diagramação após a revisão de conteúdo.

Com essas mudanças, o artigo poderá demonstrar com mais clareza o que a equipe aprendeu sobre organizar, implementar e verificar a progressão durante a ausência. A evidência técnica existente oferece uma base para essa análise. O próximo esforço deve concentrar-se em explicar as decisões e sustentar as conclusões sobre o processo.
