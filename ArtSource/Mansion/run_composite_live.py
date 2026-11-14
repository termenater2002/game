"""Exercise actual compound props in the running Mansion; capture real cameras without saving Play state."""
from pathlib import Path
import subprocess,json,sys
GAME=Path(__file__).resolve().parents[2];PROJECT=GAME.parent/'foundation-work/validation'
TAIL=['--project-path',str(PROJECT),'--port','7800','--caller','plugin','--skill','unity-pipeline','--json']
def call(*args,timeout=90):
 p=subprocess.run(['/home/deck/.local/bin/unity','command']+list(args)+TAIL,text=True,capture_output=True,timeout=timeout)
 if p.returncode:raise RuntimeError(p.stdout+p.stderr)
 r=json.loads(p.stdout)
 if r.get('success') is False:raise RuntimeError(r)
 return r

def run(method,*args,timeout=60):
 r=call('script','run','--file',str(GAME/'AgentScripts/MansionCompositeLive.cs'),'--entry','MansionCompositeLive.'+method,'--args',json.dumps(args),'--timeout',str(timeout),timeout=timeout+15).get('result')
 print(method,json.dumps(r,ensure_ascii=False),flush=True);return r

def capture(name):
 for file in ['/tmp/CompositeWarmup.png','Documentation/Mansion/Composites/'+name+'.png']:
  call('capture','game_view','--source','camera','--camera','Main Camera','--overlay','false','--width','1280','--height','720','--output-path',file)
mode=sys.argv[1] if len(sys.argv)>1 else 'proof'
if mode=='proof':
 for name in sys.argv[2:] or ['window','fridge','drawer','lamp','mirror','fruit','pillow']:
  r=run('Take' if name in ['fruit','pillow'] else 'Strike',name)
  try:capture(name)
  finally:
   if name in ['fruit','pillow']:run('ResumeCapture')
  if not r['pass']:raise RuntimeError(r)
elif mode=='stress':
 r=run('Stress',60,timeout=100)
 if not r['pass']:raise RuntimeError(r)
elif mode=='healthy':
 r=run('HealthyStart')
 if not r['pass']:raise RuntimeError(r)
elif mode=='audit':run('Audit')
else:raise ValueError(mode)
