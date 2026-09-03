# Plano rigoroso de testes — defesa da cidade e ondas

**Versão:** 1.0  
**Data:** 02 de setembro de 2026  
**Escopo:** vertical slice das Partes 1 a 5  
**Baseline automatizado:** 90/90 EditMode e 15/15 PlayMode aprovados

## 1. Objetivo

Validar as regras estabelecidas, o funcionamento integrado e a experiência do
jogador, além de procurar defeitos que testes de caminho feliz normalmente não
encontram. O plano possui duas etapas complementares:

1. Testes automatizados de unidade, propriedade, integração, persistência,
   cenas e sessões simuladas.
2. Testes manuais executados pelo responsável pelo projeto, com foco em
   compreensão, legibilidade, sensação de controle e problemas visuais.

Uma execução só pode ser considerada aprovada quando regras, dados persistidos
e experiência tiverem evidências próprias. A ausência de exceções não é
suficiente para aprovar a experiência.

## 2. Regras que formam o contrato do jogo

| ID | Regra obrigatória |
|---|---|
| REG-01 | A cidade possui exatamente quatro portões: Norte, Leste, Sul e Oeste. |
| REG-02 | Existe somente uma onda ativa, que pode atacar qualquer portão. |
| REG-03 | O mesmo portão não pode receber um terceiro ataque consecutivo. |
| REG-04 | Intervalo e ameaça escalam com sequência, tempo e progresso militar dentro dos limites definidos. |
| REG-05 | Avisos ocorrem uma vez, em ordem, nos marcos de 90, 30, 10 e 0 segundos. |
| REG-06 | Menus modais pausam relógio, avisos, chegadas e resolução da batalha. |
| REG-07 | O prompt de interação desaparece enquanto qualquer menu estiver aberto. |
| REG-08 | O jogador não pode sair da área visível da câmera. |
| REG-09 | Centro e Portão usam os markers corretos de saída e retorno. |
| REG-10 | Toda ação no mapa é iniciada ao conversar com um NPC. |
| REG-11 | Soldados podem ficar na reserva, em um portão ou em trânsito, sem duplicação ou perda silenciosa. |
| REG-12 | Uma tropa que chega exatamente no horário do ataque participa; uma posterior não participa. |
| REG-13 | Guerreiro tem bônus contra Saqueador, Arqueiro contra Xamã e Mago contra Bruto. |
| REG-14 | Prognóstico e batalha usam a mesma força efetiva. |
| REG-15 | Batalhas são determinísticas, automáticas e resolvidas uma única vez. |
| REG-16 | Baixas não excedem a guarnição e vida do portão permanece entre 0 e 100. |
| REG-17 | Defesa suficiente evita dano; defesa insuficiente produz dano crescente. |
| REG-18 | Vitória concede pouco espólio; brecha não concede ouro. |
| REG-19 | Relatório persiste e só é confirmado pelo Capitão do portão correspondente. |
| REG-20 | Portão com zero de vida encerra a campanha em derrota. |
| REG-21 | Repelir a onda 10 encerra a campanha em vitória e nunca cria a onda 11. |
| REG-22 | Campanha encerrada congela tempo e impede novos deslocamentos. |
| REG-23 | Save v7 permanece na chave `tcc.game.save.v2` do PlayerPrefs e migra versões anteriores. |
| REG-24 | Renda, compras, construções, impostos e recrutamento não podem criar saldo ou unidades por repetição de comandos. |
| REG-25 | O HUD dos quatro portões não bloqueia raycasts nem se duplica entre cenas. |

## 3. Estratégia e níveis de teste

### 3.1 EditMode — unidade e propriedades

Executar milhares de combinações rápidas sem carregar cenas. O objetivo é
provar invariantes, limites e determinismo dos serviços puros.

### 3.2 EditMode — integração de domínio

Instanciar `GameSession` com repositório e relógio falsos. Validar transações
entre economia, recrutamento, tropas, ondas, combate e persistência.

### 3.3 PlayMode — integração Unity

Carregar as cenas reais, criar os componentes por bootstrap, simular frames,
abrir menus e atravessar mapas. Validar ciclo de vida, singletons, input, pausa,
ordem de atualização e referências de cena.

### 3.4 Simulação de campanha

Executar campanhas inteiras com seeds e estratégias conhecidas, avançando o
tempo sem `WaitForSeconds`. Essa camada procura erros cumulativos que não
aparecem em uma única onda.

### 3.5 Teste manual

Validar o que não pode ser inferido apenas pelo estado: se a informação é
notada, compreendida, legível e útil para tomar uma decisão.

### 3.6 Riscos encontrados durante a inspeção

