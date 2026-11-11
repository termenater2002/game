# Run after style_core.py (the files are concatenated for the live Blender MCP).
P=V3_OUTPUT
NAME='LilacV3'
scene=bpy.data.scenes.new('V3 • Lilac • Koharu proportion study');bpy.context.window.scene=scene
collection=bpy.data.collections.new('LILAC_V3');scene.collection.children.link(collection);parts=[]
mat=v3_material(NAME,P+NAME+'_atlas.png')

# Exposed waist and collarbones; hidden skin faces under the knit are omitted.
for label,levels in [('Bare waist',[(0,0,1.055,.164,.098),(0,0,1.10,.122,.078),(0,0,1.155,.127,.081)])]:
    ob=v3_rings(collection,parts,label,levels,mat,'midriff' if label=='Bare waist' else 'skin',16,caps=False)

# Continuous upper chest, deltoid sockets and upper arms beneath the dropped knit.
shoulder_levels=[(1.425,.170,.079),(1.465,.183,.084),(1.495,.176,.080),(1.520,.145,.067),(1.545,.054,.049)]
nskin=24;vv=[];ff=[]
for z,rx,ry in shoulder_levels:
    for k in range(nskin):a=2*pi*k/nskin;vv.append((rx*sin(a),-ry*cos(a),z))
for j in range(4):
    for k in range(nskin):
        if 1<=j<3 and (4<=k<8 or 16<=k<20):continue
        a=j*nskin+k;ff.append((a,j*nskin+(k+1)%nskin,(j+1)*nskin+(k+1)%nskin,a+nskin))
for sign,lo,hi in [(1,4,8),(-1,16,20)]:
    boundary=[nskin+k for k in range(lo,hi+1)]+[j*nskin+hi for j in (2,3)]+[3*nskin+k for k in range(hi-1,lo-1,-1)]+[2*nskin+lo]
    order=[(math.atan2(vv[i][2]-1.493,-vv[i][1]),i) for i in boundary];order.sort();prev=[i for a,i in order];angles=[a for a,i in order]
    for x,ry,rz in [(.210,.053,.055),(.242,.050,.052),(.275,.046,.048),(.305,.044,.046)]:
        ring=[]
        for a in angles:ring.append(len(vv));vv.append((sign*x,-ry*cos(a),1.495+rz*sin(a)))
        for k in range(len(ring)):q=(k+1)%len(ring);ff.append((prev[k],prev[q],ring[q],ring[k]))
        prev=ring
shoulders=v3_mesh(collection,parts,'Connected shoulders and upper arms',vv,ff,mat,'skin');shoulders['kind']='shoulders'

# Project the navel onto the front midriff; the back uses the clean skin swatch.
waist=next(o for o in parts if o.name.startswith('Bare waist'))
for poly in waist.data.polygons:
    c=sum((waist.data.vertices[i].co for i in poly.vertices),Vector())/len(poly.vertices)
    for li in poly.loop_indices:
        co=waist.data.vertices[waist.data.loops[li].vertex_index].co
        waist.data.uv_layers.active.data[li].uv=v3_uv('midriff',(co.x+.164)/.328,(co.z-1.055)/.10) if c.y<0 else v3_uv('skin')

# One connected cropped knit torso and both bell sleeves.
levels=[(1.145,.132,.091),(1.18,.137,.094),(1.25,.146,.097),(1.32,.164,.101),(1.38,.178,.098),(1.425,.179,.085),(1.445,.174,.081)]
n=24;vs=[];fs=[];uvs=[]
for j,(z,rx,ry) in enumerate(levels):
    for k in range(n+1):
        a=2*pi*k/n;vs.append((rx*sin(a),-ry*cos(a),z));uvs.append((k/n,j/(len(levels)-1)))
for j in range(len(levels)-1):
    for k in range(n):
        if 3<=j<5 and (4<=k<8 or 16<=k<20):continue
        a=j*(n+1)+k;fs.append((a,a+1,a+n+2,a+n+1))
