"""Compile the versioned builder modules together and run in the existing validation Editor."""
from pathlib import Path
import subprocess,sys,json
GAME=Path(__file__).resolve().parents[2];PROJECT=GAME.parent/'foundation-work/validation'
paths=[GAME/'AgentScripts/MansionFracture.cs',GAME/'AgentScripts/BuildMansion.cs'];usings=[];bodies=[]
for p in paths:
 lines=p.read_text().splitlines();usings.extend(l for l in lines if l.startswith('using '));bodies.extend(l for l in lines if not l.startswith('using '))
out=Path('/tmp/MansionBuilderCombined.cs');out.write_text('\n'.join(dict.fromkeys(usings))+'\n'+'\n'.join(bodies))
command=['/home/deck/.local/bin/unity','command','script','run','--file',str(out),'--entry','BuildMansion.'+(sys.argv[1] if len(sys.argv)>1 else 'All'),'--timeout','300','--project-path',str(PROJECT),'--caller','plugin','--skill','unity-pipeline','--json']
raise SystemExit(subprocess.call(command))