Estes itens ainda não são defeitos confirmados, mas possuem evidência suficiente
no código para serem testados antes dos demais casos P1/P2:

| ID | Risco | Severidade potencial | Teste que confirma ou descarta |
|---|---|---|---|
| RSK-01 | Um observador de `MoneyChanged`, `WaveScheduled` ou `BattleResolved` lança exceção depois do commit; o catch pode restaurar apenas a memória e deixá-la diferente do PlayerPrefs. | P0 | AUT-E08 |
| RSK-02 | A soma de quantidades próximas de `int.MaxValue` usa operação verificada e pode abortar o cálculo em vez de normalizar/rejeitar o save. | P1 | AUT-D10 e AUT-F08 |
| RSK-03 | JSON vazio é tratado, mas JSON truncado ou sintaticamente inválido pode lançar durante `JsonUtility.FromJson` e impedir a sessão de iniciar. | P0 | AUT-F07 |
| RSK-04 | Cada alerta cria um `AudioClip` procedural novo; sessões longas podem manter recursos sem liberação explícita. | P2 | AUT-I07 e M16 |
| RSK-05 | O painel do Capitão e os quatro cartões são construídos em runtime com dimensões fixas de referência; resoluções compactas e ultrawide ainda não têm prova visual. | P1 | AUT-H10 e M10 |

Se um risco for reproduzido, abrir um defeito separado com save, seed e log. Não
alterar a expectativa do teste apenas para acomodar o comportamento atual.

## 4. Ambiente e controle dos testes

- Fixar a versão do editor em Unity 6000.3.11f1.
- Registrar commit ou hash testado antes de iniciar uma rodada.
- Guardar XML e log de cada execução automatizada em `Logs/`.
- Testes não podem depender da ordem em que são executados.
- Cada teste que usa PlayerPrefs deve preservar e restaurar o valor anterior.
- Tempo civil usa `IClock` falso; tempo estratégico usa avanço explícito.
- Aleatoriedade usa seeds fixas registradas na falha.
- Não usar atrasos reais para provar regras temporais.
- Rodar EditMode antes de PlayMode; uma falha de compilação interrompe a rodada.
- Para testes manuais, começar tanto com save limpo quanto com um save de
  campanha em andamento.

Matriz manual mínima:

| Perfil | Resolução | Tela | Áudio |
|---|---:|---|---|
| Principal | 1920×1080 | Tela cheia | Ligado |
| Compacto | 1366×768 | Janela | Ligado |
| Ultrawide | 2560×1080 | Tela cheia | Ligado |
| Acessibilidade | 1920×1080 | Janela | Desligado |

Se o projeto não suportar oficialmente ultrawide, o teste ainda deve verificar
que informações e botões essenciais não ficam inacessíveis.

## 5. Etapa automatizada

### 5.1 Gate A — compilação e integridade do projeto

| ID | Prioridade | Teste | Aprovação |
|---|---|---|---|
| AUT-A01 | P0 | Compilar assemblies Runtime, EditMode e PlayMode | Zero erro de compilação |
| AUT-A02 | P0 | Carregar todas as cenas do Build Settings | Zero referência ausente ou exceção |
| AUT-A03 | P0 | Validar wiring de NPCs, player, câmera, markers e HUD | Exatamente as referências esperadas |
| AUT-A04 | P1 | Procurar componentes e GameObjects persistentes duplicados | Um runtime, um HUD e um painel global |
| AUT-A05 | P1 | Validar que nenhum teste deixa PlayerPrefs, cena ou singleton contaminado | Estado posterior igual ao anterior |

### 5.2 Gate B — ondas e escalonamento

Adicionar `CampaignRulesBoundaryTests` e ampliar `WaveGenerationTests`:

| ID | Regra | Caso automatizado |
|---|---|---|
| AUT-B01 | REG-04 | Sequências 1, 3, 4, 7, 8 e 10 usam as faixas de intervalo corretas. |
| AUT-B02 | REG-04 | Ameaça é positiva, limitada e não diminui sem redução de progresso. |
| AUT-B03 | REG-04 | Fator de progresso respeita exatamente 0,90 e 1,15. |
| AUT-B04 | REG-02 | Para 10.000 seeds, sempre existe no máximo uma onda não resolvida. |
| AUT-B05 | REG-03 | Para 10.000 gerações, nunca há três portões iguais consecutivos. |
| AUT-B06 | REG-13 | Bruto só aparece a partir da onda 3 e Xamã a partir da onda 5. |
| AUT-B07 | REG-05 | Limites 90±ε, 30±ε, 10±ε e 0±ε geram os estágios corretos. |
| AUT-B08 | REG-05 | Um salto de tempo grande publica todos os estágios em ordem e uma vez. |
| AUT-B09 | REG-21 | Resolver a onda 10 em todas as seeds não cria sequência 11. |
| AUT-B10 | Robustez | Sequência zero, negativa, `int.MaxValue` e ameaça máxima falham de forma controlada. |

