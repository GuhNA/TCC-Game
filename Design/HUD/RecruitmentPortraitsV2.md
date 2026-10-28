# Retratos do recrutamento — revisão de 27/10/2026

Imagens geradas com a ferramenta nativa de geração de imagens (sem CLI/API externa) e salvas em `Assets/Images/UI/Recruitment/PortraitsV2/`. Os recortes antigos foram preservados.

Referência: captura enviada pelo usuário com as três classes. A referência orienta estilo, cores e identidade, não a composição da interface.

## Integração e validação

- A bandeira direita, o saldo e o fechamento têm áreas sem sobreposição.
- O subtítulo ocupa uma faixa própria abaixo do título e acima dos cartões.
- As três artes novas são referenciadas diretamente pelo prefab; o gerador não
  volta a substituí-las por recortes da imagem de conceito.
- Rodapé afastado das molduras externas para preservar a leitura.
- Corrigida a verificação de objeto destruído em `AlertPopup` durante a saída da cena.
- Cinco capturas geradas, com inspeção de 1920 × 1080 e 1366 × 768.
- Verificações geométricas confirmaram a separação de bandeira/ouro/fechamento e
  a contenção do subtítulo em sua faixa.
- Teste de compra do recrutamento: **1/1 passou** na execução final.

[Galeria da revisão](RecruitmentPortraitRevisionReview.html).

### Margem inferior dos cartões

O botão de contratação agora mantém 16 unidades de UI de distância da moldura
inferior. O bloco de preço, total e disponibilidade foi reposicionado para
acompanhar a nova margem nos três cartões.

## WARRIOR

Arquivo: Warrior.png

Prompt final:

```text
Use case: stylized-concept. Asset type: standalone landscape troop portrait for a Unity medieval fantasy pixel-art recruitment HUD, displayed at 350 by 200 pixels. Input image 1 is ONLY a STYLE and CHARACTER reference (a screenshot with three cards); DO NOT generate the screenshot, UI, border or banners. Create a fresh high-quality SINGLE character illustration in the same handcrafted detailed pixel-art style, with crisp deliberate pixel clusters, restrained medieval fantasy, rich textured materials, readable silhouette, no painterly blur, no photorealism. Landscape composition, approximately 7:4 aspect ratio. Chest-up medium portrait, head horizontally centered at x=50% and upper torso centered, generous headroom and breathing room on every edge. The whole hood/helmet and important equipment must remain inside the central 80% of the picture, with space for a separately overlaid class badge in the upper-left corner. Character and equipment remain large and readable when downscaled to 350x200. Background: subdued stone castle courtyard, distant towers, muted blue sky and greenery, lower contrast than the character, visually consistent with the supplied reference. No embedded UI, frames, borders, banners, flags, emblems floating outside the subject, labels, writing or watermarks. Opaque full background, no transparency. Subject: ONE armored medieval warrior resembling the LEFT card reference: steel closed helmet with narrow visor and small red crest, red scarf, dark silver plate armor with restrained warm gold accents, a red shield held low at the right and a sword contained within the composition. Center helmet and face at the middle, rather than pushing him to the left. Both shoulders and shield silhouette readable, calm resolute posture.
```

## ARCHER

Arquivo: Archer.png

Prompt final:

```text
Use case: stylized-concept. Asset type: standalone landscape troop portrait for a Unity medieval fantasy pixel-art recruitment HUD, displayed at 350 by 200 pixels. Input image 1 is ONLY a STYLE and CHARACTER reference (a screenshot with three cards); DO NOT generate the screenshot, UI, border or banners. Create a fresh high-quality SINGLE character illustration in the same handcrafted detailed pixel-art style, with crisp deliberate pixel clusters, restrained medieval fantasy, rich textured materials, readable silhouette, no painterly blur, no photorealism. Landscape composition, approximately 7:4 aspect ratio. Chest-up medium portrait, head horizontally centered at x=50% and upper torso centered, generous headroom and breathing room on every edge. The whole hood/helmet and important equipment must remain inside the central 80% of the picture, with space for a separately overlaid class badge in the upper-left corner. Character and equipment remain large and readable when downscaled to 350x200. Background: subdued stone castle courtyard, distant towers, muted blue sky and greenery, lower contrast than the character, visually consistent with the supplied reference. No embedded UI, frames, borders, banners, flags, emblems floating outside the subject, labels, writing or watermarks. Opaque full background, no transparency. Subject: ONE medieval archer resembling the MIDDLE card reference: green hood, brown leather bracers and green tunic, young adult with a focused face fully visible at the horizontal center. Three-quarter stance aiming a wooden longbow toward the right, bow and both hands entirely within the image; keep the face at x=50%, bow at x=75%, enough room for all equipment, do not let the bow or arrow hide the face. Balanced chest-up framing and detailed crisp pixel rendering.
```

## MAGE

Arquivo: Mage.png

Prompt final:

```text
Use case: stylized-concept. Asset type: standalone landscape troop portrait for a Unity medieval fantasy pixel-art recruitment HUD, displayed at 350 by 200 pixels. Input image 1 is ONLY a STYLE and CHARACTER reference (a screenshot with three cards); DO NOT generate the screenshot, UI, border or banners. Create a fresh high-quality SINGLE character illustration in the same handcrafted detailed pixel-art style, with crisp deliberate pixel clusters, restrained medieval fantasy, rich textured materials, readable silhouette, no painterly blur, no photorealism. Landscape composition, approximately 7:4 aspect ratio. Chest-up medium portrait, head horizontally centered at x=50% and upper torso centered, generous headroom and breathing room on every edge. The whole hood/helmet and important equipment must remain inside the central 80% of the picture, with space for a separately overlaid class badge in the upper-left corner. Character and equipment remain large and readable when downscaled to 350x200. Background: subdued stone castle courtyard, distant towers, muted blue sky and greenery, lower contrast than the character, visually consistent with the supplied reference. No embedded UI, frames, borders, banners, flags, emblems floating outside the subject, labels, writing or watermarks. Opaque full background, no transparency. Subject: ONE elderly wizard resembling the RIGHT card reference: blue hooded robe with narrow gold trim, full white beard, centered face at x=50%, wooden staff held at the right x=76%, a glowing blue orb atop the staff entirely inside the image. Mystical but restrained medieval fantasy, face and beard readable, generous space above hood and staff. No flags or duplicate banners in background.
```
