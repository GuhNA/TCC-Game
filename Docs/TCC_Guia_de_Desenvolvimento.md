# Guia de desenvolvimento do TCC

## Identificação do trabalho

**Título mantido:** Processos de desenvolvimento de jogos indies

**Curso:** Engenharia da Computação

**Autores:** Gustavo Henrique Nascimento de Almeida e Marcos Vinicius Barbieri Alves

**Orientador:** Prof. Dr. Maurício Acconcia Dias

**Natureza do documento:** artigo de Trabalho de Conclusão de Curso sobre o processo de concepção, desenvolvimento, integração e validação de um jogo digital 2D independente.

## 1. Finalidade deste guia

Este arquivo consolida os requisitos do modelo institucional, as ideias presentes no projeto inicial e as evidências encontradas no repositório do jogo. Ele deve orientar a redação do TCC, ajudar a separar fatos de propostas e manter coerência entre os objetivos declarados, a implementação existente e os resultados apresentados.

Use a [Introdução do artigo](TCC_Estrutura_Final.md#1-introdução) como referência para os próximos capítulos. O estudo acompanha o trabalho de uma equipe pequena ao definir, implementar e testar o progresso que pode ser acumulado com o jogo fechado.

## 2. Fontes internas que orientarão a redação

### 2.1 Documentos acadêmicos

- `C:\Users\marcos\Desktop\Modelo de TCC - 2026.docx`: autoridade para estrutura, extensão e formatação do artigo.
- `C:\Users\marcos\Desktop\Processos de desenvolvimento de jogos indies.docx`: fonte do título, da proposta inicial, da justificativa, dos objetivos, das ferramentas planejadas e dos resultados esperados.

### 2.2 Documentação técnica do projeto

- `Docs/PlanejamentoDefesaOndas.md`: planejamento e implementação da campanha, ondas, guarnições e batalhas.
- `Docs/PlanoDeTestesDefesaOndas.md`: estratégia de testes automatizados e manuais.
- `Docs/UnificationBehaviorContract.md`: decisões arquiteturais e unificação do estado do jogo.
- `Docs/ExplorationMapSetup.md`: funcionamento do mapa e das expedições.
- `Docs/ExplorationCombatSetup.md`: funcionamento do combate e da progressão de exploração.
- `Docs/PlanoMigracaoUIEditavel.md`: evolução das interfaces para componentes editáveis.
- `Docs/PlanoCorrecaoFalhasAutomatizadas.md`: defeitos identificados, correções e validações.
- `Design/`: registros de direção visual, interface e construção dos ambientes.

### 2.3 Implementação

As afirmações técnicas deverão ser confirmadas nos arquivos de código, cenas, assets, configurações e resultados de testes. Os principais pontos de entrada são:

- `Assets/Scripts/Core/GameSession.cs`;
- `Assets/Scripts/Core/Persistence/`;
- `Assets/Scripts/City/`;
- `Assets/Scripts/Campaign/`;
- `Assets/Scripts/Navigation/`;
- `Assets/Scripts/Exploration/`;
- `Assets/Scripts/Portao/`;
- `Assets/Scripts/Aventureiro/`;
- `Assets/Tests/`;
- `ProjectSettings/EditorBuildSettings.asset`;
- `ProjectSettings/ProjectVersion.txt`;
- `Packages/manifest.json`.

## 3. Requisitos obrigatórios do modelo institucional

### 3.1 Estrutura geral

O documento deve ser apresentado como artigo identificado e conter, no mínimo:

1. título;
2. autores e identificação institucional;
3. resumo;
4. palavras-chave;
5. introdução;
6. revisão bibliográfica;
7. metodologia;
8. estudo de caso ou resultados;
9. conclusão;
10. referências.

Anexos são opcionais e devem ser usados somente se forem realmente necessários.

### 3.2 Extensão

- Mínimo de 15 páginas.
- Máximo de 20 páginas.
- Resumo, referências e anexos entram nessa contagem.
- A primeira versão pode ser mais extensa para facilitar a organização das ideias.
- A redução para o limite final deverá ocorrer depois que todos os capítulos estiverem redigidos.

### 3.3 Página e margens

- Papel A4.
- Margem esquerda: 3 cm.
- Margem superior: 3 cm.
- Margem direita: 2 cm.
- Margem inferior: 2 cm.

### 3.4 Corpo do texto

- Fonte Times New Roman.
- Tamanho 12.
- Alinhamento justificado.
- Espaçamento entre linhas de 1,5.
- Espaçamento de 6 pontos depois de cada parágrafo.
- O texto deve começar imediatamente depois do título ou subtítulo.
- Notas de rodapé não devem ser utilizadas.

### 3.5 Títulos e subtítulos

- Alinhados à esquerda.
- Numerados com algarismos arábicos.
- Fonte Times New Roman, tamanho 12 e negrito.
- Somente a primeira inicial deve ser maiúscula, salvo nomes próprios e siglas.
- Não devem terminar com ponto final.
- A numeração deve seguir a hierarquia `1`, `1.1`, `1.1.1` quando necessário.

### 3.6 Resumo e palavras-chave

- O resumo deve possuir no máximo 200 palavras.
- Deve ser escrito em um único parágrafo.
- Deve apresentar objetivo, metodologia, resultados e conclusão.
- Deve ser escrito apenas quando os demais capítulos estiverem estabilizados.
- Devem ser informadas três palavras-chave.
- As palavras-chave devem aparecer em letras minúsculas e separadas por vírgulas.

Palavras-chave provisórias:

`desenvolvimento de jogos, engenharia de software, jogos independentes`

### 3.7 Figuras, tabelas e quadros

- Toda figura, tabela ou quadro deve ser mencionada e explicada no texto.
- O título deve aparecer acima do elemento.
- O título deve apresentar o tipo, o número e uma descrição objetiva.
- A fonte deve aparecer abaixo do elemento.
- Elemento, título e fonte devem permanecer visualmente associados.
- Imagens devem ser inseridas alinhadas ao texto, evitando objetos flutuantes.
- Tabelas devem ser reservadas prioritariamente para dados numéricos.
- Quadros devem ser usados para comparações e informações qualitativas.
- Não devem ser incluídas capturas de tela apenas para ocupar espaço.

### 3.8 Referências

- Devem aparecer em ordem alfabética.
- Não devem ser numeradas.
- Devem conter somente obras citadas no texto.
- Toda obra citada no texto deve aparecer nas referências.
- A seção deve usar fonte Times New Roman, tamanho 10.
- As referências devem ser alinhadas à margem esquerda e separadas entre si por 6 pontos.

## 4. Direcionamento acadêmico

### 4.1 Tema

Processo de desenvolvimento de um jogo *idle* 2D independente, com foco no tratamento do tempo de ausência do jogador.

### 4.2 Recorte do trabalho

O artigo acompanha a definição do escopo, a ordem das tarefas e a verificação das regras de progresso ao retomar a partida. A arrecadação de impostos é o caso principal. A defesa por ondas mostra como o jogo usa o tempo de execução da campanha. Construções, recrutamento e exploração entram onde ajudam a explicar a economia ou a integração entre sistemas.

### 4.3 Problema de pesquisa

Como uma equipe pequena pode planejar, implementar e testar o progresso durante a ausência do jogador em um jogo *idle* independente?

### 4.4 Questões para orientar os capítulos

1. **Revisão bibliográfica.** Quais conceitos ajudam a distinguir a progressão com pouca interação da progressão com o jogo fechado? Como uma equipe pequena pode documentar e priorizar essas regras?
2. **Metodologia.** Como a equipe escolheu os sistemas que consideram o tempo de ausência? Que dados, tarefas e cenários de teste foram definidos?
3. **Estudo de caso e resultados.** Como o protótipo calcula o progresso ao retomar a partida? O que os testes executados mostram sobre o acúmulo de impostos, seu limite e o relógio da campanha?
4. **Conclusão.** O que o estudo permite responder sobre planejar, implementar e testar essa mecânica? Quais pontos ainda precisam de verificação?

### 4.5 Objetivo geral

Desenvolver um protótipo de jogo *idle* 2D independente e analisar como a equipe planejou, implementou e testou o progresso associado ao tempo em que o jogo ficou fechado.

### 4.6 Objetivos específicos

1. Documentar os requisitos e as regras das principais mecânicas do jogo.
2. Integrar a economia da vila ao recrutamento, à defesa por ondas e à exploração.
3. Definir quais sistemas consideram o tempo de ausência e salvar os dados necessários para calcular o progresso ao retomar a partida.
4. Verificar as regras com testes automatizados, incluindo o cálculo do progresso após diferentes períodos de ausência e a aplicação do limite de acúmulo.
5. Analisar as decisões de desenvolvimento e as limitações da versão avaliada.

### 4.7 Justificativa

O protótipo permite estudar duas decisões sobre o tempo. Os impostos são calculados com base no tempo decorrido, até 24 horas de acúmulo entre coletas. As ondas seguem o tempo de execução da campanha. A documentação pode mostrar como a equipe transformou essas decisões em requisitos, tarefas, dados salvos e testes.

### 4.8 Escopo

O objeto de estudo é o protótipo existente no repositório `TCC-Game`. A avaliação concentra-se no cálculo de impostos na retomada, na preservação do progresso salvo e na integração com as demais mecânicas. A lista abaixo serve de inventário para selecionar exemplos ao longo da redação.

Escopo funcional observado:

- jogo digital 2D desenvolvido na Unity;
- gerenciamento de ouro e recursos;
- construções e melhorias;
- tributação e produção econômica;
- recrutamento de guerreiros, arqueiros e magos;
- contratação e gestão de aventureiros;
- inventário persistente;
- campanha de defesa com quatro portões;
- geração de ondas e alertas de ameaça;
- distribuição e deslocamento de guarnições;
- resolução automática de batalhas;
- baixas, dano estrutural, espólio e relatórios;
- navegação entre Centro, Portão, Taverna e Exploração;
- expedições com grupos de até três aventureiros;
- combate de exploração em turnos;
- captura e defesa de regiões;
- coleta de recursos por exploradores;
- persistência versionada e migração de saves;
- testes EditMode, PlayMode e testes visuais.

### 4.9 Delimitações

O trabalho não deverá prometer ou assumir como concluídos:

- lançamento comercial;
- balanceamento definitivo;
- multiplayer;
- telemetria em produção;
- testes de usabilidade com amostra formal, enquanto não forem realizados;
- validação estatística de engajamento;
- conteúdo final de todas as regiões;
- acabamento artístico equivalente a um produto comercial completo;
- compatibilidade com múltiplas plataformas, caso não tenha sido comprovada.

## 5. Caracterização metodológica sugerida

### 5.1 Natureza

Pesquisa aplicada, pois utiliza conhecimentos técnicos para desenvolver e avaliar um artefato computacional.

### 5.2 Objetivos

Pesquisa exploratória e descritiva:

- exploratória por investigar soluções de arquitetura, game design e integração;
- descritiva por registrar o processo, os componentes, os testes e os resultados obtidos.

### 5.3 Procedimento

Desenvolvimento tecnológico apresentado como estudo de caso do projeto `TCC-Game`.

### 5.4 Abordagem

Predominantemente qualitativa, com análise das decisões de gestão e implementação. A evidência quantitativa principal vem dos testes. Para cada cenário, registrar o estado inicial, o intervalo de tempo, o resultado esperado, o resultado obtido e a versão avaliada.

### 5.5 Processo de desenvolvimento

Reconstruir a ordem de implementação com os registros disponíveis e o relato da equipe. Documentar como foram definidos o escopo, as prioridades e as dependências entre economia, salvamento e testes. O projeto inicial prevê Kanban, Trello, Discord e GitHub. Confirmar como cada ferramenta foi usada antes de descrevê-la na metodologia.

### 5.6 Uso de inteligência artificial

O uso de IA generativa deverá ser descrito com transparência, incluindo as atividades em que foi empregada, a revisão humana realizada e os limites de confiabilidade. A ferramenta não deve ser apresentada como autora, fonte científica ou responsável por decisões finais.

Também deverão ser verificadas as orientações da instituição sobre declaração de uso de IA e as condições de licenciamento dos recursos visuais utilizados.

## 6. Visão técnica atual do projeto

### 6.1 Ambiente

- Unity 6000.3.11f1.
- Linguagem C#.
- Universal Render Pipeline.
- Novo Input System.
- Unity Test Framework.
- Projeto organizado em assemblies de runtime, EditMode, PlayMode e testes visuais.

### 6.2 Cenas principais habilitadas

- `Centro`;
- `Portão`;
- `Taverna`;
- `ExplorationCombat`.

### 6.3 Arquitetura

O projeto utiliza `GameSession` como ponto de composição dos serviços. O estado persistente é carregado pelo repositório e compartilhado pelos serviços de economia, construções, recrutamento, inventário, impostos, campanha, tropas, batalhas, ameaças, navegação e exploração.

Direção recomendada para a explicação:

```text
Cenas e componentes de apresentação
                ↓
            GameSession
                ↓
  Serviços de domínio especializados
                ↓
       Estado persistente comum
                ↓
       Repositório de salvamento
```

Essa arquitetura deverá ser apresentada como resultado da evolução do projeto, demonstrando os problemas que motivaram a unificação do estado e as vantagens observadas depois da mudança.

### 6.4 Persistência

Para explicar a retomada, identificar os dados que o jogo guarda entre sessões, como o instante do último cálculo, os impostos acumulados, o tempo de acúmulo já utilizado e o tempo de execução da campanha. Conferir como esses dados são lidos e atualizados antes de descrever o cálculo no artigo.

O modelo atual de save está na versão 11. O artigo poderá usar essa evolução como evidência de manutenção incremental, desde que explique:

- por que o versionamento foi necessário;
- quais dados são persistidos;
- como migrações preservam estados anteriores;
- como entradas inválidas são normalizadas;
- quais testes verificam a robustez do processo.

### 6.5 Dimensão aproximada do repositório

Levantamento realizado em 10 de novembro de 2026:

- 115 scripts em `Assets/Scripts`;
- aproximadamente 19.453 linhas nos scripts de runtime;
- 41 arquivos de teste;
- 20 arquivos de teste EditMode;
- 8 arquivos de teste PlayMode;
- 13 arquivos ligados ao framework e aos cenários de teste visual;
- 54 prefabs;
- 149 assets configuráveis;
- 176 imagens PNG;
- 9 cenas Unity no repositório, das quais quatro estão habilitadas para build.

Essas métricas são apenas uma fotografia do estado atual. Devem ser recalculadas antes da entrega final e não devem ser usadas isoladamente como prova de qualidade.

## 7. Sistemas que devem aparecer no estudo de caso

Usar impostos, relógio da campanha e salvamento para explicar a solução técnica. O restante do inventário ajuda a descrever o ciclo do jogo e as dependências relevantes para os testes.

### 7.1 Economia e cidade

- Explicar como o nível do mercado define a produção, como o intervalo desde o último cálculo entra na conta e como o limite de 24 horas de acúmulo se aplica entre coletas.
- Distinguir o valor disponível para recolher do ouro já creditado ao jogador.
- Mostrar como o ouro recolhido é usado nas construções, no recrutamento e na preparação da defesa.

### 7.2 Recrutamento e inventário

- estoque de unidades;
- preços e validação das compras;
- guerreiros, arqueiros e magos;
- aventureiros como instâncias persistentes;
- itens e recursos;
- prevenção de duplicidade e inconsistências.

### 7.3 Campanha de defesa

- relógio estratégico vinculado ao tempo de execução da campanha;
- quatro portões cardinais;
- geração determinística de ondas;
- escalonamento de ameaça;
- avisos progressivos;
- distribuição e deslocamento de tropas;
- prognóstico de batalha;
- combate automático;
- baixas, dano ao portão e espólio;
- vitória, derrota e relatórios.

### 7.4 Navegação e interação

- transição entre cenas;
- preservação da localização;
- interação centralizada;
- uso de NPCs para executar ações;
- separação entre informação no HUD e decisões por diálogo ou painéis.

### 7.5 Exploração

- desbloqueio progressivo de regiões;
- seleção de grupos;
- deslocamento e retorno;
- combate em turnos;
- recompensas;
- guarda e contra-ataques;
- exploradores e coleta passiva;
- integração de recursos com a cidade.

### 7.6 Interface e direção visual

- evolução dos protótipos;
- HUD medieval em pixel art;
- migração para interfaces editáveis;
- clareza dos alertas;
- consistência visual entre cenas;
- legibilidade, feedback e prevenção de bloqueios de interação.

### 7.7 Testes e garantia de qualidade

Usar cenários reproduzíveis, com relógio controlado quando possível. A metodologia descreve como verificar cada caso. O estudo de caso registra os testes executados e os resultados obtidos.

- Retomar sem intervalo relevante e após uma ausência curta.
- Verificar o cálculo ao atingir e ao ultrapassar o limite de 24 horas de acúmulo.
- Carregar uma partida com parte do limite já utilizada.
- Recolher os impostos e verificar o saldo, o novo ciclo de acúmulo e o comportamento em uma segunda retomada.
- Verificar se o intervalo com o jogo fechado é incorporado ao cálculo de impostos e se o relógio da campanha retoma do tempo salvo.
- Usar testes de persistência, migração e integração para complementar os cenários de retomada.

## 8. Resultados que podem ser demonstrados

Relacionar o requisito ao registro da decisão, à implementação e ao teste correspondente. Priorizar o fluxo de retomada da partida, um quadro com as regras dos sistemas que usam tempo e uma tabela com os resultados dos testes. Para explicar a gestão, usar exemplos documentados de priorização, mudança de escopo ou correção de defeitos.

Não é necessário colocar grandes trechos de código no artigo. Quando um algoritmo for importante, utilizar pseudocódigo, fluxograma, equação ou um fragmento curto acompanhado de explicação.

## 9. Pendências e limitações já identificadas

### 9.1 GDD

O documento inicial prevê um Game Design Document, mas não foi localizado um GDD consolidado. Existem diversos documentos de planejamento que podem ser reorganizados para formar esse artefato.

### 9.2 Balanceamento

Parte dos valores de economia, ondas, tropas, recompensas e exploração ainda possui caráter inicial. O texto deverá distinguir valores implementados de valores validados por testes extensos.

### 9.3 Conteúdo da exploração

A estrutura técnica está implementada, mas alguns encontros, inimigos, recompensas e elementos visuais ainda dependem de conteúdo e calibração.

### 9.4 Testes de migração

Uma execução de EditMode de outubro de 2026 registrou seis falhas em testes que esperavam uma versão anterior do save. Como o projeto evoluiu até a versão 11, os testes e os resultados deverão ser atualizados e executados novamente antes de compor a tabela final de validação.

### 9.5 Avaliação com usuários

Não foi identificada uma avaliação formal com jogadores. Se ela não for realizada, o artigo não deverá afirmar que o jogo é comprovadamente divertido, intuitivo ou engajante. Poderá afirmar apenas que as mecânicas e interfaces foram projetadas com essas finalidades.

## 10. Referencial teórico a pesquisar

### 10.1 Desenvolvimento independente

Buscar fontes acadêmicas sobre:

- definição e características de jogos independentes;
- equipes pequenas e restrições de recursos;
- escopo e sustentabilidade de projetos;
- produção de jogos no contexto brasileiro.

### 10.2 Game design

- Game Design Document;
- core loop;
- mecânicas, dinâmicas e experiência;
- progressão e balanceamento;
- feedback ao jogador.

### 10.3 Jogos idle e incrementais

- Progressão incremental e automação durante uma sessão.
- Regras de progresso com o jogo fechado e cálculo na retomada.
- Acúmulo de recursos, limites e relação com o balanceamento da economia.

### 10.4 Engenharia de software para jogos

- Tempo de execução e tempo decorrido entre sessões.
- Persistência dos dados necessários à retomada e cálculo a partir de registros de tempo.
- Separação entre as regras de cálculo e a interface.

### 10.5 Gestão de projeto

- Kanban;
- desenvolvimento incremental;
- controle de versão;
- documentação técnica;
- gestão de escopo.

### 10.6 Testes de jogos

- Testes unitários e de integração com um relógio controlável.
- Verificação de limites, carregamento da partida e repetição de operações.
- Registro dos cenários executados e das lacunas de cobertura.

## 11. Figuras e quadros recomendados

Para controlar o limite de páginas, priorizar elementos que sintetizem várias informações.

### 11.1 Figuras prioritárias

1. Fluxo da retomada, do carregamento do save ao cálculo dos impostos.
2. Ciclo de gestão e defesa, se ajudar a explicar como o jogador usa os recursos ao voltar.

### 11.2 Quadros prioritários

1. Sistemas que usam o tempo, regra aplicada e dados salvos.
2. Requisitos, decisões de implementação e evidências de atendimento.

### 11.3 Tabelas prioritárias

1. Resultados dos testes de retomada, com intervalo simulado, valor esperado e valor obtido.

## 12. Distribuição inicial de páginas

| Parte | Extensão inicial | Meta após revisão |
|---|---:|---:|
| Identificação, resumo e palavras-chave | 1 | 1 |
| Introdução | 3 | 2 a 2,5 |
| Revisão bibliográfica | 6 | 4 a 4,5 |
| Metodologia | 5 | 3 a 3,5 |
| Estudo de caso e resultados | 8 | 5 a 6 |
| Conclusão | 2 | 1 a 1,5 |
| Referências | 2 a 3 | 1,5 a 2 |
| Total estimado | 27 a 28 | 18 a 20 |

A primeira redação poderá ultrapassar o limite. A redução deverá remover repetição, detalhes excessivos de implementação e imagens pouco informativas, preservando a argumentação e as evidências.

## 13. Ordem recomendada de redação

1. Pesquisar e escrever a Revisão bibliográfica a partir da pergunta da Introdução.
2. Reunir os registros de requisitos, decisões e tarefas que sustentam a Metodologia.
3. Escrever o Estudo de caso e resultados com base na implementação e nos testes executados.
4. Responder à pergunta de pesquisa na Conclusão e retomar os objetivos da Introdução.
5. Produzir o Resumo e fechar as palavras-chave, citações e referências.
6. Revisar a extensão e aplicar o modelo institucional após a primeira redação completa.

## 14. Regras para a redação

- Escrever em linguagem acadêmica, objetiva e impessoal ou na forma aceita pelo orientador.
- Evitar expressões promocionais como “jogo inovador”, “experiência incrível” ou “sucesso garantido”.
- Diferenciar claramente planejamento, implementação, teste e resultado.
- Não apresentar uma funcionalidade planejada como se estivesse concluída.
- Não usar quantidade de código como sinônimo de qualidade.
- Explicar a razão das decisões técnicas, não apenas listar ferramentas.
- Relacionar cada seção da revisão bibliográfica a decisões do projeto.
- Apoiar afirmações sobre mercado, comportamento de jogadores e metodologias em referências confiáveis.
- Preferir fontes acadêmicas, livros técnicos e documentação oficial.
- Utilizar páginas comerciais apenas para identificar ou descrever jogos específicos.
- Evitar citações longas.
- Manter nomes de classes e arquivos somente quando forem necessários para explicar a arquitetura.
- Traduzir detalhes técnicos para uma linguagem compreensível ao leitor que não conhece o repositório.

## 15. Critérios para considerar o artigo completo

- O problema de pesquisa está formulado e respondido.
- O objetivo geral corresponde ao conteúdo entregue.
- Cada objetivo específico possui uma evidência de atendimento ou uma limitação declarada.
- A revisão bibliográfica sustenta as decisões analisadas.
- A metodologia descreve o processo realmente empregado.
- O estudo de caso explica como a equipe trata o tempo com o jogo fechado e quais dados permitem calcular o progresso na retomada.
- Os resultados distinguem implementação, teste e avaliação.
- Todas as métricas possuem data e método de obtenção.
- Os resultados automatizados foram executados novamente na versão final.
- O texto não afirma que o jogo é divertido ou engajante sem avaliação adequada.
- Figuras, quadros e tabelas são citados e explicados.
- Todas as citações possuem referência correspondente.
- O resumo apresenta objetivo, metodologia, resultados e conclusão em até 200 palavras.
- O documento completo possui entre 15 e 20 páginas.
- A formatação foi conferida no arquivo Word final.

## 16. Decisões que ainda precisarão ser confirmadas

- Evidências disponíveis sobre a ordem das tarefas e as decisões de escopo.
- Cenários de retomada cobertos pelos testes existentes e cenários que ainda precisam de verificação.
- Uso de primeira pessoa do plural ou redação impessoal.
- Necessidade de seção específica sobre uso de inteligência artificial.
- Realização ou não de testes com jogadores.
- Métricas finais que serão apresentadas.
- Funcionalidades que entrarão no escopo definitivo da entrega.
- Existência de um repositório público ou demonstração jogável.
- Regras institucionais para apêndices e anexos.
- Lista definitiva de autores, afiliações e endereços institucionais.
- Necessidade de coorientador.

