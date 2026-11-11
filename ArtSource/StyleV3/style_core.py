"""Shared authored V3 topology, atlas, soft normals and facial deformation helpers."""
import bpy,bmesh,math,json
from mathutils import Vector
from math import sin,cos,pi,exp
V3_RECTS={'face':(0,0,256,256),'cloth':(256,0,512,256),'hair':(0,256,128,512),'pants':(128,256,256,512),'skin':(256,256,384,384),'midriff':(256,256,384,384),'shoe':(384,256,512,384),'trim':(256,384,384,512),'metal':(384,384,512,512),'rider_face':(256,256,384,384),'shell':(0,0,256,128),'visor':(0,128,256,256)}

def v3_uv(role,u=.5,v=.5):
    if role in ('silver','gold','ruby','ivory'):
        u,v={'silver':(.25,.75),'gold':(.75,.75),'ruby':(.25,.25),'ivory':(.75,.25)}[role];role='metal'
    if role=='skin':u=.12;v=.88
    x0,y0,x1,y1=V3_RECTS[role];u=max(.002,min(.998,u));v=max(.002,min(.998,v))
    return ((x0+u*(x1-x0))/512,1-(y1-v*(y1-y0))/512)

def v3_material(name,path):
    m=bpy.data.materials.get(name+' • painted atlas') or bpy.data.materials.new(name+' • painted atlas');m.use_nodes=True;m.diffuse_color=(.7,.7,.7,1)
    bs=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED');bs.inputs['Base Color'].default_value=(1,1,1,1);bs.inputs['Roughness'].default_value=.9;bs.inputs['Metallic'].default_value=0;bs.inputs['Specular IOR Level'].default_value=.12
    tex=next((n for n in m.node_tree.nodes if n.type=='TEX_IMAGE'),None)
    if tex is None:tex=m.node_tree.nodes.new('ShaderNodeTexImage')
    tex.image=bpy.data.images.load(path,check_existing=True);tex.image.reload();tex.image.pack();tex.interpolation='Closest'
    m.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color']);m.node_tree.links.new(tex.outputs['Color'],bs.inputs['Emission Color']);bs.inputs['Emission Strength'].default_value=.12
    return m

def v3_mesh(collection,parts,name,vs,fs,material,role='skin',uvs=None,smooth=True,bone=None,weld=True):
    me=bpy.data.meshes.new(name);me.from_pydata(vs,[],fs);me.update();me.materials.append(material)
    uv=me.uv_layers.new(name='UVMap')
    for poly in me.polygons:
        for li in poly.loop_indices:
            index=me.loops[li].vertex_index
            uv.data[li].uv=v3_uv(role,*uvs[index]) if uvs else v3_uv(role)
    bm=bmesh.new();bm.from_mesh(me)
    if weld:bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=.00001)
    bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(me);bm.free()
    for poly in me.polygons:poly.use_smooth=smooth
    ob=bpy.data.objects.new(name,me);collection.objects.link(ob);parts.append(ob);ob['v3_role']=role
    if bone:ob['bone']=bone
    return ob

def v3_rings(collection,parts,name,levels,material,role,n=12,caps=True,bone=None):
    vs=[];fs=[];uvs=[]
    for j,(x,y,z,rx,ry) in enumerate(levels):
        for k in range(n+1):
            a=2*pi*k/n;vs.append((x+rx*sin(a),y-ry*cos(a),z));uvs.append((k/n,j/(len(levels)-1)))
    for j in range(len(levels)-1):
        for k in range(n):a=j*(n+1)+k;fs.append((a,a+1,a+n+2,a+n+1))
    if caps:fs.extend([tuple(reversed(range(n))),tuple((len(levels)-1)*(n+1)+k for k in range(n))])
    return v3_mesh(collection,parts,name,vs,fs,material,role,uvs,bone=bone)

