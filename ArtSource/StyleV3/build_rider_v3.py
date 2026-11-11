import bpy, bmesh, math, json
from mathutils import Vector
from mathutils.bvhtree import BVHTree
from mathutils.geometry import barycentric_transform
from math import sin, cos, pi, exp

P=V3_OUTPUT
OUTFIT = 'jacket'
BASENAME = 'RiderTShirtV3' if OUTFIT=='tshirt' else 'RiderV3'
scene = bpy.data.scenes.new('Rider V2 • '+OUTFIT+' low poly streetwear')
bpy.context.window.scene = scene
scene.unit_settings.system = 'METRIC'
asset = bpy.data.collections.new('RIDER_V2'); scene.collection.children.link(asset)
studio = bpy.data.collections.new('STUDIO_V2'); scene.collection.children.link(studio)
parts = []

def mat(name, color, rough=.7, metal=0, fabric=False):
    m=bpy.data.materials.get(name) or bpy.data.materials.new(name); m.diffuse_color=(*color,1);m.use_nodes=True
    bs=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
    bs.inputs['Base Color'].default_value=(*color,1);bs.inputs['Roughness'].default_value=rough;bs.inputs['Metallic'].default_value=metal
    for node in list(m.node_tree.nodes):
        if node.type in ('TEX_NOISE','BUMP'):m.node_tree.nodes.remove(node)
    return m
nylon=mat('R2 • petrol nylon',(.045,.105,.115),.78,fabric=True)
panel=mat('R2 • warm ecru fabric',(.57,.54,.45),.8,fabric=True)
denim=mat('R2 • washed indigo denim',(.026,.033,.050),.86,fabric=True)
seam=mat('R2 • dark piping',(.009,.016,.022),.75)
stitch=mat('R2 • muted stitch',(.19,.24,.25),.85)
rubber=mat('R2 • rubber',(.012,.016,.021),.88)
leather=mat('R2 • glove and shoe leather',(.025,.029,.034),.58)
sole=mat('R2 • worn ivory rubber',(.52,.50,.43),.85)
shell=mat('R2 • ceramic graphite helmet',(.045,.055,.064),.5,.05)
shelltrim=mat('R2 • helmet black trim',(.009,.014,.019),.48)
glass=mat('R2 • smoked blue visor',(.008,.017,.023),.28,.08)
chrome=mat('R2 • satin alloy',(.32,.37,.38),.32,.72)
amber=mat('R2 • muted amber',(.53,.26,.085),.55)
if OUTFIT=='tshirt':
    cotton=mat('R2 • washed cotton',(.44,.42,.35),.86)
    skin=mat('R2 • warm skin',(.38,.235,.16),.86)

def mesh(name, vs, fs, material, smooth=False, sub=0):
    me=bpy.data.meshes.new(name);me.from_pydata(vs,[],fs);me.update()
    ob=bpy.data.objects.new(name,me);asset.objects.link(ob);me.materials.append(material)
    bm=bmesh.new();bm.from_mesh(me);bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=.00003)
    bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(me);bm.free()
    for poly in me.polygons:poly.use_smooth=smooth
    parts.append(ob);return ob

def thickness(ob,t=.004,bevel=0):
    mod=ob.modifiers.new('Real edge thickness','SOLIDIFY');mod.thickness=t;mod.offset=0
    bpy.context.view_layer.objects.active=ob;ob.select_set(True);bpy.ops.object.modifier_apply(modifier=mod.name)
    if bevel:
        mod=ob.modifiers.new('Soft manufactured edges','BEVEL');mod.width=bevel;mod.segments=1
        bpy.ops.object.modifier_apply(modifier=mod.name)
    ob.select_set(False)

def line(name,points,radius,material):
    cu=bpy.data.curves.new(name,'CURVE');cu.dimensions='3D';cu.resolution_u=2;cu.bevel_depth=radius;cu.bevel_resolution=0
    sp=cu.splines.new('POLY');sp.points.add(len(points)-1)
    for p,co in zip(sp.points,points):p.co=(*co,1)
    ob=bpy.data.objects.new(name,cu);asset.objects.link(ob);cu.materials.append(material)
    bpy.context.view_layer.objects.active=ob;ob.select_set(True);bpy.ops.object.convert(target='MESH');ob=bpy.context.object;ob.select_set(False);parts.append(ob);return ob

