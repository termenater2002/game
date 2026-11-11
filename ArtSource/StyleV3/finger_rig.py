"""Add thirty weighted finger bones to the authored low-poly character hands."""
import bpy,bmesh,math
from collections import deque
from mathutils import Vector
FINGERS=('Thumb','Index','Middle','Ring','Little')
JOINTS=('Proximal','Intermediate','Distal')

def finger_parts(parts):
    result={side:{} for side in ('Left','Right')}
    for side,sign in [('Left',1),('Right',-1)]:
        fingers=[];thumbs=[]
        for ob in parts:
            if ob.type!='MESH' or sum(v.co.x for v in ob.data.vertices)*sign<=0:continue
            name=ob.name.lower()
            if name.startswith(('separate finger','shaped glove finger','curved finger')):fingers.append(ob)
            elif name.startswith(('curved thumb','curved glove thumb','shaped thumb')):thumbs.append(ob)
        assert len(fingers)==4 and len(thumbs)==1,(side,'Expected four fingers and a thumb',[o.name for o in fingers],[o.name for o in thumbs])
        fingers.sort(key=lambda ob:sum(v.co.y for v in ob.data.vertices)/len(ob.data.vertices))
        result[side]['Thumb']=thumbs[0]
        for label,ob in zip(FINGERS[1:],fingers):result[side][label]=ob
    return result

def mesh_rings(ob):
    me=ob.data;adj={v.index:set() for v in me.vertices};use={}
    for edge in me.edges:
        a,b=edge.vertices;adj[a].add(b);adj[b].add(a)
    for poly in me.polygons:
        verts=list(poly.vertices)
        for i,a in enumerate(verts):
            e=tuple(sorted((a,verts[(i+1)%len(verts)])));use[e]=use.get(e,0)+1
    boundary={}
    for (a,b),count in use.items():
        if count==1:boundary.setdefault(a,set()).add(b);boundary.setdefault(b,set()).add(a)
    assert boundary,(ob.name,'Finger tube needs a boundary at the palm')
    seen=set();loops=[]
    for first in boundary:
        if first in seen:continue
        component=set([first]);todo=[first];seen.add(first)
        while todo:
            a=todo.pop()
            for b in boundary[a]:
                if b not in seen:seen.add(b);component.add(b);todo.append(b)
        loops.append(component)
    start=min(loops,key=lambda loop:abs(sum(me.vertices[i].co.x for i in loop)/len(loop)))
    first=min(start);ordered=[first];previous=None;current=first
    while len(ordered)<len(start):
        following=sorted(i for i in boundary[current] if i!=previous and i not in ordered)
        assert following,(ob.name,'Invalid boundary ring')
        nxt=following[0];ordered.append(nxt);previous,current=current,nxt
    distance={i:0 for i in start};queue=deque(start)
    while queue:
        a=queue.popleft()
        for b in adj[a]:
            if b not in distance:distance[b]=distance[a]+1;queue.append(b)
    rings=[ordered]
    for level in range(1,max(distance.values())+1):
        row=[]
        for a in rings[-1]:
            options=[b for b in adj[a] if distance[b]==level]
            assert len(options)==1,(ob.name,'Non-tubular finger topology',level,len(options))
            row.append(options[0])
        assert len(set(row))==len(row)
        rings.append(row)
    assert len(rings) in (3,4),(ob.name,'Unexpected finger ring count',len(rings))
    return [[me.vertices[i].co.copy() for i in row] for row in rings]

def fit_hand_bones(rig):
    """Keep hand orientation, but end each palm bone at the knuckles rather than fingertips."""
    if bpy.context.object and bpy.context.object.mode!='OBJECT':bpy.ops.object.mode_set(mode='OBJECT')
    for ob in bpy.context.selected_objects:ob.select_set(False)
    rig.select_set(True);bpy.context.view_layer.objects.active=rig;bpy.ops.object.mode_set(mode='EDIT')
    for side in ('Left','Right'):
        hand=rig.data.edit_bones['MD_'+side+'Hand']
        knuckles=[rig.data.edit_bones['MD_'+side+finger+'Proximal'].head.copy() for finger in FINGERS[1:]]
        center=sum(knuckles,Vector())/len(knuckles)
        hand.tail=hand.head+(hand.tail-hand.head).normalized()*(center-hand.head).length
    bpy.ops.object.mode_set(mode='OBJECT')

