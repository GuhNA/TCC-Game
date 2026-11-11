# Evidências do TCC

## Gestão do desenvolvimento

O quadro “Jogo - TCC” mostra as listas **A Fazer**, **Fazendo**, **Revisando** e **Arquivado**. Entre os cartões visíveis em **Revisando** estão “3b - Juntar sistemas”, “4 - Idle” e “6a - A Taverna”. A captura ilustra a organização das tarefas e a atribuição de integrantes aos cartões. O uso dos checklists internos foi descrito pelos autores. [Captura do quadro do Trello](imagens/trello-quadro-jogo-tcc.png)

**Legenda sugerida** — Quadro do Trello utilizado para acompanhar as tarefas do projeto.

**Fonte** — Captura do quadro da equipe, fornecida pelos autores.

O cartão “1 - Portão Norte” detalha o escopo inicial da cena, incluindo o saldo de ouro e a compra de tropas por meio de um NPC. A atividade registra a criação na lista **A Fazer** e a captura mostra o cartão em **Revisando**. Essa imagem pode acompanhar o quadro geral como exemplo de especificação de uma tarefa. [Cartão do Portão Norte](imagens/trello-cartao-portao-norte.png)

Na Metodologia, descrever o uso do Discord com base no relato dos autores. A equipe fazia reuniões semanais e trocava mensagens ao longo da semana. Em uma reunião, Marcos assumiu o Centro e Gustavo assumiu a Taverna. Os dois implementaram suas frentes em paralelo e revisaram as alterações um do outro antes do merge. [Relato da equipe](../TCC_Plano_de_Revisao_e_Evidencias.md#exemplo-de-divisão-do-protótipo-entre-centro-e-taverna)

## Sequência de retomada e coleta

As imagens mostram a mesma vista do Mercado Local, em 1600 × 900. Foram renderizadas na Unity em 10/11/2026, a partir da revisão `9abb425` e das alterações de isolamento do salvamento usadas nos testes. O cenário gravou uma partida com o Mercado Local no nível 3, abriu uma nova sessão com seis minutos simulados e exibiu os valores calculados pelo serviço de impostos na interface do jogo.

| Momento | Imagem | O que mostra |
| --- | --- | --- |
| Estado salvo | [Antes da retomada](imagens/retomada-01-estado-salvo-2026-11-10.png) | Saldo de 1.000 e zero de imposto disponível. |
| Seis minutos depois | [Após o carregamento](imagens/retomada-02-apos-seis-minutos-2026-11-10.png) | 120 de imposto disponível. O saldo permanece em 1.000 até a coleta. |
| Após recolher | [Confirmação da coleta](imagens/retomada-03-apos-coleta-2026-11-10.png) | Crédito de 120 e saldo de 1.120. |

**Legenda sugerida** — Retomada do protótipo e coleta de impostos após seis minutos simulados.

**Fonte** — Elaboração própria, capturas da interface do protótipo em cenário controlado.

As três imagens ilustram o carregamento de uma nova sessão e a coleta. A verificação manual com encerramento e reabertura do executável segue pendente. A preservação do relógio da campanha foi verificada nos testes automatizados. [Tabela de resultados](testes/Resultados_da_retomada_2026-11-10.md) · [Relatório XML](testes/RetomadaEditMode-2026-11-10.xml)

O [código usado na captura](fontes/TccEvidenceCaptureScenario.cs) foi arquivado para reprodução. Ele usa chaves isoladas de salvamento. Para repetir a captura, inserir o arquivo temporariamente na assembly `TCCGame.VisualTests` e executar o cenário `tcc-retomada-2026-11-10` pelo coletor visual do projeto.

## Imagens de contexto

Estas capturas de revisões visuais anteriores ilustram as áreas do protótipo. As datas são as dos relatórios de origem no horário de São Paulo.

| Imagem | Data | Uso no artigo |
| --- | --- | --- |
| [Visão geral do Centro](imagens/centro-visao-geral-2026-11-05.png) | 05/11/2026 | Apresentar a organização da vila e o contexto da arrecadação. |
| [Interior da Taverna](imagens/taverna-interior-2026-11-05.png) | 05/11/2026 | Ilustrar a divisão de trabalho entre Centro e Taverna. |
| [Painel de guarnição do Portão](imagens/portao-painel-guarnicao-2026-10-27.png) | 27/10/2026 | Mostrar a contagem para a próxima onda e a gestão das tropas. |

## Capturas do Unity Editor

- [Cena Centro no editor](imagens/unity-editor-cena-centro.png). Mostra o mapa, a hierarquia da cena e a prévia do jogo.
- [Mercado Local no Inspector](imagens/unity-inspector-mercado-local-exterior.png). Mostra a fachada selecionada e a configuração do `Sprite Renderer` e do `WorldBuildingView`, incluindo as referências à fundação e à área de colisão. Os parâmetros de produção estão registrados no [asset de configuração](../../Assets/Scriptable/Buildings/LocalMarket.asset).

**Fonte** — Capturas do ambiente de desenvolvimento, fornecidas pelos autores.

## Histórico de desenvolvimento

| Registro do Git | Evidência |
| --- | --- |
| `279c0ec` | Alterações no Centro atribuídas a Marcos na mensagem do commit. |
| `f6c4a2b` e `90726d3` | Envio de aventureiros e desenvolvimento do ambiente da Taverna. |
| `4fc66a0` | Integração das contribuições dos autores e compartilhamento da economia entre Centro e Portão. |

O [contrato de unificação](../UnificationBehaviorContract.md) complementa o histórico com as regras que precisavam ser preservadas. O relato sobre reuniões, divisão de tarefas e revisão antes do merge está no [plano de revisão](../TCC_Plano_de_Revisao_e_Evidencias.md#1-gestão-do-desenvolvimento-e-escolhas-técnicas).

## Material complementar opcional

A etapa de coleta de imagens foi encerrada com dez capturas. As opções abaixo ficam registradas para uma eventual ampliação do material de apresentação.

- [ ] Captura do checklist interno de um cartão, se for útil como complemento à descrição já documentada.
- [ ] Captura opcional do asset de configuração do Mercado Local no Inspector, com produção base e produção por nível visíveis.
- [ ] Captura do Test Runner com os resultados, se for útil para apresentação. O XML e a tabela já documentam os valores.
- [ ] Sequência do executável antes do encerramento e após a reabertura, caso seja realizada a verificação manual.
