# Taverna — fachada piloto, etapa 3

> Registro histórico: esta etapa já foi integrada às cenas finais. As cenas de protótipo e seus geradores foram retirados; veja o [registro da limpeza](ApprovedPrototypeCleanup.md). As imagens e aprovações foram preservadas.

Status: usuário testou e aprovou o piloto. Baseline registrada no cenário `centro-tavern-facade`. A continuidade está em [CentroExteriorSetReview.html](CentroExteriorSetReview.html), com o novo terreno e os demais edifícios.

Aplicação limitada à cena `Assets/Scenes/Prototypes/CentroLayoutPrototype.unity`.
A área V2 continua em 1.040 u²; o jogador e os pontos de interação mantêm a escala aprovada.

## Arte e integração

- Novo exterior: `Assets/Images/Buildings/Prototypes/TavernFacadePilotV1.png`.
- Gerado pela ferramenta integrada **imagegen**, com transparência real. A primeira versão foi revisada para reduzir a largura e remover o halo externo.
- Fonte final de 1.254 × 1.254 pixels; importação Point, sem mipmaps e sem compressão, 115,6 pixels/u, escala uniforme 1.
- Pivô na soleira central, medido na imagem. Silhueta visível com cerca de 9,3 u de altura; porta com aproximadamente 2,6 u de altura, próxima da referência de 2,7 u. Essas medidas visuais são aproximadas, não dimensões de um colisor novo.
- Lote físico preservado: 9,05 × 5,66 u. A placa lateral e o telhado podem ultrapassar visualmente a base; a colisão acompanha a base do prédio.
- Ordem de desenho calculada pelos pés. Ao passar atrás do volume alto, a fachada fica a 30% de opacidade para manter o jogador visível.
- Edifício no nível 0 usa a representação provisória do lote; após construir, mostra a fachada.
- Porta e gestão continuam separadas. Nenhuma alteração na cena Centro de produção, no interior de produção ou na lista de cenas do jogo.

Um trecho de calçamento de 9,05 × 3,2 u foi aplicado diante da entrada, com o sprite `Assets/Images/Buildings/Prototypes/TavernStreetPilotV1.png`, também gerado por imagegen. A textura é repetida pelo SpriteRenderer, sem esticar as pedras nem adicionar colisores. O restante do terreno e as outras quatro construções continuam provisórios; a transição das bordas do piso pertence à integração do terreno na etapa 4. O piloto serve para revisar proporção, perspectiva, materiais e entrada antes de produzir o conjunto.

## Reprodução

No Unity, menu **TCC Game → World → Build Tavern Facade Pilot**.
O gerador normal do protótipo também reinstala a fachada quando o sprite está disponível.
Abra a cena CentroLayoutPrototype e dê Play; a partida usa dados temporários.

Captura isolada: `tools/Test-Visual.ps1 -Scenario centro-tavern-facade`.
Teste funcional único: `tools/Test-CentroLayoutPrototype.ps1`.
Artefatos: `Artifacts/VisualTests/centro-tavern-facade/`.
As imagens são candidatas à revisão humana, sem promoção automática de baseline.

## Verificação desta entrega

- Geração/importação no Unity concluída sem erro de compilação.
- Teste PlayMode de circulação: **1/1 passou**, incluindo o nível 0 sem fachada pronta, construção com exibição do exterior, acesso aos serviços, bloqueio físico e ida/volta da Taverna. A partida salva permanece preservada pelo teste existente.
- Quatro capturas em cenário visual próprio: fachada completa, entrada em 1920 × 1080, entrada em 1366 × 768 e passagem atrás do telhado.
- A inspeção das primeiras capturas identificou ordem de desenho acima do HUD e enquadramento baixo demais do personagem. Ambos foram corrigidos e as capturas atualizadas. Os ajustes seguintes atingiram apresentação e calçamento sem colisores; a suíte funcional não foi repetida.
- Aprovação artística ainda depende da revisão destas imagens. O sprite gerado serve como referência do próprio piloto, não como prova de aprovação prévia da arte.
- Conferência SHA-256 ao final: Centro.unity, Taverna.unity e EditorBuildSettings.asset iguais aos hashes registrados antes da criação do protótipo. As referências locais da página de revisão foram verificadas.

