"""Import generated atlases and append exploration resources without replacing user data.

Run with the bundled Python (Pillow required) from any working directory.
The two imagegen atlases must already exist. No artwork is generated or edited here.
"""
import argparse
import csv
import hashlib
import json
import re
import uuid
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
DOCS = Path(__file__).resolve().parent
MANIFEST = json.loads((DOCS / 'manifest.json').read_text(encoding='utf-8'))
ITEMS = ROOT / 'Assets/Images/Exploration/Items'
RESOURCES = ROOT / 'Assets/Scriptable/Exploration/Resources'
ENCOUNTERS = ROOT / 'Assets/Scriptable/Exploration/Encounters'
NAMESPACE = uuid.UUID('a956802a-00ca-4505-8b72-3391b5443ef5')
SCRIPT_GUID = '81271d94b44f47948a3a763708b768e2'


def stable_guid(key):
    return uuid.uuid5(NAMESPACE, key).hex


def sprite_file_id(item):
    return int.from_bytes(hashlib.sha256(item['persistenceId'].encode()).digest()[:8], 'big') & ((1 << 63) - 1)


def read_guid(path):
    return re.search(r'^guid: ([0-9a-f]{32})$', path.read_text(encoding='utf-8'), re.M).group(1)


def quote(text):
    return json.dumps(text, ensure_ascii=False)


def inspect_images():
    result = []
    for sheet in MANIFEST['sheets']:
        path = ITEMS / (sheet['id'] + '.png')
        if not path.exists():
            raise FileNotFoundError(f'Atlas ainda não gerado: {path}')
        with Image.open(path) as image:
            if 'A' not in image.getbands():
                raise ValueError(f'{path.name}: canal alpha ausente')
            width, height = image.size
            cw, ch = width / sheet['columns'], height / sheet['rows']
            alpha = image.getchannel('A')
            if alpha.histogram()[0] / (width * height) < 0.25:
                raise ValueError(f'{path.name}: fundo opaco ou pouco espaço entre os itens')
            cells = []
            for item in (i for i in MANIFEST['items'] if i['sheet'] == sheet['id']):
                x, y = round((item['column'] - 1) * cw), round((item['row'] - 1) * ch)
                right, bottom = round(item['column'] * cw), round(item['row'] * ch)
                cell_width, cell_height = right - x, bottom - y
                cell = alpha.crop((x, y, right, bottom))
                visible = cell.point(lambda p: 255 if p > 32 else 0)
                bounds = visible.getbbox()
                if not bounds:
                    raise ValueError(f'Célula vazia: {item["name"]}')
                solid = cell.point(lambda p: 255 if p >= 128 else 0)
                solid_bounds = solid.getbbox()
                if not solid_bounds:
                    raise ValueError(f'Célula sem objeto opaco: {item["name"]}')
                margin = min(solid_bounds[0], solid_bounds[1], cell_width - solid_bounds[2], cell_height - solid_bounds[3])
                if margin < 1:
                    raise ValueError(f'Objeto encosta na borda da célula: {item["name"]}, margem={margin}')
                cells.append({'id': item['id'], 'row': item['row'], 'column': item['column'], 'rect': {'x': x, 'y': height - bottom, 'width': cell_width, 'height': cell_height}, 'visibleBounds': bounds, 'solidMargin': margin})
            result.append({**sheet, 'actualWidth': width, 'actualHeight': height, 'nominalCellWidth': cw, 'nominalCellHeight': ch, 'uniformCells': width % sheet['columns'] == 0 and height % sheet['rows'] == 0, 'transparentPixels': alpha.histogram()[0], 'sha256': hashlib.sha256(path.read_bytes()).hexdigest(), 'cells': cells})
    return result


