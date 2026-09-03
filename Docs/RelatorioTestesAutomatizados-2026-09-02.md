# Relatório de testes automatizados — Gates E e F

**Data:** 02 de setembro de 2026  
**Editor:** Unity 6000.3.11f1  
**Escopo:** idempotência, eventos, persistência e entradas extremas  
**Resultado:** reprovado — 101 aprovados, 5 falhos, 0 ignorados em 106 casos

## 1. Resumo da execução

O primeiro lote do plano rigoroso adicionou 16 casos à baseline de 90 testes
EditMode. Todos os 90 testes anteriores continuaram aprovados. Dos novos casos,
11 passaram e 5 falharam, confirmando três defeitos distintos.

Artefatos locais:

- `Logs/RigorousGateEFResults.xml`
- `Logs/RigorousGateEF.log`

## 2. Cobertura aprovada neste lote

- Mil chamadas de resolução aplicam uma única transação.
- O evento de dinheiro observa o saldo depois do commit.
- Confirmar relatório duas vezes persiste apenas a primeira confirmação.
- Vitória final congela o relógio estratégico.
- Saves mínimos v3, v4, v5 e v6 migram para v7.
- Relatório de batalha faz round-trip pelo PlayerPrefs.
- Valores negativos, listas nulas e enums inválidos são normalizados.
- O save atual continua usando `tcc.game.save.v2`.

## 3. Defeitos confirmados

### BUG-AUT-001 — exceção de apresentação diverge memória e save

**Severidade:** P0 — bloqueador  
**Teste:** `ObserverFailure_CannotDivergeMemoryFromCommittedSave`  
**Origem planejada:** RSK-01 / AUT-E08

#### Reprodução automatizada

1. Preparar uma onda em `AwaitingResolution`.
2. Inscrever um observador de `BattleResolved` que lança exceção.
3. Resolver a batalha.
4. Comparar a sessão em memória com uma sessão recarregada do repositório.

#### Observado

- O commit grava o resultado corretamente.
- A exceção do observador escapa de `TryResolvePending`.
- O bloco `catch` restaura a memória como se o commit tivesse falhado.
- O relatório desaparece somente na sessão atual.
- O saldo em memória diverge do saldo persistido.

#### Esperado

Falhas de HUD/observadores não devem desfazer nem invalidar uma transação já
persistida. Memória e save devem permanecer no mesmo resultado.

#### Hipótese técnica

Publicação de `MoneyChanged`, eventos da onda e `BattleResolved` ocorre dentro
do mesmo `try/catch` usado para rollback do commit. O limite transacional deve
terminar imediatamente após o commit bem-sucedido; notificações precisam ser
publicadas depois desse limite e isoladas conforme a política de eventos.

### BUG-AUT-002 — overflow com quantidades extremas de tropas

**Severidade:** P1 — grave  
**Teste:** `ExtremeValidCounts_DoNotOverflowOrWrap`  
**Origem planejada:** RSK-02 / AUT-D10 / AUT-F08

#### Reprodução automatizada

1. Criar um snapshot com `int.MaxValue` em cada classe.
2. Calcular uma batalha válida com ameaça 100.000.

#### Observado

`BattleCalculator.Calculate` lança `OverflowException` ao somar a guarnição.

#### Esperado

O save deve ser rejeitado ou normalizado antes do combate, ou o cálculo deve
usar aritmética larga/saturada. Nunca pode ocorrer wraparound ou encerramento
da resolução da onda.

#### Impacto

A interface normal dificilmente alcança esses valores, mas um save editado,
corrompido ou acumulado por outro defeito pode impedir permanentemente a
campanha de avançar.

### BUG-AUT-003 — JSON inválido impede inicialização

**Severidade:** P0 — bloqueador  
**Testes:** três casos de `MalformedCurrentSave_DoesNotPreventGameStartup`  
**Origem planejada:** RSK-03 / AUT-F07

Entradas reproduzidas:

```text
{
not-json
{"version":7,"campaign":
```

#### Observado

`JsonUtility.FromJson<GameSaveData>` lança `ArgumentException`. O repositório
não captura a falha e nenhuma sessão é criada.

#### Esperado

Um save inválido não deve impedir o jogo de iniciar. A política recomendada é:

1. Preservar o conteúdo inválido em uma chave/arquivo de diagnóstico.
2. Criar e persistir um estado padrão v7.
3. Registrar um aviso claro para diagnóstico.
4. Nunca entrar em loop tentando carregar o mesmo JSON inválido.

## 4. Casos falhos no XML

| Caso | Defeito | Resultado |
|---|---|---|
| `ExtremeValidCounts_DoNotOverflowOrWrap` | BUG-AUT-002 | `OverflowException` |
| `ObserverFailure_CannotDivergeMemoryFromCommittedSave` | BUG-AUT-001 | Exceção escapou e memória divergiu |
| `MalformedCurrentSave_DoesNotPreventGameStartup("{")` | BUG-AUT-003 | `ArgumentException` |
| `MalformedCurrentSave_DoesNotPreventGameStartup("not-json")` | BUG-AUT-003 | `ArgumentException` |
| `MalformedCurrentSave_DoesNotPreventGameStartup(JSON truncado)` | BUG-AUT-003 | `ArgumentException` |

## 5. Gate de qualidade

Os Gates E e F permanecem **reprovados**. PlayMode não precisa ser repetido
antes das correções, pois as falhas estão isoladas em domínio/persistência e a
baseline anterior já foi preservada por esta execução.

Ordem de correção recomendada:

1. BUG-AUT-001 — risco de duplicação, perda aparente e divergência de estado.
2. BUG-AUT-003 — risco de impedir totalmente a inicialização do jogador.
3. BUG-AUT-002 — robustez contra estado extremo/corrompido.

Após as correções, repetir os 106 testes duas vezes e só então avançar para os
Gates C e D do plano rigoroso.

## 6. Revalidação pós-correção

As correções do plano foram aplicadas e revalidadas no Unity 6000.3.11f1:

| Plataforma | Total | Aprovados | Falhos | Artefato |
|---|---:|---:|---:|---|
| EditMode | 106 | 106 | 0 | `Logs/RigorousGateEFResults2.xml` e `Logs/RigorousGateEFResults3.xml` |
| PlayMode | 15 | 15 | 0 | `Logs/RigorousGateEFPlayModeResults.xml` |

Os três defeitos confirmados no primeiro lote não foram reproduzidos. Os
testes de regressão permanecem ativos, sem ignorar ou enfraquecer casos. A
validação manual M01–M16 ainda deve ser executada no build jogável.
