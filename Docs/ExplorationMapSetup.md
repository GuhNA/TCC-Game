# Configuração do mapa de exploração

## Relatório da região na Taverna

O objeto `relatorio` da cena Taverna possui `ExplorationAreaReportView`, associado
ao campo `Area Report View` do `ExplorationMapController`. O botão de relatório
do destino abre esse painel. A seleção de expedição e de guarda continua usando
o painel de seleção existente; cancelar a seleção de guarda retorna ao relatório.

- `Local` apresenta o nome da região selecionada.
- `Valor D` apresenta **exploradores no local / exploradores livres para envio**.
  Os livres são calculados pela construção de exploradores, descontando perdas
  e todos os exploradores já alocados nas regiões capturadas.
- O Content instancia `Assets/Prefabs/Item.prefab` para cada recurso de
  `Possible Rewards`, usando o ícone e o nome do Resource. Entradas repetidas
  do mesmo recurso são agrupadas.
- Nos itens, **D** é o total coletado nessa região, aguardando transporte.
  **T** é o máximo por ciclo: soma das quantidades máximas possíveis do recurso
  multiplicada pelos exploradores locais. Como D acumula vários ciclos, pode
  superar T; T não é um limite de armazenamento nem uma promessa de drop.
- Os slots de aventureiros seguem a ordem da guarda salva no local, mostram
  retrato, nome e vida atual/máxima e ocultam os espaços não ocupados.
- `AtaqueOn` exibe o tempo restante em `mm:ss` usando o relógio da exploração.
  Um ataque pendente mantém `00:00`; sem ataque agendado ou pendente, aparece
  `SemAtaque`.
- Enviar, Retirar, Transportar, Definir guarda e Voltar usam as ações existentes
  do controller. Quantidades e cronômetro são atualizados com o painel aberto.

Todos os textos TMP do relatório e do Item usam Pixelify Sans, inclusive os
estados inicialmente desativados. As referências podem ser ajustadas pelo
Inspector, sem depender dos nomes dos objetos em runtime.

## Fluxo implementado

1. O jogador interage com o objeto que possui `ExplorationMapController`.
2. O mapa abre e atualiza todos os `ExplorationDestinationButton`.
3. Um botão liberado abre a popup de seleção.
4. O controller cruza o catálogo de `Char_base` com os IDs contratados no save.
5. Um `AdventurerSelectionCardView` é criado para cada aventureiro contratado.
6. O jogador seleciona até três aventureiros e confirma o envio.

## Hierarquia recomendada do Canvas

```text
ExplorationMapCanvas
├─ MapPopup                         (mapPopupRoot)
│  ├─ MapImage
│  ├─ CloseButton                    (closeMapButton)
│  ├─ ColinasButton                 (+ ExplorationDestinationButton)
│  ├─ ShadowForestButton            (+ ExplorationDestinationButton)
│  └─ ...um botão por ambiente
└─ PartySelectionPopup              (partySelectionPopupRoot)
   ├─ DimmedBackground
   └─ Window
      ├─ DestinationTitle            (selectedDestinationText)
      ├─ EmptyRosterMessage          (emptyRosterMessage)
      ├─ Scroll View                 (+ ScrollRect)
      │  ├─ Viewport                 (+ RectMask2D)
      │  │  └─ Content               (partyListContent)
      │  │     ├─ GridLayoutGroup
      │  │     └─ ContentSizeFitter
      │  └─ Scrollbar Vertical
      ├─ ConfirmButton               (confirmPartyButton)
      └─ BackButton                  (backButton)
```

## Scroll View e cartões uniformes

No `ScrollRect`, marque `Vertical`, desmarque `Horizontal`, associe `Viewport`,
`Content` e `Scrollbar Vertical`. Use `Movement Type = Clamped` ou `Elastic`.

No `Viewport`, use `RectMask2D`. No `Content`, configure:

- âncoras no topo, esticadas horizontalmente;
- pivot `(0.5, 1)`;
- `GridLayoutGroup` com `Constraint = Fixed Column Count` e `Count = 3`;
- `Start Corner = Upper Left`;
- `Child Alignment = Upper Center`;
- uma única `Cell Size` para todos os cartões, por exemplo `340 x 300`;
- `Spacing`, por exemplo `20 x 20`;
- `ContentSizeFitter`: horizontal `Unconstrained`, vertical `Preferred Size`.

Três colunas por duas linhas deixam seis cartões visíveis, embora apenas três
possam ser enviados por batalha. O sétimo cartão cria
uma terceira linha, aumenta a altura preferida do `Content` e ativa a rolagem.

## Prefab do cartão

Crie um prefab com `AdventurerSelectionCardView` e ligue:

- `Portrait Image`: imagem do aventureiro;
- `Name Text`: nome do asset `Char_base`;
- `Stats Text`: vida, dano, velocidade, armadura, resistência e tipo de dano;
- `Selection Button` e seu texto;
- `Selected Highlight`: borda opcional exibida quando selecionado.

O `Selected Highlight` precisa ser filho do próprio prefab. Uma referência para
um objeto solto da cena seria compartilhada por todos os cartões e não seria
clonada corretamente. O prefab incluído no projeto já possui a `RedMedal`
desativada e vinculada a esse campo. Ao selecionar, o script a ativa e a move
para o fim da ordem dos irmãos para renderizá-la acima do restante do cartão.

Não adicione `GridLayoutGroup` em cada cartão. O tamanho e espaçamento uniformes
pertencem ao `Content` do Scroll View.

## Botões do mapa

Adicione `ExplorationDestinationButton` em cada botão `Enviar`, escolha seu
`Environment` e ligue o próprio `Button` e o texto. Em
`Locked State Prefab Or Instance`, é possível colocar tanto o prefab de botão
bloqueado quanto um objeto filho já existente na cena. Quando recebe um prefab,
o script o instancia automaticamente como irmão do botão normal e alterna os
dois estados. `Locked State Parent` é opcional; sem ele, será usado o pai do
botão `Enviar`. Depois coloque todos esses componentes no array
`Destination Buttons` do `ExplorationMapController`.

Se preferir manter os dois estados montados na cena, use um objeto pai neutro
para o componente, coloque os objetos de estado normal e bloqueado como irmãos,
e associe `Available State Root` e `Locked State Prefab Or Instance`. Essa é a
configuração mais simples para ajustar posições visualmente.

Os primeiros destinos são Colinas do Vento, Floresta das Sombras e Caverna do
Gelo. As demais regiões são liberadas pela tabela em `ExplorationProgression`.

## Catálogo e save

`Adventurer Catalog` é preenchido automaticamente no Editor com todos os assets
`Char_base`. Em runtime, somente definições cujo `PersistenceId` esteja em
`PlayerInventoryService.RecruitedAdventurerIds` geram cartões.

As vitórias são persistidas em `ExplorationSaveData.completedEnvironments`.
Quando a batalha terminar, ela deve chamar:

```csharp
GameSessionProvider.Current.Exploration.SetStatus(
    missionId,
    ExplorationMissionStatus.Won
);
```

Essa chamada conclui a região, salva o progresso e permite que o mapa libere os
próximos destinos na próxima atualização.

## Fechar e validar o grupo

Crie um `Button` dentro de `MapPopup` e arraste-o para `Close Map Button` no
`ExplorationMapController`. Não é necessário configurar manualmente o evento
`On Click`: o controller registra `CloseModal` quando é ativado. O botão
`Back Button` continua fechando somente a seleção e voltando para o mapa.

O botão de confirmação só fica interagível quando existe pelo menos um
aventureiro selecionado. A mesma regra também é validada por
`ExplorationService`, impedindo um envio vazio mesmo por chamadas de código.
