"""Original editable pixel atlases; proportions studied from Koharu, no source texture copied."""
from pathlib import Path
from PIL import Image, ImageDraw
import random
P=Path(__file__).parent
ROLES=['LilacV3','RiderV3','RiderTShirtV3','FamilyFatherV3','FamilyMotherV3']
RECTS={'face':(0,0,256,256),'cloth':(256,0,512,256),'hair':(0,256,128,512),'pants':(128,256,256,512),'skin':(256,256,384,384),'shoe':(384,256,512,384),'trim':(256,384,384,512),'metal':(384,384,512,512)}

def face(role,skin):
 im=Image.new('RGB',(256,256),skin);d=ImageDraw.Draw(im)
 dad='Father' in role;mom='Mother' in role
 ink=(27,25,34);shadow=(173,144,142) if not dad else (173,140,120)
 # Broad, quiet painted planes instead of dark polygonal contour rings.
 d.polygon([(0,35),(16,90),(24,192),(75,242),(23,224),(0,165)],fill=tuple(max(0,c-9) for c in skin))
 d.polygon([(256,35),(240,90),(232,192),(181,242),(233,224),(256,165)],fill=tuple(max(0,c-9) for c in skin))
 eye_y=124;eye_h=13 if dad else 16 if mom else 22
 for side in [-1,1]:
  cx=128+side*57;w=38 if not dad else 35;h=eye_h
  # Curved almond outline, strong upper lid, delicate lower lid: no glasses-like boxes.
  upper=[(cx-w,eye_y),(cx-w*.68,eye_y-h*.72),(cx-w*.17,eye_y-h),(cx+w*.40,eye_y-h*.84),(cx+w*.83,eye_y-h*.37),(cx+w,eye_y+1)]
  lower=[(cx+w*.71,eye_y+h*.67),(cx+w*.16,eye_y+h*.92),(cx-w*.50,eye_y+h*.72)]
  d.polygon(upper+lower,fill=(245,242,230))
  r=11 if dad else 12 if mom else 14
  iris_c=(68,42,31) if (mom or dad) else (41,37,48)
  d.ellipse((cx-r,eye_y-h+2,cx+r,eye_y+h-2),fill=iris_c)
  d.ellipse((cx-r*.42,eye_y-h+3,cx+r*.42,eye_y+h-3),fill=(23,22,29))
  d.rectangle((cx-r+3,eye_y-h+5,cx-r+9,eye_y-h+13),fill=(253,251,240))
  d.line(upper,fill=ink,width=4 if not dad else 3)
  d.line([(cx-w*.69,eye_y+h*.53),(cx-w*.30,eye_y+h*.81),(cx+w*.23,eye_y+h*.89),(cx+w*.80,eye_y+h*.50)],fill=ink,width=1)
  outer=cx+side*w
  if not dad:
   d.polygon([(outer-side*5,eye_y-4),(outer+side*10,eye_y-13),(outer+side*4,eye_y+5)],fill=ink)
   if not mom:
    for offset in [-19,1,22]:d.line([(cx+offset,eye_y+h*.77),(cx+offset+side*3,eye_y+h+7)],fill=ink,width=2)
  brow_y=eye_y-h-14
  if dad:
   # Inner brow is lower, giving a reserved stern expression.
   points=[(cx-side*32,brow_y+5),(cx+side*3,brow_y-3),(cx+side*36,brow_y-9)]
   d.line(points,fill=(52,43,42),width=5)
   d.line([(cx-25,eye_y+h+10),(cx,eye_y+h+13),(cx+27,eye_y+h+7)],fill=shadow,width=1)
  else:
   d.line([(cx-w*.86,brow_y+3),(cx-w*.30,brow_y-2),(cx+w*.4,brow_y-1),(cx+w*.8,brow_y+3)],fill=(66,53,56),width=2 if not mom else 3)
   d.line([(cx-w*.6,eye_y-h-5),(cx,eye_y-h-8),(cx+w*.6,eye_y-h-5)],fill=shadow,width=1)
   if not mom:
    for offset in [-15,1,15]:d.line([(cx+offset,eye_y+h+10),(cx+offset-3,eye_y+h+15)],fill=(195,162,171),width=2)
 # Nose is a handful of painted pixels, over the nose integrated into the head surface.
 d.polygon([(125,158),(126,170),(135,166),(132,172),(124,171),(120,166)],fill=tuple(max(0,c-42) for c in skin))
 d.line([(125,157),(126,163)],fill=tuple(min(255,c+7) for c in skin),width=2)
 if dad:
  d.line([(104,202),(121,200),(134,201),(151,200)],fill=(98,67,63),width=2)
  d.line([(112,207),(129,209),(143,206)],fill=(189,149,128),width=1)
  for side in [-1,1]:d.line([(128+side*27,176),(128+side*39,193),(128+side*40,211)],fill=(182,145,126),width=1)
  for y in [65,75]:d.line([(91,y+1),(126,y),(164,y+1)],fill=(187,154,134),width=1)
 elif mom:
  d.polygon([(104,200),(120,197),(128,200),(136,197),(153,200),(141,206),(118,206)],fill=(159,65,76))
  d.line([(106,201),(128,202),(150,201)],fill=(83,44,52),width=1)
  d.line([(119,205),(137,205)],fill=(226,143,140),width=1)
 else:
  d.line([(108,201),(117,204),(127,200),(136,203),(147,198)],fill=ink,width=2)
  d.point((146,201),fill=ink)
 return im