def add_finger_rig(rig,parts):
    if 'MD_LeftIndexProximal' in rig.data.bones:
        fit_hand_bones(rig)
        return {'alreadyRigged':True,'bones':len(rig.data.bones),'fingerBones':30,'fingerMeshes':10,'addedVertices':0}
    fingers=finger_parts(parts);surfaces={};centres={};added_vertices=0
    for side in fingers:
        for label,ob in fingers[side].items():
            rings=mesh_rings(ob)
            if len(rings)==3:
                rings.insert(2,[(a+b)*.5 for a,b in zip(rings[1],rings[2])]);added_vertices+=len(rings[0])
            surfaces[(side,label)]=rings
            centres[(side,label)]=[sum(row,Vector())/len(row) for row in rings]
    if bpy.context.object and bpy.context.object.mode!='OBJECT':bpy.ops.object.mode_set(mode='OBJECT')
    for ob in bpy.context.selected_objects:ob.select_set(False)
    rig.select_set(True);bpy.context.view_layer.objects.active=rig;bpy.ops.object.mode_set(mode='EDIT')
    for side in fingers:
        for label in FINGERS:
            path=centres[(side,label)];parent='MD_'+side+'Hand'
            for j,joint in enumerate(JOINTS):
                name='MD_'+side+label+joint;bone=rig.data.edit_bones.new(name);bone.head=path[j];bone.tail=path[j+1]
                bone.parent=rig.data.edit_bones[parent];bone.use_connect=j>0;bone.align_roll(Vector((0,-1,0)));parent=name
    bpy.ops.object.mode_set(mode='OBJECT')
    for side in fingers:
        for label,ob in fingers[side].items():
            old=ob.data;uv=old.uv_layers.active.data[0].uv.copy();material=old.materials[0];smooth=old.polygons[0].use_smooth
            rings=surfaces[(side,label)];n=len(rings[0]);vertices=[v for row in rings for v in row];faces=[]
            for j in range(3):
                for k in range(n):a=j*n+k;b=j*n+(k+1)%n;faces.append((a,b,b+n,a+n))
            # Close both ends inside the palm / at the fingertip.
            faces.extend([tuple(reversed(range(n))),tuple(3*n+k for k in range(n))])
            me=bpy.data.meshes.new(ob.name+' • three finger joints');me.from_pydata(vertices,[],faces);me.update();me.materials.append(material)
            layer=me.uv_layers.new(name='UVMap')
            for poly in me.polygons:
                poly.use_smooth=smooth
                for li in poly.loop_indices:layer.data[li].uv=uv
            bm=bmesh.new();bm.from_mesh(me);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(me);bm.free()
            ob.data=me;ob.vertex_groups.clear();names=['MD_'+side+'Hand']+['MD_'+side+label+j for j in JOINTS]
            groups=[ob.vertex_groups.new(name=n) for n in names]
            for row in range(4):
                blends={0:((0,.5),(1,.5)),1:((1,.5),(2,.5)),2:((2,.5),(3,.5)),3:((3,1),)}[row]
                for k in range(n):
                    for group,weight in blends:groups[group].add([row*n+k],weight,'REPLACE')
            ob['finger']=side+label;ob['bone']=side+label+'Proximal'
    # Jewellery follows the ring finger rather than staying behind on the palm.
    for ob in parts:
        if ob.name.startswith(('Signet ring','Ruby ring','Ring gemstone')):
            side='Left' if sum(v.co.x for v in ob.data.vertices)>0 else 'Right'
            ob.vertex_groups.clear();group=ob.vertex_groups.new(name='MD_'+side+'RingProximal');group.add(list(range(len(ob.data.vertices))),1,'REPLACE')
            ob['bone']=side+'RingProximal'
    for bone in rig.pose.bones:
        if any(bone.name=='MD_'+side+finger+joint for side in fingers for finger in FINGERS for joint in JOINTS):bone.rotation_mode='XYZ'
    fit_hand_bones(rig)
    bpy.context.view_layer.update()
    return {'bones':len(rig.data.bones),'fingerBones':30,'fingerMeshes':10,'addedVertices':added_vertices}

def verify_fingers(rig,parts):
    fingers=finger_parts(parts);records=[]
    def point(ob,index):
        bpy.context.view_layer.update();ev=ob.evaluated_get(bpy.context.evaluated_depsgraph_get());return ev.matrix_world@ev.data.vertices[index].co
    for side,sign in [('Left',1),('Right',-1)]:
        for label,ob in fingers[side].items():
            tip=max(ob.data.vertices,key=lambda v:abs(v.co.x)).index
            for joint in JOINTS:
                name='MD_'+side+label+joint;bone=rig.pose.bones[name];basis=bone.matrix_basis.copy();before=point(ob,tip)
                bone.rotation_euler.z+=-sign*math.radians(25);after=point(ob,tip);movement=(after-before).length;bone.matrix_basis=basis
                assert movement>.0005,(rig.name,name,'Finger joint does not deform fingertip',movement)
                records.append({'bone':name,'tipMovementMetres':movement})
            for v in ob.data.vertices:assert abs(sum(g.weight for g in v.groups)-1)<.00001
    bpy.context.view_layer.update()
    return records
