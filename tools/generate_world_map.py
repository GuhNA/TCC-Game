from pathlib import Path
from PIL import Image, ImageDraw
import random

atlas = Image.open('Assets/tileMaps/fantasyhextiles_v3.png').convert('RGBA')
out = Path('Assets/tileMaps/Maps/world-adventure-map-v1.png')
out.parent.mkdir(parents=True, exist_ok=True)

def crop(x, y):
    image = atlas.crop((x * 32, y * 48 + 2, x * 32 + 32, y * 48 + 50))
    px = image.load()
    for iy in range(image.height):
        for ix in range(image.width):
            r, g, b, a = px[ix, iy]
            if r < 12 and g < 12 and b < 12:
                px[ix, iy] = (0, 0, 0, 0)
    return image

tiles = {(x, y): crop(x, y) for y in range(6) for x in range(8)}
cols, rows = 49, 30
img = Image.new('RGBA', (1600, 1120), (13, 88, 143, 255))
random.seed(1907)

def tile_pos(q, r): return q * 32 + (16 if r % 2 else 0) + 15, r * 36 + 18
def paste(q, r, sprite):
    x, y = tile_pos(q, r)
    img.alpha_composite(tiles[sprite], (x - 16, y - 24))

def land(q, r):
    # Main island with a western bay and a thin eastern peninsula.
    x = (q - 25) / 21
    y = (r - 15) / 12
    body = x*x + y*y
    bay = ((q - 22) / 6)**2 + ((r - 11) / 5)**2 < 1
    peninsula = q > 38 and 11 < r < 20
    return (body < 1 and not bay) or peninsula

for r in range(rows):
    for q in range(cols):
        if not land(q, r):
            paste(q, r, (6, 0))
            continue
        if r < 8 and q > 27: base = (5, 0) # snowy north
        elif random.random() < .18: base = (2, 0)
        elif random.random() < .13: base = (1, 0)
        else: base = (0, 0)
        paste(q, r, base)

# Mountains, forests, towns, cave/ruins, ship and port.
for q, r in [(30,9),(31,10),(32,10),(33,11),(34,12),(14,18),(15,19),(16,20),(38,16)]: paste(q,r,(5,0))
for q, r in [(11,11),(13,12),(15,13),(17,14),(19,16),(35,18),(36,20),(39,19),(41,17),(28,21),(27,22)]: paste(q,r,(4,0))
for q, r, sprite in [(10,16,(0,1)),(22,8,(2,1)),(39,14,(6,1)),(24,23,(1,2)),(32,20,(6,2)),(43,17,(6,3)),(7,9,(6,2)),(18,24,(7,2))]: paste(q,r,sprite)
for q, r, sprite in [(6,20,(6,3)),(8,7,(6,3)),(44,8,(6,3))]: paste(q,r,sprite)

# Golden roads connect landmarks; they are intentionally separate from the hex artwork.
draw = ImageDraw.Draw(img)
route = [(10,16),(16,16),(22,8),(30,12),(39,14),(39,20),(32,20),(24,23),(18,24)]
for first, second in zip(route, route[1:]):
    ax, ay = tile_pos(*first); bx, by = tile_pos(*second)
    draw.line((ax, ay, bx, by), fill=(111,68,24,255), width=7)
    draw.line((ax, ay, bx, by), fill=(225,183,70,255), width=3)

for q, r, sprite in [(17,10,(6,3)), (21,13,(6,3)), (27,7,(6,3))]: paste(q,r,sprite)
img.save(out)
print(out.resolve())