Usar `ε = 0,001` segundo para os limites temporais.

### 5.3 Gate C — tropas e conservação

Adicionar testes baseados em tabela e sequências aleatórias reproduzíveis:

| ID | Regra | Caso automatizado |
|---|---|---|
| AUT-C01 | REG-11 | Reserva → portão, portão → reserva e portão → portão conservam cada classe. |
| AUT-C02 | REG-11 | Quantidade zero, negativa, maior que disponível e overflow não alteram estado nem salvam. |
| AUT-C03 | REG-11 | Origem igual ao destino e `GateId` inválido são rejeitados. |
| AUT-C04 | REG-12 | Chegada em `ataque-ε`, `ataque` e `ataque+ε` produz a participação esperada. |
| AUT-C05 | REG-11 | Cem transferências simultâneas chegam uma vez e na ordem determinística. |
| AUT-C06 | REG-11 | Fechar/reabrir durante trânsito preserva quantidade e horário. |
| AUT-C07 | REG-22 | Vitória e derrota rejeitam qualquer nova transferência. |
| AUT-C08 | Invariante | Reserva + portões + trânsito é constante entre recrutamento e baixas. |

AUT-C08 deve rodar pelo menos 1.000 sequências pseudoaleatórias de comandos e
imprimir a seed na mensagem de falha.

### 5.4 Gate D — prognóstico e batalha

Ampliar `BattleCalculatorTests` com testes de propriedade:

| ID | Regra | Caso automatizado |
|---|---|---|
| AUT-D01 | REG-13 | Cada classe recebe 1,5 somente contra seu alvo favorecido. |
| AUT-D02 | REG-14 | Força exibida e força gravada no relatório são iguais em 10.000 composições. |
| AUT-D03 | REG-15 | Mesma entrada e regras sempre produzem resultado byte a byte equivalente. |
| AUT-D04 | REG-16 | Para valores válidos aleatórios, `0 ≤ perdas ≤ tropas`. |
| AUT-D05 | REG-16 | `0 ≤ vida final ≤ vida inicial ≤ 100`. |
| AUT-D06 | REG-17 | Aumentar defesa sem mudar a onda nunca aumenta dano ou baixas totais. |
| AUT-D07 | REG-17 | Aumentar ameaça sem mudar defesa nunca reduz dano. |
| AUT-D08 | REG-18 | Repelida paga `max(1, floor(ameaça×0,15))`; brecha paga zero. |
| AUT-D09 | Limites | Zero soldados, um de cada classe, vida 1 e ameaça máxima. |
| AUT-D10 | Limites | Quantidades próximas de `int.MaxValue` não causam wraparound. |
| AUT-D11 | Distribuição | Maiores restos conserva o total e desempata na ordem documentada. |
| AUT-D12 | Explicação | Todas as vantagens presentes aparecem no texto; nenhuma vantagem falsa aparece. |

Qualquer overflow encontrado em AUT-D10 é P0, mesmo que não seja alcançável
pela interface atual, porque um save editado pode fornecer essa entrada.

### 5.5 Gate E — transação, idempotência e eventos

Ampliar `BattleResolutionServiceTests`:

| ID | Regra | Caso automatizado |
|---|---|---|
| AUT-E01 | REG-15 | Chamar resolução 2, 10 e 1.000 vezes aplica consequências uma vez. |
| AUT-E02 | REG-15 | Trocar de cena no frame da resolução não duplica o evento nem a recompensa. |
| AUT-E03 | REG-15 | Reload antes e depois do commit produz um único resultado válido. |
| AUT-E04 | REG-18 | `MoneyChanged` ocorre após commit e contém o saldo final. |
| AUT-E05 | REG-19 | Confirmar relatório duas vezes salva apenas a primeira confirmação. |
| AUT-E06 | REG-19 | Capitão de outro portão não confirma o relatório. |
| AUT-E07 | Atomicidade | Falha injetada no save restaura onda, portão, tropas, ouro, relatório e sequência. |
| AUT-E08 | Eventos | Exceção de um observador não deve reaplicar a transação em nova atualização. |
| AUT-E09 | REG-20 | Brecha não agenda onda e congela o relógio. |
| AUT-E10 | REG-21 | Vitória final não agenda onda e congela o relógio. |

AUT-E08 merece atenção especial: o estado deve estar persistido antes da
publicação; uma exceção de UI nunca pode transformar um sucesso salvo em uma
segunda resolução.

