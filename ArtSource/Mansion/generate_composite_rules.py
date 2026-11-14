import json,re
from pathlib import Path
root=Path(__file__).resolve().parents[2];rows=json.loads((root/'Documentation/Mansion/Composites/inventory.json').read_text());models={int(r['name'].split('_')[1]):r for r in rows};rules={}
def add(ids,prefix,kind,role='attachment',mass=.3,group=False,mode='mounted',hp=35):
 if isinstance(ids,int):ids=[ids]
 for id in ids:
  r=models[id];names=[p['name'].removeprefix(r['name']+'_') for p in r['parts']];matches=[n for n in names if re.fullmatch(prefix,n)]
  if not matches:raise ValueError((id,prefix))
  selections=[matches] if group else [[n] for n in matches]
  for sel in selections:
   rules.setdefault(id,[]).append(dict(name=role+'_'+sel[0],kind=kind,role=role,mass=mass,hp=hp,mode=mode,meshes=sel))
# Structural members break independently as well as the remaining frame.
add([21,22,23,24,25,28,30,41,42,43,44,45,47,76,77,79,80,81,95,111,114],r'turned_leg(?:\.\d+)?','wood','leg',1,hp=45)
add([21,22,23,24],r'(?:back_cushion_\d|seat_cushion(?:_\d)?|back_upholstery)','fabric','cushion',1.2,hp=60)
add([42,43],r'seat_pad','fabric','cushion',.4,hp=40)
add([78,112],r'(?:backrest|padded_seat|arm_pad(?:\.\d+)?)','fabric','upholstery',.8,hp=60)
add([78,112],r'caster(?:\.\d+)?','plastic','caster',.1,hp=24)
add([25,41,81],r'thick_inlaid_top','wood','tabletop',4,hp=90)
add([44,47],r'marble_worktop','ceramic','worktop',4,hp=90)
# Loose contents keep their identity; no arbitrary items are invented in empty cabinets.
add(69,r'fruit(?:\.\d+)?','fabric','fruit',.15,mode='contained',hp=20)
# Stems belong to their fruit, paired explicitly below by the factory.
add(122,r'paper_sheet(?:\.\d+)?','fabric','paper',.03,mode='contained',hp=15)
add(106,r'toilet_roll(?:\.\d+)?','fabric','roll',.08,mode='contained',hp=20)
add(120,r'pen(?:\.\d+)?','plastic','pen',.025,mode='contained',hp=20)
add(120,r'pen_cup','metal','cup',.2,hp=40)
add(20,r'firewood(?:\.\d+)?','wood','log',.8,mode='contained',hp=45)
add(20,r'grate_(?:bar|top)(?:\.\d+)?','metal','grate',1,group=True,hp=80)
add(35,r'(?:broad_leaf|plant_stem)(?:\.\d+)?','fabric','plant',.35,group=True,hp=45)
add([71,72],r'plump_pillow(?:\.\d+)?','fabric','pillow',.4,mode='contained',hp=35)
add([71,72],r'separate_mattress','fabric','mattress',8,hp=95)
add([71,72],r'stitched_quilt','fabric','quilt',1,hp=50)
add([71,72],r'(?:headboard|footboard)','wood','bedboard',4,hp=85)
add(110,r'stitched_quilt','fabric','cloth',.2,mode='contained',hp=35)
add(110,r'bucket_handle','metal','handle',.2,hp=45)
# Glass, optics and frames. The frame remains when glass breaks.
add([11,12],r'glass_pane(?:\.\d+)?','glass','pane',.8,hp=24)
add(10,r'glazed_insert','glass','pane',.8,hp=24)
add(92,r'fixed_glass(?:\.\d+)?','glass','pane',1,hp=24)
add([81,82,96],r'(?:vanity_)?mirror_inset','glass','mirror',.7,hp=24)
add([36,90,130],r'clock_face','glass','clockface',.12,hp=20)
add([37,38],r'carved_frame_inset','fabric','canvas',.3,hp=45)
add(129,r'award_frame_inset','glass','awardglass',.15,hp=20)
add(129,r'award_medal','metal','medal',.1,hp=35)
# Lamps: break the bulb, tear the shade, retain the metal fixture.
add([31,32,125],r'bulb','glass','bulb',.04,hp=12)
add([31,32,125],r'(?:pleated_shade|shade_spoke)','fabric','shade',.15,group=True,hp=40)
add(33,r'amber_bulb(?:\.\d+)?','glass','bulb',.04,hp=12)
# Electronics and appliance subassemblies.
add([49,50,51],r'control_panel','plastic','controls',.2,hp=35)
add([49,50,51],r'interior_rack(?:\.\d+)?','metal','rack',.35,hp=50)
add(48,r'interior_shelf(?:\.\d+)?','plastic','shelf',.3,hp=40)
add(52,r'hood_filter(?:\.\d+)?','metal','filter',.25,hp=40)
add(55,r'rack_crossbar(?:\.\d+)?','metal','rack',.8,group=True,hp=65)
add(55,r'status_display','glass','display',.05,hp=18)
add(56,r'water_tank','plastic','tank',.3,hp=30)
add(56,r'drip_tray','metal','tray',.15,hp=40)
add(56,r'(?:brew_head|coffee_spout(?:\.\d+)?)','metal','brewhead',.4,group=True,hp=55)
add(57,r'(?:lid|lid_knob)','metal','lid',.18,group=True,hp=50)
add(57,r'power_base','plastic','powerbase',.25,hp=40)
add(57,r'insulated_handle','plastic','handle',.12,hp=30)
add(58,r'(?:lever_grip|toaster_lever)','plastic','lever',.06,group=True,hp=25)
add(62,r'bottle_cap','metal','cap',.02,hp=25)
add(63,r'lid_grip','plastic','grip',.04,hp=25)
add(63,r'pot_handle(?:\.\d+)?','metal','handle',.08,hp=40)
add(64,r'pan_handle','plastic','handle',.1,hp=30)
add(68,r'box_lid','fabric','lid',.05,hp=25)
add([91,94,95],r'(?:faucet_mount|gooseneck_\d|mixer_control)','metal','tap',.6,group=True,hp=65)
add(53,r'(?:faucet_mount|gooseneck_\d|mixer_control)','metal','tap',.6,group=True,hp=65)
add(91,r'claw_foot(?:\.\d+)?','metal','foot',1,hp=65)
add(93,r'cistern_lid','ceramic','cisternlid',1,hp=35)
add(93,r'separate_toilet_seat','plastic','seat',.5,hp=40)
add(92,r'(?:rain_head|shower_pipe_\d|shower_mixer)','metal','shower',1,group=True,hp=70)
add(101,r'screw_cap','plastic','cap',.02,hp=20)
add(102,r'paste_cap','plastic','cap',.01,hp=20)
add(104,r'screw_cap','plastic','cap',.02,hp=20)
add(105,r'pump_\d','plastic','pump',.06,group=True,hp=25)
add(107,r'dryer_barrel','plastic','barrel',.2,hp=35)
add(108,r'foil_head(?:\.\d+)?','metal','head',.04,group=True,hp=35)
add(117,r'scroll_wheel','plastic','wheel',.01,hp=18)
add(119,r'handset_\d','plastic','handset',.25,group=True,mode='contained',hp=50)
add(124,r'globe','plastic','globe',.45,hp=45)
add(124,r'meridian_frame','metal','meridian',.2,hp=55)
add([86,89,109,123],r'carry_handle_(?:grip|mount)(?:\.\d+)?','fabric','handle',.12,group=True,hp=45)
add(86,r'luggage_wheel(?:\.\d+)?','plastic','wheel',.06,hp=30)
add(86,r'telescopic_(?:grip|rail)(?:\.\d+)?','metal','trolley',.3,group=True,hp=60)
add([39,40,128],r'stone_plinth','ceramic','plinth',1,hp=60)
add(126,r'cane_handle_\d','metal','handle',.25,group=True,hp=55)
# Packs and cases also retain independent identities, rather than becoming one rigid pair.
for n in range(3):add(84,rf'(?:clothes_hanger_{n}|hanger_hook_{n})','wood','hanger',.15,group=True,hp=35)
add(84,r'hanger_hook_\d','metal','hook',.02,hp=25)
add([86,89,109,123],r'brass_lock(?:\.\d+)?','metal','locks',.08,group=True,hp=40)
add(127,r'box_latch','metal','latch',.03,hp=30)
# Single-mesh objects (cup, vase, plate, keyboard, etc.) retain material-specific whole-object destruction.
# Moving assemblies are discovered from the authored moving_parts manifest, not texture guesses.
result={'schema':1,'description':'Reviewed semantic components of all authored compound assets; moving parts added from assets.json; simple shells retain their material fracture.','assets':[{'id':id,'parts':p} for id,p in sorted(rules.items())]}
out=root/'ArtSource/Mansion/composite_parts.json';out.write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n');print('assets',len(rules),'assemblies',sum(map(len,rules.values())))
