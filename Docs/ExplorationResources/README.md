# Recursos de exploração

Entrega: **96 recursos novos**, sendo 64 recursos regionais e 32 materiais associados aos monstros. A Madeira existente foi preservada, totalizando 97 definições de recurso na pasta. Arte produzida por Arte Astra com a ferramenta integrada imagegen, a partir dos prompts deste diretório e do roster de inimigos do projeto.

## Arquivos e uso na Unity

| Conteúdo | Caminho | Dimensões reais | Organização |
|---|---|---|---|
| 64 recursos regionais | `Assets/Images/Exploration/Items/exploration_resources_atlas_v1.png` | 1254 × 1254 | 8 colunas × 8 linhas |
| 32 drops de monstros | `Assets/Images/Exploration/Items/monster_drops_atlas_v1.png` | 1774 × 887 | 8 colunas × 4 linhas |
| 96 ScriptableObjects novos | `Assets/Scriptable/Exploration/Resources/` | — | Nome, identificador persistente, descrição e ícone |
| Recompensas por local | `Assets/Scriptable/Exploration/Encounters/` | — | Campo Possible Rewards dos 14 encontros |

As imagens possuem transparência real. Os sprites **já estão recortados e nomeados nos arquivos .meta**, com Sprite Mode Multiple, filtro Point, sem mipmaps, sem compressão, Full Rect, 100 pixels por unidade e pivot central (0.5, 0.5).

A geração entregou dimensões diferentes das solicitadas. Para preservar os pixels, os recortes usam limites inteiros arredondados proporcionalmente à grade. A primeira folha tem células com 156 ou 157 pixels de largura/altura; a segunda, 221 ou 222. A tabela exata de cada retângulo está em `atlas_validation.json` e no respectivo `.meta`. As células são contíguas; o espaço transparente ao redor dos objetos está dentro de cada recorte. A conferência verificou que nenhum objeto com alpha de pelo menos 128 toca o limite de sua célula. A margem mínima solicitada de 14 pixels não foi atingida em todos os itens; o menor intervalo entre objeto e limite de célula é 1 pixel.

Os sprites já prontos podem ser selecionados pelo nome no Sprite Editor. Como as dimensões não dividem exatamente pela grade, não existe um único Grid Cell Size inteiro que reproduza todos os retângulos atuais. As coordenadas do catálogo usam linha e coluna começando em 1, de cima para baixo e da esquerda para a direita.

## Locais e recompensas

| Local | Entradas em Possible Rewards | Exemplos de recursos |
|---|---:|---|
| Colinas do Vento | 16 | Pedra, fibras, ervas, cobre, cristal do vento e materiais goblins |
| Floresta das Sombras | 15 | Madeira escura, cogumelos, raízes, resina, ectoplasma e galhada amaldiçoada |
| Caverna de Gelo | 12 | Cristais de gelo, prata, líquen, safira e materiais de criaturas gélidas |
| Torre Antiga | 14 | Pergaminho, fragmentos rúnicos, pó arcano e materiais de mortos-vivos |
| Vila do Mar | 7 | Algas, conchas, sal marinho, peixe, madrepérola e esponja |
| Floresta Esmeralda | 11 | Folhas, frutas, flor vital, resina dourada e materiais de xamãs e lobos |
| Montanhas Gélidas | 14 | Prata, quartzo, líquen, safira, materiais de troll, yeti e lobo |
| Reino Real | 9 | Ferro, trigo, linho, cera, selo real e fragmentos de armadura |
| Ilha Verde | 11 | Coco, bambu, palmeira, frutas tropicais e materiais goblins |
| Caverna Sombria | 12 | Ferro, carvão, salitre, cogumelo luminoso e materiais espectrais |
| Montanha de Fogo | 8 | Obsidiana, enxofre, pedra-pomes, núcleo ígneo e armadura negra |
| Deserto dos Ventos | 10 | Areia, cacto, flor do deserto, fulgurito, ouro e materiais de mortos-vivos |
| Santuário | 12 | Incenso, água benta, relicário, pó bendito e materiais arcanos/profanos |
| Ilhas Coral | 7 | Coral, pérola, madrepérola, esponja, algas, conchas e sal |

Há 158 associações no total: 157 adicionadas nesta entrega e a entrada de Madeira que já existia nas Colinas do Vento. Todos os 96 itens novos estão associados a pelo menos um local. Os demais campos dos encontros foram preservados, incluindo inimigos, cenários e parâmetros de combate.

As origens dos drops foram propostas a partir do roster: goblins, hobgoblin, xamãs, espectros, esqueletos, necromante, cavaleiro negro, zumbi, troll, yeti e lobos. O sistema atual sorteia recompensas **por local**, sem consultar a espécie de cada inimigo derrotado. Os materiais foram distribuídos tematicamente pelas regiões; não foi implementada uma tabela nova de drops por monstro.

## Balanceamento inicial

| Categoria sugerida | Chance por entrada | Quantidade quando sorteado |
|---|---:|---:|
| Comum | 65% | 1–3 |
| Incomum | 32% | 1–2 |
| Raro | 12% | 1 |
| Épico | 4% | 1 |

A Madeira das Colinas do Vento conserva sua configuração anterior: 100%, de 1 a 4 unidades. Nas outras regiões adicionadas, Madeira usa o padrão comum.

As categorias acima são orientações do catálogo, e não um novo campo de raridade nos ScriptableObjects. As porcentagens e quantidades são propostas próprias para revisão. A distribuição por biomas se inspira no princípio de recursos específicos por ambiente descrito no [FAQ oficial de Valheim](https://www.valheimgame.com/faq/); os números não foram copiados desse jogo.

Cada entrada é sorteada de forma independente. No código atual, a mesma lista alimenta as vitórias e a coleta passiva dos exploradores; a coleta repete os sorteios por trabalhador. Portanto, o volume total aumenta com a quantidade de trabalhadores. Esses valores não foram calibrados por testes extensos da economia do jogo.

Para balancear, selecione o encontro em `Assets/Scriptable/Exploration/Encounters` e altere **Drop Chance**, **Minimum Quantity** e **Maximum Quantity** em **Possible Rewards** no Inspector.

## Catálogo e verificação

- `catalog.csv`: 96 itens, nomes em português, raridades sugeridas, origem, folha, linha, coluna e regiões.
- `rewards.csv`: mapa das associações e dos valores propostos. A coluna `preservedExisting` identifica a entrada preservada; os valores dessa linha são propostas, e a configuração efetiva da Madeira continua sendo 100% / 1–4 nas Colinas do Vento.
- `manifest.json`: especificação estruturada dos 96 itens e da distribuição regional.
- `prompts.md`: os dois prompts completos encaminhados para Arte Astra.
- `atlas_validation.json`: dimensões efetivas, transparência, margens, hashes das imagens e retângulos exatos.
- `unity_validation.json`: resultado da validação real na Unity.

Os CSVs documentam a entrega; editar um CSV não altera automaticamente os assets no jogo.

**Validação concluída na Unity 6000.3.11f1**, em projeto isolado com cópias dos arquivos e as classes reais de recurso/encontro: 96 recursos, 96 sprites, 14 locais e 158 entradas de recompensa; nenhuma referência ausente, nome incorreto, retângulo incorreto ou quantidade inválida. Foram exercitados 100 sorteios por entrada. Também foi conferida a preservação integral da Madeira e dos campos dos encontros fora de Possible Rewards.

A validação cobre importação e dados. Não substitui um teste da interface, do combate completo ou do balanceamento econômico na cena do jogo.