### 5.6 Gate F — save, migração e corrupção

Adicionar `GameSaveRobustnessTests` com cópias representativas de cada versão:

| ID | Regra | Caso automatizado |
|---|---|---|
| AUT-F01 | REG-23 | Saves v3, v4, v5 e v6 migram para v7 preservando seus campos conhecidos. |
| AUT-F02 | REG-23 | Save v7 faz round-trip de quatro portões, trânsito, onda e relatório. |
| AUT-F03 | REG-15 | `AwaitingResolution` sem relatório resolve; `Resolved` legado não reaplica consequências. |
| AUT-F04 | Normalização | Listas nulas, itens nulos, portões duplicados e ondas duplicadas são normalizados. |
| AUT-F05 | Normalização | Vida, tropas, ouro, timestamps e perdas negativos são limitados corretamente. |
| AUT-F06 | Normalização | Enum desconhecido não produz NPC, destino ou resultado inválido. |
| AUT-F07 | Corrupção | JSON vazio, truncado, texto aleatório e JSON sintaticamente inválido têm comportamento definido. |
| AUT-F08 | Corrupção | NaN, infinito, sequência máxima e relatório inconsistente não quebram o runtime. |
| AUT-F09 | Recuperação | Falha ao gravar não apaga o último save válido. |
| AUT-F10 | Compatibilidade | A chave continua `tcc.game.save.v2`; nenhuma chave paralela é criada. |

Antes de implementar AUT-F07 e AUT-F09, definir a política desejada: recuperar
um save padrão e preservar o conteúdo inválido para diagnóstico, ou apresentar
erro explícito. Nunca apagar silenciosamente o único save do jogador.

### 5.7 Gate G — economia, construções e recrutamento

| ID | Regra | Caso automatizado |
|---|---|---|
| AUT-G01 | REG-24 | Upgrade cobra exatamente uma vez e falha atomicamente sem dinheiro. |
| AUT-G02 | REG-24 | Recrutar 1, 10 e quantidade digitada cobra preço × quantidade. |
| AUT-G03 | REG-24 | Estoque insuficiente, saldo insuficiente, zero, negativo e overflow não alteram nada. |
| AUT-G04 | REG-24 | Restock respeita nível da construção e ocorre uma vez no limite do relógio. |
| AUT-G05 | REG-24 | Imposto acumula e coleta exatamente uma vez nos limites de tempo. |
| AUT-G06 | REG-18 | Espólio e imposto no mesmo frame preservam a soma, independentemente da ordem. |
| AUT-G07 | Integração | Compra → reserva → envio → baixa mantém dinheiro e tropas consistentes após reload. |
| AUT-G08 | Visão econômica | Uma simulação representativa prova que o espólio não cobre continuamente a reposição. |

AUT-G08 não deve exigir um único estilo ótimo de jogo. Ele deve registrar custo
de reposição, renda urbana, espólio e saldo por onda para permitir calibragem.

### 5.8 Gate H — navegação, input e UI

Ampliar testes PlayMode carregando as cenas reais:

| ID | Regra | Caso automatizado |
|---|---|---|
| AUT-H01 | REG-01/09 | Visitar cada portão e voltar posiciona o jogador no marker correspondente. |
| AUT-H02 | REG-09 | Salvar no Centro e no Portão restaura cena, contexto e posição lógica. |
| AUT-H03 | REG-08 | Movimento contínuo em oito direções nunca ultrapassa os limites calculados da câmera. |
| AUT-H04 | REG-07 | Abrir cada menu oculta o prompt no mesmo frame. |
| AUT-H05 | REG-07 | Pressionar E, Esc e os dois rapidamente não deixa prompt ou lock órfão. |
| AUT-H06 | REG-06 | Dois modais/owners simultâneos exigem liberar ambos para retomar o tempo. |
| AUT-H07 | REG-10 | Construção, imposto, viagem, recrutamento e guarnição só abrem dentro do alcance do NPC correto. |
| AUT-H08 | REG-25 | Centro → Portão → Centro repetido 50 vezes mantém um HUD e um runtime. |
| AUT-H09 | REG-25 | HUD e banners possuem `raycastTarget=false`/raycaster desativado. |
| AUT-H10 | Layout | Em 1920×1080, 1366×768 e 2560×1080, cartões não cobrem ouro ou banner. |
| AUT-H11 | Relatório | Resolver fora do portão atualiza cartão correto e o Capitão correto. |
| AUT-H12 | Terminal | Vitória/derrota troca textos, bloqueia comandos e não deixa contador negativo. |

Testes de layout automatizados verificam retângulos e visibilidade, mas não
substituem inspeção visual e leitura humana.

