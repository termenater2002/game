"""Prepare independent pose-ready copies and a gallery; never overwrite source models."""
import bpy,json,math,runpy
from pathlib import Path
from mathutils import Matrix,Vector
P=Path(__file__).resolve().parent
OUT=P/'Rigs';OUT.mkdir(exist_ok=True)
NAMES=['LilacV3','RiderV3','RiderTShirtV3','FamilyFatherV3','FamilyMotherV3']
GROUPS={'Torso':(1,.58,.12),'Left arm':(.12,.48,1),'Right arm':(1,.20,.20),'Left leg':(.12,.65,.85),'Right leg':(1,.37,.20),'Head':(.7,.35,1),'Left fingers':(.25,.75,1),'Right fingers':(1,.45,.35)}
reports=[]
def select_bone(rig,name,value):
 pose=rig.pose.bones[name]
 if hasattr(pose,'select'):pose.select=value
 else:pose.bone.select=value
def refresh():
 bpy.context.view_layer.update();return bpy.context.evaluated_depsgraph_get()
def sample(obj,index):
 ev=obj.evaluated_get(refresh());return ev.matrix_world@ev.data.vertices[index].co
for name in NAMES:
 bpy.ops.wm.open_mainfile(filepath=str(P/(name+'.blend')),use_scripts=False)
 scene=bpy.context.scene;scene.frame_set(1)
 rig=next(o for o in scene.objects if o.type=='ARMATURE')
 meshes=[o for o in scene.objects if o.type=='MESH' and o.parent==rig]
 assert len(rig.data.bones)==52,(name,'Unexpected skeleton')
 # Keep actions available in the Action editor, while letting FK posing work directly.
 if rig.animation_data and rig.animation_data.action:
  rig.animation_data.action.use_fake_user=True;rig.animation_data_clear()
 rig.show_in_front=True;rig.hide_select=False;rig.hide_set(False);rig.data.pose_position='POSE'
 styles=[i.identifier for i in rig.data.bl_rna.properties['display_type'].enum_items]
 if 'OCTAHEDRAL' in styles:rig.data.display_type='OCTAHEDRAL'
 collections={n:rig.data.collections.get(n) or rig.data.collections.new(n) for n in GROUPS}
 for bone in rig.data.bones:
  bone.hide=False;bone.hide_select=False
  group='Torso'
  if 'Left' in bone.name:group='Left leg' if any(x in bone.name for x in ['Leg','Foot','Toes']) else 'Left arm'
  elif 'Right' in bone.name:group='Right leg' if any(x in bone.name for x in ['Leg','Foot','Toes']) else 'Right arm'
  elif bone.name.endswith(('Neck','Head')):group='Head'
  if any(k in bone.name for k in ['Thumb','Index','Middle','Ring','Little']):group='Left fingers' if 'Left' in bone.name else 'Right fingers'
  collections[group].assign(bone)
  palettes=[i.identifier for i in bone.color.bl_rna.properties['palette'].enum_items]
  if 'CUSTOM' in palettes:
   bone.color.palette='CUSTOM';bone.color.custom.normal=GROUPS[group];bone.color.custom.select=(1,.8,.35);bone.color.custom.active=(1,1,.55)
  select_bone(rig,bone.name,False)
 rig.data.bones.active=rig.data.bones['MD_Hips'];select_bone(rig,'MD_Hips',True)
 unweighted=0;max_error=0
 for ob in meshes:
  assert any(m.type=='ARMATURE' and m.object==rig for m in ob.modifiers),(name,ob.name,'Missing skin binding')
  for v in ob.data.vertices:
   total=sum(g.weight for g in v.groups if ob.vertex_groups[g.group].name in rig.data.bones)
   if total<.0001:unweighted+=1
   max_error=max(max_error,abs(total-1))
 assert unweighted==0 and max_error<.0001,(name,'Invalid skin weights',unweighted,max_error)
 finger=runpy.run_path(str(P/'finger_rig.py'))
 finger_checks=finger['verify_fingers'](rig,meshes)
 movement={}
 for bone_name in ['MD_LeftLowerArm','MD_LeftFoot','MD_Head']:
  candidates=[]
  for ob in meshes:
   group=ob.vertex_groups.get(bone_name)
   if group:
    for v in ob.data.vertices:
     if any(g.group==group.index and g.weight>.25 for g in v.groups):
      candidates.append(((v.co-rig.data.bones[bone_name].head_local).length,ob,v.index))
  assert candidates,(name,bone_name,'No vertices influenced')
  candidate=max(candidates,key=lambda p:p[0]);ob,index=candidate[1:]
  before=sample(ob,index);pb=rig.pose.bones[bone_name];basis=pb.matrix_basis.copy()
  pb.matrix_basis=basis@Matrix.Rotation(math.radians(18),4,'X')
  after=sample(ob,index);distance=(after-before).length
  pb.matrix_basis=basis;refresh()
  assert distance>.001,(name,bone_name,'Bone does not deform mesh')
  movement[bone_name]=distance
 for obj in scene.objects:obj.select_set(False)
 rig.select_set(True);bpy.context.view_layer.objects.active=rig
 for screen in bpy.data.screens:
  for area in screen.areas:
   if area.type=='VIEW_3D':
    area.spaces.active.overlay.show_overlays=True;area.spaces.active.overlay.show_bones=True
    area.spaces.active.shading.type='MATERIAL';area.spaces.active.region_3d.view_perspective='CAMERA'
 bpy.ops.object.mode_set(mode='POSE')
 path=OUT/(name+'_Rig.blend');bpy.ops.wm.save_as_mainfile(filepath=str(path),compress=True)
 reports.append({'name':name,'bones':len(rig.data.bones),'skinnedMeshes':len(meshes),'vertices':sum(len(o.data.vertices) for o in meshes),'unweightedVertices':unweighted,'maximumWeightError':max_error,'poseMovementMetres':movement,'fingerJointsVerified':len(finger_checks),'assetCollection':rig.users_collection[0].name,'file':str(path.relative_to(P))})
