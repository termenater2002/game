import bpy,json
from mathutils import Vector
P='/var/home/deck/Desktop/tests/family-work/'
sources=[bpy.data.objects['FamilyFather_Rig'],bpy.data.objects['FamilyMother_Rig']]
keep={rig.users_scene[0] for rig in sources}
bpy.ops.wm.save_as_mainfile(filepath=P+'Family_DraftHistory.blend',compress=True)
for draft in list(bpy.data.scenes):
    if draft.name.startswith('Major Damage • Family') and draft not in keep:
        for ob in list(draft.objects):
            if len(ob.users_scene)==1:bpy.data.objects.remove(ob,do_unlink=True)
        bpy.data.scenes.remove(draft)
for rig in sources:
    scene=rig.users_scene[0];bpy.context.window.scene=scene
    name='FamilyFather' if rig.name.startswith('FamilyFather') else 'FamilyMother'
    scene.render.filepath=P+name+'_Front.png';bpy.ops.render.render(write_still=True)
    camera=scene.camera;old_location=camera.location.copy();old_rotation=camera.rotation_euler.copy();old_scale=camera.data.ortho_scale
    target=Vector((0,-.01,1.90 if name=='FamilyFather' else 1.76));camera.location=(.65,-4.8,target.z+.08);camera.rotation_euler=(target-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.ortho_scale=.49 if name=='FamilyFather' else .44
    scene.render.resolution_x=800;scene.render.resolution_y=800;scene.render.filepath=P+name+'_Portrait.png';bpy.ops.render.render(write_still=True)
    camera.location=old_location;camera.rotation_euler=old_rotation;camera.data.ortho_scale=old_scale;scene.render.resolution_x=1000;scene.render.resolution_y=1100
scene=bpy.data.scenes.new('Major Damage • Family Parents');bpy.context.window.scene=scene
asset=bpy.data.collections.new('FAMILY_PARENT_PREVIEW');scene.collection.children.link(asset)
for src in sources:
    rig=src.copy();rig.data=src.data.copy();asset.objects.link(rig);rig.name=src.name+'_Preview';rig.location.x=-.68 if src.name.startswith('FamilyFather') else .68
    for original in src.children:
        ob=original.copy();ob.data=original.data;asset.objects.link(ob);ob.parent=rig
        for modifier in ob.modifiers:
            if modifier.type=='ARMATURE':modifier.object=rig
source_scene=sources[1].users_scene[0];scene.world=source_scene.world
for ob in source_scene.objects:
    if ob.type=='LIGHT' or ob.name.startswith('Family studio floor'):
        copy=ob.copy();asset.objects.link(copy)
data=bpy.data.cameras.new('Family Parents Camera');camera=bpy.data.objects.new('Family Parents Camera',data);asset.objects.link(camera);camera.location=(.7,-8,2.7);camera.rotation_euler=(Vector((0,0,1.02))-camera.location).to_track_quat('-Z','Y').to_euler();data.type='ORTHO';data.ortho_scale=3.45;scene.camera=camera
try:scene.render.engine='BLENDER_EEVEE'
except TypeError:pass
scene.render.resolution_x=1400;scene.render.resolution_y=1000;scene.render.resolution_percentage=100;scene.render.image_settings.file_format='PNG';scene.render.filepath=P+'Family_Preview.png';scene.view_settings.view_transform='AgX';scene.view_settings.exposure=-.25
bpy.ops.render.render(write_still=True)
for ob in scene.objects:ob.select_set(False)
for screen in bpy.data.screens:
    for area in screen.areas:
        if area.type=='VIEW_3D':area.spaces.active.region_3d.view_distance=4.4;area.spaces.active.region_3d.view_location=(0,0,1.05);area.spaces.active.shading.type='MATERIAL'
bpy.ops.wm.save_as_mainfile(filepath=P+'FamilyParents.blend',compress=True)
print(json.dumps({'file':P+'FamilyParents.blend','preview':P+'Family_Preview.png','face_textures':'256px painted UV','cloth_textures':'128px pixel motifs','characters':2}))
