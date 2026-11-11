import bpy, bmesh, math, json
from mathutils import Vector
from mathutils.bvhtree import BVHTree
from math import sin,cos,pi,exp

P=V3_OUTPUT
PERSON='father'
FATHER=PERSON=='father'
NAME='FamilyFatherV3' if FATHER else 'FamilyMotherV3'
scene=bpy.data.scenes.new('Major Damage • '+NAME)
bpy.context.window.scene=scene
scene.unit_settings.system='METRIC'
collection=bpy.data.collections.new(NAME);scene.collection.children.link(collection)
studio=bpy.data.collections.new(NAME+' Studio');scene.collection.children.link(studio)
parts=[];props=[]

def material(name,color,rough=.8,metal=0):
    m=bpy.data.materials.get('FAM • '+name) or bpy.data.materials.new('FAM • '+name)
    m.use_nodes=True;m.diffuse_color=(*color,1)
    bs=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
    bs.inputs['Base Color'].default_value=(*color,1);bs.inputs['Roughness'].default_value=rough;bs.inputs['Metallic'].default_value=metal
    return m
skin=material('father warm skin' if FATHER else 'mother warm skin',(.52,.31,.225) if FATHER else (.63,.39,.285),.84)
shadow=material('father skin shadow' if FATHER else 'mother skin shadow',(.30,.16,.12) if FATHER else (.40,.22,.17),.9)
cloth=material('cashmere slate' if FATHER else 'carmine wool',(.15,.19,.205) if FATHER else (.43,.018,.025))
clothdark=material('coat facing' if FATHER else 'burgundy facing',(.095,.12,.135) if FATHER else (.23,.008,.016))
trousers=material('black tailored wool' if FATHER else 'red tailored wool',(.018,.022,.03) if FATHER else (.31,.012,.019))
shirt=material('ivory silk',(.74,.70,.60),.72)
purple=material('imperial plum silk',(.115,.014,.18),.6)
hair=material('soft black hair',(.009,.011,.014),.72)
hairlight=material('black hair facet',(.022,.025,.030),.7)
leather=material('polished oxford leather' if FATHER else 'burgundy patent leather',(.015,.018,.022) if FATHER else (.055,.005,.012),.4)
gold=material('antique gold',(.52,.31,.085),.35,.65)
silver=material('white gold',(.48,.53,.56),.3,.7)
ruby=material('garnet',(.24,.004,.012),.3,.25)
white=material('eye ivory',(.72,.70,.64),.83)
iris=material('brown iris',(.07,.028,.011),.75)
pupil=material('ink',(.003,.004,.006),.9)
lips=material('muted lip' if FATHER else 'rose lip',(.28,.10,.075) if FATHER else (.39,.075,.065),.8)


atlas=v3_material(NAME,P+NAME+'_atlas.png')

def mesh(name,verts,faces,mat,bone=None,role=None):
    data=bpy.data.meshes.new(name);data.from_pydata(verts,[],faces);data.update()
    ob=bpy.data.objects.new(name,data);collection.objects.link(ob);data.materials.append(mat)
    bm=bmesh.new();bm.from_mesh(data);bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=.00002);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(data);bm.free()
    for face in data.polygons:face.use_smooth=False
    if bone:ob['bone']=bone
    if role:ob['role']=role
    parts.append(ob);return ob

def rings(name,levels,mat,n=16,caps=True,bone=None,role=None):
    vs=[];fs=[]
    for x,y,z,rx,ry in levels:
        for k in range(n):a=2*pi*k/n;vs.append((x+rx*sin(a),y-ry*cos(a),z))
    for j in range(len(levels)-1):
        for k in range(n):a=j*n+k;b=j*n+(k+1)%n;fs.append((a,b,b+n,a+n))
    if caps:fs.extend([tuple(reversed(range(n))),tuple((len(levels)-1)*n+k for k in range(n))])
    return mesh(name,vs,fs,mat,bone,role)

def line(name,points,radius,mat,bone=None,role=None):
    cu=bpy.data.curves.new(name,'CURVE');cu.dimensions='3D';cu.bevel_depth=radius;cu.bevel_resolution=0;cu.resolution_u=1
    sp=cu.splines.new('POLY');sp.points.add(len(points)-1)
    for v,p in zip(sp.points,points):v.co=(*p,1)
    ob=bpy.data.objects.new(name,cu);collection.objects.link(ob);cu.materials.append(mat)
    for old in bpy.context.selected_objects:old.select_set(False)
    ob.select_set(True);bpy.context.view_layer.objects.active=ob;bpy.ops.object.convert(target='MESH');ob=bpy.context.object;ob.select_set(False)
    if bone:ob['bone']=bone
    if role:ob['role']=role
    parts.append(ob);return ob