def rings(name,levels,material,n=16,sub=1,caps=True):
    vs=[];fs=[]
    for j,(x,y,z,rx,ry) in enumerate(levels):
        for i in range(n):
            a=2*pi*i/n; f=0
            vs.append((x+(rx+f)*sin(a),y-(ry+f*.65)*cos(a),z))
    for j in range(len(levels)-1):
        for i in range(n):a=j*n+i;b=j*n+(i+1)%n;fs.append((a,b,b+n,a+n))
    if caps:fs.extend([tuple(reversed(range(n))),tuple((len(levels)-1)*n+i for i in range(n))])
    return mesh(name,vs,fs,material,sub=sub)

def patch(name,coords,material,t=.002,sub=1):
    ob=mesh(name,coords,[tuple(range(len(coords)))],material,sub=0)
    thickness(ob,t,.0015);return ob

# Connected garment; colour panels are faces of the same surface, never floating decals.
if OUTFIT=='tshirt':
    body_levels=[(1.055,.169,.116),(1.08,.171,.114),(1.12,.170,.113),(1.20,.175,.112),(1.30,.184,.109),(1.36,.200,.104),(1.42,.218,.099),(1.48,.222,.091),(1.51,.186,.074),(1.55,.066,.053)]
else:
    body_levels=[(1.075,.179,.119),(1.09,.183,.121),(1.12,.190,.122),(1.20,.203,.124),(1.30,.221,.126),(1.36,.234,.121),(1.42,.244,.113),(1.48,.239,.103),(1.51,.199,.080),(1.55,.071,.059)]
N=24;vs=[];fs=[];colour_faces=[]
for j,(z,rx,ry) in enumerate(body_levels):
    for i in range(N):
        a=2*pi*i/N
        fold=.004*sin(5*a+.5*j)*exp(-((z-1.14)/.12)**2)+.002*sin(3*a-1.2*j)
        vs.append(((rx+fold)*sin(a),-(ry+fold*.8)*cos(a),z+.003*sin(3*a+.4*j)))
for j in range(len(body_levels)-1):
    for i in range(N):
        sidehole=(4<=j<7 and (4<=i<8 or 16<=i<20))
        if not sidehole:
            if OUTFIT=='jacket' and j==6 and (i<=3 or i>=20):colour_faces.append(len(fs))
            fs.append((j*N+i,j*N+(i+1)%N,(j+1)*N+(i+1)%N,(j+1)*N+i))
for sign,lo,hi in [(1,4,8),(-1,16,20)]:
    boundary=[4*N+i for i in range(lo,hi+1)]+[j*N+hi for j in range(5,8)]+[7*N+i for i in range(hi-1,lo-1,-1)]+[j*N+lo for j in (6,5)]
    ordered=[(math.atan2(vs[idx][2]-1.39,-vs[idx][1]),idx) for idx in boundary];ordered.sort();boundary=[idx for angle,idx in ordered]
    angles=[math.atan2(vs[idx][2]-1.39,-vs[idx][1]) for idx in boundary]
    prev=boundary
    if OUTFIT=='tshirt':
        sleeve=[(.255,1.435,.068,.073),(.29,1.45,.065,.068),(.325,1.46,.060,.061),(.338,1.46,.060,.061)]
    else:
        sleeve=[(.275,1.415,.086,.092),(.31,1.43,.087,.093),(.36,1.445,.081,.089),(.402,1.45,.083,.084),(.426,1.453,.081,.081),(.449,1.46,.082,.082),(.475,1.46,.077,.08),(.51,1.46,.075,.078),(.55,1.46,.070,.072),(.586,1.46,.065,.066),(.61,1.46,.051,.052),(.632,1.46,.047,.047)]
    for j,(x,z,ry,rz) in enumerate(sleeve):
        ring=[]
        for a in angles:
            f=.005*sin(2*a+j*.65)*exp(-((x-.44)/.14)**2)
            ring.append(len(vs));vs.append((sign*x,-(ry+f)*cos(a),z+(rz+f)*sin(a)+.007*sin(a*2+j*.8)*exp(-((x-.43)/.1)**2)))
        for i in range(len(ring)):k=(i+1)%len(ring);fs.append((prev[i],prev[k],ring[k],ring[i]))
        prev=ring
