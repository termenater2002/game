import bpy,json
from pathlib import Path
P=Path(bpy.data.filepath).parent
s=bpy.context.scene
ob=next(o for o in s.objects if o.get('kind')=='unified_upper_body')
mesh=ob.data
adj=[set() for v in mesh.vertices]
edge_faces={}
for poly in mesh.polygons:
 indices=list(poly.vertices)
 for i,a in enumerate(indices):
  b=indices[(i+1)%len(indices)];adj[a].add(b);adj[b].add(a)
  edge_faces.setdefault(tuple(sorted((a,b))),[]).append(poly.index)
seen=set();components=0
for v in mesh.vertices:
 if v.index in seen:continue
 components+=1;todo=[v.index];seen.add(v.index)
 while todo:
  a=todo.pop()
  for b in adj[a]:
   if b not in seen:seen.add(b);todo.append(b)
assert components==1,('Disconnected upper-body surface',components)
bad=[]
for v in mesh.vertices:
 weights={ob.vertex_groups[g.group].name:g.weight for g in v.groups if g.weight>.00001}
 assert abs(sum(weights.values())-1)<.0001
 if abs(v.co.x)>.285 and any(not n.endswith(('UpperArm','LowerArm','Hand')) for n in weights):bad.append(v.index)
assert not bad,('Sleeve vertices attached to torso',bad)
# Each boundary between skin and cloth is a shared mesh edge, not two overlaid surfaces.
def role(poly):
 uv=mesh.uv_layers.active.data[poly.loop_start].uv
 if abs(uv.x-.53)<.00001 and abs(uv.y-.47)<.00001:return 'skin'
 return 'fabric'
shared=[e for e,faces in edge_faces.items() if len(faces)==2 and role(mesh.polygons[faces[0]])!=role(mesh.polygons[faces[1]])]
assert len(shared)>10
# Inspect actual deformation at every frame and reject stretched cloth triangles.
ratios=[];max_edge=0
for frame in range(1,73):
 s.frame_set(frame);bpy.context.view_layer.update();ev=ob.evaluated_get(bpy.context.evaluated_depsgraph_get());me=ev.to_mesh()
 for a,b in edge_faces:
  base=(mesh.vertices[a].co-mesh.vertices[b].co).length
  length=(me.vertices[a].co-me.vertices[b].co).length;max_edge=max(max_edge,length)
  if base>.003:ratios.append(length/base)
 ev.to_mesh_clear()
s.frame_set(1)
assert max(ratios)<2.8,('Stretched garment edge',max(ratios))
report={'connectedComponents':components,'sharedSkinClothEdges':len(shared),'sleeveVerticesOnTorso':len(bad),'framesChecked':72,'maximumEdgeStretch':max(ratios),'maximumEdgeLength':max_edge,'vertices':len(mesh.vertices),'triangles':sum(len(p.vertices)-2 for p in mesh.polygons)}
(P/'upper_body_audit.json').write_text(json.dumps(report,indent=2));print('ARM_SYNC_AUDIT',json.dumps(report))
