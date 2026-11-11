"""Editable pixel-painted art, extending Lilac's original PIL drawing workflow."""
from pathlib import Path
from PIL import Image, ImageDraw
import random
P=Path(__file__).parent
INK=(27,23,28);EYE_WHITE=(247,242,227)

def face(person):
    father=person=='father'
    bg=(215,183,155) if father else (233,210,188)
    image=Image.new('RGB',(256,256),bg);d=ImageDraw.Draw(image)
    xmin,xmax,zmin,zmax=(-.18,.18,1.70,2.10) if father else (-.16,.16,1.59,1.93)
    def pt(x,z):return (round((x-xmin)/(xmax-xmin)*255),round((zmax-z)/(zmax-zmin)*255))
    def polygon(points,color):d.polygon([pt(x,z) for x,z in points],fill=color)
    def stroke(points,color,width=1):d.line([pt(x,z) for x,z in points],fill=color,width=width)
    for side in [-1,1]:
        def shape(points,color):polygon([(side*x,z) for x,z in points],color)
        if father:
            # Narrower, mature anime eye, lowered inner brow and painted age lines.
            shape([(.025,1.923),(.052,1.935),(.115,1.932),(.137,1.916),(.122,1.885),(.047,1.885)],INK)
            shape([(.037,1.919),(.058,1.926),(.112,1.924),(.123,1.913),(.114,1.895),(.051,1.895)],EYE_WHITE)
            shape([(.067,1.927),(.091,1.925),(.095,1.914),(.090,1.895),(.068,1.895),(.062,1.909)],(65,41,33))
            shape([(.074,1.926),(.086,1.925),(.088,1.912),(.084,1.897),(.074,1.897)],INK)
            shape([(.071,1.923),(.078,1.923),(.078,1.915),(.071,1.915)],EYE_WHITE)
            shape([(.021,1.944),(.026,1.956),(.124,1.971),(.139,1.963),(.064,1.955)],INK)
            stroke([(side*.044,1.875),(side*.083,1.87),(side*.122,1.88)],(163,128,110),2)
            stroke([(side*.051,1.847),(side*.065,1.827),(side*.067,1.798)],(169,129,110),2)
            shape([(.112,1.906),(.142,1.895),(.135,1.886),(.117,1.893)],(184,144,119))
        else:
            # Almond drawn eyes and eyeliner, consistent with the first girl's pixel face.
            shape([(.022,1.790),(.044,1.809),(.105,1.811),(.133,1.796),(.117,1.761),(.045,1.758)],INK)
            shape([(.034,1.787),(.049,1.800),(.103,1.801),(.121,1.791),(.109,1.770),(.048,1.769)],EYE_WHITE)
            shape([(.064,1.801),(.087,1.802),(.096,1.791),(.090,1.771),(.068,1.769),(.061,1.784)],(85,48,34))
            shape([(.072,1.802),(.084,1.802),(.088,1.787),(.083,1.771),(.072,1.771)],INK)
            shape([(.067,1.797),(.075,1.798),(.075,1.787),(.067,1.787)],EYE_WHITE)
            shape([(.119,1.802),(.140,1.814),(.133,1.788)],INK)
            shape([(.023,1.797),(.019,1.806),(.037,1.802)],INK)
            shape([(.029,1.831),(.065,1.843),(.111,1.838),(.128,1.829),(.104,1.831),(.065,1.836)],(57,41,39))
            stroke([(side*.034,1.815),(side*.067,1.82),(side*.110,1.816)],(179,138,131),1)
            shape([(.064,1.743),(.071,1.752),(.078,1.751),(.070,1.741)],(204,163,153))
            shape([(.091,1.744),(.098,1.753),(.105,1.751),(.099,1.742)],(211,169,157))
    if father:
        # Painted bridge, nostrils, restrained lips, double-chin shadow and bald scalp planes.
        polygon([(-.005,1.912),(.007,1.912),(.020,1.856),(.006,1.849),(-.017,1.852)],(183,144,119))
        polygon([(-.022,1.853),(-.011,1.847),(0,1.852),(.014,1.846),(.025,1.854),(.014,1.858),(-.01,1.86)],(128,94,81))
        polygon([(-.045,1.813),(-.019,1.818),(0,1.814),(.020,1.818),(.045,1.81),(.015,1.807),(-.021,1.807)],(138,91,81))
        stroke([(-.041,1.813),(0,1.810),(.04,1.811)],(68,49,46),2)
        stroke([(-.028,1.797),(0,1.793),(.025,1.797)],(230,198,168),2)
        stroke([(-.056,1.767),(0,1.760),(.052,1.765)],(181,143,120),2)
        for z in [1.998,2.019]:stroke([(-.075,z),(0,z+.004),(.073,z)],(187,152,130),1)
        stroke([(-.008,1.961),(-.008,1.976)],(160,121,105),1)
        stroke([(.008,1.961),(.008,1.979)],(177,137,116),1)
    else:
        polygon([(-.005,1.736),(.002,1.714),(.013,1.719),(.011,1.726)],(189,153,139))
        stroke([(-.01,1.715),(-.004,1.712),(.005,1.714)],(149,107,102),1)
        polygon([(-.036,1.675),(-.013,1.68),(0,1.676),(.013,1.68),(.038,1.676),(.023,1.666),(-.019,1.666)],(166,68,74))
        stroke([(-.035,1.675),(0,1.671),(.036,1.676)],(83,43,49),2)
        polygon([(-.017,1.667),(.019,1.669),(.010,1.662),(-.009,1.662)],(202,112,112))
        stroke([(-.012,1.666),(.012,1.666)],(238,164,147),1)
    image.save(P/('father_face.png' if father else 'mother_face.png'))