def atlas_meta(sheet):
    path = ITEMS / (sheet['id'] + '.png.meta')
    guid = read_guid(path) if path.exists() else stable_guid('texture/' + sheet['id'])
    reference = ROOT / (MANIFEST['reference'] + '.meta')
    source = reference.read_text(encoding='utf-8')
    prefix = source.split('  spriteSheet:\n', 1)[0]
    prefix = re.sub(r'^guid: .*$', 'guid: ' + guid, prefix, flags=re.M)
    items = [i for i in MANIFEST['items'] if i['sheet'] == sheet['id']]
    table = '  internalIDToNameTable:\n' + ''.join(f'  - first:\n      213: {sprite_file_id(i)}\n    second: {i["id"]}\n' for i in items)
    prefix = re.sub(r'  internalIDToNameTable:\n.*?(?=  externalObjects:)', table, prefix, flags=re.S)
    prefix = re.sub(r'^(\s*textureCompression:) \d+', r'\g<1> 0', prefix, flags=re.M)
    maximum_size = max(2048, 1 << (max(sheet['actualWidth'], sheet['actualHeight']) - 1).bit_length())
    prefix = re.sub(r'^(\s*maxTextureSize:) \d+', r'\g<1> ' + str(maximum_size), prefix, flags=re.M)
    prefix = re.sub(r'^  spriteMeshType: .*$', '  spriteMeshType: 0', prefix, flags=re.M)
    prefix = re.sub(r'^  spriteGenerateFallbackPhysicsShape: .*$', '  spriteGenerateFallbackPhysicsShape: 0', prefix, flags=re.M)
    rows = []
    for item in items:
        rect = next(c['rect'] for c in sheet['cells'] if c['id'] == item['id'])
        rows.append(f'''    - serializedVersion: 2
      name: {item['id']}
      rect:
        serializedVersion: 2
        x: {rect['x']}
        y: {rect['y']}
        width: {rect['width']}
        height: {rect['height']}
      alignment: 0
      pivot: {{x: 0.5, y: 0.5}}
      border: {{x: 0, y: 0, z: 0, w: 0}}
      customData:
      outline: []
      physicsShape: []
      tessellationDetail: -1
      bones: []
      spriteID: {stable_guid('sprite/' + item['persistenceId'])}
      internalID: {sprite_file_id(item)}
      vertices: []
      indices:
      edges: []
      weights: []
''')
    settings = {'sliceOnImport': False, 'gridCellCount': {'x': sheet['columns'], 'y': sheet['rows']}, 'gridSpriteSize': {'x': round(sheet['nominalCellWidth']), 'y': round(sheet['nominalCellHeight'])}, 'gridSpriteOffset': {'x': 0, 'y': 0}, 'gridSpritePadding': {'x': 0, 'y': 0}, 'pivot': {'x': 0.5, 'y': 0.5}, 'pivotPixels': {'x': 0, 'y': 0}, 'autoSlicingMethod': 0, 'spriteAlignment': 0, 'pivotUnitMode': 0, 'slicingType': 2, 'keepEmptyRects': False, 'isAlternate': False}
    suffix = '''    outline: []
    customData:
    physicsShape: []
    bones: []
    spriteID: ''' + stable_guid('sheet/' + sheet['id']) + '''
    internalID: 0
    vertices: []
    indices:
    edges: []
    weights: []
    secondaryTextures: []
    spriteCustomMetadata:
      entries:
      - key: SpriteEditor.SliceSettings
        value: ''' + "'" + json.dumps(settings, separators=(',', ':')) + "'\n" + '''    nameFileIdTable:
''' + ''.join(f'      {i["id"]}: {sprite_file_id(i)}\n' for i in items) + '''  mipmapLimitGroupName:
  pSDRemoveMatte: 0
  userData: exploration-resources-v1
  assetBundleName:
  assetBundleVariant:
'''
    return path, guid, prefix + '  spriteSheet:\n    serializedVersion: 2\n    sprites:\n' + ''.join(rows) + suffix


def resource_asset(item, texture_guid):
    regions = [r['name'] for r in MANIFEST['regions'] if item['id'] in r['items']]
    description = ('Material associado a ' + item['source'] + '. ' if item['index'] > 64 else 'Recurso regional de exploração. ') + 'Encontrado em: ' + ', '.join(regions) + '.'
    return f'''%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {SCRIPT_GUID}, type: 3}}
  m_Name: {item['id']}
  m_EditorClassIdentifier: TCCGame.Runtime::TCCGame.Gameplay.Exploration.ExplorationResourceDefinition
  persistenceId: {item['persistenceId']}
  displayName: {quote(item['name'])}
  description: {quote(description)}
  icon: {{fileID: {sprite_file_id(item)}, guid: {texture_guid}, type: 3}}
'''


def asset_meta(guid):
    return f'''fileFormatVersion: 2
guid: {guid}
NativeFormatImporter:
  externalObjects: {{}}
  mainObjectFileID: 11400000
  userData:
  assetBundleName:
  assetBundleVariant:
'''