def floral(base,purple=False):
 im=Image.new('RGB',(256,256),base);d=ImageDraw.Draw(im)
 colors=((204,147,229),(231,181,239),(143,93,172),(119,76,135)) if purple else ((167,44,57),(191,71,75),(123,29,48),(100,31,45))
 for y in [4,46,96,140,191,238]:d.rectangle((0,y,255,y+6),fill=colors[0]);d.line((0,y+9,255,y+9),fill=colors[1],width=2)
 for cx,cy in [(39,31),(174,38),(101,118),(228,125),(32,216),(164,210)]:
  petal=colors[3]
  for dx,dy in [(-9,-9),(9,-9),(-9,9),(9,9)]:
   d.polygon([(cx+dx-7,cy+dy-2),(cx+dx-2,cy+dy-9),(cx+dx+7,cy+dy-4),(cx+dx+6,cy+dy+6),(cx+dx-4,cy+dy+7)],fill=petal)
  d.rectangle((cx-5,cy-5,cx+5,cy+5),fill=(241,187,218) if purple else (188,132,73))
  d.line([(cx+8,cy+12),(cx+17,cy+24),(cx+23,cy+36)],fill=colors[2],width=2)
  d.polygon([(cx+17,cy+23),(cx+31,cy+18),(cx+25,cy+30)],fill=colors[3])
 # Painted shadows, stitch courses and broken highlights complement the modeled folds.
 for pts in [[(3,85),(15,100),(11,113),(7,99),(2,98)],[(133,68),(143,80),(145,95),(138,85)],[(58,155),(70,167),(69,185),(63,174)]]:d.polygon(pts,fill=colors[2])
 return im

def cloth(role):
 if role=='LilacV3':return floral((185,122,213),True)
 if 'Mother' in role:return floral((157,37,51),False)
 if 'Father' in role:
  im=Image.new('RGB',(256,256),(104,119,125));d=ImageDraw.Draw(im)
  for y in range(0,256,8):
   for x in range(0,256,16):d.line([(x,y),(x+4,y+3),(x+8,y),(x+12,y+3),(x+16,y)],fill=(117,130,134))
  for x in range(12,256,32):d.line((x,0,x,255),fill=(83,100,110))
  for x in [26,130,235]:d.polygon([(x,48),(x-9,120),(x+1,205),(x+4,126)],fill=(87,102,114))
  return im
 base=(65,102,103) if role=='RiderV3' else (207,200,180)
 im=Image.new('RGB',(256,256),base);d=ImageDraw.Draw(im)
 dark=(31,52,61) if role=='RiderV3' else (75,79,87);light=(183,190,170) if role=='RiderV3' else (152,150,139)
 d.rectangle((0,27,255,44),fill=light);d.line((0,48,255,48),fill=dark,width=2)
 for x in [18,127,238]:d.line([(x,58),(x+3,150),(x,248)],fill=dark,width=2)
 for x in [36,170]:
  d.line([(x,174),(x+36,157)],fill=dark,width=4);d.line([(x,180),(x+36,163)],fill=light,width=1)
 # Original abstract street-racing back/chest print, no Z or external brand.
 d.polygon([(82,83),(123,61),(174,84),(156,124),(112,137),(82,113)],fill=dark)
 d.polygon([(97,83),(123,74),(155,89),(141,115),(113,123),(96,107)],outline=light,width=3)
 for j in range(3):d.line((93+j*8,151,154+j*8,134),fill=light,width=3)
 for pts in [[(2,192),(24,222),(6,245)],[(205,76),(218,114),(221,88)]]:d.polygon(pts,fill=dark)
 return im