### 5.9 Gate I — campanhas completas e soak

Criar `CampaignSimulationTests`:

| ID | Prioridade | Cenário |
|---|---|---|
| AUT-I01 | P0 | Campanha sem defesa termina em derrota, sem loop ou onda órfã. |
| AUT-I02 | P0 | Estratégia superdimensionada chega à vitória na onda 10, sem onda 11. |
| AUT-I03 | P1 | Estratégia balanceada completa 10 ondas com relatório e conservação após cada uma. |
| AUT-I04 | P1 | Rodar 1.000 campanhas com seeds sequenciais e validar todos os invariantes. |
| AUT-I05 | P1 | Salvar/recarregar antes de cada marco e depois de cada batalha em uma campanha completa. |
| AUT-I06 | P2 | Simular 24 horas estratégicas ou 100.000 atualizações sem crescimento indevido de filas/listas. |
| AUT-I07 | P2 | Medir alocações e quantidade de `AudioClip`, Canvas e objetos persistentes após 1.000 alertas. |

Cada falha de simulação deve informar seed, onda, portão, tropas, ameaça, vida,
ouro e últimos comandos para reprodução imediata.

### 5.10 Comandos e artefatos da rodada

Executar, nesta ordem:

```powershell
& 'D:\Unity\Editors\6000.3.11f1\Editor\Unity.exe' `
  -batchmode -nographics -projectPath 'D:\TCC-Game' `
  -runTests -testPlatform EditMode `
  -testResults 'D:\TCC-Game\Logs\FullEditModeResults.xml' `
  -logFile 'D:\TCC-Game\Logs\FullEditMode.log'

& 'D:\Unity\Editors\6000.3.11f1\Editor\Unity.exe' `
  -batchmode -nographics -projectPath 'D:\TCC-Game' `
  -runTests -testPlatform PlayMode `
  -testResults 'D:\TCC-Game\Logs\FullPlayModeResults.xml' `
  -logFile 'D:\TCC-Game\Logs\FullPlayMode.log'
```

Arquivar por rodada:

- Hash do commit.
- XML EditMode e PlayMode.
- Logs completos.
- Seeds de qualquer falha.
- Save usado para reprodução.
- Capturas ou vídeo de falhas visuais.
- Checklist manual preenchido.

## 6. Etapa manual — preparação

Antes de testar:

1. Criar uma build Development da revisão aprovada na automação.
2. Fechar o Editor para que apenas a build controle o PlayerPrefs.
3. Fazer cópia do save atual antes de testar migração ou corrupção.
4. Separar dois perfis: `SAVE-LIMPO` e `SAVE-EM-ANDAMENTO`.
5. Ativar gravação de tela e deixar visível resolução, FPS e horário.
6. Não consultar o código ou este plano durante os testes de descoberta M01 e
   M02; eles medem se o jogo se explica sozinho.

Para acelerar cenários tardios, é recomendável criar uma ferramenta de teste
somente para Development Build capaz de avançar tempo, escolher seed, alterar
ouro/tropas e preparar a onda 10. Ela não pode existir em builds de produção e
deve usar as mesmas APIs do jogo sempre que possível.

## 7. Etapa manual — roteiro funcional e de experiência

### M01 — leitura inicial da cidade

**Prioridade:** P0  
**Save:** limpo

1. Iniciar sem instruções externas e observar a tela por 30 segundos.
2. Caminhar pela cidade e localizar NPCs interativos.
3. Tentar descobrir como obter ouro, contratar e chegar a um portão.

**Aprovar se:** em até 5 minutos for possível descrever o ciclo “renda →
contratação → deslocamento → defesa” e realizar a primeira ação de cada tipo
sem ficar bloqueado. Registrar qualquer tentativa que pareça válida mas não
gere feedback.

### M02 — compreensão da onda

**Prioridade:** P0  
**Save:** primeira onda

1. Não olhar todos os cartões continuamente.
2. Ao notar o primeiro aviso, apontar portão, tempo restante e gravidade.
3. Aos 30 segundos, explicar a composição usando apenas a interface.
4. Dizer se a defesa parece favorável e qual unidade ajudaria mais.

**Aprovar se:** portão for identificado em até 5 segundos após o aviso; tempo e
prognóstico forem entendidos sem tentativa; e a relação de vantagem sugerir
uma decisão coerente.

### M03 — quatro rotas e markers

**Prioridade:** P0

Para Norte, Leste, Sul e Oeste:

1. Conversar com a sentinela correspondente.
2. Confirmar viagem e observar ponto de chegada no Portão.
3. Retornar ao Centro.
4. Confirmar que o jogador reaparece junto da mesma saída, sem atravessar
   cenário, câmera ou NPC.