def v3_ribbon(collection,parts,name,rows,material,axis=(1,0,0),normal=(0,-1,0),role='hair',bone='Head'):
    u=Vector(axis).normalized();normal=Vector(normal).normalized();vs=[];uvs=[];fs=[]
    for j,(x,y,z,width) in enumerate(rows):
        c=Vector((x,y,z));width=max(.0005,width)
        for t,h in [(-.5,0),(-.28,.0018),(.28,.0018),(.5,0)]:
            vs.append(c+u*(width*t)+normal*h);uvs.append((.075+(t+.5)*.16,1-j/(len(rows)-1)) if role=='hair' else (t+.5,1-j/(len(rows)-1)))
    for j in range(len(rows)-1):
        for k in range(3):a=j*4+k;fs.append((a,a+1,a+5,a+4))
    count=len(vs);vs.extend([v-normal*.002 for v in list(vs)]);uvs.extend(list(uvs))
    fs.extend([tuple(i+count for i in reversed(f)) for f in list(fs)])
    edge=[0,1,2,3]+[j*4+3 for j in range(1,len(rows))]+[(len(rows)-1)*4+k for k in (2,1,0)]+[j*4 for j in range(len(rows)-2,0,-1)]
    for k,a in enumerate(edge):b=edge[(k+1)%len(edge)];fs.append((a,b,b+count,a+count))
    return v3_mesh(collection,parts,name,vs,fs,material,role,uvs,True,bone)