for sign,lo,hi in [(1,4,8),(-1,16,20)]:
    boundary=[3*(n+1)+k for k in range(lo,hi+1)]+[j*(n+1)+hi for j in range(4,6)]+[5*(n+1)+k for k in range(hi-1,lo-1,-1)]+[4*(n+1)+lo]
    ordered=[(math.atan2(vs[i][2]-1.3725,-vs[i][1]),i) for i in boundary];ordered.sort();prev=[i for a,i in ordered];angles=[a for a,i in ordered]
    sleeve=[(.24,1.465,.061,.066),(.29,1.485,.058,.061),(.36,1.49,.056,.059),(.405,1.485,.060,.065),(.44,1.49,.066,.071),(.51,1.495,.081,.082),(.59,1.495,.096,.097),(.65,1.495,.102,.103)]
    for j,(x,z,ry,rz) in enumerate(sleeve):
        ring=[]
        for k,a in enumerate(angles):
            ring.append(len(vs));vs.append((sign*x,-ry*cos(a),z+rz*sin(a)));uvs.append(((a+pi)/(2*pi),1-j/(len(sleeve)-1)))
        for k in range(len(ring)):q=(k+1)%len(ring);fs.append((prev[k],prev[q],ring[q],ring[k]))
        prev=ring
knit=v3_mesh(collection,parts,'Connected floral bell-sleeve knit',vs,fs,mat,'cloth',uvs);knit['kind']='top'
for label,levels2 in [('Knit hem',[(0,0,1.138,.134,.093),(0,0,1.156,.137,.095)]),('Off shoulder rib edge',[(0,0,1.430,.181,.087),(0,0,1.447,.177,.084)])]:v3_rings(collection,parts,label,levels2,mat,'trim',24,caps=False)
for sign in [-1,1]:
    vv=[];ff=[];uu=[];n2=12
    for j,(x,r) in enumerate([(.628,.104),(.653,.105)]):
        for k in range(n2+1):a=2*pi*k/n2;vv.append((sign*x,-r*cos(a),1.495+r*sin(a)));uu.append((k/n2,j))
    for k in range(n2):ff.append((k,k+1,k+n2+2,k+n2+1))
    cuff=v3_mesh(collection,parts,'Flared rib cuff',vv,ff,mat,'trim',uu);cuff['kind']='arms'

# Welded pelvis / crotch, long thighs and a deliberate knee-to-hem flare.
vs=[];fs=[];uvs=[];n=12
for sign in [-1,1]:
    prev=[]
    for z,rx,ry in [(1.083,.153,.096),(1.04,.170,.105),(.98,.177,.113)]:
        ring=[]
        for k in range(n+1):a=pi*k/n;ring.append(len(vs));vs.append((sign*rx*sin(a),-ry*cos(a),z));uvs.append((k/n,(z-.09)/1.0))
        if prev:
            for k in range(n):fs.append((prev[k],prev[k+1],ring[k+1],ring[k]))
        prev=ring
    for j,(z,rx,ry,center) in enumerate([(.91,.100,.103,.105),(.83,.099,.099,.108),(.73,.090,.088,.11),(.63,.077,.075,.112),(.575,.071,.070,.115),(.52,.078,.074,.117),(.41,.089,.084,.122),(.29,.103,.095,.129),(.17,.115,.105,.137),(.095,.116,.107,.139)]):
        ring=[]
        for k in range(n+1):
            a=-pi/2+2*pi*k/n;x=sign*(center+rx*sin(a));y=-ry*cos(a)
            if j==0 and k in (0,n):x=0;y=0
            ring.append(len(vs));vs.append((x,y,z));uvs.append((k/n,(z-.09)/1.0))
        for k in range(n):fs.append((prev[k],prev[k+1],ring[k+1],ring[k]))
        prev=ring
pants=v3_mesh(collection,parts,'Continuous flared denim',vs,fs,mat,'pants',uvs);pants['kind']='legs'
v3_rings(collection,parts,'Waistband',[(0,0,1.065,.157,.1),(0,0,1.085,.155,.098)],mat,'trim',16,caps=False,bone='Hips')
button=[(.009*cos(k*2*pi/8),-.108,1.067+.009*sin(k*2*pi/8)) for k in range(8)]
v3_mesh(collection,parts,'Denim front button',button,[tuple(range(8))],mat,'silver',bone='Hips')
# Shoes have a coherent toe box, side walls, sole and painted lace detail.
for sign in [-1,1]:
    x=sign*.139;bone='LeftFoot' if sign>0 else 'RightFoot'
    v3_rings(collection,parts,'Platform shoe',[(x,-.04,.018,.076,.142),(x,-.04,.04,.080,.148),(x,-.04,.062,.080,.147),(x,-.035,.085,.076,.14),(x,-.016,.124,.064,.108),(x,.013,.16,.052,.066)],mat,'shoe',12,bone=bone)
    # Thin cream welt is a manufactured hard edge.
    v3_rings(collection,parts,'Ivory welt',[(x,-.04,.042,.082,.15),(x,-.04,.059,.082,.15)],mat,'ivory',12,caps=False,bone=bone)
