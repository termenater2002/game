"""Run targeted real-Editor suites, tolerating discovery gaps during domain reload only."""
from pathlib import Path
import subprocess,json,time,sys
GAME=Path(__file__).resolve().parents[2];PROJECT=GAME.parent/'foundation-work/validation';OUT=GAME/'Documentation/Mansion/DestructionRework';OUT.mkdir(parents=True,exist_ok=True)
BASE=['/home/deck/.local/bin/unity','command'];TAIL=['--project-path',str(PROJECT),'--port','7800','--caller','plugin','--skill','unity-pipeline','--json']
def call(*args):
 p=subprocess.run(BASE+list(args)+TAIL,text=True,capture_output=True,timeout=60)
 if p.returncode:raise RuntimeError(p.stdout+p.stderr)
 return json.loads(p.stdout)
# Auto Refresh may be disabled in the Editor. Never test stale assemblies after copying source.
call('script','recompile')
for attempt in range(120):
 try:r=call('script','recompile_status')
 except RuntimeError as e:
  if 'EDITOR_NOT_RUNNING' not in str(e) and 'closed the connection' not in str(e):raise
  time.sleep(1);continue
 if r.get('status') in ('completed','up_to_date'):
  if r.get('failed') or r.get('compilationFailed'):raise RuntimeError(r)
  break
 time.sleep(1)
else:raise TimeoutError('Compilation did not finish')
failed=False
fixtures=sys.argv[1:] or ['editor:VoxelArchitectureTests','playmode:DestructionReworkTests']
for spec in fixtures:
 mode,name=spec.split(':',1)
 p=subprocess.run(BASE+['test','run']+TAIL+['--','--mode',mode,'--filter',name,'--filter-type','assembly' if name.endswith('.Tests') else 'testName','--async-tests','true'],text=True,capture_output=True,timeout=60)
 if p.returncode:raise RuntimeError(p.stdout+p.stderr)
 until=time.monotonic()+240
 while time.monotonic()<until:
  try:r=call('test','status')
  except RuntimeError as e:
   if 'EDITOR_NOT_RUNNING' not in str(e) and 'closed the connection' not in str(e):raise
   time.sleep(1);continue
  if r.get('status')=='completed':
   (OUT/('tests_'+name+'.json')).write_text(json.dumps(r,ensure_ascii=False,indent=2)+'\n')
   print(name,r['summary'],flush=True)
   failed |= r['summary'].get('failed',0)>0
   for row in r.get('results',[]):
    if row.get('Status')!='Passed':print(row,flush=True)
   break
  time.sleep(1)
 else:raise TimeoutError(name)

raise SystemExit(1 if failed else 0)
