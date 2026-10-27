# Maga roxa — canalização e respiração

## Arquivos

- `Sprites/MagePurple_Channel_12f_v1.png`: 1536 × 1152 px, 4 colunas × 3 linhas, 12 poses, ciclo de 2,5 s.
- `Sprites/MagePurple_Breathe_8f_v1.png`: 1536 × 768 px, 4 colunas × 2 linhas, 8 poses, ciclo de 2 s.
- `Sprites/MagePurple_Rest_v1.png`: pose comum de repouso.
- `Clips/`: dois AnimationClips que animam somente `SpriteRenderer.m_Sprite`, sem curvas de posição, rotação ou escala.
- `Preview/mage_preview.html`: reprodução dos PNGs originais, com linha de apoio e marca de pivot.
- `Preview/*.gif`: prévias reduzidas a 256 cores; use os PNGs no jogo.
- `prompts.md`: prompts da ferramenta integrada imagegen e preparação técnica.

## Slice / Inspector

As imagens e os `.meta` devem ser mantidos juntos. Os `.meta` fornecidos já contêm o recorte e o pivot de cada sprite.

| Campo | Valor |
| --- | --- |
| Texture Type | Sprite (2D and UI) |
| Sprite Mode | Multiple |
| Slice | Grid by Cell Size |
| Pixel Size | **384 × 384** |
| Offset / Padding | 0 / 0 |
| Pivot | Custom: **X 0.5; Y 0.041666667** |
| Pixels Per Unit | **100**, igual nos dois arquivos |
| Filter Mode | Point (no filter) |
| Compression | None |
| Generate Mip Maps | desativado |
| Mesh Type | Full Rect |
| Alpha Is Transparency | ativado |

Leitura dos frames: esquerda para direita, de cima para baixo. Na canalização use `_00` até `_11`; na respiração `_00` até `_07`. Os clips têm taxa-base de 12 samples/s, mas cada pose dura um número diferente de samples para acentuar repouso e pulso.

O pivot fica no centro horizontal da célula, a 16 px da borda inferior. Não use Automatic nem pivots calculados individualmente pela caixa que inclui a magia: o efeito muda de tamanho e deslocaria a personagem.

## Uso dos clips

Use um Animator e um SpriteRenderer no mesmo GameObject. Adicione os dois clips ao seu Animator e deixe a respiração como estado padrão. Para uma canalização acionada por evento, desative Loop Time no clip de canalização; para repetir a canalização como idle alternativo, deixe ativado. A transição de retorno deve ocorrer ao final do clip (Exit Time 1; duração de transição 0). A pose neutra de ambos é idêntica.

Não foi alterada nenhuma cena, prefab ou controller existente. O pacote adiciona somente imagens, configurações de importação e clips.

## Validação

Verificados por leitura dos pixels: dimensões, transparência, ausência de cortes nas bordas, última linha visível y=367 em todos os frames, e igualdade RGBA dos frames inicial/final entre os dois clips. Os desenhos internos não são pixel a pixel idênticos: há pequenas variações de formas entre as poses geradas. Consulte `validation.json`.

Os clips foram preparados no formato do projeto; a reprodução no Animation/Animator da Unity não foi testada nesta entrega.