def atlas(role):
 skin={'LilacV3':(227,224,217),'RiderV3':(201,168,147),'RiderTShirtV3':(201,168,147),'FamilyFatherV3':(216,190,168),'FamilyMotherV3':(234,214,194)}[role]
 im=Image.new('RGB',(512,512),(35,35,42));im.paste(face(role,skin),(0,0));im.paste(cloth(role),(256,0))
 if role.startswith('Rider'):
  helmet=Image.new('RGB',(256,256),(42,48,59));d=ImageDraw.Draw(helmet)
  # Symmetric angular livery follows the shell's own UV island; dark lower tile is the visor.
  for side in [-1,1]:
   cx=128+side*64
   d.polygon([(cx-side*17,0),(cx+side*6,0),(cx+side*33,106),(cx+side*9,121),(cx-side*1,69)],fill=(185,188,175))
   d.line([(cx-side*7,4),(cx+side*15,74),(cx+side*24,111)],fill=(174,124,57),width=3)
  d.rectangle((0,128,255,255),fill=(17,28,41))
  d.polygon([(0,144),(119,134),(255,144),(255,157),(110,146),(0,158)],fill=(42,60,73))
  d.line([(8,175),(114,164),(244,176)],fill=(28,43,58),width=2)
  im.paste(helmet,(0,0))

 # Hair: broad locks and sparse broken glints, not uniform bright stripes.
 hair=Image.new('RGB',(128,256),(33,33,40));d=ImageDraw.Draw(hair)
 for x in [9,42,78,112]:
  d.polygon([(x,0),(x+8,17),(x+5,99),(x+13,168),(x+4,245),(x-3,201),(x-2,70)],fill=(42,42,50))
 for x,y in [(13,31),(46,47),(81,25),(111,57)]:d.polygon([(x,y),(x+9,y+3),(x+8,y+7),(x-2,y+5)],fill=(72,72,81))
 if 'Father' in role:
  hair=Image.new('RGB',(128,256),(70,32,96));d=ImageDraw.Draw(hair)
  for y in range(12,256,31):
   for x in range(12,128,28):
    d.polygon([(x,y-5),(x+4,y),(x,y+5),(x-4,y)],outline=(141,103,153))
    d.point((x,y),fill=(185,149,101))
 im.paste(hair,(0,256))
 base=(43,44,56) if 'Mother' not in role else (122,31,45);denim=Image.new('RGB',(128,256),base);d=ImageDraw.Draw(denim)
 for x in [6,121]:d.line((x,0,x,255),fill=(76,72,86) if 'Mother' not in role else (168,56,63),width=1)
 for pts in [[(12,27),(28,45),(34,73),(21,95),(22,64),(6,54)],[(114,71),(105,78),(83,87),(87,98),(77,104),(95,112),(115,96)],[(10,128),(28,132),(44,145),(38,153),(30,146),(19,152),(12,169),(8,149)],[(119,161),(102,179),(99,196),(87,213),(95,235),(110,217),(114,190),(124,176)],[(0,232),(14,235),(31,223),(48,219),(39,234),(18,248),(2,252)]]:
  d.polygon(pts,fill=(25,26,36) if 'Mother' not in role else (82,21,34))
 d.line([(9,8),(51,14),(53,40),(15,46),(9,8)],fill=(90,83,94) if 'Mother' not in role else (173,70,76),width=1)
 d.line([(66,17),(109,11),(117,43),(74,44),(66,17)],fill=(78,73,84) if 'Mother' not in role else (173,70,76),width=1)
 im.paste(denim,(128,256))
 tile=Image.new('RGB',(128,128),skin);d=ImageDraw.Draw(tile)
 d.polygon([(2,0),(17,48),(8,122),(0,128)],fill=tuple(max(0,c-10) for c in skin));d.polygon([(126,0),(111,48),(120,122),(128,128)],fill=tuple(max(0,c-10) for c in skin))
 d.line([(61,66),(65,68),(63,72)],fill=tuple(max(0,c-65) for c in skin),width=2)
 im.paste(tile,(256,256))
 shoe=Image.new('RGB',(128,128),(32,34,40));d=ImageDraw.Draw(shoe);d.rectangle((0,94,127,110),fill=(199,198,186));d.line((0,92,127,92),fill=(76,78,84),width=2)
 for y in [22,34,46,58]:d.line((36,y,92,y+9),fill=(182,183,176),width=3);d.line((92,y,36,y+9),fill=(182,183,176),width=2)
 d.line([(13,13),(16,76),(34,86),(99,86),(114,76),(116,13)],fill=(82,84,91),width=2)
 im.paste(shoe,(384,256))
 trim=Image.new('RGB',(128,128),(64,49,87) if role=='LilacV3' else (31,37,46));d=ImageDraw.Draw(trim)
 for y in range(0,128,6):d.line((0,y,127,y),fill=(111,82,142) if role=='LilacV3' else (54,60,69))
 im.paste(trim,(256,384))
 metals=Image.new('RGB',(128,128),(192,193,184));d=ImageDraw.Draw(metals);d.rectangle((64,0,127,63),fill=(164,121,48));d.rectangle((0,64,63,127),fill=(118,20,41));d.rectangle((64,64,127,127),fill=(223,216,195));im.paste(metals,(384,384))
 im.save(P/(role+'_atlas.png'))
for role in ROLES:atlas(role)
print('Created five 512px original character atlases (256px face regions).')