jacket=mesh('T-Shirt • connected torso and short sleeves' if OUTFIT=='tshirt' else 'Jacket • continuous torso and sleeves',vs,fs,cotton if OUTFIT=='tshirt' else nylon)
jacket.data.materials.append(panel)
for i in colour_faces:jacket.data.polygons[i].material_index=1
# Thin seams follow the actual garment triangles and inherit their barycentric skin weights.
jacket.data.calc_loop_triangles()
surface_vertices=[v.co.copy() for v in jacket.data.vertices]
surface_triangles=[tuple(t.vertices) for t in jacket.data.loop_triangles]
garment_bvh=BVHTree.FromPolygons(surface_vertices,surface_triangles,all_triangles=True)
def on_front(x,z,offset=.003):
    hit,normal,index,distance=garment_bvh.ray_cast(Vector((x,-1,z)),Vector((0,1,0)))
    if hit is None:raise ValueError('Seam point missed garment: '+str((x,z)))
    return hit+normal*offset

def front_line(name,xz,radius,material):
    ob=line(name,[on_front(x,z,max(.002,radius+.001)) for x,z in xz],radius,material)
    ob['bind_garment']=True
    return ob

if OUTFIT=='tshirt':
    # Bare arms continue beneath the short sleeve; no hidden full-length jacket sleeves.
    for sign in [-1,1]:
        av=[];af=[];n=12
        for x,ry,rz in [(.315,.046,.047),(.35,.047,.047),(.40,.044,.045),(.435,.040,.042),(.47,.045,.044),(.53,.043,.042),(.59,.039,.036),(.642,.036,.033)]:
            for k in range(n):a=2*pi*k/n;av.append((sign*x,-ry*cos(a),1.46+rz*sin(a)))
        for j in range(7):
            for k in range(n):a=j*n+k;b=j*n+(k+1)%n;af.append((a,b,b+n,a+n))
        mesh('Bare arm',av,af,skin)
        # Sleeve hem is an actual narrow band of the shirt surface.
        for poly in jacket.data.polygons:
            centre=sum((jacket.data.vertices[i].co for i in poly.vertices),Vector())/len(poly.vertices)
            if abs(centre.x)>.327:poly.material_index=1
    for poly in jacket.data.polygons:
        centre=sum((jacket.data.vertices[i].co for i in poly.vertices),Vector())/len(poly.vertices)
        if centre.z>1.525 or centre.z<1.071:poly.material_index=1
else:
    # Hem sits outside the waistband, including the forward extreme of the trousers.
    rings('Jacket ribbed hem',[(0,0,1.068,.180,.122),(0,0,1.077,.185,.125),(0,0,1.096,.187,.127),(0,0,1.106,.185,.125)],seam,24,0,caps=False)
    for sign in [-1,1]:
        front_line('Lower welt pocket',[(sign*.081,1.16),(sign*.105,1.18),(sign*.13,1.21),(sign*.153,1.24)],.003,seam)
        front_line('Pocket top stitching',[(sign*.085,1.153),(sign*.109,1.173),(sign*.134,1.203),(sign*.157,1.233)],.0009,stitch)
        cv=[];cf=[]
        for j,(x,r) in enumerate([(.612,.056),(.621,.057),(.640,.050),(.648,.042)]):
            for k in range(16):a=2*pi*k/16;cv.append((sign*x,-r*cos(a),1.46+r*sin(a)))
        for j in range(3):
            for k in range(16):a=j*16+k;b=j*16+(k+1)%16;cf.append((a,b,b+16,a+16))
        mesh('Elastic cuff',cv,cf,seam)
    front_line('Main jacket zip',[(0,1.103),(0,1.12),(0,1.20),(0,1.30),(0,1.36),(0,1.42),(0,1.48),(0,1.515)],.0025,seam)
    for j in range(12):
        z=1.12+j*.032
        front_line('Zip teeth',[(-.004,z),(.004,z)],.001,chrome)
    front_line('Zip pull',[(0,1.40),(0,1.38),(.008,1.38),(.008,1.40),(0,1.40)],.0015,chrome)

# One welded trouser mesh: waist/seat flow into both legs at a shared crotch vertex.
vs=[];fs=[]
for s in [-1,1]:
    upper=[]
    for z,rx,ry in [(1.065,.148,.092),(1.04,.155,.10),(.995,.173,.112)]:
        ring=[]
        for i in range(17):a=pi*i/16;ring.append(len(vs));vs.append((s*rx*sin(a),-ry*cos(a),z))
        if upper:
            for i in range(16):fs.append((upper[i],upper[i+1],ring[i+1],ring[i]))
        upper=ring
    levels=[(.89,.092,.095),(.85,.094,.098),(.78,.091,.09),(.70,.085,.084),(.63,.084,.081),(.585,.083,.079),(.56,.081,.077),(.54,.085,.081),(.51,.082,.079),(.47,.084,.081),(.40,.085,.082),(.32,.093,.09),(.24,.103,.094),(.16,.112,.103),(.12,.113,.104),(.10,.109,.10)]
    for j,(z,rx,ry) in enumerate(levels):
        ring=[]
        for i in range(17):
            a=-pi/2+2*pi*i/16
            f=.003*sin(2*a+j*.45)+.006*sin(2*a+j*1.1)*exp(-((z-.55)/.13)**2)
            spread=.040*max(0,min(1,(.60-z)/.50))
            x=s*(.092+spread+(rx+f)*sin(a));y=-(ry+f)*cos(a)+.008*sin(j*.25)
            if j==0 and i in (0,16):x=0;y=0
            ring.append(len(vs));vs.append((x,y,z+.009*sin(2*a+j*.6)*exp(-((z-.55)/.16)**2)))
        for i in range(16):fs.append((upper[i],upper[i+1],ring[i+1],ring[i]))
        upper=ring
