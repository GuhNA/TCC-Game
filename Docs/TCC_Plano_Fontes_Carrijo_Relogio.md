# Plano para revisão das citações sobre GDD e testes de tempo

Este plano considera a versão mesclada do artigo e apresenta alterações propostas para as seções 2.2 e 2.5. Os trechos devem ser conferidos antes da aplicação.

## 1. Seção 2.2 — Carrijo e colaboradores

### Situação encontrada

A referência a Carrijo et al. já consta na bibliografia do artigo, com título, evento, ano, páginas e endereço para o texto. Ela precisa ser conferida e completada com o DOI, caso corresponda à publicação usada pelos autores.

A programação oficial do SBGames 2023 registra um trabalho com o mesmo título e os autores Gabriel F. Carrijo, Vitor Yudi G. Kawahara, Marcos Wagner S. Ribeiro e Ana Carolina G. Inocencio. Uma publicação posterior identifica esse trabalho pelo DOI informado. O resumo disponibilizado na página de uma das autoras descreve uma revisão de literatura e relaciona as características do GDD ao gênero do jogo. [Programação oficial do SBGames](https://sbgames.org/sbgames2023/wp-content/uploads/2023/11/Sessoes-AD-2023_v5.pdf) · [Referência bibliográfica em publicação posterior](https://www.revista.faculdadeitop.edu.br/index.php/revista/article/view/1003) · [Resumo apresentado na página da autora](https://www.researchgate.net/scientific-contributions/Ana-Carolina-Gondim-Inocencio-2230167589)

O DOI e o PDF na biblioteca SBC não abriram nesta consulta. A identificação é compatível com o título, os autores e os dados já presentes no artigo, mas a comparação com o texto integral e a confirmação de que esta foi a publicação efetivamente utilizada continuam pendentes.

### Alteração proposta no texto

**Trecho atual**

> Carrijo et al. (2023) discutem como o gênero e as necessidades do projeto influenciam a organização desse documento.

**Trecho proposto**

> Na revisão de literatura de Carrijo et al. (2023), os autores discutem características dos documentos de design de jogos e sua relação com o gênero do jogo.

A proposta se limita ao assunto e à relação com o gênero apresentados no resumo localizado. Ela pode ser conferida contra o artigo integral antes de entrar no TCC. A discussão sobre como a equipe deste projeto usou proposta, cartões e documentos técnicos continua como descrição do próprio projeto, nas frases seguintes.

### Alteração proposta na bibliografia

Manter a entrada de Carrijo que já existe e acrescentar o DOI ao final, sem criar outra referência:

> CARRIJO, Gabriel F. et al. Uma revisão sistemática de literatura acerca de Game Design Documents, uma proposta para a criação de um roteiro de jogo otimizado. In: SIMPÓSIO BRASILEIRO DE JOGOS E ENTRETENIMENTO DIGITAL, 22., 2023, Rio Grande. *Anais Estendidos [...]*. Porto Alegre: Sociedade Brasileira de Computação, 2023. p. 43–53. DOI: 10.5753/sbgames_estendido.2023.234022.

O endereço da SBC e a data de acesso que já estão no documento também podem ser mantidos. Antes de fechar a referência, conferir no original os nomes, o título dos anais e as páginas.

### Verificações antes da aplicação

1. Confirmar com os autores se essa é a obra consultada.
2. Acessar o texto integral pela biblioteca institucional, pelos autores ou pela SBC e conferir a paráfrase.
3. Conferir o DOI e os demais dados bibliográficos no registro da publicação.
4. Atualizar a única entrada de Carrijo existente e verificar se a chamada autor-data permanece correspondente.

## 2. Seção 2.5 — Referência ao controle do tempo

### Situação encontrada

A documentação da Microsoft apresenta FakeTimeProvider como uma forma de controlar o tempo em testes e exemplifica o avanço por um intervalo sem esperar o relógio real. No projeto, a interface IClock expõe o horário usado pelos serviços. Nos cenários de retomada examinados, a classe FixedClock implementa essa interface e fornece horários escolhidos ao criar cada sessão. [Documentação da Microsoft](https://learn.microsoft.com/pt-br/dotnet/core/extensions/timeprovider-testing) · [Interface IClock](D:/TCC-Game/Assets/Scripts/Core/Time/IClock.cs) · [Cenários de retomada](D:/TCC-Game/Assets/Tests/EditMode/OfflineResumeTests.cs)

O trecho atual apresenta a documentação como exemplo da técnica. A redação proposta explicita o instrumento da Microsoft e a implementação utilizada no projeto.

### Alteração proposta no texto

**Trecho atual**

> Para reproduzir situações como essa, o relógio dos testes pode avançar por um intervalo conhecido, sem que a equipe espere horas entre as execuções. A documentação da Microsoft (2026) exemplifica essa técnica.

**Trecho proposto**

> Para testar regras que dependem do tempo, a documentação da Microsoft (2026) apresenta o FakeTimeProvider, que permite controlar o horário sem esperar o intervalo real. Neste projeto, os testes de retomada usam a interface IClock e uma implementação FixedClock para fornecer horários definidos às sessões testadas.

Essa formulação descreve os cenários examinados com precisão. Neles, sessões distintas recebem instantes escolhidos, em vez de se afirmar que o mesmo objeto FixedClock avança durante o teste.

### Conferência da metodologia e da referência

A seção 3.4 já descreve o uso de IClock e de um relógio controlado. Após a alteração da seção 2.5, reler os dois trechos para que a fundamentação apresente a técnica e a metodologia explique sua aplicação no projeto.

A entrada da Microsoft já consta nas referências, com título, endereço, data de atualização e data de acesso. Manter essa entrada e conferir, ao final, a correspondência com a citação “Microsoft (2026)”.

## 3. Ordem de execução

1. Confirmar com os autores a publicação de Carrijo utilizada e buscar seu texto integral.
2. Conferir a paráfrase e os dados da referência de Carrijo.
3. Revisar a frase da seção 2.5 conforme a implementação examinada.
4. Relê-la junto da seção 3.4 para evitar repetição.
5. Conferir citações e referências e verificar a paginação do Word.