def patch(name,points,mat,bone=None):return mesh(name,points,[tuple(range(len(points)))],mat,bone)

def jewel(name,c,rx,ry,rz,mat,bone=None):
    x,y,z=c
    return mesh(name,[(x-rx,y,z),(x,y-ry,z),(x+rx,y,z),(x,y+ry,z),(x,y,z+rz),(x,y,z-rz)],[(0,1,4),(1,2,4),(2,3,4),(3,0,4),(1,0,5),(2,1,5),(3,2,5),(0,3,5)],mat,bone)

# Tailored torso and sleeves share their armhole boundaries.
if FATHER:
    levels=[(.59,.267,.165),(.65,.284,.18),(.84,.305,.20),(1.05,.352,.25),(1.20,.36,.255),(1.35,.345,.236),(1.46,.347,.202),(1.56,.381,.175),(1.635,.324,.139),(1.69,.105,.074)]
    arm_z=1.57;shoulder=.315;elbow=.565;wrist=.805
    sleeve=[(.395,1.565,.112,.113),(.43,1.57,.111,.112),(.49,1.57,.105,.104),(.55,1.57,.097,.10),(.58,1.57,.092,.095),(.64,1.57,.091,.092),(.71,1.57,.083,.087),(.77,1.57,.073,.077),(.805,1.57,.068,.071)]
else:
    levels=[(.985,.202,.126),(1.04,.197,.128),(1.13,.171,.116),(1.22,.158,.108),(1.30,.180,.125),(1.37,.207,.129),(1.43,.232,.112),(1.49,.246,.097),(1.525,.202,.081),(1.567,.066,.053)]
    arm_z=1.465;shoulder=.198;elbow=.435;wrist=.65
    sleeve=[(.273,1.46,.070,.075),(.31,1.465,.067,.071),(.36,1.465,.061,.066),(.408,1.465,.059,.062),(.435,1.465,.058,.060),(.47,1.465,.055,.058),(.53,1.465,.051,.055),(.595,1.465,.046,.05),(.646,1.465,.043,.047)]
N=24;vs=[];fs=[];lapel_faces=[]
for j,(z,rx,ry) in enumerate(levels):
    for k in range(N):
        a=2*pi*k/N;f=.003*sin(3*a+j*.7)
        x=(rx+f)*sin(a);y=-(ry+f)*cos(a)
        if not FATHER and j>=4 and k in (1,2,22,23):
            sign=1 if k<12 else -1
            opening=min(rx*.75,max(.008,.35*(z-1.31)))
            x=sign*(opening if k in (1,23) else min(rx*.9,opening+.04))
            y=-ry*math.sqrt(max(0,1-(x/rx)**2))
        if not FATHER and y<0:
            y-=.061*exp(-((abs(x)-.086)/.067)**2-((z-1.385)/.089)**2)*max(0,min(1,-y/.075))
        vs.append((x,y,z))
for j in range(len(levels)-1):
    for k in range(N):
        hole=(5<=j<8 and (4<=k<8 or 16<=k<20))
        opening=(FATHER and ((j<3 and (k<3 or k>=21)) or (j>=3 and k in (23,0))))
        vent=(FATHER and j<2 and k in (11,12))
        if not hole and not opening and not vent:
            if (FATHER and j>=5 and k in (1,2,21,22)) or (not FATHER and j>=4 and k in (1,22)):lapel_faces.append(len(fs))
            fs.append((j*N+k,j*N+(k+1)%N,(j+1)*N+(k+1)%N,(j+1)*N+k))
for sign,lo,hi in [(1,4,8),(-1,16,20)]:
    center_z=(levels[5][0]+levels[8][0])/2
    boundary=[5*N+k for k in range(lo,hi+1)]+[j*N+hi for j in range(6,9)]+[8*N+k for k in range(hi-1,lo-1,-1)]+[j*N+lo for j in (7,6)]
    order=[(math.atan2(vs[i][2]-center_z,-vs[i][1]),i) for i in boundary];order.sort();prev=[i for a,i in order];angles=[a for a,i in order]
    for j,(x,z,ry,rz) in enumerate(sleeve):
        new=[]
        for a in angles:
            f=.003*sin(2*a+j*.8)
            new.append(len(vs));vs.append((sign*x,-(ry+f)*cos(a),z+(rz+f)*sin(a)))
        for k in range(len(new)):q=(k+1)%len(new);fs.append((prev[k],prev[q],new[q],new[k]))
        prev=new