pants=mesh('Trousers • continuous seat crotch and legs',vs,fs,denim)
rings('Denim waistband',[(0,0,1.043,.153,.099),(0,0,1.052,.151,.098),(0,0,1.073,.147,.092)],denim,16,0)
for s in [-1,1]:
    # Folded side cargo pocket panels, not cuboid blocks.
    x=s*.18
    patch('Cargo pocket gusset',[(x,-.045,.79),(s*.198,-.055,.80),(s*.20,.052,.80),(x,.055,.79),(x,.05,.91),(s*.198,.045,.91),(s*.198,-.045,.91),(x,-.05,.91)],denim,.003)
    patch('Cargo pocket flap',[(s*.181,-.056,.917),(s*.205,-.05,.91),(s*.205,.05,.91),(s*.181,.057,.917)],denim,.004)
    line('Cargo pocket stitching',[(s*.207,-.044,.888),(s*.208,-.044,.812),(s*.208,.045,.812),(s*.207,.045,.888)],.0012,stitch)
    line('Front pocket seam',[(s*.025,-.10,1.047),(s*.057,-.113,1.015),(s*.127,-.086,.981)],.0015,stitch)
    line('Denim outer seam',[(s*.187,.007,.89),(s*.181,.01,.75),(s*.164,.015,.6),(s*.167,.008,.42),(s*.196,.012,.23),(s*.205,.01,.115)],.0013,stitch)
    line('Denim hem double stitch',[(s*(.132+.112*sin(a)),.01-.104*cos(a),.123) for a in [i*2*pi/16 for i in range(17)]],.0013,stitch)
line('Jeans fly',[(.008,-.104,1.035),(.009,-.117,.995),(.006,-.115,.967)],.0014,seam)

# Chunky lace-up street boots with a shaped toe box, welt, heel counter and tread.
for s in [-1,1]:
    x=s*.095
    rings('Boot outsole',[(x,-.044,.012,.077,.148),(x,-.044,.018,.080,.151),(x,-.044,.04,.079,.15),(x,-.044,.046,.075,.146)],sole,12,0)
    rings('Boot upper',[(x,-.043,.045,.075,.143),(x,-.043,.075,.077,.141),(x,-.025,.11,.071,.121),(x,.005,.155,.061,.077),(x,.014,.202,.055,.066),(x,.014,.217,.053,.064)],leather,12,0)
    line('Boot welt',[(x+.079*sin(a),-.044-.15*cos(a),.043) for a in [i*2*pi/16 for i in range(17)]],.0018,seam)
    patch('Boot tongue',[(x-.038,-.125,.105),(x+.038,-.125,.105),(x+.032,-.066,.185),(x-.032,-.066,.185)],leather,.004)
    for j in range(5):
        y=-.113+j*.011;z=.119+j*.011
        line('Crossed shoelace',[(x-.037,y,z),(x+.035,y+.009,z+.01)],.0024,sole)
        line('Crossed shoelace',[(x+.037,y,z),(x-.035,y+.009,z+.01)],.0024,sole)
    line('Toe cap seam',[(x-.065,-.135,.077),(x-.046,-.169,.077),(x,-.184,.077),(x+.046,-.169,.077),(x+.065,-.135,.077)],.0018,stitch)
    for j in range(3):line('Sole tread',[(x-.063,-.145+j*.1,.014),(x+.063,-.145+j*.1,.014)],.003,rubber)

