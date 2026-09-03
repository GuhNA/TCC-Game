# Plano de correção das falhas automatizadas

**Versão:** 1.0  
**Data:** 02 de setembro de 2026  
**Fonte:** [Relatório de testes automatizados](RelatorioTestesAutomatizados-2026-09-02.md)  
**Estado atual:** implementado e validado; 106/106 EditMode e 15/15 PlayMode aprovados

## 1. Objetivo

Corrigir os três defeitos confirmados no primeiro lote rigoroso sem reduzir a
força dos testes, preservar a correção anterior dos pop-ups e voltar a uma
suíte integral verde. A correção deve manter o comportamento funcional já
aprovado e tornar explícitos os limites de transação, recuperação de save e
aritmética de combate.

## 2. Critérios de sucesso

- Os 106 casos atuais passam em duas execuções consecutivas.
- O resultado persistido e o estado em memória permanecem iguais após qualquer
  exceção de observador.
- JSON inválido não impede o boot e nunca apaga silenciosamente a evidência.
- Entradas válidas extremas não causam overflow, wraparound ou loop infinito.
- Nenhum teste existente é enfraquecido, removido ou transformado em `Ignored`.
- PlayMode continua aprovado após a correção, incluindo os 15 casos atuais.
- A linha da correção do prompt em `PlayerInteractor` permanece intacta.

## 3. Ordem de execução

1. Criar testes focados e reproduzir os três defeitos na revisão atual.
2. Corrigir BUG-AUT-001, pois a fronteira transacional afeta todo evento de
   domínio e pode mascarar outros resultados.
3. Corrigir BUG-AUT-003, garantindo que o jogo se recupere antes de consultar
   saves de campanha extremos.
4. Corrigir BUG-AUT-002, fechando a robustez matemática do cálculo.
5. Rodar os casos focados, a suíte EditMode completa duas vezes e PlayMode.
6. Fazer revisão manual dos diffs, PlayerPrefs e correção de pop-up.

Não misturar correção funcional com balanceamento. As constantes de combate,
ameaça, espólio e economia devem continuar inalteradas nesta etapa.

## 4. BUG-AUT-001 — transação e exceções de observadores

### Causa confirmada

`BattleResolutionService.TryResolvePending` engloba mutação, commit, publicação
de `MoneyChanged`, publicação da onda e `BattleResolved` no mesmo `try/catch`.
Se um observador falha depois do commit, o rollback restaura apenas a memória;
o PlayerPrefs já contém o resultado novo.

### Decisão de correção

Separar claramente as duas fases:

```text
fase transacional
    validar → calcular → mutar → commit
    rollback somente se o commit falhar

fase de notificação
    publicar eventos depois do commit
    nunca executar rollback por falha de apresentação
```

Além disso, publicar cada observador de forma isolada. Um HUD ou painel com
defeito deve ser registrado com `Debug.LogException`, mas não pode interromper
os demais observadores nem lançar para o `Update` do runtime. Aplicar a mesma
política a `MoneyChanged`, `BattleResolved`, `BattleReportsChanged`,
`WaveScheduled`, `WaveWarningChanged` e `WaveBecameDue`.

### Mudanças previstas

- Extrair um método de publicação segura que percorra a lista de delegates.
- Remover publicação de eventos de dentro do bloco que faz rollback.
- Marcar o commit como concluído antes de qualquer notificação.
- Preservar rollback completo para falha real do repositório.
- Não publicar evento de sucesso quando o commit falhar.
- Manter a ordem atual: ouro, agenda/avisos, resultado da batalha.
- Garantir que os eventos vejam o saldo e o relatório já persistidos.

### Testes de aceite

- `ObserverFailure_CannotDivergeMemoryFromCommittedSave` passa sem exceção.
- Um observador que falha não impede o segundo observador de receber o evento.
- A sessão atual e uma sessão recarregada exibem o mesmo relatório, tropas,
  portão, ouro e onda seguinte.
- Falha injetada no commit continua restaurando tudo e emitindo zero eventos.
- Uma segunda atualização não reaplica a batalha.

## 5. BUG-AUT-003 — recuperação de JSON inválido

### Causa confirmada

`GameSaveRepository.LoadCurrentSave` chama `JsonUtility.FromJson` sem capturar
`ArgumentException`. Um conteúdo truncado ou não-JSON interrompe a criação da
sessão antes da normalização.

### Política de recuperação

Ao encontrar JSON inválido na chave atual:

1. Ler e preservar o texto bruto em uma chave de diagnóstico única,
   `tcc.game.save.v2.corrupt`.
2. Emitir `Debug.LogWarning` com a causa, sem incluir conteúdo sensível no log.
3. Criar `GameSaveData.CreateDefault()`.
4. Normalizar e salvar o novo estado v7 na chave original.
5. Retornar o estado padrão sem tentar carregar novamente a mesma entrada.

Se já existir um diagnóstico anterior, não sobrescrevê-lo automaticamente;
manter a primeira evidência até uma ferramenta explícita de limpeza. O save
válido continua usando somente `tcc.game.save.v2` para o jogo.

A mesma proteção deve envolver a migração do save legado. Se o legado estiver
inválido e não houver save atual válido, iniciar estado padrão e registrar
aviso.

### Mudanças previstas

- Adicionar constante interna para a chave de diagnóstico.
- Separar parse seguro de `LoadCurrentSave` e `MigrateLegacyCitySave`.
- Capturar apenas erros de parse/conversão esperados; não esconder falhas de
  escrita ou de permissão.
- Criar método de recuperação que não chama `Load` recursivamente.
- Garantir que `Save` do estado padrão atualize a versão para 7.
- Atualizar setup/teardown dos testes para preservar a chave de diagnóstico.