coat=mesh('Cashmere coat connected body and sleeves' if FATHER else 'Structured red blazer connected body and sleeves',vs,fs,cloth,role='coat')
coat.data.materials.append(clothdark);coat.data.materials.append(shirt)
for i in lapel_faces:coat.data.polygons[i].material_index=1
if not FATHER:
    # Silk neckline is part of the blazer's surface. No floating chest insert.
    for p in coat.data.polygons:
        c=sum((coat.data.vertices[i].co for i in p.vertices),Vector())/len(p.vertices)
        if c.y<-.07 and c.z>1.30 and abs(c.x)<max(.008,.35*(c.z-1.31))*.65:p.material_index=2
coat.data.calc_loop_triangles()
coat_bvh=BVHTree.FromPolygons([v.co for v in coat.data.vertices],[tuple(t.vertices) for t in coat.data.loop_triangles],all_triangles=True)
def front(x,z,offset=.003):
    hit,normal,i,d=coat_bvh.ray_cast(Vector((x,-1,z)),Vector((0,1,0)))
    if hit is None:return Vector((x,-.18,z))
    return hit+normal*offset

if FATHER:
    # Full inner waistcoat stays strictly inside the open outer cashmere coat.
    vest=rings('Plum waistcoat',[(0,0,1.05,.26,.183),(0,0,1.13,.312,.213),(0,0,1.28,.312,.215),(0,0,1.43,.28,.178),(0,0,1.55,.26,.145),(0,0,1.63,.102,.072)],trousers,24,role='torso')
    # Shirt and tie use one exposed front strip; folded collar has generous clearance.
    patch('Silk shirt opening',[(-.073,-.163,1.52),(.073,-.163,1.52),(.078,-.082,1.67),(-.078,-.082,1.67)],shirt)
    patch('Silk collar left',[(-.086,-.088,1.67),(-.018,-.096,1.653),(-.051,-.178,1.54),(-.11,-.148,1.59)],shirt)
    patch('Silk collar right',[(.086,-.088,1.67),(.018,-.096,1.653),(.051,-.178,1.54),(.11,-.148,1.59)],shirt)
    patch('Plum ascot',[(-.033,-.102,1.655),(.033,-.102,1.655),(.043,-.177,1.57),(0,-.191,1.52),(-.043,-.177,1.57)],purple)
    jewel('Ascot pin',(0,-.19,1.574),.010,.006,.010,gold)
    for z,y in [(1.15,-.201),(1.26,-.204),(1.37,-.193)]:jewel('Waistcoat button',(0,y,z),.009,.004,.009,gold)
    for sign in [-1,1]:
        for z in [1.19,1.32]:jewel('Coat horn button',front(sign*.11,z,.007),.011,.005,.011,leather)
        line('Coat welt pocket',[front(sign*.19,1.08),front(sign*.25,1.11)],.004,clothdark)
        line('Coat lapel edge',[front(sign*.074,1.38),front(sign*.14,1.49),front(sign*.20,1.56)],.002,clothdark)
    line('Gold watch chain',[(.02,-.207,1.27),(.07,-.213,1.22),(.15,-.195,1.21),(.23,-.171,1.27)],.002,gold)
else:
    for z in [1.17,1.28]:jewel('Jacket gold clasp',front(.025,z,.006),.007,.004,.007,gold)
    for sign in [-1,1]:
        line('Blazer pocket welt',[front(sign*.09,1.09),front(sign*.16,1.115)],.0028,clothdark)
    jewel('Ruby lapel brooch',front(.13,1.42,.011),.014,.007,.018,silver)
    jewel('Brooch garnet',front(.13,1.42,.018),.009,.006,.012,ruby)
    # Necklace follows the neckline front; each stone is simple faceted geometry.
    necklace=[]
    for j in range(27):
        a=-pi/2+pi*j/26;x=.071*sin(a);z=1.54-.22*cos(a)
        co=front(x,z,.011);necklace.append(co);jewel('Necklace white gold link',co,.006,.004,.007,silver)
    line('Necklace chain',necklace,.0016,silver)
    jewel('Pendant setting',front(0,1.30,.017),.022,.009,.028,silver)
    jewel('Ruby pendant',front(0,1.30,.025),.015,.006,.020,ruby)

