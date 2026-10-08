# Configuração do combate de exploração

## Fluxo pronto

1. O jogador envia de um a três aventureiros pelo mapa.
2. Enquanto viajam, o botão do destino mostra `A caminho...`.
3. Após cinco segundos, o mesmo botão muda para `Batalhar`.
4. Ao clicar, a missão é salva como `InBattle` e a cena
   `ExplorationCombat` é carregada.
5. A cena consulta o asset da região, sorteia de um a três integrantes do pool
   de inimigos, troca o fundo, instancia os prefabs 2D e mostra as recompensas.
6. `Batalhar` executa os turnos em ordem de velocidade. `Recuar` encerra a
   missão e retorna ao mapa.
7. Uma vitória salva a região como concluída, libera as próximas regiões e
   inicia o retorno da equipe. Os recursos só entram no inventário quando o
   retorno termina.
8. Ao selecionar novamente uma região capturada, o painel mostra a vida final
   de cada aventureiro e os recursos obtidos.

## Onde configurar cada região

Os 14 encontros já foram criados em:

`Assets/Exploration/Encounters`

Abra, por exemplo, `ColinasDeVento.asset` e preencha:

- `Background`: imagem de fundo usada na cena de combate;
- `Enemies`: de um a três inimigos;
- `Minimum/Maximum Enemy Count`: tamanho sorteado da equipe inimiga;
- `Allow Repeated Enemies`: permite sortear a mesma definição mais de uma vez;
- `Possible Rewards`: recurso, chance de obtenção e quantidades mínima/máxima.

O catálogo central fica em:

`Assets/Resources/Exploration/EncounterCatalog.asset`

Ele já contém todos os ambientes e é carregado automaticamente. Não é
necessário duplicar a cena de combate para cada região.

## Estrutura visual da cena

O fundo e os seis pontos de combatentes agora são objetos 2D no mundo, não
imagens dentro do Canvas. O Canvas permanece somente como interface para título,
detalhes, log, recompensas e botões.

- `Environment Background`: recebe o sprite da região e o ajusta à câmera sem
  deformar a imagem;
- `Adventurer Team/Slot 1..3`: pontos dos prefabs dos aventureiros;
- `Enemy Team/Slot 1..3`: pontos dos prefabs inimigos;
- `Exploration Combat Canvas`: somente controles e textos de interface.

Para recriar essa organização manualmente, use o menu:

`TCC > Exploration > Create or Update Combat Scene`

O gerador abre a cena de combate de forma aditiva e não fecha a cena que está
sendo editada.

## Como criar inimigos

No Project, use:

`Create > TCC > Exploration > Enemy`

Salve preferencialmente em `Assets/Exploration/Enemies` e configure:

- identificador persistente e nome exibido;
- retrato/sprite usado como alternativa;
- `Combat Prefab`, preferencialmente contendo apenas SpriteRenderers, Animator
  e scripts estritamente visuais;
- posição, rotação, escala e espelhamento locais do visual;
- vida máxima;
- dano mínimo e máximo;
- velocidade;
- armadura e resistência mágica;
- tipo de dano físico ou mágico.

Cada equipe aceita no máximo três integrantes. A restrição existe no envio,
nos assets de encontro e no motor de combate.

A lista `Enemies` funciona como um pool de possibilidades e pode possuir mais
de três tipos. Com repetição habilitada, um único asset `Goblin` no pool pode
gerar três goblins na mesma batalha.

Os aventureiros possuem os mesmos campos de visual de combate em seus assets
`Char_base`. Se `Combat Prefab` estiver vazio, o sistema mostra automaticamente
o sprite já existente e normaliza sua altura para caber no ponto da equipe.

Evite usar diretamente o prefab completo do Player, pois scripts de movimento,
interação, câmera ou áudio poderiam ser ativados também na batalha. Crie um
prefab visual separado quando o personagem tiver esses componentes.

## Como criar recursos

No Project, use:

`Create > TCC > Exploration > Resource`

Salve preferencialmente em `Assets/Exploration/Resources`. O identificador do
resource é usado pelo inventário e poderá ser consumido futuramente por crafting
e construções da cidade.

Sugestões de distribuição:

- Colinas do Vento: madeira leve, ervas e couro;
- Floresta das Sombras: madeira sombria, cogumelos e essência obscura;
- Caverna do Gelo: minério congelado e essência de gelo;
- Torre Antiga: fragmentos arcanos e páginas de grimório;
- Montanha de Fogo: obsidiana e núcleo ígneo;
- Ilhas Coral: coral, pérolas e sal alquímico;
- Santuário: relíquias e cristais sagrados.

## Regras atuais dos turnos

- todos os combatentes vivos são ordenados por velocidade no início da rodada;
- valores maiores agem primeiro;
- aventureiros acertam sempre o inimigo vivo mais à esquerda;
- inimigos acertam sempre o aventureiro vivo mais à direita;
- o dano é sorteado entre mínimo e máximo;
- dano físico é reduzido por armadura;
- dano mágico é reduzido por resistência mágica;
- fórmula: `danoFinal = max(1, danoSorteado - defesaRelevante)`;
- os pontos de vida dos aventureiros são salvos durante o combate.
- o histórico de golpes é acumulado até o fim e apresentado em um Scroll View.

## Retorno, relatórios e guarda

- o retorno inicial dura `ExplorationService.ReturnSeconds` (cinco segundos);
- as recompensas ficam anexadas à missão durante o retorno;
- ao final, o inventário recebe os recursos e o mapa libera o relatório;
- o relatório reutiliza os cards da seleção e mostra a vida que cada membro
  conservou ao terminar o combate;
- no relatório, `Definir guarda` permite selecionar de um a três aventureiros;
- após cinco minutos, a região recebe um contra-ataque;
- se houver guarda, uma missão de defesa fica imediatamente pronta para batalha;
- sem guarda, o botão da região muda para `Defender` e exige o envio de equipe;
- vencer a defesa sorteia recompensas novamente e agenda o próximo ataque;
- perder ou recuar de uma defesa devolve a região aos inimigos.

## Construção e coleta dos exploradores

O asset inicial da construção foi criado em:

`Assets/Scripts/Scriptables/Buildings/ExplorersLodge.asset`

Cada nível da `Construção de Exploradores` libera exatamente um explorador. O
nível da construção é o limite global, portanto um edifício nível 3 permite
distribuir três exploradores entre todas as regiões capturadas.

Custos iniciais configurados, todos editáveis no Inspector:

- nível 1: 200 ouro e 5 Madeira;
- nível 2: 350 ouro e 10 Madeira;
- nível 3: 550 ouro e 20 Madeira;
- nível 4: 800 ouro e 35 Madeira;
- nível 5: 1200 ouro e 50 Madeira.

Para colocar a construção na cidade:

1. duplique um GameObject de construção existente na cena `Centro`;
2. substitua seu `Building Definition` por `ExplorersLodge`;
3. configure `Unbuilt Sprite` e `Level Sprites` no asset;
4. inclua o novo GameObject no array `Buildings` do `CityManager`.

O sistema de construções agora aceita, para qualquer edifício, um array
`Level Upgrade Costs`. O elemento zero é o custo para construir o nível 1, o
elemento um melhora para o nível 2 e assim por diante. Cada elemento aceita ouro
e uma lista de recursos de exploração. A operação é atômica: nada é consumido
se faltar ouro ou qualquer recurso.

No relatório de uma região capturada aparecem os botões `+ Explorador` e
`- Explorador`. Eles também podem ser atribuídos manualmente nos campos do
`ExplorationMapController`; quando vazios, o controlador cria cópias do botão
Confirmar em tempo de execução.

Cada explorador atribuído realiza uma coleta a cada 60 segundos usando a tabela
`Possible Rewards` daquela região. Mais exploradores fazem mais rolagens por
ciclo. A coleta entra no inventário, aparece no relatório e pausa enquanto a
região estiver sob contra-ataque.

## Informações ainda necessárias para preencher o conteúdo

O sistema e a cena estão prontos, mas os encontros foram deixados sem inimigos
para não inventar o balanceamento do jogo. Para preencher cada região serão
necessários:

1. sprites/backgrounds de cada local;
2. nomes, sprites e atributos dos inimigos;
3. quais recursos cada local fornece;
4. chance e quantidade de cada recompensa.

Depois que esses dados forem definidos, basta criar os assets e arrastá-los para
os encontros correspondentes; nenhuma nova cena ou alteração de código será
necessária.
