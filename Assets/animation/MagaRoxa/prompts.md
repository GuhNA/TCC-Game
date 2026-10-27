# Prompts e processo

Modo: ferramenta integrada de geração de imagens (imagegen). Sem CLI/API externa.
Referência: maga de robe roxo, linha 3 / coluna 2 do roster anexado pelo usuário.
A segunda referência de estrelas não estava anexada; foram usadas formas originais de quatro e oito pontas.

## Canalização

Use case: identity-preserve
Asset type: Unity 2D pixel-art animation sprite sheet, RGBA transparent.
Reference image 1: character identity and art style. Animate ONLY the purple-robed novice woman in ROW 3 COLUMN 2 (at x39%, y60%): light tan skin, long wavy black hair tied loosely high, purple patched shabby robe, rope belt, cloth shoes, crooked wooden branch staff with violet magic. Preserve her face, proportions, novice equipment, purple robe patches, hair color and detailed crisp pixel-art shading.
Primary request: Draw EXACTLY TWELVE consecutive full-body frames of one continuous idle spell-channeling animation. Arrange 4 equal columns by 3 equal rows. Reading order left to right, top to bottom. Requested canvas 2048 x1536, 512x512 cells. This is one character across time, NOT different character designs.
REGISTRATION IS CRITICAL: use the very same two stationary feet in every cell, anchored around local x=256 and a common ground line local y=480. Same character pixel size (body about 330 pixels tall). Same hips and torso root coordinates throughout. No walking, no stepping, no camera movement, no resizing. Leave generous empty margins for moving staff, hair and magic. Do not individually center the artwork's changing outer bounding box. Cloth and hair can sway while shoes remain anchored.
Camera: fixed three-quarter view facing slightly screen-right, same angle every frame.
Animation:
1. Neutral/rest: eyes open, branch staff rests on ground at screen-left beside her body, free hand relaxed near waist. Small violet ember on branch tip, no hand fireball.
2. Both arms move staff inward halfway; feet stay anchored.
3. Staff upright in CENTER in front of chest, both hands hold it; head bows gently and eyes close.
4. Eyes shut, head bowed, tiny violet energy dashes begin spiraling inward toward staff tip; loose black hair and lower robe start flowing outward from staff breeze.
5. Energy gathers into a SMALL original four-point violet star hovering over the centered staff. Hair and robe sway.
6. Star reaches full size: bright lavender core, purple edges matching original staff magic, thin inward energy streaks. Closed eyes.
7. Star rotates clockwise and contracts to a TINY almost invisible spark, with an eight-point spiky shape briefly visible.
8. Star expands again to full size, now a rotated eight-point star, still violet. Hair and robe sway in opposite direction.
9. Star rotates further, changes back to four elongated points and shrinks to half size.
10. Energy star fades to a violet ember; wind settles, head begins lifting, staff begins moving to the side.
11. Staff returns almost to ground at screen-left; eyes reopen, cloth settles.
12. Return EXACTLY to frame 1: staff planted beside her on ground, same open-eyed relaxed neutral pose and identical feet. Must connect to a second breathing animation in this same stance.
Style: match supplied detailed medieval fantasy pixel art; small sharp visible pixels, dark charcoal outlines, grouped dark shading, warm dusty cloth, worn wood. Violet effect uses sharp pixel clusters and short lines, no foggy painted glow.
Background: real transparent alpha, including between limbs. No floor, no shadows, no labels, no numbers, no drawn grid, no UI, no checkerboard printed in art. Full staff, shoes, all star particles contained in each cell. No extraneous people. No hood or hat added. No change of outfit. Keep moving arms anatomical and connected.

## Respiração

Use case: identity-preserve
Asset: second Unity animation sprite sheet for EXACTLY the same purple novice mage, a gentle breathing idle.
Reference 1: the EXACT neutral/rest frame of the already created channeling animation. Preserve this identity, camera, clothes, silhouette, stance, hair length, face and staff. This pose must be shared between both animations.
Draw EXACTLY 8 frames, 4 columns x 2 rows, in time order left-right then next row, requested canvas 1448 x724 with 362x362 cells. The source reference itself is a 362x362 cell. At this drawing scale replicate that cell's artwork size and exact feet locations in every cell. Do not enlarge or shrink the character to fill space. Keep her foot positions and ground contact line about y350 in each cell; foot locations around x120 and x300. The branch staff is planted on the ground at screen-left beside her, its tip touching the same ground line throughout all 8 frames. Staff is not waved.
1 EXACT same pose as reference: open eyes, relaxed free hand near waist, staff beside body, violet ember on top.
2 subtle inhale begins: chest and shoulders rise 1 pixel, feet and staff tip immobile.
3 inhale midway: shoulders rise 2 pixels, slight hair settling, same robe patches.
4 inhale peak: chest 3 pixels up; same body mass and scale. Eyes blink closed softly.
5 exhale begins: shoulders 2 pixels above neutral; eyes open.
6 exhale midway: 1 pixel above neutral.
7 neutral reached, slightly settling loose hair.
8 EXACT same rest pose as first frame and reference, completing seamless loop.
Only subtle upper-body breathing; legs, shoes, pelvis placement and staff ground contact static. No spell star, no wind or pose changes from the channeling animation. Keep small violet staff ember restrained and nearly static.
Detailed crisp medieval fantasy PIXEL ART exactly matching reference. Dark outlines, clustered dark shadows, dusty patched purple robe, rope belt, wooden branch, brown cloth shoes, warm skin, black wavy tied hair. Do not invent hat, cape, jewelry, ornaments.
Real transparent alpha. No backdrop, no floor, no shadow, no text, no labels, no visible grid. Equal margins around whole frames, no effects crossing cells. No pasted checkerboard.

## Correção pontual do pulso

Use case: precise-object-edit
Edit target: supplied 12-frame mage sprite sheet, 4 columns x3 rows, 1448x1086 pixels. Preserve this exact image and all character poses, feet registration, scale, colors, staff motion, clothes, hair, transparent background and grid. Change ONLY the star/magic in these specified cells, counting left to right then top to bottom.
Frame 7 = row2 column3: this is the MINIMUM of the pulse. Shrink the current star to a tiny almost-null violet/lavender point, about 3 pixels across. Reduce surrounding magic cloud accordingly to a few short inward streaks so the star clearly contracts almost to nothing.
Frame 8 = row2 column4: full expansion after the minimum. Keep the full-size eight-point violet star, but rotate it clockwise about 22.5 degrees so points are visibly diagonal compared with the upright four-point star in frame6. Sharp pixel sparkle, consistent purple hue.
Frame 9 = row3 column1: half-sized FOUR-point star visibly rotated clockwise 45 degrees, thin violet energy strokes.
No other edits. Do not redraw the mage, reposition parts, change her face or move her shoes. Transparent PNG, preserve exact original sheet grid and dimensions.

## Preparação técnica

As dimensões efetivamente retornadas foram diferentes das solicitadas. Os frames completos foram recortados e transladados por âncoras de apoio; a sequência de respiração recebeu escala uniforme 0,8 em nearest-neighbor para compatibilizar o tamanho. As duas folhas finais usam células transparentes de 384 × 384, âncora local (192, 368) medida a partir do topo. O frame de repouso da respiração foi usado inteiro no início e no fim de ambas as sequências. Nenhum membro foi reconstruído por recortes retangulares. Isso não torna os desenhos internos pixel a pixel idênticos: pequenas variações de desenho entre poses permanecem.