def install(sheets):
    # Plan every mutation first. Conflicts or malformed input abort before writing.
    writes = []
    textures = {}
    for sheet in sheets:
        path, guid, content = atlas_meta(sheet)
        writes.append((path, content))
        textures[sheet['id']] = guid
    resources = {}
    for item in MANIFEST['items']:
        path = RESOURCES / (item['id'] + '.asset')
        meta = path.with_suffix('.asset.meta')
        guid = read_guid(meta) if meta.exists() else stable_guid('resource/' + item['id'])
        if path.exists():
            old = path.read_text(encoding='utf-8')
            if not re.search(r'^  persistenceId: ' + re.escape(item['persistenceId']) + '$', old, re.M):
                raise ValueError(f'Outro recurso já usa o caminho {path}')
            # Keep manual descriptions/names/balance after the first installation.
            if not meta.exists():
                raise ValueError(f'Recurso existente sem meta: {path}')
        else:
            writes.append((path, resource_asset(item, textures[item['sheet']])))
            writes.append((meta, asset_meta(guid)))
        resources[item['id']] = guid
    rewards = []
    for region in MANIFEST['regions']:
        path = ENCOUNTERS / (region['asset'] + '.asset')
        old = path.read_text(encoding='utf-8')
        match = re.search(r'^  possibleRewards:(?: \[\])?\s*$', old, re.M)
        if not match:
            raise ValueError(f'possibleRewards não encontrado: {path}')
        # Preserve all existing entries and append only missing references.
        after = old[match.end():]
        end = re.search(r'^  [A-Za-z_][A-Za-z_0-9]*:', after, re.M)
        block = after[:end.start()] if end else after
        existing = set(re.findall(r'guid: ([0-9a-f]{32})', block))
        new_entries = []
        requested = [(resources[key], next(i for i in MANIFEST['items'] if i['id'] == key)) for key in region['items']]
        if region['includeExistingWood']:
            requested.append((MANIFEST['existingWood']['guid'], {'id': 'Madeira', 'name': 'Madeira', 'chance': 0.65, 'min': 1, 'max': 3}))
        for guid, item in requested:
            if guid not in existing:
                new_entries.append(f'  - resource: {{fileID: 11400000, guid: {guid}, type: 2}}\n    dropChance: {item["chance"]}\n    minimumQuantity: {item["min"]}\n    maximumQuantity: {item["max"]}\n')
            rewards.append({'region': region['name'], 'resource': item['id'], 'name': item['name'], 'proposedChance': item['chance'], 'minimum': item['min'], 'maximum': item['max'], 'preservedExisting': guid in existing})
        if new_entries:
            prefix = old[:match.start()] + '  possibleRewards:\n'
            content = prefix + block.strip('\n') + ('\n' if block.strip() else '') + ''.join(new_entries)
            if end:
                content += after[end.start():]
            writes.append((path, content))
    for path, content in writes:
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(content, encoding='utf-8', newline='\n')
    with (DOCS / 'catalog.csv').open('w', encoding='utf-8-sig', newline='') as f:
        writer = csv.writer(f, delimiter=';')
        writer.writerow(['ID', 'Nome', 'Raridade sugerida', 'Folha', 'Linha', 'Coluna', 'Origem', 'Locais'])
        for item in MANIFEST['items']:
            writer.writerow([item['id'], item['name'], item['rarity'], item['sheet'], item['row'], item['column'], item['source'], ', '.join(r['name'] for r in MANIFEST['regions'] if item['id'] in r['items'])])
    with (DOCS / 'rewards.csv').open('w', encoding='utf-8-sig', newline='') as f:
        writer = csv.DictWriter(f, fieldnames=list(rewards[0]), delimiter=';')
        writer.writeheader()
        writer.writerows(rewards)
    (DOCS / 'atlas_validation.json').write_text(json.dumps(sheets, ensure_ascii=False, indent=2), encoding='utf-8')
    print(json.dumps({'newResources': len(resources), 'regions': len(MANIFEST['regions']), 'rewardAssociations': len(rewards), 'filesWritten': len(writes)}, ensure_ascii=False))


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--inspect-only', action='store_true')
    args = parser.parse_args()
    sheets = inspect_images()
    if args.inspect_only:
        print(json.dumps(sheets, ensure_ascii=False, indent=2))
    else:
        install(sheets)