# Choker masks the neckline seam while keeping the head anatomically shaped.
v3_rings(collection,parts,'Slim choker',[(0,0,1.55,.049,.047),(0,0,1.56,.049,.047)],mat,'trim',16,caps=False,bone='Neck')

# Hands: palm and five fingers are sized in proportion to the longer reference arms.
for sign in [-1,1]:
    bone='LeftHand' if sign>0 else 'RightHand';vv=[];ff=[];n=8;wrist=.67
    for distance,ry,rz in [(0,.031,.025),(.028,.044,.029),(.073,.043,.028),(.097,.036,.024)]:
        for k in range(n):a=2*pi*k/n;vv.append((sign*(wrist+distance),ry*cos(a),1.495+rz*sin(a)))
    for j in range(3):
        for k in range(n):a=j*n+k;b=j*n+(k+1)%n;ff.append((a,b,b+n,a+n))
    ff.extend([tuple(reversed(range(n))),tuple(3*n+k for k in range(n))]);v3_mesh(collection,parts,'Shaped palm',vv,ff,mat,'skin',bone=bone)
    for j,length in enumerate([.060,.071,.066,.052]):
        vv=[];ff=[];yy=(j-1.5)*.021
        for q in range(4):
            t=q/3;xx=wrist+.088+length*t;zz=1.495-.014*t*t;r=.010*(1-.22*t)
            for k in range(6):a=2*pi*k/6;vv.append((sign*xx,yy+r*cos(a),zz+r*sin(a)))
        for q in range(3):
            for k in range(6):a=q*6+k;b=q*6+(k+1)%6;ff.append((a,b,b+6,a+6))
        ff.append(tuple(18+k for k in range(6)));v3_mesh(collection,parts,'Separate finger',vv,ff,mat,'skin',bone=bone)
    # Tapered, curved thumb surface.
    vv=[];ff=[]
    for j,(xx,yy,zz,r) in enumerate([(wrist+.025,-.033,1.49,.014),(wrist+.056,-.055,1.485,.013),(wrist+.09,-.065,1.47,.010)]):
        for k in range(6):a=2*pi*k/6;vv.append((sign*xx,yy+r*cos(a),zz+r*sin(a)))
    for j in range(2):
        for k in range(6):a=j*6+k;b=j*6+(k+1)%6;ff.append((a,b,b+6,a+6))
    ff.append(tuple(12+k for k in range(6)));v3_mesh(collection,parts,'Curved thumb',vv,ff,mat,'skin',bone=bone)

head=v3_head(collection,parts,NAME,mat)
# Compact layered shag: a fitted smooth crown and thin curved sheets, never pyramidal locks.
v3_rings(collection,parts,'Fitted shag crown',[(0,.012,1.795,.134,.115),(0,.015,1.85,.143,.128),(0,.015,1.91,.103,.101),(0,.014,1.946,.010,.013)],mat,'hair',16,bone='Head')
# Nape shell stays behind the face.
vv=[];ff=[];uu=[];n=16
for j,(z,rx,ry) in enumerate([(1.555,.111,.103),(1.64,.143,.125),(1.75,.148,.13),(1.86,.131,.12)]):
    for k in range(n+1):a=pi*(.34+1.32*k/n);vv.append((rx*sin(a),.015-ry*cos(a),z));uu.append((k/n,j/3))
for j in range(3):
    for k in range(n):a=j*(n+1)+k;ff.append((a,a+1,a+n+2,a+n+1))
v3_mesh(collection,parts,'Painted nape shell',vv,ff,mat,'hair',uu,bone='Head')
for k in range(11):
    a=pi*(.35+1.30*k/10);sx=sin(a);cy=-cos(a);tip=1.53+.02*(k%3)
    rows=[(sx*.128,.015+cy*.116,1.86,.048),(sx*.153,.015+cy*.139,1.75,.054),(sx*.143,.015+cy*.13,1.62,.043),(sx*.166,.015+cy*.124,tip,0)]
    v3_ribbon(collection,parts,'Layered shag nape',rows,mat,(cos(a),sin(a),0),(sx,cy,0))