# Trouser seat is welded across the centre and flows into separate legs.
vs=[];fs=[]
waist=1.09 if FATHER else 1.025
hiprx=.235 if FATHER else .191;hipry=.16 if FATHER else .126
for sign in [-1,1]:
    prev=[]
    for z,rx,ry in [(waist,hiprx*.93,hipry*.92),(waist-.06,hiprx,hipry),(waist-.12,hiprx*1.03,hipry*1.03)]:
        ring=[]
        for k in range(17):a=pi*k/16;ring.append(len(vs));vs.append((sign*rx*sin(a),-ry*cos(a),z))
        if prev:
            for k in range(16):fs.append((prev[k],prev[k+1],ring[k+1],ring[k]))
        prev=ring
    levels2=[(.83,.125,.13),(.73,.12,.119),(.62,.11,.105),(.54,.105,.096),(.47,.105,.097),(.36,.105,.091),(.23,.102,.086),(.115,.10,.085)] if FATHER else [(.82,.094,.104),(.72,.091,.097),(.61,.085,.085),(.54,.079,.079),(.47,.081,.078),(.35,.083,.077),(.24,.083,.075),(.13,.081,.073)]
    legcenter=.14 if FATHER else .105
    for j,(z,rx,ry) in enumerate(levels2):
        ring=[]
        for k in range(17):
            a=-pi/2+2*pi*k/16;f=.002*sin(2*a+j*.7)
            x=sign*(legcenter+(rx+f)*sin(a));y=-(ry+f)*cos(a)
            if j==0 and k in (0,16):x=0;y=0
            ring.append(len(vs));vs.append((x,y,z))
        for k in range(16):fs.append((prev[k],prev[k+1],ring[k+1],ring[k]))
        prev=ring
mesh('Tailored trousers continuous seat and legs',vs,fs,trousers,role='legs')
for sign in [-1,1]:
    x=sign*(.14 if FATHER else .105)
    for frontside in [-1,1]:
        ys=[frontside*(.124 if FATHER else .10),frontside*.091,frontside*.078]
        line('Pressed trouser crease',[(x,ys[0],.80),(x,ys[1],.50),(x,ys[2],.15)],.0012,clothdark,role='legs')
    # A sculpted leather shoe rather than an ellipsoid or box.
    if FATHER:
        rings('Oxford welt',[(x,-.045,.015,.094,.171),(x,-.045,.033,.097,.174),(x,-.045,.041,.093,.169)],clothdark,16,bone=('LeftFoot' if sign>0 else 'RightFoot'))
        rings('Oxford shoe',[(x,-.045,.04,.091,.166),(x,-.043,.073,.092,.165),(x,-.026,.12,.081,.139),(x,.015,.17,.065,.085),(x,.025,.195,.064,.08)],leather,16,bone=('LeftFoot' if sign>0 else 'RightFoot'))
        for j in range(3):line('Oxford laces',[(x-.034,-.106+j*.012,.124+j*.011),(x+.034,-.098+j*.012,.131+j*.011)],.0017,clothdark,bone=('LeftFoot' if sign>0 else 'RightFoot'))
    else:
        footbone='LeftFoot' if sign>0 else 'RightFoot'
        rings('Pump shaped toe and sole',[(x,-.065,.025,.056,.12),(x,-.065,.035,.058,.125),(x,-.055,.067,.059,.122),(x,-.012,.10,.054,.084),(x,.025,.137,.048,.052)],leather,12,bone=footbone)
        rings('Sculpted tapered heel',[(x,.067,.012,.023,.028),(x,.067,.03,.024,.029),(x,.056,.10,.033,.036),(x,.047,.127,.042,.041)],leather,8,bone=footbone)
        line('Pump opening trim',[(x+.049*sin(a),.015-.068*cos(a),.122-.03*max(0,cos(a))) for a in [k*2*pi/16 for k in range(17)]],.0016,gold,bone=footbone)

