# Resultados dos testes de retomada

**Execução** — 10/11/2026 às 21h09, horário de São Paulo

**Ambiente** — Unity 6000.3.11f1, Unity Test Framework, Edit Mode, Windows

**Revisão** — `9abb42536bcb37cbf3d5888b0509c2fbba515320` com as alterações de teste e de isolamento de salvamento desta execução

**Resultado** — 23 de 23 casos aprovados, com zero falhas e zero ignorados no conjunto selecionado.

[Relatório XML do Unity Test Framework](RetomadaEditMode-2026-11-10.xml)

## Procedimento

Os testes de retomada usaram a configuração do Mercado Local do projeto. O nível 3 produz 20 unidades de ouro por minuto. Cada caso iniciou com 1.000 unidades de ouro e uma chave de teste exclusiva. O repositório gravou e leu o JSON pelo `PlayerPrefs`, como no jogo. Uma nova instância de `GameSession` carregou o estado salvo em cada retomada. O relógio controlado forneceu o intervalo de ausência. As chaves de teste foram removidas ao final de cada caso.

O relatório XML contém as observações da execução nas linhas identificadas por `TCC_OFFLINE`. Nos casos de produção, os testes comparam o valor da prévia, o crédito da coleta e o saldo final.

## Retomada de impostos e campanha

| Cenário | Esperado | Observado | Situação |
| --- | --- | --- | --- |
| Retomada imediata, com acúmulo inicial zero | 0 de imposto, saldo 1.000 | Prévia 0, coleta 0, saldo 1.000 | Aprovado |
| Ausência de 6 minutos, limite livre | 120 de imposto, saldo 1.120 | Prévia 120, coleta 120, saldo 1.120 | Aprovado |
| Ausência de 23h59min59s, limite livre | 28.799 de imposto após o arredondamento, saldo 29.799 | Prévia 28.799, coleta 28.799, saldo 29.799 | Aprovado |
| Ausência de 24 horas, limite livre | 28.800 de imposto, saldo 29.800 | Prévia 28.800, coleta 28.800, saldo 29.800 | Aprovado |
| Ausência de 48 horas, limite livre | 28.800 de imposto, saldo 29.800 | Prévia 28.800, coleta 28.800, saldo 29.800 | Aprovado |
| Ausência de 2 horas com 23h30 de limite já utilizadas | Acrescentar 600 ao valor já acumulado de 28.200 | Acréscimo de 600, prévia de 28.800 | Aprovado |
| Coleta após 6 minutos e nova retomada sem avanço do relógio | Primeiro crédito de 120, segundo crédito de 0, saldo final 1.120 | Primeiro crédito de 120, segundo crédito de 0, saldo final 1.120 após outro carregamento | Aprovado |
| Retomada após 48 horas com a primeira onda programada | Contador de 47,25 s, 132,75 s até a onda e contador de 52,25 s após avançar 5 s na sessão | 47,25 s, 132,75 s e 52,25 s, respectivamente | Aprovado |

## Migração e persistência

| Grupo | Executados | Aprovados | Verificação |
| --- | ---: | ---: | --- |
| Retomada da partida | 8 | 8 | Cálculo, coleta, saldo após novo carregamento e tempo de campanha |
| Persistência da campanha | 5 | 5 | Gravação e leitura da campanha, normalização e migração dos estados preparados |
| Robustez do salvamento | 10 | 10 | Migração de formatos anteriores, dados inválidos e isolamento das chaves |

Os seis casos de migração que falharam no relatório histórico de outubro passaram nesta execução. As asserções agora comparam o formato carregado com a versão atual, 11 nesta revisão. Continuam verificando os dados relevantes dos cenários, como saldo, nível da construção, tropas, contratação e estado de uma onda pendente.

## Cobertura da execução

Os resultados abrangem gravação e carregamento em instâncias distintas da sessão no mesmo processo do Unity. A simulação de 48 horas confirma o comportamento no limite, e o cenário de segunda coleta confirma que o crédito anterior permanece no saldo sem ser aplicado de novo. A verificação do encerramento e da reabertura do executável jogável permanece como etapa manual separada.

## Como repetir

No Test Runner da Unity, selecionar Edit Mode e executar `OfflineResumeTests`, `CampaignPersistenceTests` e `GameSaveRobustnessTests`. Os testes usam chaves exclusivas e instantes simulados. O relatório desta rodada contém apenas essas três classes.
