"""Run only in an isolated Blender --factory-startup process, never the user's session."""
import bpy,runpy,json,sys,tempfile
from pathlib import Path
P=Path(__file__).resolve().parent
core=runpy.run_path(str(P/'style_core.py'));core['V3_OUTPUT']=str(P)+'/'
reports=[]
for name,script,option in [('LilacV3','build_lilac_v3.py',None),('RiderV3','build_rider_v3.py','jacket'),('RiderTShirtV3','build_rider_v3.py','tshirt'),('FamilyFatherV3','build_family_v3.py','father'),('FamilyMotherV3','build_family_v3.py','mother')]:
    if '--only' in sys.argv and name!=sys.argv[sys.argv.index('--only')+1]:continue
    # The authoring scripts expose one outfit/person constant. Write a reviewed local variant,
    # preserving the canonical sources and all user projects.
    source=(P/script).read_text()
    if option=='tshirt':source=source.replace("OUTFIT = 'jacket'","OUTFIT = 'tshirt'")
    if option=='mother':source=source.replace("PERSON='father'","PERSON='mother'")
    variant=Path(tempfile.gettempdir())/('major_damage_'+name+'.py');variant.write_text(source)
    ns=runpy.run_path(str(variant),init_globals=core)
    scene=bpy.context.scene;rig=ns['rig'];parts=ns['parts'];collection=ns.get('collection',ns.get('asset'))
    for other in list(bpy.data.scenes):
        if other!=scene:bpy.data.scenes.remove(other)
    bpy.data.orphans_purge(do_recursive=True)
    # Native scene now contains only this authored character and its presentation stage.
    scene.frame_set(1)
    if name=='LilacV3':
        ns['v3_export'](scene,collection,parts,rig,name,str(P/(name+'_Salute.fbx')),bake=True)
    bpy.ops.wm.save_as_mainfile(filepath=str(P/(name+'.blend')),compress=True)
    scene.render.resolution_x=700;scene.render.resolution_y=850
    scene.eevee.taa_render_samples=32 if hasattr(scene.eevee,'taa_render_samples') else 32
    for view in ['Front','Side','Back','Portrait']:
        cam=scene.camera;h=2.06 if 'Father' in name else 1.95
        pos={'Front':(.2,-6,h*.9),'Side':(6,-.3,h*.9),'Back':(-.2,6,h*.9),'Portrait':(.18,-6,h*.92)}[view]
        target=ns['Vector']((0,0,h*(.88 if view=='Portrait' else .51)))
        cam.location=pos;cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=.69 if view=='Portrait' else h*1.16
        scene.render.filepath=str(P/(name+'_'+view+'.png'));bpy.ops.render.render(write_still=True)
    if name=='LilacV3':
        cam=scene.camera;cam.location=(.2,-6,1.8);cam.rotation_euler=(ns['Vector']((0,0,1.75))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=.7
        scene.frame_set(9);scene.render.filepath=str(P/(name+'_Blink.png'));bpy.ops.render.render(write_still=True)
        cam.data.ortho_scale=2.26;cam.rotation_euler=(ns['Vector']((0,0,.995))-cam.location).to_track_quat('-Z','Y').to_euler()
        scene.frame_set(28);scene.render.filepath=str(P/(name+'_Salute.png'));bpy.ops.render.render(write_still=True)
    reports.append({'name':name,'triangles':sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in parts),'bones':len(rig.data.bones),'atlas':512,'file':name+'.blend'})
report=P/'model_report.json'
old=json.loads(report.read_text()) if report.exists() else []
by_name={r['name']:r for r in old}
for r in reports:by_name[r['name']]=r
report.write_text(json.dumps(list(by_name.values()),indent=2))
print('V3_ALL_MODELS_BUILT',json.dumps(reports))