# Gloves: a palm plus bent finger surfaces, unified and softened at the joints.
for s in [-1,1]:
    # Palm surface along X, made with custom rounded cross sections.
    vs=[];fs=[];n=8
    for x,ry,rz in [(.637,.039 if OUTFIT=='tshirt' else .025,.036 if OUTFIT=='tshirt' else .024),(.66,.041 if OUTFIT=='tshirt' else .038,.032 if OUTFIT=='tshirt' else .027),(.69,.041,.027),(.713,.035,.023)]:
        for k in range(n):a=2*pi*k/n;vs.append((s*x,ry*cos(a),1.46+rz*sin(a)))
    for j in range(3):
        for k in range(n):a=j*n+k;b=j*n+(k+1)%n;fs.append((a,b,b+n,a+n))
    fs.extend([tuple(reversed(range(n))),tuple(3*n+i for i in range(n))])
    hand=mesh('Gloved palm',vs,fs,leather,sub=1)
    for j in range(4):
        yy=(j-1.5)*.019;length=[.048,.059,.055,.043][j];vs=[];fs=[]
        for q in range(3):
            t=q/2;xx=.705+length*t;zz=1.46-.025*t*t;radius=.010*(1-.32*t)
            for k in range(8):a=2*pi*k/8;vs.append((s*xx,yy+radius*cos(a),zz+radius*sin(a)))
        for q in range(2):
            for k in range(8):a=q*8+k;b=q*8+(k+1)%8;fs.append((a,b,b+8,a+8))
        fs.append(tuple(16+k for k in range(8)));mesh('Shaped glove finger',vs,fs,leather,sub=1)
    line('Glove knuckle seam',[(s*.687,-.034,1.48),(s*.692,0,1.491),(s*.687,.034,1.48)],.0025,stitch)
    # Thumb is a curved tapered surface, not a rectangular peg.
    vs=[];fs=[]
    for j,(xx,yy,zz,r) in enumerate([(.66,-.034,1.454,.016),(.68,-.054,1.445,.014),(.708,-.059,1.428,.012),(.719,-.05,1.414,.009)]):
        for k in range(8):a=2*pi*k/8;vs.append((s*xx,yy+r*cos(a),zz+r*sin(a)))
    for j in range(3):
        for k in range(8):a=j*8+k;b=j*8+(k+1)%8;fs.append((a,b,b+8,a+8))
    fs.append(tuple(24+k for k in range(8)));mesh('Curved glove thumb',vs,fs,leather,sub=1)

# Full-face sports helmet. The shell has a real visor cutout and an elongated chin guard.
helmet_levels=[(1.635,.103,.138,.112),(1.655,.128,.222,.137),(1.69,.149,.252,.155),(1.74,.158,.222,.162),(1.785,.160,.173,.164),(1.84,.157,.163,.163),(1.865,.154,.147,.157),(1.91,.14,.12,.145),(1.953,.10,.086,.105),(1.972,.048,.043,.052)]
n=20;vs=[];fs=[]
for j,(z,rx,front,back) in enumerate(helmet_levels):
    for i in range(n):
        a=2*pi*i/n;c=cos(a);y=-(front if c>=0 else back)*c
        # Planar-looking brow and temple break instead of a vertical egg profile.
        if c>.5 and 1.725<=z<=1.865:y-=.006*(2*c-1)
        vs.append((rx*sin(a),y,z))
for j in range(len(helmet_levels)-1):
    for i in range(n):
        a=(i+.5)*2*pi/n
        opening=(3<=j<=4 and cos(a)>.25)
        if not opening:fs.append((j*n+i,j*n+(i+1)%n,(j+1)*n+(i+1)%n,(j+1)*n+i))
fs.append(tuple((len(helmet_levels)-1)*n+i for i in range(n)))
helmet=mesh('Helmet • sculpted shell with open eye port',vs,fs,shell,sub=1);thickness(helmet,.009)
# Visor shape is a separate curved surface fitting the eye port.
vs=[];fs=[];N=12
for j in range(3):
    t=j/2
    for i in range(N+1):
        a=-1.28+2.56*i/N;z=(1-t)*(1.742+.008*abs(sin(a)))+t*(1.851-.025*abs(sin(a)))
        rx=.157+.003*sin(pi*t);depth=.226*(1-t)+.167*t
        vs.append((rx*sin(a),-depth*cos(a)-.004,z))
for j in range(2):
    for i in range(N):a=j*(N+1)+i;fs.append((a,a+1,a+N+2,a+N+1))
visor=mesh('Visor • curved smoked shield',vs,fs,glass,sub=1);thickness(visor,.003)
for top in [False,True]:
    pts=[]
    for i in range(13):
        a=-1.28+2.56*i/12;t=1 if top else 0;z=(1-t)*(1.742+.008*abs(sin(a)))+t*(1.851-.025*abs(sin(a)))
        pts.append((.16*sin(a),-(.226*(1-t)+.167*t)*cos(a)-.007,z))
    line('Eye port gasket',pts,.004,shelltrim)