# Hands have broad palms, substantial wrists and four distinct curled fingers plus thumbs.
for sign in [-1,1]:
    bone='LeftHand' if sign>0 else 'RightHand';scale=1.18 if FATHER else 1.0
    hv=[];hf=[];n=8
    for distance,ry,rz in [(0,.039,.030),(.026,.047,.032),(.066,.047,.032),(.091,.039,.027)]:
        for k in range(n):a=2*pi*k/n;hv.append((sign*(wrist+distance*scale),ry*scale*cos(a),arm_z+rz*scale*sin(a)))
    for j in range(3):
        for k in range(n):a=j*n+k;b=j*n+(k+1)%n;hf.append((a,b,b+n,a+n))
    hf.extend([tuple(reversed(range(n))),tuple(3*n+k for k in range(n))]);mesh('Shaped palm',hv,hf,skin,bone)
    for j,length in enumerate([.061,.073,.069,.055]):
        vv=[];ff=[];yy=(j-1.5)*.022*scale
        for q in range(4):
            t=q/3;xx=wrist+(.084+length*t)*scale;zz=arm_z-.036*scale*t*t;r=.011*scale*(1-.25*t)
            for k in range(6):a=2*pi*k/6;vv.append((sign*xx,yy+r*cos(a),zz+r*sin(a)))
        for q in range(3):
            for k in range(6):a=q*6+k;b=q*6+(k+1)%6;ff.append((a,b,b+6,a+6))
        ff.append(tuple(18+k for k in range(6)));mesh('Curved finger',vv,ff,skin,bone)
    line('Shaped thumb',[(sign*(wrist+.023*scale),-.035*scale,arm_z),(sign*(wrist+.05*scale),-.062*scale,arm_z-.01),(sign*(wrist+.085*scale),-.068*scale,arm_z-.036)],.016*scale,skin,bone)
    jewel('Signet ring' if FATHER else 'Ruby ring',(sign*(wrist+.105*scale),.011*scale,arm_z+.009),.011,.01,.006,gold if FATHER else silver,bone)
    if not FATHER:jewel('Ring gemstone',(sign*(wrist+.105),.011,arm_z+.016),.006,.006,.006,ruby,bone)
    # Wrist watch follows the forearm instead of the hand.
    watchbone='LeftLowerArm' if sign>0 else 'RightLowerArm'
    if sign>0:
        line('Watch bracelet',[(sign*(wrist-.024),-.046*cos(k*2*pi/12),arm_z+.045*sin(k*2*pi/12)) for k in range(13)],.005,gold if FATHER else silver,watchbone)
        jewel('Watch face',(sign*(wrist-.024),0,arm_z+.048),.021,.019,.004,gold if FATHER else silver,watchbone)


# V3 curved face with cheek/jaw/neck topology, conforming eyelids and painted features.
head=v3_head(collection,parts,NAME,atlas,chin=1.76 if FATHER else 1.61,height=.30 if FATHER else .245,width=.15 if FATHER else .119,neck_base=1.65 if FATHER else 1.54,neck_radius=.077 if FATHER else .047,age='father' if FATHER else 'mother')
for sign in [-1,1]:
    ex=sign*(.151 if FATHER else .123);ez=1.897 if FATHER else 1.728
    ear=rings('Shaped ear',[(ex,0,ez-.022,.009,.015),(ex,-.001,ez-.012,.013,.021),(ex,0,ez+.015,.013,.021),(ex,.002,ez+.024,.007,.014)],skin,6,bone='Head')
    if not FATHER:
        jewel('Earring stud',(ex,-.022,ez-.022),.006,.004,.006,silver,'Head')
        jewel('Garnet drop earring',(ex,-.022,ez-.047),.011,.005,.019,ruby,'Head')
