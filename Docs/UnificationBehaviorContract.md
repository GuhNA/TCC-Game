# Contrato de comportamento — unificação Centro/Portão

Este documento congela o comportamento que deve permanecer estável durante a
unificação e separa explicitamente os defeitos que não devem ser preservados.

## Escopo unificado

- A cena Centro continua permitindo construir e melhorar quatro construções.
- O custo de melhoria continua sendo arredondado a partir de
  `baseUpgradeCost * upgradeCostMultiplier ^ currentLevel`.
- A produção do Mercado Local e o limite de 24 horas dos impostos permanecem
  inalterados.
- O vendedor do Portão continua abrindo com a ação de interação, exibindo o
  estoque de arqueiros, guerreiros e magos, e aceitando os botões 1, 10 e 100
  e a entrada manual.
- Os preços unitários permanecem: arqueiro 5, guerreiro 3 e mago 8.
- Mensagens, textos visíveis e duração dos alertas não serão alterados nesta
  refatoração.
- A Taverna é considerada consumidora da mesma economia, pois a contratação de
  aventureiros também gasta ouro.
- Aventureiros contratados e itens pertencem ao inventário persistente da
  sessão, identificado por IDs estáveis em vez de referências a assets.

## Fonte de verdade

- O save mantido pelo repositório é a única fonte de verdade para ouro,
  construções, tropas, estoque, relógios, aventureiros e itens.
- Os antigos `Inventario.asset` e `Cidade.asset` e os respectivos componentes
  foram removidos após a validação da migração.
- Se o save `tcc.city.save.v1` possuir ouro, esse valor tem precedência.
- Um jogador sem save começa com 1000 de ouro, preservando a regra atual do
  Centro. Valores presentes nos assets do projeto não substituem esse padrão.
- O formato atual é a versão 3. A chave `tcc.game.save.v2` foi mantida para que
  os saves da primeira etapa sejam atualizados de forma aditiva e idempotente,
  sem perda de estado.

## Entrada do jogador

- Movimento e interação usam exclusivamente as ações globais
  `Player/Move` e `Player/Interact` do novo Input System.
- A interação preserva a tecla E e também aceita o binding de controle já
  configurado no asset de ações.

## Interface compartilhada

- Centro e Portão exibem o saldo da sessão pelo mesmo prefab
  `GoldBalanceHUD`.
- O texto usa o formato `Ouro: {saldo}`, é preenchido ao carregar a cena e
  acompanha o evento `EconomyService.MoneyChanged`.
- O HUD é somente visual: ele não bloqueia raycasts nem mantém uma cópia do
  saldo.
- Os textos visuais do popup de venda usam TextMesh Pro, com fonte, material,
  cor e opacidade explícitos. O popup também usa `TMP_InputField`; não há
  `UnityEngine.UI.Text` ou `InputField` legado nessa interface.
- As definições de aventureiros e itens são somente dados; atributos calculados,
  vida e estado vivo pertencem à instância runtime do aventureiro.
- A ação `Player/Interact` é processada uma única vez pelo `PlayerInteractor`.
  Alvos de interação implementam `IInteractable` e não leem o input diretamente.
- O nível de cada construção pertence ao `BuildingService`. Componentes de cena
  apenas renderizam o nível recebido do serviço.
- Cada linha do vendedor é um `RecruitmentRowView` identificado por `UnitType`;
  botões e entradas não dependem de nomes de GameObjects.

## Limpeza após a migração

- As fachadas temporárias `CitySaveRepository` e `EconomyManager` foram
  removidas; todo acesso ao estado passa por `GameSession` e seus serviços.
- O componente genérico `GameManager` do Portão foi substituído pelo
  apresentador compartilhado `GoldBalancePresenter`.
- Eventos, objetos de resultado e métodos de compatibilidade sem consumidores
  foram removidos após a atualização das cenas e dos testes.
- Scripts e assets legados só podem ser removidos quando nenhuma cena, prefab,
  `UnityEvent`, teste ou migrador depender deles.

## Regras corrigidas

Os itens abaixo foram classificados como bugs. Desde a introdução do save v2 e
dos serviços compartilhados, todos possuem testes de regressão ativos e sem
`Ignore`.

### BUG-PORTAO-001 — preço total incorreto

O preço total é `quantidade * preço unitário`, com aritmética validada. Comprar
10 arqueiros custa 50, não 5.

### BUG-PORTAO-002 — compra sem saldo

Uma compra só pode ser confirmada quando há ouro e estoque suficientes. Em
caso de falha, ouro, estoque e tropas permanecem completamente inalterados.
Ouro e contagens nunca podem sofrer underflow ou ficar negativos.

### BUG-PORTAO-003 — quantidade ou entrada inválida

A quantidade deve ser um inteiro estritamente positivo. Campo vazio, zero,
número negativo, texto parcialmente numérico ou valor fora do intervalo são
rejeitados sem lançar exceção e sem alterar o estado.

### BUG-PORTAO-004 — renovação composta pelo estoque anterior

A renovação é uma reposição determinística, não um crescimento composto.
Enquanto não houver configuração própria por unidade, a capacidade de
compatibilidade é:

`floor(10 * (1 + 0.30 * nível da construção))`

O resultado depende apenas do nível, nunca do estoque restante. O nível zero
mantém capacidade 10.

### BUG-PORTAO-005 — data dependente de cultura e fuso

O intervalo de renovação continua sendo seis horas e é processado ao abrir o
vendedor. A comparação correta é `agora >= próxima renovação`. O novo save usa
segundos Unix em UTC. A string legada é aceita apenas pelo migrador, usando
formatos explícitos, e nunca volta a ser gravada.

### BUG-PORTAO-006 — textos do popup de venda invisíveis

Rótulos, estoque, cronômetro e legendas dos botões do popup usam o mesmo
renderizador TextMesh Pro. Fonte ou material ausente, texto transparente e
texto visual bloqueando raycasts são regressões cobertas pelos testes de cena.

### BUG-ESTADO-001 — duas fontes para ouro e construções

Centro, Portão e Taverna devem observar e alterar o mesmo saldo. Os níveis
`MagicSchool`, `ArcherStand`, `Barracks` e `LocalMarket` também devem ser os
mesmos em todas as cenas.

## Critérios de regressão

- Os testes de caracterização existentes devem continuar passando durante a
  refatoração.
- Os testes de regressão descrevem o resultado correto, não o comportamento
  defeituoso anterior.
- Cada regra corrigida deve continuar passando sem alteração do resultado
  esperado.
- Nenhum UnityEvent ou referência serializada pode ficar ausente durante a
  migração.