# Chin intake: shaped recess with real grille slots and a surround.
patch('Chin intake surround',[(-.062,-.232,1.681),(-.048,-.216,1.654),(.048,-.216,1.654),(.062,-.232,1.681),(.037,-.247,1.705),(-.037,-.247,1.705)],shelltrim,.004)
for j in range(3):line('Chin vent grille',[(-.036,-.226-j*.009,1.662+j*.013),(.036,-.226-j*.009,1.662+j*.013)],.0018,chrome)
for s in [-1,1]:
    patch('Brow vent recess',[(s*.043,-.12,1.914),(s*.093,-.098,1.906),(s*.084,-.135,1.878),(s*.039,-.151,1.885)],shelltrim,.003)
    line('Brow intake divider',[(s*.063,-.144,1.885),(s*.073,-.113,1.909)],.002,shell)
    patch('Cheek aerodynamic break',[(s*.098,-.134,1.66),(s*.145,-.072,1.689),(s*.146,-.022,1.75),(s*.132,-.10,1.73)],shell,.004)
    # Visor hinge pivot disc is a contoured concentric 12-sided manufactured part.
    coords=[];faces=[]
    for xx,r in [(s*.151,.023),(s*.163,.022),(s*.164,.016)]:
        for i in range(16):a=2*pi*i/16;coords.append((xx,.006+r*cos(a),1.79+r*sin(a)))
    for j in range(2):
        for i in range(16):a=j*16+i;b=j*16+(i+1)%16;faces.append((a,b,b+16,a+16))
    faces.append(tuple(32+i for i in range(16)));mesh('Recessed visor pivot',coords,faces,shelltrim,sub=0)
    line('Pivot fastener',[(s*.166,-.003,1.79),(s*.166,.015,1.79)],.002,chrome)
    line('Helmet temple seam',[(s*.143,.03,1.82),(s*.141,.10,1.82),(s*.11,.15,1.81)],.0015,shelltrim)
patch('Rear aero spoiler',[(-.118,.115,1.916),(.118,.115,1.916),(.135,.17,1.9),(-.135,.17,1.9)],shell,.006)
line('Rear exhaust slit',[(-.075,.155,1.864),(0,.169,1.862),(.075,.155,1.864)],.004,shelltrim)
line('Crown centre pinstripe',[(0,-.137,1.918),(0,-.075,1.964),(0,.025,1.978),(0,.11,1.948)],.0018,amber)
rings('Balaclava neck',[(0,0,1.50,.052,.049),(0,0,1.63,.057,.055),(0,0,1.69,.062,.061)],rubber,16,1)

# Low-poly construction: keep broad flat panels, a flatter racing crown and short neck.
helmet_prefixes=('Helmet','Visor','Eye port','Chin','Brow','Cheek','Recessed','Pivot','Rear','Crown')
for ob in parts:
    if ob.name.startswith(helmet_prefixes):
        for v in ob.data.vertices:
            if v.co.z>1.89:v.co.z=1.89+(v.co.z-1.89)*.60
            v.co.z-=.035
    for poly in ob.data.polygons:poly.use_smooth=False

# Larger hands: length and palm breadth grow away from the fitted wrist cuff.
for ob in parts:
    if ob.name.startswith(('Gloved palm','Shaped glove finger','Curved glove thumb','Glove knuckle seam')):
        for v in ob.data.vertices:
            ax=abs(v.co.x);sign=1 if v.co.x>=0 else -1
            t=max(0,min(1,(ax-.637)/.045));t=t*t*(3-2*t)
            v.co.x=sign*(.637+(ax-.637)*1.30)
            v.co.y*=1+.22*t
            v.co.z=1.46+(v.co.z-1.46)*(1+.18*t)
        ob.data.update()