if not FATHER:
    # Rounded bob shell, open around the face, plus a swept fringe made of curved sheets.
    vv=[];ff=[];uu=[];n=24
    hlevels=[(1.593,.137,.113),(1.63,.143,.129),(1.73,.144,.137),(1.82,.142,.129),(1.872,.118,.108),(1.902,.065,.061),(1.913,.012,.017)]
    for j,(z,rx,ry) in enumerate(hlevels):
        for k in range(n+1):a=2*pi*k/n;vv.append((-.007+rx*sin(a),.012-ry*cos(a),z));uu.append((k/n,j/(len(hlevels)-1)))
    for j in range(len(hlevels)-1):
        for k in range(n):
            if j<3 and cos((k+.5)*2*pi/n)>.35:continue
            a=j*(n+1)+k;ff.append((a,a+1,a+n+2,a+n+1))
    v3_mesh(collection,parts,'Tailored black bob',vv,ff,atlas,'hair',uu,bone='Head')
    v3_ribbon(collection,parts,'Swept bob fringe',[(-.045,-.09,1.892,.070),(.014,-.126,1.866,.080),(.073,-.139,1.819,.070),(.111,-.139,1.755,.050),(.129,-.113,1.657,.038),(.129,-.102,1.595,0)],atlas)
    v3_ribbon(collection,parts,'Tucked left bob',[(-.10,-.10,1.83,.035),(-.127,-.115,1.76,.035),(-.13,-.103,1.66,.035),(-.122,-.096,1.59,.024)],atlas)

    # Keep the bob silhouette, but curve the front strands along the actual scalp.
    # Projection preserves X/Z, so the fringe does not collapse into narrow strips.
    scalp=BVHTree.FromPolygons([v.co for v in head.data.vertices],[tuple(p.vertices) for p in head.data.polygons],all_triangles=False)
    for ob in parts:
        if not ob.name.startswith(('Swept bob fringe','Tucked left bob')):continue
        for vertex in ob.data.vertices:
            co=vertex.co
            if co.z<1.69:continue
            hit,normal,index,distance=scalp.ray_cast(Vector((co.x,-1,co.z)),Vector((0,1,0)))
            if hit is None:hit,normal,index,distance=scalp.find_nearest(co)
            if hit is not None:
                influence=max(0,min(1,(co.z-1.69)/.035))
                layer_offset=.002 if vertex.index>=len(ob.data.vertices)//2 else 0
                co.y=co.y*(1-influence)+(hit.y-.008+layer_offset)*influence
        ob.data.update()

# Humanoid skeleton; same naming convention as the existing protagonist.
rig_name=NAME+'_Rig';previous=bpy.data.objects.get(rig_name)
if previous is not None:previous.name=NAME+'_DraftRig'
arm=bpy.data.armatures.new(NAME+' anatomical rig');rig=bpy.data.objects.new(rig_name,arm);collection.objects.link(rig)
for ob in bpy.context.selected_objects:ob.select_set(False)
rig.select_set(True);bpy.context.view_layer.objects.active=rig;bpy.ops.object.mode_set(mode='EDIT')
hip=1.04 if FATHER else .99;knee=.56 if FATHER else .54;legx=.14 if FATHER else .105
chest=1.37 if FATHER else 1.32;neck=1.65 if FATHER else 1.54;headz=1.73 if FATHER else 1.625
spec=[('Hips',(0,0,hip),(0,0,hip+.13),None),('Spine',(0,0,hip+.13),(0,0,chest),'Hips'),('Chest',(0,0,chest),(0,0,chest+.12),'Spine'),('UpperChest',(0,0,chest+.12),(0,0,neck),'Chest'),('Neck',(0,0,neck),(0,0,headz),'UpperChest'),('Head',(0,0,headz),(0,0,2.10 if FATHER else 1.91),'Neck')]
for sign,label in [(1,'Left'),(-1,'Right')]:
    spec.extend([(label+'Shoulder',(sign*(shoulder-.08),0,arm_z),(sign*shoulder,0,arm_z),'UpperChest'),(label+'UpperArm',(sign*shoulder,0,arm_z),(sign*elbow,0,arm_z),label+'Shoulder'),(label+'LowerArm',(sign*elbow,0,arm_z),(sign*wrist,0,arm_z),label+'UpperArm'),(label+'Hand',(sign*wrist,0,arm_z),(sign*(wrist+.17),0,arm_z-.02),label+'LowerArm'),(label+'UpperLeg',(sign*legx,0,hip),(sign*legx,-.01,knee),'Hips'),(label+'LowerLeg',(sign*legx,-.01,knee),(sign*legx,-.005,.12),label+'UpperLeg'),(label+'Foot',(sign*legx,-.005,.12),(sign*legx,-.14,.055),label+'LowerLeg'),(label+'Toes',(sign*legx,-.14,.055),(sign*legx,-.23,.045),label+'Foot')])
for name,headco,tail,parent in spec:
    bone=arm.edit_bones.new('MD_'+name);bone.head=headco;bone.tail=tail
    if parent:bone.parent=arm.edit_bones['MD_'+parent]
    bone.align_roll(Vector((0,-1,0)))
