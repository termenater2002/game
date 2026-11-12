"""Copy the authored factory assets into this level without changing the source libraries."""
from pathlib import Path
import json,shutil,math,random,wave,struct
GAME=Path(__file__).resolve().parents[2];SRC=GAME.parent/'MajorDamage_AssetFactory';OUT=GAME/'Assets/Game/Locations/Mansion';ART=OUT/'Art'
for d in ['Models','Textures','Audio','Data']:(ART/d).mkdir(parents=True,exist_ok=True)
cat=json.loads((SRC/'reports/catalog.json').read_text());chosen=[e for e in cat if isinstance(e['id'],int) and 1<=e['id']<=130]
for e in chosen:shutil.copy2(SRC/e['fbx'],ART/'Models'/(e['name']+'.fbx'))
materials=[]
for source,base in [('source_material_catalog.json',SRC/'textures/source'),('extra_materials.json',SRC)]:
 for e in json.loads((SRC/'reports'/source).read_text()):
  filename=f"T{e['id']}_{Path(e['path']).name}";shutil.copy2(base/e['path'],ART/'Textures'/filename)
  materials.append(dict(name='MD_'+e['name'],texture=filename,roughness=e.get('roughness',.8),metallic=e.get('metallic',0),tileable=e.get('tileable',False),alpha=.36 if e['id']==1003 else 1))
(ART/'Data/assets.json').write_text(json.dumps(chosen,ensure_ascii=False,indent=2));(ART/'Data/materials.json').write_text(json.dumps(materials,ensure_ascii=False,indent=2))
# Short original synthesized Foley; generated locally, no downloaded or claimed recordings.
rate=22050
for kind in ['wood','ceramic','glass','metal','fabric']:
 for broken in [False,True]:
  rand=random.Random(kind+str(broken));duration=.55 if broken else .19;freq={'wood':180,'ceramic':1320,'glass':2760,'metal':700,'fabric':85}[kind];samples=[]
  for n in range(int(rate*duration)):
   t=n/rate;env=math.exp(-t*(8 if broken else 24));noise=rand.uniform(-1,1);tone=sum(math.sin(2*math.pi*freq*f*t)*math.exp(-t*f*5) for f in [1,1.41,2.1])/3
   tick=max(0,1-((t-.03)% .075)/.013) if broken else 0
   v=(noise*(.65 if kind in ('wood','fabric') else .20)+tone*(.3 if kind=='fabric' else .65)+noise*tick*.3)*env*.6;samples.append(struct.pack('<h',int(max(-1,min(1,v))*32700)))
  with wave.open(str(ART/'Audio'/(kind+('_break' if broken else '_hit')+'.wav')),'wb') as f:f.setparams((1,2,rate,0,'NONE','not compressed'));f.writeframes(b''.join(samples))
print('Prepared',len(chosen),'models and',len(materials),'textures')