# Anatomical rig retained in proportions, with new mesh weights rather than geometry glued to primitives.
previous=bpy.data.objects.get('RiderV3_Rig')
if previous is not None:previous.name='RiderV3_Draft_Rig'
arm=bpy.data.armatures.new('Rider V2 anatomical rig');rig=bpy.data.objects.new('RiderV3_Rig',arm);asset.objects.link(rig)
for o in bpy.context.selected_objects:o.select_set(False)
rig.select_set(True);bpy.context.view_layer.objects.active=rig;bpy.ops.object.mode_set(mode='EDIT')
spec=[('Hips',(0,0,1.0),(0,0,1.12),None),('Spine',(0,0,1.12),(0,0,1.32),'Hips'),('Chest',(0,0,1.32),(0,0,1.44),'Spine'),('UpperChest',(0,0,1.44),(0,0,1.54),'Chest'),('Neck',(0,0,1.54),(0,0,1.67),'UpperChest'),('Head',(0,0,1.67),(0,0,1.94),'Neck')]
# FBX -Z/Y maps Blender +X to Unity -X; Left must therefore be +X here.
for s,label in [(1,'Left'),(-1,'Right')]:
    spec.extend([(label+'Shoulder',(s*.11,0,1.46),(s*.195,0,1.46),'UpperChest'),(label+'UpperArm',(s*.195,0,1.46),(s*.435,0,1.46),label+'Shoulder'),(label+'LowerArm',(s*.435,0,1.46),(s*.64,0,1.46),label+'UpperArm'),(label+'Hand',(s*.64,0,1.46),(s*.76,0,1.44),label+'LowerArm'),(label+'UpperLeg',(s*.085,0,.99),(s*.09,-.01,.55),'Hips'),(label+'LowerLeg',(s*.09,-.01,.55),(s*.095,-.005,.105),label+'UpperLeg'),(label+'Foot',(s*.095,-.005,.105),(s*.095,-.15,.065),label+'LowerLeg'),(label+'Toes',(s*.095,-.15,.065),(s*.095,-.235,.055),label+'Foot')])
for name,head,tail,parent in spec:
    b=arm.edit_bones.new('MD_'+name);b.head=head;b.tail=tail
    if parent:b.parent=arm.edit_bones['MD_'+parent]
    b.align_roll(Vector((0,-1,0)))
bpy.ops.object.mode_set(mode='OBJECT');rig.select_set(False)
def smooth(v):v=max(0,min(1,v));return v*v*(3-2*v)
def weights(p,ob):
    if ob.get('bind_garment',False):
        hit,normal,index,distance=garment_bvh.find_nearest(p)
        tri=surface_triangles[index]
        bary=barycentric_transform(hit,surface_vertices[tri[0]],surface_vertices[tri[1]],surface_vertices[tri[2]],Vector((1,0,0)),Vector((0,1,0)),Vector((0,0,1)))
        result={}
        for k in range(3):
            for name,w in weights(surface_vertices[tri[k]],jacket).items():result[name]=result.get(name,0)+max(0,bary[k])*w
        total=sum(result.values())
        return {name:w/total for name,w in result.items()}
    x,y,z=p;ax=abs(x);side='Left' if x>0 else 'Right'
    if ob.name.startswith(('Helmet','Visor','Eye port','Chin','Brow','Cheek','Recessed','Pivot','Rear','Crown')):return {'Head':1}
    if z>1.56:return {'Head':smooth((z-1.55)/.12),'Neck':1-smooth((z-1.55)/.12)}
    if ax>.19 and z>1.27:
        if ax<.28:
            t=smooth((ax-.17)/.11);return {side+'UpperArm':t,'UpperChest':1-t}
        if ax<.47:
            t=smooth((ax-.37)/.10);return {side+'UpperArm':1-t,side+'LowerArm':t}
        if ax<.65:
            t=smooth((ax-.60)/.05);return {side+'LowerArm':1-t,side+'Hand':t}
        return {side+'Hand':1}
    if z<1.0:
        if z<.20 and ob.name.startswith(('Boot','Crossed','Toe cap','Sole')):return {side+'Foot':1}
        if z<.45:return {side+'LowerLeg':1}
        if z<.65:
            t=smooth((z-.45)/.20);return {side+'LowerLeg':1-t,side+'UpperLeg':t}
        t=smooth((z-.86)/.14);return {side+'UpperLeg':1-t,'Hips':t}
    if z<1.20:
        t=smooth((z-1.02)/.18);return {'Hips':1-t,'Spine':t}
    if z<1.40:
        t=smooth((z-1.2)/.2);return {'Spine':1-t,'Chest':t}
    return {'UpperChest':1}
for ob in parts:
    # Curves are converted; authored low-poly faces are retained without subdivision.
    groups={n:ob.vertex_groups.new(name='MD_'+n) for n,_,_,_ in spec}
    for v in ob.data.vertices:
        for name,w in weights(v.co,ob).items():
            if w>.0001:groups[name].add([v.index],w,'REPLACE')
    mod=ob.modifiers.new('Rider V2 deformation','ARMATURE');mod.object=rig;ob.parent=rig
    ob['asset_role']='Rider V2 authored clothing / equipment'


