"""Stage only the resource integration in an isolated project for the same Unity version."""
import argparse
import importlib.util
import json
import re
import shutil
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
DOCS = Path(__file__).resolve().parent


def stage(destination):
    destination = destination.resolve()
    if destination == ROOT or ROOT in destination.parents:
        raise ValueError('Use a separate validation directory outside the game project.')
    manifest = json.loads((DOCS / 'manifest.json').read_text(encoding='utf-8'))

    def copy(relative):
        source = ROOT / relative
        target = destination / relative
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(source, target)

    for sheet in manifest['sheets']:
        path = 'Assets/Images/Exploration/Items/' + sheet['id'] + '.png'
        copy(path)
        copy(path + '.meta')
    for item in manifest['items']:
        path = 'Assets/Scriptable/Exploration/Resources/' + item['id'] + '.asset'
        copy(path)
        copy(path + '.meta')
    for path in ['Assets/Scriptable/Exploration/Resources/Madeira.asset',
                 'Assets/Images/Exploration/Items/tronco_inventario_v1.png']:
        copy(path)
        copy(path + '.meta')
    for region in manifest['regions']:
        path = 'Assets/Scriptable/Exploration/Encounters/' + region['asset'] + '.asset'
        copy(path)
        copy(path + '.meta')
    for name in ['ExplorationResourceDefinition', 'ExplorationEncounterDefinition', 'ExplorationEnemyDefinition', 'ExplorationTypes']:
        path = 'Assets/Scripts/Exploration/' + name + '.cs'
        copy(path)
        copy(path + '.meta')
    scripts = destination / 'Assets/Scripts'
    (scripts / 'TCCGame.Runtime.asmdef').write_text(json.dumps({'name': 'TCCGame.Runtime', 'references': [], 'autoReferenced': True}), encoding='utf-8')
    # Use the actual project's enum definition without unrelated character dependencies.
    character = (ROOT / 'Assets/Scripts/Aventureiro/Char_base.cs').read_text(encoding='utf-8')
    damage = re.search(r'public enum DamageType\s*\{[^}]+\}', character).group(0)
    (scripts / 'DamageType.cs').write_text(damage, encoding='utf-8')
    editor = destination / 'Assets/Editor'
    editor.mkdir(parents=True, exist_ok=True)
    shutil.copy2(DOCS / 'UnityResourceAudit.cs', editor / 'UnityResourceAudit.cs')
    packages = destination / 'Packages'
    packages.mkdir(parents=True, exist_ok=True)
    (packages / 'manifest.json').write_text(json.dumps({'dependencies': {'com.unity.modules.imageconversion': '1.0.0'}}), encoding='utf-8')
    copy('ProjectSettings/ProjectVersion.txt')
    print(destination)


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('destination', type=Path)
    stage(parser.parse_args().destination)
