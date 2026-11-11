# Plano de revisão e evidências do TCC

**Artigo** — Processos de desenvolvimento de jogos indies

**Pergunta de pesquisa** — Como uma equipe pequena pode planejar, implementar e testar o progresso durante a ausência do jogador em um jogo *idle* independente?

O plano organiza o trabalho até a revisão do [artigo](TCC_Estrutura_Final.md). O estudo de caso analisa o acúmulo de impostos com o jogo fechado e a preservação do tempo da campanha. Construções, recrutamento e exploração ajudam a explicar a integração do protótipo.

## 1. Gestão do desenvolvimento e escolhas técnicas

### Tópicos para a Metodologia

- **Trello e priorização.** Descrever o acompanhamento dos cartões nas listas **A Fazer**, **Fazendo**, **Revisando** e **Arquivado**. Usar o quadro “Jogo - TCC” para ilustrar a atribuição de tarefas. [Captura do quadro](Evidencias/imagens/trello-quadro-jogo-tcc.png)
- **Discord e decisões.** Usar o relato dos autores sobre as reuniões, as mensagens e a divisão do protótipo entre Centro e Taverna.
- **Divisão das tarefas.** Identificar as frentes de cada integrante, as tarefas feitas em conjunto e como as entregas eram integradas.
- **Git e integração.** Descrever o uso habitual de uma branch por desenvolvedor e uma branch principal. Usar um caso concreto para mostrar como as alterações eram integradas e verificadas.

### Fluxo de trabalho relatado pela equipe

A equipe usava o Discord para as reuniões semanais e para trocar mensagens durante a semana. Nas reuniões, os integrantes discutiam as próximas tarefas, criavam cartões no Trello quando necessário e dividiam as pendências. Depois, trabalhavam de forma assíncrona e atualizavam o estado dos cartões e seus checklists para acompanhar o andamento.

O Git permitia que os dois integrantes trabalhassem em paralelo. Em geral, a equipe usava uma branch por desenvolvedor e uma branch principal. Ao fim de uma entrega, cada integrante revisava o trabalho do outro antes do merge. As referências disponíveis no repositório incluem `main` e `GuzinTeste`.

### Exemplo de divisão do protótipo entre Centro e Taverna

Em uma reunião no Discord, a equipe dividiu a prototipação de duas cenas e das respectivas mecânicas. Marcos ficou responsável pelo Centro da cidade e Gustavo assumiu a Taverna. Cada um implementou sua parte em paralelo. Segundo o relato da equipe, a divisão feita na reunião evitou conflitos de merge nessa entrega. Ao final, os dois revisaram as alterações um do outro e integraram o trabalho pelo Git.

O histórico oferece registros das duas frentes. O commit `279c0ec`, de 13/08/2026, registra alterações de Marcos no Centro. Os commits `f6c4a2b` e `90726d3`, de 29 e 30/09/2026, registram mudanças no envio de aventureiros e no ambiente da Taverna. Esses commits ilustram as contribuições em cada cena. A reunião, a divisão das responsabilidades e a revisão mútua são descritas com base no relato dos autores.

### Caso técnico de integração entre Centro e Portão

A unificação de Centro e Portão é um caso documentado que permite acompanhar duas frentes de trabalho até a integração. As datas abaixo são as registradas no histórico.

| Registro | O que permite afirmar |
| --- | --- |
| `279c0ec` · 13/08/2026 | Alterações no Centro, com arte de construções e do mapa. A mensagem do commit atribui essa frente a Marcos nessa entrega. |
| `f775fe5` · 13/08/2026 | Alterações na cena Portão e no vendedor de tropas. O commit documenta uma frente de implementação separada. |
| `4fc66a0` · 18/08/2026 | Integração do trabalho dos dois autores. A alteração reúne a sessão de jogo, a economia, o salvamento, o recrutamento e testes de integração. |
| `e79782b` · 03/09/2026 | Ampliação da campanha com reserva, quatro portões e novos elementos de interface. |

