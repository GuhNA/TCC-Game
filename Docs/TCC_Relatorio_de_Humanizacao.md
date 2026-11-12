# Relatório de revisão de linguagem

Este relatório analisa a versão mesclada do artigo e apresenta sugestões para deixar a escrita mais próxima da forma como os autores explicariam o projeto. Os exemplos preservam o sentido dos trechos analisados. As propostas devem ser conferidas por Marcos e Gustavo, que podem acrescentar detalhes da experiência de desenvolvimento.

## Direcionamento geral

O artigo já apresenta situações concretas, como a divisão entre Centro e Taverna e a unificação do saldo. A escrita fica mais convincente quando essas situações aparecem ligadas às decisões tomadas pela equipe. Em outras passagens, o texto recorre a expressões amplas, muda de assunto com pouca preparação ou repete a mesma ideia em seções diferentes.

Para a próxima revisão, proponho trabalhar parágrafo por parágrafo com quatro perguntas.

1. O que a equipe precisava fazer nesse momento?
2. Qual dificuldade encontrou?
3. Que decisão tomou e por quê?
4. Qual registro ou resultado permite contar esse episódio?

Nem todo parágrafo precisa responder às quatro perguntas. Elas ajudam a reconhecer trechos que poderiam ser escritos de forma mais específica.

## O que mudar ao longo do artigo

### 1. Aproximar o texto da experiência dos autores

Na metodologia e na discussão, cabem frases que mostrem quem realizou as ações. O artigo pode usar “a equipe” ou a primeira pessoa do plural, desde que a escolha seja coerente e aceita pelo orientador. A revisão bibliográfica deve continuar distinguindo as ideias dos autores citados das decisões do projeto.

Na seção 3.3, por exemplo, a frase “A divisão também orientava o uso do Git” abre um parágrafo que depois explica a divisão entre Centro e Taverna. Uma abertura mais direta seria:

> Em uma das reuniões, dividimos o protótipo em duas frentes. Marcos ficou responsável pelo Centro, e Gustavo, pela Taverna. Como cada um trabalhava em uma cena diferente, foi possível avançar em paralelo e revisar as alterações antes da integração.

Os identificadores dos commits podem continuar em seguida, como apoio ao histórico. Antes de adotar a primeira pessoa, convém verificar se ela corresponde à preferência dos dois autores e do orientador.

### 2. Explicar os efeitos de decisões técnicas

Na seção 3.4, a frase “Essa escolha liga a arquitetura ao procedimento de avaliação” encerra a explicação sobre a interface de relógio, mas não diz diretamente o que a escolha permitiu fazer.

Uma alternativa é:

> Com a interface `IClock`, os testes puderam fornecer horários conhecidos. Assim, a equipe conseguiu conferir o que acontecia após períodos diferentes de ausência sem precisar esperar cada intervalo passar.

O benefício da alteração está em explicar a relação entre a ferramenta e seu uso no projeto. Os testes e seus limites continuam descritos na seção de avaliação.

### 3. Variar o ritmo das frases com naturalidade

O texto usa várias sequências de frases curtas com a mesma estrutura, sobretudo nas descrições do jogo. Frases curtas são úteis para apresentar regras. Quando duas delas tratam da mesma decisão, podem ser reunidas. Quando uma frase reúne muitos sistemas, pode ser dividida.

Na seção 4.2, o trecho sobre a Taverna pode ser revisto assim:

> Na Taverna, o jogador usa parte do ouro para contratar aventureiros e escolhe até três para uma expedição. Depois de conquistar uma região, pode enviar exploradores, definir guardas e transportar os materiais obtidos para a vila. Esses recursos são usados em algumas melhorias do Centro.

Esse ajuste mantém a sequência da ação e reduz repetições de “o jogador”. A informação sobre o mapa deve continuar no parágrafo se for necessária para explicar a figura ou o percurso.

### 4. Criar transições entre os assuntos

Há mudanças rápidas entre a descrição do jogo, a gestão do projeto e os detalhes técnicos. Uma frase de ligação ajuda quando mostra por que o próximo assunto aparece.

Entre a seção sobre trabalho em cenas e a explicação da sessão compartilhada, a transição pode ser:

> Trabalhar em cenas diferentes facilitou a divisão das tarefas. Quando chegou o momento de reuni-las, porém, foi preciso garantir que o ouro e os demais dados da partida fossem os mesmos em cada área.

Essa passagem prepara a explicação da arquitetura com um problema já apresentado na metodologia.

### 5. Dar espaço aos exemplos específicos

