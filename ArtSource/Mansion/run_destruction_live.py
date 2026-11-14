"""Repeatable live Mansion demonstrations. Run after run_builder.py SceneTest; never saves broken runtime state."""
from pathlib import Path
import subprocess,json,sys
GAME=Path(__file__).resolve().parents[2];PROJECT=GAME.parent/'foundation-work/validation';OUT=PROJECT/'Documentation/Mansion/DestructionRework';OUT.mkdir(parents=True,exist_ok=True)
TAIL=['--project-path',str(PROJECT),'--port','7800','--caller','plugin','--skill','unity-pipeline','--json']
def call(*args,timeout=90):
 p=subprocess.run(['/home/deck/.local/bin/unity','command']+list(args)+TAIL,capture_output=True,text=True,timeout=timeout)
 if p.returncode:raise RuntimeError(p.stdout+p.stderr)
 r=json.loads(p.stdout)
 if r.get('success') is False:raise RuntimeError(r)
 return r

def run(file,entry,*args,timeout=60):
 r=call('script','run','--file',str(GAME/'AgentScripts'/file),'--entry',entry,'--args',json.dumps(args),'--timeout',str(timeout),timeout=timeout+20)
 value=r.get('result',r);print(entry,json.dumps(value,ensure_ascii=False),flush=True)
 return value

def capture(name,camera='Main Camera',overlay='false',width=1280,height=720):
 for path in ['/tmp/DestructionCaptureWarmup.png',str(OUT/(name+'.png'))]:
  call('capture','game_view','--source','camera','--camera',camera,'--overlay',overlay,'--width',str(width),'--height',str(height),'--output-path',path)
 print('Captured',name,flush=True)
mode=sys.argv[1] if len(sys.argv)>1 else 'examples'
if mode=='rooms':
 for room in ['Hall','Dining','Kitchen','Study','Lounge','Bath','Bedroom']:
  run('MansionCaptures.cs','MansionCaptures.Set',room);capture('After_'+room)
 run('MansionCaptures.cs','MansionCaptures.Character');capture('After_Rider','Side Test Camera (F6)')
 run('MansionCaptures.cs','MansionCaptures.PaletteAudit')
elif mode=='examples':
 for method,name in [('WallExample','Voxel_craters'),('TelevisionExample','TV_screen'),('BookcaseExample','Independent_books'),('RugExample','Flexible_rug')]:
  result=run('DestructionReworkLive.cs','DestructionReworkLive.'+method)
  capture(name,'Side Test Camera (F6)' if method=='RugExample' else 'Main Camera')
  if method=='RugExample':run('DestructionReworkLive.cs','DestructionReworkLive.Resume')
  if result.get('pass') is not True:raise RuntimeError(result)
elif mode=='details':
 r=run('DestructionReworkLive.cs','DestructionReworkLive.CraterCloseup');capture('Voxel_closeup');capture('Damage_HUD',overlay='true',width=r['screenWidth'],height=r['screenHeight'])
 r=run('DestructionReworkLive.cs','DestructionReworkLive.UpholsteryExample');capture('Upholstery_tear')
 if r.get('pass') is not True:raise RuntimeError(r)
elif mode=='stress':run('DestructionReworkLive.cs','DestructionReworkLive.Stress',180,timeout=240)
elif mode=='routes':run('DestructionReworkLive.cs','DestructionReworkLive.Routes')
elif mode=='settle':run('MansionRuntimeProof.cs','MansionRuntimeProof.Settle')
else:raise ValueError(mode)