Repetir o ciclo duas vezes por portão e uma vez após fechar/reabrir o jogo.

### M04 — limites de movimento

**Prioridade:** P0

1. Pressionar continuamente cada direção e cada diagonal nas cenas Centro e
   Portão.
2. Repetir perto de cantos, durante movimento da câmera e após uma transição.

**Aprovar se:** personagem inteiro permanece na área jogável, sem tremor,
teleporte, velocidade diagonal anormal ou posição impossível após reload.

### M05 — prompts, E e menus

**Prioridade:** P0

Executar com todos os menus: construção, coleta, viagem, recrutamento e Capitão.

1. Entrar e sair do alcance lentamente.
2. Abrir com E e verificar que o prompt some imediatamente.
3. Pressionar E repetidamente e alternar E/Esc rapidamente.
4. Fechar andando na direção do NPC e reabrir.
5. Tentar interagir com dois NPCs cujas áreas se sobreponham.

**Aprovar se:** existe um único prompt, um único menu, nenhum clique atravessa
o modal, movimento/tempo retomam ao fechar e nenhuma interação fica perdida.

### M06 — contratação e deslocamento

**Prioridade:** P0

1. Comprar cada classe em quantidades 1 e 10.
2. Tentar comprar sem estoque e sem dinheiro.
3. Enviar unidades Reserva → portão e entre dois portões.
4. Observar origem, trânsito, destino e HUD durante todo o percurso.
5. Salvar/sair no meio do trânsito e retornar.

**Aprovar se:** preço, estoque e quantidade são previsíveis; o jogador entende
que tropas em trânsito não defendem; não há duplicação, desaparecimento ou
chegada no portão errado.

### M07 — pausa temporal por modal

**Prioridade:** P0

1. Abrir cada modal próximo de 91, 31, 11 e 1 segundo.
2. Manter aberto por pelo menos 15 segundos reais.
3. Fechar e observar contador, chegada e alertas.

**Aprovar se:** nenhum tempo estratégico passa, batalha não resolve atrás do
menu e, ao fechar, o fluxo continua uma vez e sem saltos visuais indevidos.

### M08 — batalha, bônus e consequências

**Prioridade:** P0

Preparar três ondas equivalentes, cada uma dominada por um tipo de inimigo, e
usar a unidade favorecida e uma não favorecida com força base semelhante.

**Aprovar se:** o preview explica corretamente a vantagem; a configuração
favorecida tem resultado coerentemente melhor; baixas, dano e espólio vistos no
HUD coincidem com o relatório do Capitão e com o estado posterior.

### M09 — relatório do Capitão

**Prioridade:** P0

1. Receber um resultado estando no Centro ou em outro portão.
2. Localizar o indicador de relatório pendente.
3. Visitar primeiro um Capitão de outro portão e depois o correto.
4. Ler, confirmar, fechar e reabrir.
5. Reiniciar o jogo antes e depois da confirmação.

**Aprovar se:** o relatório correto é fácil de localizar, permanece até a
confirmação correta e nunca concede novo ouro ao ser reaberto.

### M10 — clareza e colisão do HUD

**Prioridade:** P0  
**Perfis:** todas as resoluções da matriz

1. Observar HUD seguro, anunciado, iminente, crítico, relatório pendente,
   vitória e derrota.
2. Abrir todos os menus e provocar banners.
3. Conferir ouro, quatro cartões, textos, banner e botões.

**Aprovar se:** nenhum elemento essencial se sobrepõe ou sai da tela; textos
não cortam; portão local é identificável; pulso não é agressivo; HUD não impede
cliques; informação continua compreensível sem áudio.

### M11 — áudio e sinais de alerta

**Prioridade:** P1

1. Ouvir alertas 90, 30, 10 e 0 em sequência normal e após salto de tempo.
2. Provocar resultado positivo e brecha.
3. Repetir com áudio desligado.

**Aprovar se:** sons não acumulam de forma desagradável, severidade é
distinguível e toda informação sonora possui equivalente visual.

### M12 — recuperação em pontos críticos

**Prioridade:** P0

Fechar e reabrir o jogo nos seguintes pontos:

- Antes e depois de comprar.
- Durante transferência.
- Em 91, 30, 10 e 0 segundos.
- Imediatamente após a batalha.
- Antes e depois de confirmar relatório.
- Após vitória e após derrota.

**Aprovar se:** nenhum recurso ou evento é duplicado, perdido ou reaplicado; a
cena e o portão ativos são restaurados coerentemente.

### M13 — derrota

**Prioridade:** P0