## Prompt inicial

Create a production game sprite: one medieval fantasy TAVERN EXTERIOR, isolated with real transparent alpha background. Textured crisp pixel art, carefully defined pixel clusters, warm oak timber framing, rough gray stone ground floor, cream plaster upper walls, terracotta red tile roof, iron fittings, two small warm lanterns and a hanging wooden sign with a clearly readable tankard pictogram, absolutely no lettering. Front elevation for a top-down 2D RPG: symmetric near-frontal facade with visible roof planes viewed slightly from above, vertical walls, horizontal foundation. Building silhouette approximately square, entire roof and chimney and foundation visible, modest transparent padding. Single generously sized entrance exactly at horizontal center at the bottom front, its threshold at the bottom edge of the building foundation, no projecting long stairs. Door dark welcoming wooden arch, about 29 percent of total building height and 15 percent of width, large relative to facade so a human character can enter without looking tiny. Main facade roughly 3.4 door heights tall and 6.7 door widths wide. Balanced two-storey volume, grounded medieval architecture and detailed material textures matching an adventurous strategy RPG. Avoid isometric diamond bases, exaggerated perspective, cutaway interiors, people, characters, detached props, ground terrain, scenery, painted shadows outside silhouette, labels, HUD, border, duplicated doors, banners, photorealism, smooth vector art, blur. Output one complete standalone transparent exterior sprite, no sheet.

## Prompt final de revisão

Edit target: the supplied tavern sprite. Preserve its beautiful textured medieval pixel art, terracotta tiles, warm timber, stone base, tankard sign and single central entrance. Correct the silhouette to be almost square (building width INCLUDING SIGN approximately equal to total height), narrower building, taller second storey, fewer windows if necessary. Single central door opening height 29% of total building height, width 15% total facade width. CRITICAL remove ALL surrounding glow, colored atmosphere, gradient, shadows and scenery: clean actual transparent alpha immediately outside the hard pixel silhouette. No bloom. Full complete building visible with small transparent margins, roof and foundation not clipped. Threshold at bottom center, no stairs or ground. This must be a standalone transparent game sprite, not a scene illustration. Keep the tankard sign attached to wall, no words, no characters. The supplied image is the edit target, not an extra background.

## Prompt do calçamento

A seamless repeatable square texture tile of medieval street cobblestones for a top-down 2D pixel art RPG. Entire canvas filled edge to edge with small uneven warm gray and taupe stone pavers, subtle worn stone texture and narrow dark earthy mortar, subdued contrast so a colorful player character remains readable. Orthographic directly overhead, consistent stone scale, crisp pixel clusters, detailed restrained medieval fantasy game art. No perspective, no frame, no sidewalk border, no building, no grass, no characters, no objects, no text, no vignette, no directional gradient. Uniform ambient illumination. Continuous tileable pattern in all directions, opaque background.

## Placa de gestão da Taverna

No Centro, a placa de martelo tem o texto **MELHORAR TAVERNA** e fica junto ao
ponto de interação, separado da soleira. O comando contextual da placa abre
o modal de gestão, inclusive antes da construção. A arte foi gerada pelo
ImageGen integrado usando a fachada do protótipo como referência.
[TavernManagementSign.png](../../Assets/Images/Buildings/Prototypes/TavernManagementSign.png)

> Use case: stylized-concept. Create one small transparent in-world prop sprite for a medieval fantasy Unity village. Reference is the tavern scene screenshot; match only the warm wood, forged dark iron outlines and slightly downward frontal pixel-art perspective. A compact freestanding wooden work sign with a single sturdy short post and small stone footing, a broad oak plaque and a very readable gold hammer over a small upward-pointing chevron, a simple symbol for upgrading a building. Centered composition, full post and stone base, broad enough to recognize at around 90 pixels, restrained wood grain and pixel clusters. The existing tavern mug sign is a reference for the craft, do not include a mug. No letters, no frame outside the sign, no foliage, floor, cast shadow, ground plane or background. Genuine transparent alpha. One upright sprite, isolated.