def v3_head(collection,parts,name,material,chin=1.59,height=.25,width=.12,neck_base=1.53,neck_radius=.047,age='girl'):
    # Closed ring topology with integrated jaw, cheeks, nose and neck; no fan-shaped face card.
    profile=[(-.24,neck_radius,.048,.046),(-.10,neck_radius*.94,.045,.047),(0,width*.33,width*.49,width*.43),(.08,width*.57,width*.61,width*.64),(.22,width*.83,width*.74,width*.85),(.35,width*.97,width*.85,width*.98),(.45,width,width*.87,width*1.02),(.56,width,width*.86,width*1.04),(.68,width*.98,width*.84,width*1.02),(.81,width*.93,width*.78,width*.95),(.94,width*.64,width*.56,width*.66),(1,width*.14,width*.14,width*.22)]
    if age=='father':
        profile[2]=(0,width*.56,width*.58,width*.55)
        profile[3]=(.08,width*.76,width*.72,width*.76)
        profile[4]=(.22,width*.94,width*.80,width*.94)
    # Extra eyelid rows keep the blink deformation local to the painted eye.
    dense=[]
    for a,b in zip(profile,profile[1:]):
        dense.append(a)
        if .22<=a[0]<=.68:
            for f in (.333333,.666667):dense.append(tuple(x+(y-x)*f for x,y in zip(a,b)))
    dense.append(profile[-1]);profile=dense
    vs=[];fs=[];n=32
    for t,rx,front,back in profile:
        z=chin+height*t
        if age=='mother' and t>.68:
            q=(t-.68)/.32;z+=.040*q*q*(3-2*q)
        if t<0:z=neck_base+(t+.24)/.24*(chin-neck_base)
        for k in range(n):
            a=2*pi*k/n;c=cos(a);x=rx*sin(a)
            y=-front*(max(0,c)**.50) if c>=0 else back*(-c)**.85
            if c>0 and t>=0:
                nose=.014*exp(-((a if a<pi else a-2*pi)/.23)**2)*exp(-((t-.35)/.18)**2)
                socket=.0045*(exp(-((x-width*.46)/(width*.24))**2)+exp(-((x+width*.46)/(width*.24))**2))*exp(-((t-.53)/.14)**2)
                y-=nose;y+=socket
            vs.append((x,y,z))
    for j in range(len(profile)-1):
        for k in range(n):a=j*n+k;b=j*n+(k+1)%n;fs.append((a,b,b+n,a+n))
    fs.extend([tuple(reversed(range(n))),tuple((len(profile)-1)*n+k for k in range(n))])
    # Thin conforming eyelids belong to the same skinned head, with constant skin UVs.
    # Their closed morph covers the painted eye without crushing the cheek topology.
    face_count=len(fs);lid_polys={};closed={1:{},-1:{}}
    def surface(x,z,offset=.008):
        t=(z-chin)/height
        row=profile[-1]
        for aa,bb in zip(profile,profile[1:]):
            if aa[0]<=t<=bb[0]:
                q=(t-aa[0])/(bb[0]-aa[0]);row=tuple(a+(b-a)*q for a,b in zip(aa,bb));break
        rx,front=row[1],row[2];angle=math.asin(max(-.999,min(.999,x/rx)));c=cos(angle)
        nose=.014*exp(-(angle/.23)**2)*exp(-((t-.35)/.18)**2)
        socket=.0045*(exp(-((x-width*.46)/(width*.24))**2)+exp(-((x+width*.46)/(width*.24))**2))*exp(-((t-.53)/.14)**2)
        return (x,-front*c**.5-nose+socket-offset,z)
    eye_h=(13 if age=='father' else 16 if age=='mother' else 22)/256*height
    for side in [-1,1]:
        cx=side*57/256*width*2.08;half=(35 if age=='father' else 38)/256*width*2.08
        mid=chin+height*(1-124/256)
        for upper in [True,False]:
            start=len(vs);samples=9
            for row in [0,1]:
                for k in range(samples):
                    q=-1+2*k/(samples-1);x=cx+half*q;arc=max(0,1-q*q)**.65
                    edge=mid+(eye_h*arc if upper else -eye_h*.94*arc)
                    z=edge+(.0017 if upper else -.0017)*row
                    idx=len(vs);vs.append(surface(x,z))
                    zclosed=z if row==1 else mid+(.0006 if upper else -.0006)
                    closed[side][idx]=surface(x,zclosed,.0085)
            for k in range(samples-1):
                lid_polys[len(fs)]='skin';fs.append((start+k,start+k+1,start+samples+k+1,start+samples+k))
            # An ink line follows the lower edge of the upper lid.
            if upper:
                begin=len(vs)
                for row in [0,1]:
                    for k in range(samples):
                        q=-1+2*k/(samples-1);x=cx+half*q;arc=max(0,1-q*q)**.65;edge=mid+eye_h*arc
                        z=edge+row*.0013;idx=len(vs);vs.append(surface(x,z,.011));closed[side][idx]=surface(x,mid+.0006+row*.0013,.0115)
                for k in range(samples-1):lid_polys[len(fs)]='hair';fs.append((begin+k,begin+k+1,begin+samples+k+1,begin+samples+k))
    ob=v3_mesh(collection,parts,name+' • shaped head',vs,fs,material,'skin',smooth=True,weld=False)
    uv=ob.data.uv_layers.active
    for poly in ob.data.polygons:
        center=sum((ob.data.vertices[i].co for i in poly.vertices),Vector())/len(poly.vertices)
        is_front=center.y<0 and center.z>=chin-.003
        for li in poly.loop_indices:
            co=ob.data.vertices[ob.data.loops[li].vertex_index].co
            if poly.index in lid_polys:uv.data[li].uv=v3_uv(lid_polys[poly.index],.2,.1)
            else:uv.data[li].uv=v3_uv('face',(co.x+width*1.04)/(width*2.08),(co.z-chin)/height) if is_front else v3_uv('skin')
    ob['v3_head']=True;ob['head_chin']=chin;ob['head_height']=height;ob['head_width']=width
    ob.shape_key_add(name='Basis')
    for side,label in [(1,'Blink L'),(-1,'Blink R')]:
        key=ob.shape_key_add(name=label);key.value=0
        for idx,co in closed[side].items():key.data[idx].co=co
    return ob