1. Deixar um portão vulnerável até chegar a zero.
2. Observar banner, cartões, tempo, comandos do Capitão e save após reload.

**Aprovar se:** derrota é inequívoca, não há espólio, nenhuma onda nova surge,
tempo fica parado e deslocamentos não são aceitos.

### M14 — campanha completa e vitória

**Prioridade:** P0

Jogar as dez ondas sem editar estado durante a rodada. Registrar após cada uma:

| Onda | Portão | Ameaça | Defesa | Baixas | Dano | Espólio | Renda | Gastos | Saldo |
|---:|---|---:|---:|---:|---:|---:|---:|---:|---:|
| 1–10 | | | | | | | | | |

**Aprovar se:** dificuldade cresce sem salto inexplicável; decisões de compra e
movimento importam; renda urbana tende a perder capacidade de reposição; onda
10 termina em vitória; onda 11 não aparece.

### M15 — estresse de interação

**Prioridade:** P1

Durante 10 minutos:

- Alternar cenas rapidamente.
- Pressionar E/Esc e clicar botões repetidamente.
- Despachar várias classes e destinos.
- Abrir menus no instante de alertas.
- Minimizar/restaurar e perder/recuperar foco da janela.

**Aprovar se:** não há exceção, duplicação, tela preta, input travado, áudio
preso, jogador invisível ou crescimento evidente de objetos.

### M16 — sessão prolongada

**Prioridade:** P2

Manter a build aberta por pelo menos 60 minutos, alternando períodos ativos e
ociosos. Registrar memória e FPS no início e no fim.

**Aprovar se:** não há degradação perceptível, alertas antigos reaparecendo,
objetos persistentes duplicados ou aumento contínuo de memória.

## 8. Sessões exploratórias

Além dos roteiros, reservar sessões de 30 minutos com uma missão, sem passos
fixos:

- EXP-01 — tentar enriquecer ou duplicar tropas usando cliques, reload e troca
  de cena.
- EXP-02 — tentar quebrar navegação e markers entrando e saindo rapidamente.
- EXP-03 — tentar deixar dois menus, dois prompts ou dois locks ativos.
- EXP-04 — tentar produzir contador negativo, alerta repetido ou onda órfã.
- EXP-05 — jogar deliberadamente “mal” e verificar se a derrota ainda é clara e
  recuperável como estado salvo.
- EXP-06 — ignorar textos e jogar apenas por cores/posição; depois desligar o
  áudio e repetir.

Registrar todas as ideias tentadas, inclusive as que não encontrarem defeitos.

## 9. Perguntas de experiência após a rodada

Responder de 1 a 5 e acrescentar uma frase de justificativa:

1. Ficou claro qual portão seria atacado?
2. Ficou claro quanto tempo restava?
3. Ficou claro por que uma composição era boa ou ruim?
4. Foi fácil descobrir onde contratar e movimentar soldados?
5. Foi fácil entender o que aconteceu depois da batalha?
6. O relatório ajudou a decidir a próxima ação?
7. A viagem entre cidade e portões pareceu previsível?
8. A pressão econômica pareceu justa, e não arbitrária?
9. Alertas chamaram atenção sem atrapalhar?
10. Em algum momento você não sabia o que fazer em seguida?

Qualquer resposta 1 ou 2 nas perguntas 1–7 bloqueia a aprovação de UX. Nas
perguntas 8 e 9, exige correção ou decisão explícita de design documentada.

## 10. Registro de defeitos

Usar este formato:

```text
ID:
Título curto:
Severidade: P0 / P1 / P2 / P3
Build/commit:
Ambiente e resolução:
Save/seed:
Pré-condições:
Passos numerados:
Resultado observado:
Resultado esperado:
Frequência: 1/1, 3/5 etc.
Evidência: vídeo, imagem, log, XML
Hipótese ou observação adicional:
```

Classificação:

- **P0 — bloqueador:** perda/corrupção de save, crash, duplicação de recursos,
  campanha impossível de concluir ou regra central incorreta.
- **P1 — grave:** navegação/input travado, batalha/portão errado, HUD essencial
  ilegível, modal não pausa ou resultado inconsistente.
- **P2 — moderado:** feedback confuso, layout ruim em perfil suportado, áudio
  excessivo ou ação recuperável com dificuldade.
- **P3 — menor:** texto, alinhamento, acabamento ou inconsistência cosmética.

## 11. Critério de aprovação da entrega

Uma revisão está pronta para demonstração somente quando:

- Compilação não possui erros.
- 100% dos testes EditMode e PlayMode passam em duas execuções consecutivas.
- Gates AUT-A a AUT-I P0 foram implementados e aprovados.
- M01 a M10 e M12 a M14 foram executados no perfil principal.
- M03, M04, M05 e M10 foram repetidos no perfil compacto.
- Não existe P0 ou P1 aberto.
- Todo P2 possui correção planejada ou aceite explícito.
- Uma campanha completa foi vencida e uma derrota foi reproduzida.
- Save limpo, migração e reload em ponto crítico foram verificados.
- Checklist, XML, logs, saves e evidências estão arquivados.

## 12. Ordem recomendada de implementação dos novos testes

1. Robustez do save e idempotência de eventos: AUT-E e AUT-F.
2. Propriedades de combate e conservação de tropas: AUT-C e AUT-D.
3. Limites de ondas e campanha completa: AUT-B e AUT-I.
4. Integração de economia: AUT-G.
5. Navegação, input, UI e layout real: AUT-H.
6. Executar a etapa manual somente depois dos gates P0 automatizados.

Essa ordem prioriza defeitos capazes de corromper progresso ou recursos antes
de investir tempo na validação subjetiva da experiência.

## 13. Matriz de rastreabilidade

`Existente` indica que a regra já possui cobertura direta na baseline;
`Parcial` indica cobertura de caminho feliz sem todos os limites deste plano;
`Nova` indica uma lacuna que deve ser implementada.

| Regra | Automação principal | Manual principal | Baseline |
|---|---|---|---|
| REG-01 | AUT-A03, AUT-H01 | M03 | Parcial |
| REG-02 | AUT-B04, AUT-I04 | M02, M14 | Parcial |
| REG-03 | AUT-B05 | M14 | Existente |
| REG-04 | AUT-B01–B03 | M02, M14 | Parcial |
| REG-05 | AUT-B07–B08 | M02, M07, M11 | Parcial |
| REG-06 | AUT-H06 | M07 | Parcial |
| REG-07 | AUT-H04–H05 | M05 | Parcial |
| REG-08 | AUT-H03 | M04 | Parcial |
| REG-09 | AUT-H01–H02 | M03, M12 | Parcial |
| REG-10 | AUT-H07 | M01, M05 | Nova |
| REG-11 | AUT-C01–C03, AUT-C05–C08 | M06 | Parcial |
| REG-12 | AUT-C04 | M06, M08 | Existente |
| REG-13 | AUT-D01 | M08 | Parcial |
| REG-14 | AUT-D02 | M08 | Parcial |
| REG-15 | AUT-D03, AUT-E01–E03, AUT-E07–E08 | M08, M12 | Parcial |
| REG-16 | AUT-D04–D05, AUT-D09–D11 | M08 | Parcial |
| REG-17 | AUT-D06–D07 | M08 | Parcial |
| REG-18 | AUT-D08, AUT-G06, AUT-I03 | M08, M14 | Parcial |
| REG-19 | AUT-E05–E06, AUT-H11 | M09 | Parcial |
| REG-20 | AUT-E09, AUT-I01 | M13 | Existente |
| REG-21 | AUT-B09, AUT-E10, AUT-I02 | M14 | Parcial |
| REG-22 | AUT-C07, AUT-H12 | M13, M14 | Parcial |
| REG-23 | AUT-F01–F10 | M12 | Parcial |
| REG-24 | AUT-G01–G08 | M01, M06, M14 | Parcial |
| REG-25 | AUT-A04, AUT-H08–H10 | M10, M15, M16 | Parcial |

Nenhuma regra pode permanecer sem pelo menos um teste automatizado e um teste
manual associado, exceto quando houver justificativa técnica registrada junto
à aprovação da rodada.

## 14. Folha de execução manual

Preencher uma linha por perfil executado. Estados permitidos: `Não executado`,
`Aprovado`, `Falhou` ou `Bloqueado`.

| Caso | Perfil/resolução | Estado | Defeitos | Evidência | Observações |
|---|---|---|---|---|---|
| M01 | | Não executado | | | |
| M02 | | Não executado | | | |
| M03 | | Não executado | | | |
| M04 | | Não executado | | | |
| M05 | | Não executado | | | |
| M06 | | Não executado | | | |
| M07 | | Não executado | | | |
| M08 | | Não executado | | | |
| M09 | | Não executado | | | |
| M10 | | Não executado | | | |
| M11 | | Não executado | | | |
| M12 | | Não executado | | | |
| M13 | | Não executado | | | |
| M14 | | Não executado | | | |
| M15 | | Não executado | | | |
| M16 | | Não executado | | | |

**Responsável:**  
**Build/commit:**  
**Data e duração:**  
**Resultado da rodada:**  
**P0/P1 abertos:**  
**Decisão:** aprovado / reprovado / nova rodada necessária
