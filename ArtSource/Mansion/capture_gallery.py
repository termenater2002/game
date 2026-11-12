"""Capture the live mansion test. Reload the saved scene afterwards to restore normal controls."""
from pathlib import Path
import subprocess, json
GAME=Path(__file__).resolve().parents[2]
PROJECT=GAME.parent/'foundation-work/validation'
BASE=['/home/deck/.local/bin/unity','command']
TAIL=['--project-path',str(PROJECT),'--caller','plugin','--skill','unity-pipeline','--json']
def call(*args):
    p=subprocess.run(BASE+list(args)+TAIL,text=True,capture_output=True,timeout=45)
    if p.returncode: raise RuntimeError(p.stdout+p.stderr)
    return json.loads(p.stdout)
def script(entry,*args):
    return call('script','run','--file',str(GAME/'AgentScripts/MansionCaptures.cs'),'--entry','MansionCaptures.'+entry,'--args',json.dumps(args))
def capture(name,camera='Main Camera',overlay='auto'):
    return call('capture','game_view','--source','camera','--camera',camera,'--overlay',overlay,'--output-path',name)
cursor=call('console','status')['cursor']
for number,room in enumerate(['Hall','Lounge','Dining','Kitchen','Study','Bedroom','Bath'],1):
    script('Set',room)
    capture('/tmp/MansionCaptureWarmup.png')
    capture(f'Documentation/Mansion/Final_{number:02}_{room}.png')
    print(room,flush=True)
script('PaletteAudit')
r=script('Character')['result'];camera=r['camera']
capture('/tmp/MansionCaptureWarmup.png',camera,'false')
capture('Documentation/Mansion/Final_08_Rider.png',camera,'false')
script('Set','Dining')
call('script','run','--file',str(GAME/'AgentScripts/MansionInspect.cs'),'--entry','MansionInspect.PlanView','--args','[true]')
capture('/tmp/MansionCaptureWarmup.png','Mansion Plan Camera','false')
capture('Documentation/Mansion/Final_09_Plan.png','Mansion Plan Camera','false')
call('script','run','--file',str(GAME/'AgentScripts/MansionInspect.cs'),'--entry','MansionInspect.PlanView','--args','[false]')
# Restore primary camera after the temporary character angle.
call('script','eval','UnityEngine.GameObject.Find("Main Camera").GetComponent<UnityEngine.Camera>().enabled = true;')
script('Set','Dining')
capture('/tmp/MansionCaptureWarmup.png')
print(script('Feedback')['result'],flush=True)
capture('Documentation/Mansion/Final_10_Shards.png')

report=call('console','get','--level','warn','--since',str(cursor),'--tail','20')
(PROJECT/'Documentation/Mansion/capture_console.json').write_text(json.dumps(report,indent=2))
print('New warnings/errors during capture:',report['returned'],flush=True)
