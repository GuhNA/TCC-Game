# Arte do fundo estático do Centro

As três gerações usaram o ImageGen integrado. A primeira foi um estudo de
composição feito com a máscara da planta V3 e a captura geral da V2. A imagem
gerada de 1347 × 1168 desviou alguns acessos e ficou abaixo da densidade de
108 pixels por unidade do enquadramento em 1080p.

O Unity renderiza o fundo definitivo em 6480 × 5616 a partir dos caminhos da
planta e das texturas de origem. Usa as novas artes de terra e grama com o
calçamento V2. O jogo carrega um único sprite para o chão.

## Estudo de composição

> Use case: stylized-concept. Edit target is ground-guide.png, a strict location mask for a Unity 2D map. Style reference is overview-1920x1440.png, use only its warm medieval pixel art palette and orthographic ground texture, never its content. Repaint the guide into ONE production game ground sprite, orthographic overhead with zero perspective, filling the entire 15:13 landscape canvas edge to edge. Preserve image bounds and every road centerline and branch as closely as possible, especially the upper T with two short upward ends, the lower right L, and the four wide cardinal roads meeting in a rounded plaza. Gray is warm gray and muted brown hand-clustered cobblestone, tan is compacted warm dirt and worn grass edges, green is olive mossy grass with local patches, tufts, little field flowers and scattered small pebbles flush with ground. Tiny crisp pixel clusters, sufficiently fine dense native pixel art, detailed and readable at gameplay zoom. Stones around one percent of image width or smaller, natural ragged silhouettes along edges, light subtle variation, no procedural grid appearance, warmer than the flat mask. Keep paths continuous right up to their endpoints; leave quiet open grass in all building lots. Surface details only; no buildings, characters, trees, props, walls, raised objects, icons, signs, lettering, borders, shadows from absent objects, interface, gradients or vignette. Produce the largest native resolution possible at approximately 15:13 with exact full-canvas alignment.

## Grama

[GrassFine.png](../../Assets/Images/World/CentroTerrainV2/GrassFine.png)

> Use case stylized-concept. Generate ONE square seamless terrain texture for Unity, pure orthographic top-down overhead grass ground for a sophisticated medieval village pixel-art RPG, like detailed hand-painted late-1990s PC RPG sprites. Every edge must tile. The camera zooms in so use dense, very fine crisp pixel clusters, not chunky blocks: hundreds of varied, tiny blades and multi-pixel grass strokes across the image, small scattered light straw tufts and tiny cream flowers. Low, flat walkable mown meadow of olive and moss greens, slightly warm daylight, restrained contrast, natural irregular scattered detail and small bare flecks. Thousands of high-frequency details but broad quiet tone. The image is a material swatch entirely filled with grass, no trails, roads, trees, objects, borders, gradients, text or perspective. Finest largest native resolution. Keep scale consistent with a 120 px tall pixel character, blade length around 3–8 pixels on the source image. Zero blurry or oversized clusters.

## Terra

[DirtFine.png](../../Assets/Images/World/CentroTerrainV2/DirtFine.png)

> Use case stylized-concept. Generate ONE square seamless ground texture for Unity, pure orthographic top-down overhead compacted ochre dirt material for the path of a sophisticated medieval village pixel-art RPG, like detailed late-1990s PC RPG pixel sprites. All four edges tile. Dense fine crisp native pixel art, hand-placed minute grain, small earth clumps, pale sand flecks and a scattering of very small embedded gray stones and tiny dried grass hints. Varied but quiet warm honey-brown, toasted ochre and tan with subtle depth. Tiny fine-scale detail down to individual pixels, no large solid blotches or mosaic blocks. This is one flat material swatch across the entire square, no road shape, grass field, borders, objects, text, perspective, vignette, or gradients. Finest largest native resolution. Pebble details usually 3–8 source pixels wide. Use natural pixel clusters with the delicacy of high-detail pixel art used on 120 px character sprites.