# V3 proportions: reference high hips, narrow torso and longer arms. Skin weights
# were authored in V2 bind space; warp mesh and rest bones together afterwards.
def reference_warp(co):
    x,y,z=co;ax=abs(x);sx=1 if x>=0 else -1
    xx=ax*.90 if ax<=.20 else .18+(ax-.20)*1.114
    landmarks=[(0,0),(.105,.105),(.55,.575),(1.0,1.04),(1.46,1.495),(1.54,1.56),(1.94,1.96),(2.1,2.12)]
    zz=z
    for (a,b),(c,d) in zip(landmarks,landmarks[1:]):
        if a<=z<=c:zz=b+(z-a)/(c-a)*(d-b);break
    return Vector((sx*xx,y,zz))
atlas=v3_material(BASENAME,P+BASENAME+'_atlas.png')
for ob in parts:
    uv=ob.data.uv_layers.new(name='UVMap')
    oldmats=list(ob.data.materials)
    for poly in ob.data.polygons:
        original=oldmats[poly.material_index];role='trim'
        if original in (nylon,):role='cloth'
        if OUTFIT=='tshirt' and original==cotton:role='cloth'
        if OUTFIT=='tshirt' and original==skin:role='skin'
        if original==panel:role='ivory'
        if original==denim:role='pants'
        if original==shell:role='shell'
        if original==glass:role='visor'
        if original==chrome:role='silver'
        if original==amber:role='gold'
        if original==sole:role='ivory'
        if original==stitch:role='silver'
        if original==leather and ob.name.startswith('Boot'):role='shoe'
        local_uvs=[]
        for li in poly.loop_indices:
            co=ob.data.vertices[ob.data.loops[li].vertex_index].co;x,y,z=co;ax=abs(x)
            if role=='cloth':
                if ax>.24:u=(math.atan2(z-1.46,-y)+pi)/(2*pi);v=(ax-.24)/.41
                else:u=(x+.25)/.50;v=(z-1.055)/.5
            elif role=='pants':
                center=.092+.040*max(0,min(1,(.60-z)/.50));xx=ax-center
                u=(math.atan2(xx,-y)+pi)/(2*pi);v=(z-.10)/.97
            elif role=='shell':u=(math.atan2(x,-y)+pi)/(2*pi);v=(z-1.60)/.35
            elif role=='visor':u=(x+.17)/.34;v=(z-1.70)/.14
            elif role=='shoe':u=(y+.20)/.28;v=(z-.045)/.17
            else:u=.5;v=.5
            local_uvs.append((li,u,v))
        seam=role in ('pants','shell') or (role=='cloth' and ax>.26)
        cross=seam and max(t[1] for t in local_uvs)-min(t[1] for t in local_uvs)>.5
        high=sum(t[1] for t in local_uvs)/len(local_uvs)>.5
        for li,u,v in local_uvs:
            if cross:u=(1 if high else u) if u<.5 else (u if high else 0)
            uv.data[li].uv=v3_uv(role,u,v)
        poly.material_index=0
        poly.use_smooth=role in ('cloth','pants','skin','shoe','shell','visor')
    ob.data.materials.clear();ob.data.materials.append(atlas)
    for v in ob.data.vertices:v.co=reference_warp(v.co)
    ob['asset_role']='V3 original painted streetwear'
rig.name=BASENAME+'_Rig'
for ob in scene.objects:ob.select_set(False)
rig.select_set(True);bpy.context.view_layer.objects.active=rig;bpy.ops.object.mode_set(mode='EDIT')
for bone in rig.data.edit_bones:bone.head=reference_warp(bone.head);bone.tail=reference_warp(bone.tail);bone.align_roll(Vector((0,-1,0)))
bpy.ops.object.mode_set(mode='OBJECT')
v3_export(scene,asset,parts,rig,BASENAME,P+BASENAME+'.fbx')
for name,angle in [('LeftUpperArm',-63),('RightUpperArm',63)]:rig.pose.bones['MD_'+name].rotation_mode='XYZ';rig.pose.bones['MD_'+name].rotation_euler.z=math.radians(angle)
v3_studio(scene,BASENAME,1.96)
bpy.ops.wm.save_as_mainfile(filepath=P+BASENAME+'.blend',compress=True)
print(json.dumps({'character':BASENAME,'triangles':sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in parts),'bones':len(rig.data.bones),'atlas':'512px','fbx':P+BASENAME+'.fbx'}))