O [contrato de unificação](UnificationBehaviorContract.md) registra o problema de haver duas fontes para o saldo e os níveis das construções. A mudança adotou um estado compartilhado entre cenas e preservou a produção do Mercado Local e o limite de 24 horas entre coletas. O teste `CentroAndPortao_ReadTheSameMoneyAndBuildingState` fornece um caso concreto para verificar o comportamento compartilhado. [Teste de integração](../Assets/Tests/PlayMode/SceneIntegrationTests.cs#L105)

O histórico sustenta a sequência das entregas. Um cartão ou uma conversa ligada a essa integração ajudaria a mostrar como a equipe distribuiu as tarefas e tomou a decisão de compartilhar o saldo.

### Justificativas técnicas para desenvolver na Metodologia

A equipe escolheu Unity e C# pela familiaridade prévia e pela disponibilidade de recursos para desenvolver jogos 2D. O projeto aproveitou o editor de cenas, a renderização de sprites, os componentes de interface, o Input System e o Unity Test Framework. Esses recursos permitiram montar as cenas e testar o protótipo enquanto as regras de economia, defesa e exploração eram implementadas em C#. [Configuração dos pacotes](../Packages/manifest.json)

O uso da Unity em jogos independentes 2D publicados também pesou na escolha. *Hollow Knight*, da Team Cherry, e *Cuphead*, do Studio MDHR, são exemplos documentados pela própria Unity. Citar esses casos na seção de ferramentas como referências de adoção do motor em produções independentes. [Hollow Knight](https://unity.com/made-with-unity/hollow-knight) · [Cuphead](https://unity.com/made-with-unity/cuphead)

| Escolha | Problema que resolve no protótipo | Evidência disponível |
| --- | --- | --- |
| Tempo do dispositivo para impostos e contador de execução para a campanha | Calcular a arrecadação ao voltar e manter o tempo de preparação para a próxima onda. | [Serviço de impostos](../Assets/Scripts/City/TaxAccrualService.cs#L104), [relógio da campanha](../Assets/Scripts/Campaign/CampaignRuntimeDriver.cs#L45) |
| Relógio substituível nos testes | Simular ausências longas e repetir os cenários com as mesmas condições iniciais. | [Interface de relógio](../Assets/Scripts/Core/Time/IClock.cs), [teste do Centro](../Assets/Tests/PlayMode/CentroTaxSmokeTests.cs#L34) |
| Estado compartilhado entre cenas | Usar o ouro recolhido no Centro em compras no Portão e preservar os níveis das construções. | [Contrato de unificação](UnificationBehaviorContract.md), [sessão de jogo](../Assets/Scripts/Core/GameSession.cs#L48) |
| Salvamento local em JSON com repositório substituível | Recuperar saldo, impostos acumulados, limite já usado e tempo de campanha. O repositório em memória permite testar cenários com dados isolados. | [Repositório de salvamento](../Assets/Scripts/Core/Persistence/GameSaveRepository.cs), [teste do Centro](../Assets/Tests/PlayMode/CentroTaxSmokeTests.cs#L25) |

### Informações a confirmar com a equipe

- [x] Frequência das reuniões, divisão das pendências na reunião e execução assíncrona ao longo da semana.
- [x] Uso do Trello para registrar tarefas e atualizar o estado dos cartões e os checklists.
- [x] Uso do Discord nas reuniões semanais e na troca de mensagens.
- [x] Uso do Git para trabalho paralelo, com uma branch por desenvolvedor e uma principal na maior parte do projeto, e revisão mútua antes do merge.
- [x] Exemplo de divisão de tarefas entre Marcos no Centro e Gustavo na Taverna.
- [x] Escolha de Unity e C# pela familiaridade da equipe, pelo uso em jogos independentes 2D e pelos recursos disponíveis para agilizar a prototipação.
- [x] Listas do quadro do Trello e exemplos de cartões visíveis.
- [x] Exemplo de tarefa detalhada no Trello, com a descrição e a atividade do cartão “1 - Portão Norte”.
- [x] Relato dos autores como fonte para o uso do Discord e a divisão Centro–Taverna.
- [ ] Confirmar o motivo da escolha do salvamento local e, se houver registro, como foi definido o limite de 24 horas.

Usar a divisão Centro–Taverna como exemplo de gestão e a unificação Centro–Portão para explicar a decisão de compartilhar o estado da partida.

## 2. Testes e resultados da retomada

Na execução de 10/11/2026, os oito casos de retomada e os quinze de persistência e migração passaram em Edit Mode. Os testes criaram novas sessões com dados gravados em chaves isoladas. A verificação com encerramento e reabertura do executável permanece pendente. [Resultados da execução](Evidencias/testes/Resultados_da_retomada_2026-11-10.md)

1. Reaproveitar os testes existentes de cálculo, coleta e persistência. Identificar quais partes do roteiro cada teste já cobre.
2. Preparar testes com duas sessões e salvamento isolado. Simular os intervalos longos com um relógio controlado, mantendo intacto o relógio do computador e a partida dos autores.
3. Verificar o fluxo de encerramento e reabertura em uma versão jogável. Registrar separadamente os resultados desse fluxo e os resultados obtidos por simulação automatizada.
4. Revisar os testes de migração que esperam o formato 7. Verificar a atualização para o formato atual e a preservação dos dados relevantes de cada partida usada como entrada.
5. Executar os testes na revisão avaliada. Guardar os relatórios XML e registrar commit, ambiente, estado inicial, intervalo, valor esperado, valor observado e situação. Identificar casos sem execução ou com cobertura parcial.

### Cenários prioritários

Para os cenários de impostos, usar o Mercado Local no nível 3, com produção de 20 unidades de ouro por minuto. Registrar a parte do limite de 24 horas já utilizada em cada estado inicial.

| Cenário | Resultado esperado |
| --- | --- |
| Reabrir sem avanço do relógio | O valor disponível permanece igual. |
| Reabrir após seis minutos, com acúmulo inicial zero | 120 unidades de ouro disponíveis. |
| Reabrir após 24 horas e após 48 horas, com o limite livre em cada caso | 28.800 unidades em ambos os casos. |
| Reabrir após duas horas com 23h30 de acúmulo já utilizadas | Apenas os 30 minutos restantes entram no cálculo, com acréscimo de 600 unidades. |
| Recolher os impostos, salvar e reabrir sem avanço do relógio | O crédito anterior permanece no saldo e a nova coleta não credita o mesmo período. |
| Reabrir com uma onda programada | O contador da campanha parte do valor salvo e avança durante a execução. |

## 3. Checklist de imagens e registros

A coleta de imagens foi encerrada com dez capturas. O [índice de evidências](Evidencias/Indice_de_evidencias.md) reúne o material selecionado e as opções complementares.

- [x] Quadro do Trello com colunas e cartões legíveis.
- [x] Cartão aberto com descrição e registro de criação em **A Fazer**, exibido em **Revisando** na captura.
- [ ] Captura do checklist interno de um cartão, se necessária para a apresentação.
- [x] Relato dos autores para documentar o uso do Discord e a decisão de divisão de trabalho.
- [x] Histórico do Git com commits das frentes Centro e Taverna e da integração Centro–Portão.
- [x] Unity Editor com a cena Centro e o exterior do Mercado Local selecionado no Inspector.
- [ ] Captura opcional do asset do Mercado Local com os parâmetros de produção no Inspector. Os valores estão no arquivo de configuração do projeto.
- [x] Sequência da interface em cenário controlado, com o estado salvo, os impostos na nova sessão e o saldo após a coleta.
- [x] Imagens de contexto do Centro, da Taverna e do Portão.
- [ ] Jogo antes do encerramento e depois da reabertura do executável, com enquadramento e estado inicial comparáveis.
- [x] Relatório XML e tabela de resultados para comprovação dos testes.
- [ ] Captura do Test Runner para apresentação, se necessária.

O material está em `Docs/Evidencias/`, com origem e descrição no índice. O artigo usa o quadro de gestão, duas imagens da retomada e a tabela de resultados.

## 4. Revisão do artigo

- [x] Atualizar a **Metodologia** com a gestão efetivamente praticada, as justificativas técnicas e o procedimento de validação.
- [x] Atualizar o **Estudo de caso e resultados** com a cronologia da integração e a relação entre cenários, resultados e evidências.
- [x] Revisar o **Resumo**, a **Introdução** e a **Conclusão** para que as afirmações correspondam à cobertura demonstrada pelos testes.
- [x] Concentrar a cronologia em 4.2, o cálculo em 4.4 e os valores observados em 4.7.
- [x] Selecionar as figuras para o corpo do artigo.
- [ ] Completar os e-mails institucionais e conferir o limite de 15 a 20 páginas na versão Word.