# A separate Blender file with all five independent rigs, ready for multi-object Pose Mode.
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene;scene.name='MAJOR DAMAGE • Character skeletons'
rigs=[]
for i,report in enumerate(reports):
 with bpy.data.libraries.load(str(P/report['file']),link=False) as (source,target):target.collections=[report['assetCollection']]
 coll=target.collections[0];scene.collection.children.link(coll)
 rig=next(o for o in coll.all_objects if o.type=='ARMATURE');rig.location.x=(i-2)*1.5;rigs.append(rig)
 text=bpy.data.curves.new(report['name']+' label','FONT');text.body=report['name'];text.align_x='CENTER';text.size=.10
 label=bpy.data.objects.new(report['name']+' label',text);scene.collection.objects.link(label);label.location=(rig.location.x,-.31,.035);label.rotation_euler=(math.pi/2,0,0)
core=runpy.run_path(str(P/'style_core.py'));camera=core['v3_studio'](scene,'Character rigs',2.1)
camera.location=(0,-12,2.25);camera.rotation_euler=(Vector((0,0,1.04))-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.ortho_scale=8.4
scene.render.resolution_x=1800;scene.render.resolution_y=750;scene.render.fps=24;scene.frame_end=72
note=bpy.data.texts.new('README • Pose the characters')
note.write('MAJOR DAMAGE — character skeletons\n\nAll five models now have 52 weighted Humanoid bones, including 30 finger joints.\nThese are independent posing copies; original models and Unity FBX files stay intact.\n\nSelect a rig, use Ctrl-Tab / Pose Mode, then select a bone and press R. G moves the hips.\nAlt-R clears the selected rotation; Alt-G clears translation.\nOrange: torso; blue: left side; red: right side; purple: neck/head.\nBone collections in Armature Data filter the head, torso and individual limbs.\n\nThe original girl salute action is kept in the Action editor and remains in LilacV3.blend.\nThe posing copies do not auto-play the body action, so your pose is editable.\nEach thumb, index, middle, ring and little finger has proximal, intermediate and distal bones.\n')
for ob in scene.objects:ob.select_set(False)
for rig in rigs:
 rig.select_set(False)
 for bone in rig.data.bones:select_bone(rig,bone.name,False)
rigs[0].select_set(True);bpy.context.view_layer.objects.active=rigs[0]
select_bone(rigs[0],'MD_Hips',True);rigs[0].data.bones.active=rigs[0].data.bones['MD_Hips']
for screen in bpy.data.screens:
 for area in screen.areas:
  if area.type=='VIEW_3D':
   space=area.spaces.active;space.shading.type='MATERIAL';space.overlay.show_overlays=True;space.overlay.show_bones=True;space.region_3d.view_perspective='CAMERA'
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'Character_Rigs.blend'),compress=True)
(OUT/'rig_report.json').write_text(json.dumps(reports,indent=2))
print('POSE_RIGS_READY',json.dumps(reports))
