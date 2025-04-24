---
uid: keyframes
---

# Edit a pose with effectors

An effector manipulates the position and rotation of specific body parts in an animation. By adjusting effectors, you can control the movement and pose of the characters. Effectors have different colors to display their status: 

* Orange effectors: These are active effectors that allow for immediate adjustments to the character's pose. When you select an active effector, it's color changes to cyan. 

* Black effectors: These are inactive and not selected. A black effector indicates that it hasn't been modified at the moment. When you select a black effector, it's color changes to a grey.

## About effectors

A toolbar on top of the Viewport provides options to manage effectors. 

> [!NOTE]
> This toolbar appears only for **Editable Takes**, not for Muse Animate generated animations. 

* **Drag**: Moves the effector along the plane that's perpendicular to the [camera's](https://docs.unity3d.com/Manual/Cameras.html) viewport or screen space. This means you can adjust the position of the effector within the two-dimensional plane you see on your screen. It enables you to make precise adjustments without affecting the depth.
* **Translate**: Moves the effector along a specific axis or on a plane to control the position of the effector. To use **Translate**, select and drag the arrows to move the effector.
* **Rotate**: Rotates the effectors around a pivot point to adjust the orientation.
* **Adjust Tolerance**: Controls the sensitivity of the effector. Select **Adjust Tolerance** and then select the effector. The effector color turns from orange to blue. Left-click and drag the circle to set the tolerance radius of the effector. A larger circle means the effector has a lesser impact on the result and requires more adjustments for noticeable changes. Conversely, a smaller circle makes the effector more sensitive, so smaller adjustments will have a more pronounced effect.  
* **Disable selected effectors**: Temporarily turns off the selected effector and lets the machine learning (ML) model decide the movement of the effector based on the position of other effectors.

When you select an effector and then select **Translate**, three different colored shapes appear. They represent movement along specific axes.

* Red arrow (x-axis): Moves the effector along the X axis.
* Green arrow (y-axis): Moves the effector along the Y axis.
* Blue arrow (z-axis): Moves the effector along the Z axis.
* Red square (yz-plane): Moves the effector along the YZ plane.
* Green square (xz-plane): Moves the effector along the XZ plane.
* Blue square (xy-plane): Moves the effector along the XZ plane.
* White circle: Moves the effector along the plane that's perpendicular to the camera's viewport or screen space. This means you can adjust the position of the effector within the two-dimensional plane you see on your screen. You can also make precise adjustments without affecting the depth.

To move or rotate the entire rig, select the ring around the hips.

## Apply constraint on effectors

You can use three types of constraints to control the behavior of effectors during animation: position constraint, rotation constraint, and Look At constraint.

### Position constraint (represented by a dot on the effector)

Position constraints constrain the effector to a specific position. To use this constrain, follow these steps:

1. Select the effector you want to adjust.
2. Select **Translate** from the toolbar.
3. Use any of the three planes, three arrows, or the center circle to move the effector around.

When you move the effector, you put a position constraint on it. The inner dot highlights in orange color to show the constraint on position.

### Rotation constraint (represented by a ring around the effector)

Rotation constraints constrain to a specific rotation. To use this constraint, follow these steps:

1. Select the effector you want to rotate.
2. Select **Rotate** from the toolbar.
3. Select and drag any of the three circles (red, green, blue) to change the rotation on those respective axis.

When you rotate the effector, you activate its rotation constraint. The ring around the effector becomes orange, representing the activated constraint on rotation.

### Look At constraint (represented by a line)
   
Look At constraint controls the orientation of the character's head using the line associated with the head effector. 

1. Select the line extending from the head effector to enable it.

   This line represents the direction along which the head orients itself.
2. Select **Translate** from the toolbar, and then select and move the cross at the tip of the line to adjust the head's orientation and change the direction in which the head is facing.
3. To reset changes made to this effector, select it and then select **Disable selected effectors** to remove the constraint on the effector. The head will go back to the orientation that ML decides.

## Additional resources

* [Work with playback controls](playback-controls.md)
* [Refine with keyframes](keyframes.md)
* [Unity cameras](https://docs.unity3d.com/Manual/CamerasOverview.html) 