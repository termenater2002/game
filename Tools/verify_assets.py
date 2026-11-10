"""Verify retained Unity GUIDs after asset moves. Python standard library only."""
from pathlib import Path
import collections
import json
import re

ROOT = Path(__file__).resolve().parents[1]
baseline = json.loads((ROOT / 'Tools/original_asset_guids.json').read_text())
current = collections.defaultdict(list)
for meta in (ROOT / 'Assets').rglob('*.meta'):
    match = re.search(r'(?m)^guid: ([0-9a-f]+)', meta.read_text())
    if match:
        current[match[1]].append(str(meta.relative_to(ROOT)))
missing = {guid: path for guid, path in baseline.items() if guid not in current}
duplicates = {guid: paths for guid, paths in current.items() if len(paths) > 1}
print(json.dumps({'original_guids': len(baseline), 'current_guids': len(current),
                  'missing': missing, 'duplicates': duplicates}, indent=2))
raise SystemExit(1 if missing or duplicates else 0)