for sign in [-1,1]:
    def rows(points):return [(sign*x,y,z,w) for x,y,z,w in points]
    v3_ribbon(collection,parts,'Curved face framing lock',rows([(.115,-.11,1.72,.033),(.115,-.13,1.61,.035),(.099,-.14,1.50,.028),(.108,-.143,1.40,.025),(.131,-.14,1.30,0)]),mat)
    v3_ribbon(collection,parts,'Temple sweep',rows([(.104,-.08,1.885,.045),(.134,-.112,1.80,.048),(.12,-.13,1.70,.025),(.094,-.14,1.62,0)]),mat)
    v3_ribbon(collection,parts,'Short cheek wisp',rows([(.135,-.04,1.73,.029),(.147,-.055,1.64,.025),(.175,-.04,1.60,0)]),mat)
    v3_ribbon(collection,parts,'Outer shag flick',rows([(.137,0,1.82,.034),(.151,-.009,1.74,.031),(.181,0,1.71,0)]),mat)
    v=[(sign*.101,.026,1.90),(sign*.18,.031,1.943),(sign*.157,.02,1.864),(sign*.135,.054,1.906)]
    v3_mesh(collection,parts,'Small silhouette ear',v,[(0,1,2),(0,3,1),(1,3,2),(2,3,0)],mat,'hair',bone='Head')
v3_ribbon(collection,parts,'Soft central fringe',[(-.033,-.093,1.925,.085),(-.016,-.121,1.882,.098),(-.004,-.143,1.827,.085),(-.016,-.151,1.77,.048),(-.031,-.15,1.728,0)],mat)
v3_ribbon(collection,parts,'Asymmetric left fringe',[(-.080,-.083,1.903,.065),(-.098,-.12,1.855,.067),(-.093,-.145,1.792,.041),(-.073,-.149,1.751,0)],mat)
v3_ribbon(collection,parts,'Asymmetric right fringe',[(.045,-.09,1.919,.075),(.073,-.12,1.87,.069),(.077,-.144,1.818,.050),(.048,-.153,1.771,0)],mat)

# Shape the knit surface itself around the bust; no separate primitive volumes.
for ob in parts:
    if ob.name.startswith(('Connected floral','Off shoulder rib')):
        for v in ob.data.vertices:
            x,y,z=v.co
            if y<0 and abs(x)<.182:
                fullness=.038*exp(-((abs(x)-.069)/.052)**2-((z-1.355)/.074)**2)
                v.co.y-=fullness*max(0,min(1,-y/.07))

rig,spec=v3_skeleton(collection,NAME);v3_skin(parts,rig,spec)
scene.frame_start=1;scene.frame_end=72;scene.render.fps=24
v3_export(scene,collection,parts,rig,NAME,P+NAME+'.fbx')
# Three-second salute loop, plus a gently moving root, retained as editable Blender animation.
for bone in rig.pose.bones:bone.rotation_mode='XYZ'
for frame,amount in [(1,0),(15,0),(28,1),(44,1),(59,0),(72,0)]:
    for b,base,extra in [('LeftUpperArm',-65,0),('RightUpperArm',65,-85),('RightLowerArm',0,-125),('RightHand',0,-15)]:
        pb=rig.pose.bones['MD_'+b];pb.rotation_euler.z=math.radians(base+extra*amount);pb.keyframe_insert(data_path='rotation_euler',frame=frame)
for frame,value in [(1,0),(20,.012),(38,0),(56,.012),(72,0)]:rig.location.z=value;rig.keyframe_insert(data_path='location',frame=frame)
if rig.animation_data and rig.animation_data.action:rig.animation_data.action.name=NAME+' • 3s salute'
for label in ['Blink L','Blink R']:
    key=head.data.shape_keys.key_blocks[label]
    for frame,value in [(1,0),(7,0),(9,1),(12,0),(40,0),(42,1),(45,0),(72,0)]:key.value=value;key.keyframe_insert(data_path='value',frame=frame)
# Clamp the facial keys between 0 and 1 without Bezier overshoot between blinks.
for layer in head.data.shape_keys.animation_data.action.layers:
    for strip in layer.strips:
        for bag in strip.channelbags:
            for curve in bag.fcurves:
                for point in curve.keyframe_points:point.handle_left_type='AUTO_CLAMPED';point.handle_right_type='AUTO_CLAMPED'
scene.frame_set(1);v3_studio(scene,NAME);scene.render.filepath=P+NAME+'_Front.png'
bpy.ops.wm.save_as_mainfile(filepath=P+NAME+'.blend',compress=True)
print(json.dumps({'name':NAME,'triangles':sum(sum(len(p.vertices)-2 for p in ob.data.polygons) for ob in parts),'vertices':sum(len(ob.data.vertices) for ob in parts),'bones':len(rig.data.bones),'head_vertices':len(head.data.vertices),'head_shapes':[k.name for k in head.data.shape_keys.key_blocks],'file':P+NAME+'.blend'}))
