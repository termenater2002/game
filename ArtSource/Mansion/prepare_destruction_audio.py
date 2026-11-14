"""Original plastic clicks/cracks, generated locally; does not rewrite imported libraries."""
from pathlib import Path
import math,random,wave,struct,shutil
root=Path(__file__).resolve().parents[2];audio=root/'Assets/Game/Locations/Mansion/Art/Audio';live=root.parent/'foundation-work/validation/Assets/Game/Locations/Mansion/Art/Audio'
for broken in [False,True]:
 rng=random.Random(782+broken);rate=22050;duration=.37 if broken else .13;out=[]
 for i in range(int(rate*duration)):
  t=i/rate;tick=(max(0,1-((t-.01)%.045)/.007) if broken else 0);signal=(rng.uniform(-1,1)*(.35+tick*.5)+math.sin(2*math.pi*320*t)*.35)*math.exp(-t*(12 if broken else 30))*.6
  out.append(struct.pack('<h',int(max(-1,min(1,signal))*32700)))
 p=audio/('plastic_break.wav' if broken else 'plastic_hit.wav')
 with wave.open(str(p),'wb') as f:f.setparams((1,2,rate,0,'NONE','not compressed'));f.writeframes(b''.join(out))
 shutil.copy2(p,live/p.name)
print('Prepared two original plastic Foley clips')