def v3_skeleton(collection,name,hip=1.04,knee=.575,shoulder=.18,arm_z=1.495,elbow=.415,wrist=.67,leg_x=.105,neck=1.54,head=1.62,head_top=1.84):
    old=bpy.data.objects.get(name+'_Rig')
    if old:old.name=name+'_DraftRig'
    data=bpy.data.armatures.new(name+' Humanoid');rig=bpy.data.objects.new(name+'_Rig',data);collection.objects.link(rig)
    for ob in bpy.context.selected_objects:ob.select_set(False)
    rig.select_set(True);bpy.context.view_layer.objects.active=rig;bpy.ops.object.mode_set(mode='EDIT')
    spec=[('Hips',(0,0,hip),(0,0,hip+.12),None),('Spine',(0,0,hip+.12),(0,0,1.34),'Hips'),('Chest',(0,0,1.34),(0,0,1.45),'Spine'),('UpperChest',(0,0,1.45),(0,0,neck),'Chest'),('Neck',(0,0,neck),(0,0,head),'UpperChest'),('Head',(0,0,head),(0,0,head_top),'Neck')]
    for sign,label in [(1,'Left'),(-1,'Right')]:
        spec.extend([(label+'Shoulder',(sign*(shoulder-.07),0,arm_z),(sign*shoulder,0,arm_z),'UpperChest'),(label+'UpperArm',(sign*shoulder,0,arm_z),(sign*elbow,0,arm_z),label+'Shoulder'),(label+'LowerArm',(sign*elbow,0,arm_z),(sign*wrist,0,arm_z),label+'UpperArm'),(label+'Hand',(sign*wrist,0,arm_z),(sign*(wrist+.16),0,arm_z-.02),label+'LowerArm'),(label+'UpperLeg',(sign*leg_x,0,hip),(sign*leg_x,-.008,knee),'Hips'),(label+'LowerLeg',(sign*leg_x,-.008,knee),(sign*leg_x,-.003,.10),label+'UpperLeg'),(label+'Foot',(sign*leg_x,-.003,.10),(sign*leg_x,-.15,.055),label+'LowerLeg'),(label+'Toes',(sign*leg_x,-.15,.055),(sign*leg_x,-.23,.05),label+'Foot')])
    for bone_name,start,end,parent in spec:
        b=data.edit_bones.new('MD_'+bone_name);b.head=start;b.tail=end
        if parent:b.parent=data.edit_bones['MD_'+parent]
        b.align_roll(Vector((0,-1,0)))
    bpy.ops.object.mode_set(mode='OBJECT');rig.select_set(False)
    return rig,spec

def v3_smooth(x):x=max(0,min(1,x));return x*x*(3-2*x)
def v3_weight(p,ob):
    if ob.get('bone'):return {ob['bone']:1}
    x,y,z=p;ax=abs(x);side='Left' if x>=0 else 'Right'
    if ob.get('v3_head'):
        t=v3_smooth((z-ob['head_chin']+.02)/.055);return {'Neck':1-t,'Head':t}
    kind=ob.get('kind','torso')
    if kind=='legs':
        if ax<.015 or z>1.015:return {'Hips':1}
        if z<.47:return {side+'LowerLeg':1}
        if z<.67:
            t=v3_smooth((z-.47)/.2);return {side+'LowerLeg':1-t,side+'UpperLeg':t}
        t=v3_smooth((z-.90)/.14);return {side+'UpperLeg':1-t,'Hips':t}
    if kind=='shoulders':
        t=v3_smooth((ax-.115)/.12);return {'UpperChest':1-t,side+'UpperArm':t}
    if kind=='arms' or (kind=='top' and ax>.205):
        if ax<.285:
            t=v3_smooth((ax-.17)/.115);return {'UpperChest':1-t,side+'UpperArm':t}
        if ax<.47:
            t=v3_smooth((ax-.36)/.11);return {side+'UpperArm':1-t,side+'LowerArm':t}
        if ax<.685:
            t=v3_smooth((ax-.635)/.05);return {side+'LowerArm':1-t,side+'Hand':t}
        return {side+'Hand':1}
    if z<1.19:
        t=v3_smooth((z-1.04)/.15);return {'Hips':1-t,'Spine':t}
    if z<1.4:
        t=v3_smooth((z-1.19)/.21);return {'Spine':1-t,'Chest':t}
    if z<1.52:return {'UpperChest':1}
    return {'Neck':1}