### Testes de aceite

- `{`, `not-json` e JSON truncado iniciam sem exceção.
- O estado retornado está na versão 7, com quatro portões e listas válidas.
- A chave original passa a conter um save padrão válido.
- A chave `.corrupt` contém a evidência original e não é sobrescrita na segunda
  tentativa.
- Save legado inválido também possui recuperação definida.
- Falha durante a escrita do save continua sendo propagada; não transformar
  erro de disco em falsa recuperação.
- Carregar o estado recuperado novamente é idempotente e não cria nova chave.

## 6. BUG-AUT-002 — overflow de quantidades extremas

### Causa confirmada

`BattleCalculator` soma três contagens `int` usando operação verificada. Três
valores válidos próximos de `int.MaxValue` excedem o limite antes de a taxa de
baixas ser calculada.

### Decisão de correção

Usar aritmética `long`/saturada apenas no cálculo interno. Não reduzir contagens
válidas nem alterar a fórmula para partidas normais.

```text
totalSoldadosLong = (long)G + A + M
totalBaixasLong = clamp(round(totalSoldadosLong × taxa), 0, totalSoldadosLong)
```

A distribuição calcula cada classe com `long` e converte cada perda individual
para `int` somente depois de limitar à contagem daquela classe. O total exposto
de perdas deve usar `long` quando a soma puder exceder `int`; textos da UI
continuam formatando normalmente.

Se qualquer entrada negativa chegar diretamente ao calculador, rejeitar com
`ArgumentOutOfRangeException`; a normalização do save continua limitando dados
persistidos negativos a zero.

### Mudanças previstas

- Remover somas verificadas em `int` do caminho de cálculo.
- Alterar a alocação de restos para aceitar totais `long`.
- Revisar `BattleReportSnapshot.TotalLosses` e formatação do HUD/Capitão.
- Manter o algoritmo de maiores restos e seu desempate fixo.
- Usar limites finitos para evitar loops de distribuição em entrada extrema.

### Testes de aceite

- `ExtremeValidCounts_DoNotOverflowOrWrap` passa.
- Perdas por classe nunca são negativas nem maiores que a classe inicial.
- Resultado é determinístico para valores extremos.
- Casos normais existentes permanecem com os mesmos números.
- Nenhuma soma de apresentação gera overflow ao exibir o relatório.

## 7. Regressão e validação pós-correção

### 7.1 EditMode focado

Executar primeiro:

- Todos os casos `BattleResolutionServiceTests`.
- Todos os casos `GameSaveRobustnessTests`.
- Todos os casos `BattleCalculatorTests`.

Esperado: nenhum caso vermelho, inclusive os três testes que atualmente
reproduzem os defeitos.

### 7.2 Suíte integral

Rodar EditMode duas vezes consecutivas. O resultado esperado é 106/106 em cada
execução. Comparar XMLs para confirmar que a contagem e os nomes dos testes não
mudaram.

### 7.3 PlayMode

Rodar os 15 casos atuais e repetir pelo menos:

- chegada de tropa no instante do ataque;
- batalha pendente durante modal;
- troca Centro–Portão;
- persistência após resultado;
- HUD sem duplicação;
- prompt fechado ao abrir menu.

### 7.4 Inspeção manual curta

Antes de liberar a correção para os testes manuais completos:

1. Fazer backup do PlayerPrefs.
2. Corromper uma cópia do save e confirmar recuperação visual/logada.
3. Provocar resultado e abrir/fechar o Capitão repetidamente.
4. Confirmar que o ouro não muda depois da primeira resolução.
5. Verificar visualmente a linha preservada em `PlayerInteractor`.

## 8. Critério de encerramento

A correção só será encerrada quando os três testes originalmente falhos
passarem, os 106 EditMode passarem duas vezes, os 15 PlayMode passarem, nenhum
P0/P1 permanecer aberto e o relatório de testes for atualizado com os novos
artefatos. Se uma correção exigir alterar regra de balanceamento ou formato de
save além do v7, parar e registrar uma decisão de produto separada.

## 9. Arquivos previstos

| Área | Arquivos |
|---|---|
| Eventos/transação | `BattleResolutionService.cs`, `WaveDirectorService.cs`, `EconomyService.cs` |
| Recuperação | `GameSaveRepository.cs`, testes de robustez e PlayerPrefs |
| Aritmética | `BattleCalculator.cs`, `CampaignTypes.cs`, apresentação de relatório |
| Regressão | `BattleResolutionServiceTests.cs`, `GameSaveRobustnessTests.cs`, `BattleCalculatorTests.cs` |
| Documentação | relatório de execução e plano rigoroso de testes |

Nenhuma alteração de cena, prefab, navegação ou input é necessária para a
correção inicial, salvo se um teste de regressão demonstrar dependência.

## 10. Implementação e validação

As três correções foram aplicadas:

- o commit encerra a transação de batalha antes das notificações; cada
  observador é isolado e não pode fazer a memória divergir do save;
- saves atuais inválidos são preservados em `tcc.game.save.v2.corrupt`,
  substituídos por um save v7 padrão e tratados sem exceção ou loop;
- a aritmética de efetivo e perdas usa `long` internamente, evitando overflow
  em contagens extremas.

Validação executada em 02/09/2026:

- EditMode: 106/106 aprovados (execução de revalidação);
- PlayMode: 15/15 aprovados;
- testes originalmente falhos cobertos por BUG-AUT-001, BUG-AUT-002 e
  BUG-AUT-003: todos aprovados.

A etapa manual do plano rigoroso continua necessária para validar a experiência
visual e o fluxo completo em jogo.