O parágrafo de balanceamento na seção 2.4 afirma que essa foi a atividade mais trabalhosa. A afirmação fica mais pessoal e informativa se os autores explicarem uma decisão real. Há perguntas que podem orientar a escrita:

- Qual custo, intervalo ou força de onda precisou ser ajustado?
- O que aconteceu no jogo antes da mudança?
- Como a equipe decidiu qual valor usar?

Essas informações precisam vir dos autores ou dos registros do projeto. Se não houver registro suficiente, a frase pode continuar como avaliação da própria equipe, sem atribuir um resultado preciso ao ajuste.

### 6. Reduzir repetições entre introdução, metodologia e conclusão

O texto explica em mais de uma seção que a campanha preserva o tempo de preparação e que a renda considera a ausência. As duas regras são centrais, então podem aparecer ao longo do artigo. A função de cada menção pode ser mais clara.

- Na introdução, apresentar a dificuldade que levou à escolha.
- Na metodologia, explicar como a equipe definiu e implementou as regras.
- Nos resultados, apresentar o que foi observado nos cenários.
- Na conclusão, dizer o que a equipe aprendeu com a decisão.

Essa organização permite encurtar repetições sem apagar o raciocínio.

### 7. Tornar o resumo mais claro

O resumo mesclado contém a expressão “dados da gameplay”, que pode ser substituída por “dados da partida”. Também alterna a lista das ações do jogador com a organização das cenas e os testes em um único bloco.

Uma revisão pode seguir a ordem do trabalho: proposta do jogo, organização da equipe, principal dificuldade de integração e resultado observado. O resumo deve continuar em um parágrafo, com até 200 palavras conforme o modelo institucional.

### 8. Corrigir frases que já estão difíceis de entender

Alguns trechos da mesclagem pedem correção antes de uma revisão de estilo:

- **Seção 1.1.** “A defesa acrescenta uma condição ao ciclo, na preparação para cada onda depende...” pode ser escrita como “Na defesa, o jogador precisa tomar decisões durante a partida para preparar cada onda.”
- **Seção 1.5.** “Os anúncios visuais por meio de pop-ups permitem a noção do jogador a respeito de eventos referentes a outras cenas” pode ser “Os avisos na tela informam o jogador sobre eventos que acontecem em outras cenas.”
- **Seção 3.4.** “Isso permite voltar ao jogo tendo os dados salvos ao realizar o carregamento do jogo” pode ser “Ao abrir o jogo novamente, o jogador recupera os dados salvos da partida.”

Antes de aplicar essas propostas, vale conferir se cada redação corresponde exatamente ao comportamento que os autores pretendem descrever.

### 9. Uniformizar termos

O documento alterna “indie” e “independente”, além de usar “offline”, “singleplayer” e “gameplay”. Esses termos podem ser mantidos quando ajudarem os autores a explicar o projeto. Uma escolha consistente tende a facilitar a leitura. Por exemplo, após apresentar “jogo independente”, o texto pode usar “jogo” quando o contexto já estiver claro. “Partida local para um jogador” pode explicar o escopo na primeira ocorrência.

Também convém usar “tempo de campanha” e “tempo de ausência” de forma consistente, pois os dois intervalos têm funções diferentes no protótipo.

## Sobre pequenos erros de escrita

Erros de acentuação e concordância podem ocorrer em qualquer rascunho. Inserir erros deliberadamente torna o texto menos cuidadoso e não oferece uma forma confiável de prever o resultado de um detector. Em um TCC, recomendo que a versão entregue passe por correção ortográfica.

Para avaliar o efeito da escolha, dois exemplos comuns seriam escrever “tambem” em vez de “também” ou “as cena foram integradas” em vez de “as cenas foram integradas”. São desvios visíveis que não acrescentam informação à descrição do projeto. O relatório registra os exemplos para discussão, e o artigo mesclado segue com sua redação atual.

## Ordem sugerida de revisão

1. Os autores leem resumo e introdução e ajustam a descrição do jogo à forma como explicariam a proposta.
2. Na metodologia, acrescentam fatos concretos que ambos reconheçam sobre reuniões, divisão e integração.
3. No capítulo de resultados, conferem se as decisões de design e os comportamentos descritos correspondem ao protótipo.
4. Na discussão e na conclusão, retiram repetições e deixam explícito o que aprenderam com o desenvolvimento.
5. Ao final, revisam citações, termos, ortografia e formatação.

As sugestões deste relatório não garantem um resultado específico em plataformas de detecção. Elas procuram tornar o texto mais claro, específico ao projeto e fácil de sustentar durante a apresentação.
