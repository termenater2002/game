"""Capture real Unity A/B frames and measure square pixel blocks without resampling images."""
from pathlib import Path
import subprocess,json
from PIL import Image
GAME=Path(__file__).resolve().parents[2];PROJECT=GAME.parent/'foundation-work/validation'
BASE=['/home/deck/.local/bin/unity','command'];TAIL=['--project-path',str(PROJECT),'--caller','plugin','--skill','unity-pipeline','--json']
def call(*args):
 p=subprocess.run(BASE+list(args)+TAIL,text=True,capture_output=True,timeout=45)
 if p.returncode:raise RuntimeError(p.stdout+p.stderr)
 return json.loads(p.stdout)
def run(file,entry,*args):return call('script','run','--file',str(GAME/'AgentScripts'/file),'--entry',entry,'--args',json.dumps(args))
native=call('script','eval','return new {width=UnityEngine.Screen.width,height=UnityEngine.Screen.height};')['result']
width,height=native['width'],native['height']
block=max(1,round(height/180))
def capture(name):return call('capture','game_view','--source','camera','--camera','Main Camera','--width',str(width),'--height',str(height),'--output-path',name)
cursor=call('console','status')['cursor']
run('MansionCaptures.cs','MansionCaptures.Set','Dining')
for enabled,name in [(False,'Pixelation_Off.png'),(True,'Pixelation_On.png')]:
 run('InstallPixelation.cs','InstallPixelation.Toggle',enabled)
 capture('/tmp/PixelationWarmup.png')
 capture('Documentation/Mansion/'+name)
folder=PROJECT/'Documentation/Mansion'
a=Image.open(folder/'Pixelation_Off.png').convert('RGB')
b=Image.open(folder/'Pixelation_On.png').convert('RGB')
def blocks(image,block,rect):
 x0,y0,x1,y1=rect;pixels=image.load();uniform=total=0
 for y in range(y0,y1-block+1,block):
  for x in range(x0,x1-block+1,block):
   colors=[pixels[x+dx,y+dy] for dy in range(block) for dx in range(block)]
   uniform+=all(max(c[k] for c in colors)-min(c[k] for c in colors)<=1 for k in range(3));total+=1
 return uniform/total
# Native Game View dimensions prevent Pipeline's overlay capture from rescaling the finished image.
world=tuple((int(v)//block)*block for v in (width*.6,height*.2,width*.95,height*.8))
hud=(24,20,min(320,width),80)
ink=[(x,y) for y in range(hud[1],hud[3]) for x in range(hud[0],hud[2]) if min(a.getpixel((x,y)))>230]
report={'resolution':list(b.size),'verticalPixels':180,'blockSize':block,
 'worldUniformBlocksBefore':blocks(a,block,world),'worldUniformBlocksAfter':blocks(b,block,world),
 'hudUniformBlocksAfter':blocks(b,block,hud),
 'hudWhiteInkUnchangedFraction':sum(min(b.getpixel(p))>230 for p in ink)/max(1,len(ink))}
report['pass']=report['worldUniformBlocksAfter']>.97 and report['worldUniformBlocksAfter']>report['worldUniformBlocksBefore']+.2 and report['hudUniformBlocksAfter']<.9 and report['hudWhiteInkUnchangedFraction']>.99
call('capture','game_view','--source','camera','--camera','Main Camera','--overlay','false','--width','1280','--height','720','--output-path','/tmp/Pixelation720Warmup.png')
call('capture','game_view','--source','camera','--camera','Main Camera','--overlay','false','--width','1280','--height','720','--output-path','Documentation/Mansion/Pixelation_World_720p.png')
world720=Image.open(folder/'Pixelation_World_720p.png').convert('RGB')
report['world720pUniform4x4']=blocks(world720,4,(720,160,1200,576))
report['pass']=report['pass'] and report['world720pUniform4x4']>.97
(folder/'pixelation_proof.json').write_text(json.dumps(report,indent=2)+'\n')
console=call('console','get','--level','warn','--since',str(cursor),'--tail','20')
(folder/'pixelation_console.json').write_text(json.dumps(console,indent=2)+'\n')
print(json.dumps(report,indent=2));print('New warnings/errors:',console['returned'])
if not report['pass']:raise SystemExit('Pixel block / HUD verification failed')