face('father');face('mother')

# Red brocade: large, legible floral rosettes and scrolling leaves, as on Lilac's knit.
im=Image.new('RGB',(128,128),(164,34,44));d=ImageDraw.Draw(im)
for y in range(0,128,8):d.line((0,y,127,y),fill=(169,38,47))
for cx,cy in [(22,24),(86,24),(54,88),(118,88)]:
    for dx,dy in [(-6,-6),(6,-6),(-6,6),(6,6)]:
        d.polygon([(cx+dx-4,cy+dy),(cx+dx,cy+dy-5),(cx+dx+4,cy+dy),(cx+dx,cy+dy+5)],fill=(116,26,43))
    d.rectangle((cx-3,cy-3,cx+3,cy+3),fill=(187,117,71))
    for sign in [-1,1]:
        d.line([(cx+sign*5,cy+9),(cx+sign*11,cy+17),(cx+sign*15,cy+26)],fill=(135,71,50),width=1)
        d.polygon([(cx+sign*11,cy+16),(cx+sign*18,cy+13),(cx+sign*16,cy+20)],fill=(139,65,50))
im.save(P/'red_brocade.png')
# Grey cashmere: clear hand-painted folds, herringbone, restrained thin stripes.
im=Image.new('RGB',(128,128),(108,125,130));d=ImageDraw.Draw(im)
for y in range(-8,136,8):
    for x in range(-8,136,16):d.line([(x,y),(x+4,y+4),(x+8,y),(x+12,y+4),(x+16,y)],fill=(118,134,137))
for x in range(4,128,24):d.line((x,0,x,127),fill=(87,106,115),width=1)
for pts in [[(5,25),(21,57),(10,45)],[(50,5),(43,37),(56,29)],[(98,75),(120,110),(108,87)],[(35,85),(30,117),(43,110)],[(75,42),(90,64),(80,61)]]:d.polygon(pts,fill=(88,106,115))
im.save(P/'cashmere.png')
# Embroidered plum ascot.
im=Image.new('RGB',(128,128),(73,28,98));d=ImageDraw.Draw(im)
for y in range(0,128,32):
    for x in range(0,128,32):
        d.polygon([(x+16,y+5),(x+24,y+16),(x+16,y+27),(x+8,y+16)],outline=(145,105,71))
        d.rectangle((x+14,y+14,x+17,y+17),fill=(191,143,78))
im.save(P/'plum_embroidery.png')
for name,base,light,dark in [('black_wool',(35,36,46),(48,47,57),(24,24,34)),('red_wool',(128,27,38),(151,36,47),(95,21,33))]:
    im=Image.new('RGB',(128,128),base);d=ImageDraw.Draw(im)
    for x in range(8,128,16):d.line((x,0,x,127),fill=light)
    for x,y in [(16,8),(61,42),(104,73),(32,96)]:d.polygon([(x,y),(x-7,y+27),(x+6,y+39),(x+2,y+21)],fill=dark)
    im.save(P/(name+'.png'))
im=Image.new('RGB',(128,128),(19,20,27));d=ImageDraw.Draw(im)
for x in range(-4,128,16):
    d.polygon([(x+4,0),(x+9,7),(x+7,67),(x+14,115),(x+7,126),(x+2,63)],fill=(37,38,47))
    d.line([(x+9,13),(x+8,58),(x+13,105)],fill=(51,50,59),width=1)
im.save(P/'painted_hair.png')
print('Created 2 drawn faces + 7 patterned pixel texture tiles.')

im=Image.new('RGB',(128,128),(227,216,193));d=ImageDraw.Draw(im)
for x in range(8,128,24):d.line((x,0,x,127),fill=(218,208,187))
for x,y in [(20,14),(68,55),(107,18)]:d.polygon([(x,y),(x-8,y+32),(x+2,y+47)],fill=(201,190,171))
im.save(P/'ivory_silk.png')