bpy.ops.object.mode_set(mode='OBJECT');rig.select_set(False)
def smooth(x):x=max(0,min(1,x));return x*x*(3-2*x)
def weights(p,ob):
    if ob.get('v3_head'):return v3_weight(p,ob)
    if ob.get('bone'):return {ob['bone']:1}
    x,y,z=p;ax=abs(x);side='Left' if x>=0 else 'Right';role=ob.get('role','torso')
    if role=='legs':
        if ax<.025 or z>hip-.1:return {'Hips':1}
        if z<knee-.1:return {side+'LowerLeg':1}
        if z<knee+.1:
            t=smooth((z-(knee-.1))/.2);return {side+'LowerLeg':1-t,side+'UpperLeg':t}
        t=smooth((z-(hip-.18))/.18);return {side+'UpperLeg':1-t,'Hips':t}
    if role=='coat' and z<hip+.08:
        t=.40*smooth((hip+.08-z)/.38) if FATHER and ax>.05 else 0
        return {'Hips':1-t,side+'UpperLeg':t}
    if role=='coat' and ax>(.386 if FATHER else .252) and z>arm_z-.25:
        if ax<shoulder+.12:
            t=smooth((ax-shoulder*.8)/(.12+shoulder*.2));return {'UpperChest':1-t,side+'UpperArm':t}
        if ax<elbow+.055:
            t=smooth((ax-(elbow-.07))/.125);return {side+'UpperArm':1-t,side+'LowerArm':t}
        return {side+'LowerArm':1}
    if z<hip+.18:
        t=smooth((z-hip)/.18);return {'Hips':1-t,'Spine':t}
    t=smooth((z-(hip+.18))/(chest-hip-.18));return {'Spine':1-t,'Chest':t} if z<chest else {'UpperChest':1}
for ob in parts:
    groups={name:ob.vertex_groups.new(name='MD_'+name) for name,a,b,c in spec}
    for vertex in ob.data.vertices:
        for name,w in weights(vertex.co,ob).items():
            if w>.00001:groups[name].add([vertex.index],w,'REPLACE')
    mod=ob.modifiers.new('Family Humanoid deformation','ARMATURE');mod.object=rig;ob.parent=rig

# Relaxed presentation pose. Cane is separately skinned, so it stays an editable prop.
for name,angle in [('LeftUpperArm',-68),('RightUpperArm',68)]:
    pb=rig.pose.bones['MD_'+name];pb.rotation_mode='XYZ';pb.rotation_euler.z=math.radians(angle)
bpy.context.view_layer.update()
if FATHER:
    pb=rig.pose.bones['MD_RightHand'];palm=pb.matrix@Vector((0,.095,0));x=palm.x;y=palm.y-.005;z=palm.z
    cane=line('PROP_Cane shaft',[(x,y,.025),(x,y,z-.03)],.012,leather,'RightHand')
    handle=line('PROP_Cane gold handle',[(x-.055,y,z-.005),(x-.039,y,z+.007),(x+.025,y,z+.007),(x+.045,y,z-.008)],.018,gold,'RightHand')
    tip=rings('PROP_Cane ferrule',[(x,y,.017,.014,.014),(x,y,.065,.014,.014)],gold,8,bone='RightHand')
    props=[cane,handle,tip]
    transform=arm.bones['MD_RightHand'].matrix_local@pb.matrix.inverted()
    for ob in props:
        for v in ob.data.vertices:v.co=transform@v.co
        vg=ob.vertex_groups.new(name='MD_RightHand');vg.add(list(range(len(ob.data.vertices))),1,'REPLACE')
        mod=ob.modifiers.new('Cane hand attachment','ARMATURE');mod.object=rig;ob.parent=rig
else:
    pb=rig.pose.bones['MD_LeftHand'];palm=pb.matrix@Vector((0,.095,0));x=palm.x;y=palm.y-.015;z=palm.z-.04
    clutch=mesh('PROP_Clutch body',[(x-.08,y-.03,z-.05),(x+.08,y-.03,z-.05),(x+.08,y+.028,z-.05),(x-.08,y+.028,z-.05),(x-.08,y-.03,z+.035),(x+.08,y-.03,z+.035),(x+.08,y+.028,z+.035),(x-.08,y+.028,z+.035)],[(0,1,2,3),(0,4,5,1),(1,5,6,2),(2,6,7,3),(3,7,4,0),(4,7,6,5)],leather,'LeftHand')
    clasp=jewel('PROP_Clutch clasp',(x,y-.034,z+.026),.021,.006,.009,gold,'LeftHand');props=[clutch,clasp]
    transform=arm.bones['MD_LeftHand'].matrix_local@pb.matrix.inverted()
    for ob in props:
        for v in ob.data.vertices:v.co=transform@v.co
        vg=ob.vertex_groups.new(name='MD_LeftHand');vg.add(list(range(len(ob.data.vertices))),1,'REPLACE')
        mod=ob.modifiers.new('Clutch hand attachment','ARMATURE');mod.object=rig;ob.parent=rig