def v3_skin(parts,rig,spec):
    for ob in parts:
        groups={name:ob.vertex_groups.new(name='MD_'+name) for name,a,b,c in spec}
        for v in ob.data.vertices:
            for name,w in v3_weight(v.co,ob).items():
                if w>.00001:groups[name].add([v.index],w,'REPLACE')
        mod=ob.modifiers.new('V3 skin deformation','ARMATURE');mod.object=rig;mod.use_deform_preserve_volume=True;ob.parent=rig

def v3_export(scene,collection,parts,rig,name,path,props=None,bake=False):
    props=props or [];export=[];rig.data.pose_position='POSE' if bake else 'REST'
    for ob in scene.objects:ob.select_set(False)
    for group,label in [([o for o in parts if not o.get('v3_head') and o not in props],'Body'),([o for o in parts if o.get('v3_head')],'Head'),(props,'Prop')]:
        if not group:continue
        copies=[]
        for original in group:
            ob=original.copy();ob.data=original.data.copy();collection.objects.link(ob);ob.select_set(True);copies.append(ob)
        bpy.context.view_layer.objects.active=copies[0]
        if len(copies)>1:bpy.ops.object.join()
        joined=bpy.context.object;joined.name=name+'_'+label;joined.select_set(False);export.append(joined)
    for ob in export:ob.select_set(True)
    rig.select_set(True)
    bpy.ops.export_scene.fbx(filepath=path,use_selection=True,object_types={'MESH','ARMATURE'},add_leaf_bones=False,bake_anim=bake,bake_anim_use_all_actions=False,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,axis_forward='-Z',axis_up='Y',apply_unit_scale=True,use_mesh_modifiers=True)
    for ob in export:bpy.data.objects.remove(ob,do_unlink=True)
    rig.data.pose_position='POSE'

def v3_studio(scene,name,height=1.95):
    studio=bpy.data.collections.new(name+' Studio');scene.collection.children.link(studio)
    def add(label,data):ob=bpy.data.objects.new(label,data);studio.objects.link(ob);return ob
    m=bpy.data.materials.get('V3 neutral stage') or bpy.data.materials.new('V3 neutral stage');m.use_nodes=True
    bs=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED');bs.inputs['Base Color'].default_value=(.20,.21,.23,1);bs.inputs['Roughness'].default_value=1
    me=bpy.data.meshes.new('V3 ground');me.from_pydata([(-20,-20,0),(20,-20,0),(20,20,0),(-20,20,0)],[],[(0,1,2,3)]);me.materials.append(m);add('V3 Ground',me)
    world=bpy.data.worlds.new(name+' World');world.use_nodes=True;bg=next(n for n in world.node_tree.nodes if n.type=='BACKGROUND');bg.inputs[0].default_value=(.22,.24,.29,1);bg.inputs[1].default_value=.6;scene.world=world
    for label,pos,energy,size in [('V3 Key',(-3,-4,4),420,4),('V3 Fill',(3,-3,2.7),260,4),('V3 Rim',(1,3,4),500,3)]:
        d=bpy.data.lights.new(label,'AREA');d.energy=energy;d.size=size;ob=add(label,d);ob.location=pos;ob.rotation_euler=(Vector((0,0,height*.55))-ob.location).to_track_quat('-Z','Y').to_euler()
    data=bpy.data.cameras.new(name+' Camera');camera=add(name+' Camera',data);camera.location=(.5,-6,height*1.1);camera.rotation_euler=(Vector((0,0,height*.51))-camera.location).to_track_quat('-Z','Y').to_euler();data.type='ORTHO';data.ortho_scale=height*1.16;scene.camera=camera
    try:scene.render.engine='BLENDER_EEVEE'
    except TypeError:pass
    scene.render.resolution_x=900;scene.render.resolution_y=1100;scene.render.resolution_percentage=100;scene.render.image_settings.file_format='PNG';scene.view_settings.view_transform='Standard';scene.view_settings.exposure=-.4
    for screen in bpy.data.screens:
        for area in screen.areas:
            if area.type=='VIEW_3D':area.spaces.active.region_3d.view_distance=3.2;area.spaces.active.region_3d.view_location=(0,0,height*.52);area.spaces.active.shading.type='MATERIAL'
    return camera
