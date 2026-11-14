"""Copy task-owned baked assets from the existing validation Editor, preserving source settings/GUIDs."""
from pathlib import Path
import shutil,re,json,hashlib
GAME=Path(__file__).resolve().parents[2];LIVE=GAME.parent/'foundation-work/validation'
BASE=Path('Assets/Game/Locations/Mansion');OUT=GAME/'Documentation/Mansion/DestructionRework';OUT.mkdir(parents=True,exist_ok=True)
def digest(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def guid(p):
 m=re.search(r'^guid: (\w+)',p.read_text(),re.M);return m.group(1) if m else None
protected={str(p.relative_to(GAME)):digest(p) for parent in ['ProjectSettings','Packages'] for p in (GAME/parent).rglob('*') if p.is_file()}
metas={str(p.relative_to(GAME)):guid(p) for p in (GAME/'Assets').rglob('*.meta')}
files=[]
for name in ['Architecture','DestructionProfiles','Props','Fragments','FragmentMeshes']:
 folder=LIVE/BASE/name;files+=list(p.relative_to(LIVE) for p in folder.rglob('*') if p.is_file())
 if folder.with_suffix('.meta').exists():files.append(folder.with_suffix('.meta').relative_to(LIVE))
for name in ['Materials/Cut_plaster.mat','Materials/PropContact.physicMaterial','Art/Audio/plastic_hit.wav','Art/Audio/plastic_break.wav']:
 files.extend([BASE/name,Path(str(BASE/name)+'.meta')])
files.extend([Path('Assets/Game/Scenes/MansionPrototype.unity'),Path('Assets/Game/Scenes/MansionPrototype.unity.meta')])
# Fail before any copy if an existing asset's identity changed.
for rel in files:
 if rel.suffix=='.meta' and str(rel) in metas:assert guid(LIVE/rel)==metas[str(rel)],f'GUID changed: {rel}'
for rel in files:
 target=GAME/rel;target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(LIVE/rel,target)
for rel,h in protected.items():assert digest(GAME/rel)==h,f'Protected settings changed: {rel}'
for rel,g in metas.items():assert guid(GAME/rel)==g,f'Existing GUID lost: {rel}'
result={'copiedFiles':len(files),'existingAssetGuidsPreserved':len(metas),'protectedSettingsFilesUnchanged':len(protected),'scene':'Assets/Game/Scenes/MansionPrototype.unity','validationEditor':'6000.6.8f1','sourceEditor':'6000.0.43f1'}
(OUT/'source_sync.json').write_text(json.dumps(result,indent=2)+'\n');print(result)
