First-person walk/body verification

Runtime changes: FunnyMovement matches the existing animation reference speeds,
WASD walks, Shift sprints, grounded release brakes, air momentum stays physical.
The player capsule gets its own zero-friction material; source assets are untouched.
FirstPersonCamera fits the existing capsule and uses grounded animation phase for subtle gait motion.
FirstPersonBodyVisibility draws a runtime headless skin only in the FPS camera; the original
full skin casts shadows, and other cameras retain the complete model. No source mesh is changed.

Real Unity Editor: 6000.6.8f1, foundation-work/validation. Source project version unchanged.
Edit Mode: 23/23. Play Mode: 29/29, including actual rendered FPS pixels, head masking,
walk/sprint/braking, ground/air camera motion, backward root-motion and jump regressions.
Motion.json: actual player on a temporary floor on its configured ground layer, then restored.
The player loop advanced 216 frames. Reference speeds: 0.1914945 and 0.47873625 world units/s
for the pre-existing 0.1-scale player; stopping stage reaches zero velocity.
FPS_Down_*: real scene camera at 0/60/75/85 degrees. Walk/Sprint/Backward: moving body.
Shadow proof isolates the original caster: 93,880 darkened pixels; the FPS viewmesh is suppressed
only during this shadow measurement, so body pixels are not counted as a shadow.

Reproduce with Unity CLI/Pipeline script run:
AgentScripts/ConfigureFirstPersonWalk.cs -> ConfigureFirstPersonWalk.Apply (Edit Mode, saved SampleScene).
AgentScripts/FirstPersonWalkProof.cs -> FirstPersonWalkProof.Views / Motion (Play Mode).
AgentScripts/FirstPersonWalkAudit.cs -> FirstPersonWalkAudit.Inspect (Play Mode).