# All clothing, equipment and skin use one original atlas. The face retains its UVs.
for ob in parts:
    if ob.get('v3_head') or ob.get('v3_role')=='hair':continue
    uv=ob.data.uv_layers.active or ob.data.uv_layers.new(name='UVMap')
    oldmats=list(ob.data.materials)
    for poly in ob.data.polygons:
        original=oldmats[poly.material_index];role='trim'
        if original==skin or original==shadow:role='skin'
        if original==cloth:role='cloth'
        if original==clothdark:role='cloth'
        if original==trousers:role='pants'
        if original==shirt:role='ivory'
        if original==purple:role='hair' # Father's unused hair region contains ascot embroidery.
        if original==gold:role='gold'
        if original==silver:role='silver'
        if original==ruby:role='ruby'
        if original==white:role='ivory'
        if original==leather and ob.name.startswith(('Oxford','Heel','Shoe','Tailored shoe')):role='shoe'
        local_uvs=[]
        for li in poly.loop_indices:
            co=ob.data.vertices[ob.data.loops[li].vertex_index].co;x,y,z=co;ax=abs(x)
            if role=='cloth':
                if ax>(.39 if FATHER else .26):u=(math.atan2(z-arm_z,-y)+pi)/(2*pi);v=(ax-shoulder)/(wrist-shoulder)
                else:u=(x+(.39 if FATHER else .25))/(.78 if FATHER else .50);v=(z-(.59 if FATHER else .985))/(1.10 if FATHER else .582)
            elif role=='pants':u=(math.atan2(ax-legx,-y)+pi)/(2*pi);v=(z-.10)/(hip-.10)
            elif role=='hair':u=(x+.045)/.09;v=(z-1.52)/.14
            elif role=='shoe':u=(y+.2)/.28;v=z/.19
            else:u=.5;v=.5
            local_uvs.append((li,u,v))
        seam=role in ('pants','shell') or (role=='cloth' and ax>.26)
        cross=seam and max(t[1] for t in local_uvs)-min(t[1] for t in local_uvs)>.5
        high=sum(t[1] for t in local_uvs)/len(local_uvs)>.5
        for li,u,v in local_uvs:
            if cross:u=(1 if high else u) if u<.5 else (u if high else 0)
            uv.data[li].uv=v3_uv(role,u,v)
        poly.material_index=0;poly.use_smooth=role in ('cloth','pants','skin','shoe')
    ob.data.materials.clear();ob.data.materials.append(atlas)
# Mother retains her age and tailoring; torso/legs now follow the same proportion language.
if not FATHER:
    def mother_warp(co):
        x,y,z=co;ax=abs(x);sx=1 if x>=0 else -1
        xx=(ax*.90 if ax<=.20 else .18+(ax-.20)*1.09) if z<1.57 else ax
        landmarks=[(0,0),(.12,.12),(.54,.575),(.99,1.04),(1.465,1.50),(1.54,1.57),(1.61,1.64),(1.94,1.97)]
        zz=z
        for (a,b),(c,d) in zip(landmarks,landmarks[1:]):
            if a<=z<=c:zz=b+(z-a)/(c-a)*(d-b);break
        return Vector((sx*xx,y,zz))
    for ob in parts:
        if ob.data.shape_keys:
            for key in ob.data.shape_keys.key_blocks:
                for v in key.data:v.co=mother_warp(v.co)
        else:
            for v in ob.data.vertices:v.co=mother_warp(v.co)
    for ob in scene.objects:ob.select_set(False)
    rig.select_set(True);bpy.context.view_layer.objects.active=rig;bpy.ops.object.mode_set(mode='EDIT')
    for bone in rig.data.edit_bones:bone.head=mother_warp(bone.head);bone.tail=mother_warp(bone.tail);bone.align_roll(Vector((0,-1,0)))
    bpy.ops.object.mode_set(mode='OBJECT')
v3_export(scene,collection,parts,rig,NAME,P+NAME+'.fbx',props)
v3_studio(scene,NAME,2.06 if FATHER else 1.92)
bpy.ops.wm.save_as_mainfile(filepath=P+NAME+'.blend',compress=True)
print(json.dumps({'character':NAME,'triangles':sum(sum(len(f.vertices)-2 for f in ob.data.polygons) for ob in parts),'head_vertices':len(head.data.vertices),'bones':len(arm.bones),'props':[o.name for o in props]}))
