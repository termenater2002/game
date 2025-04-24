---
uid: keyframes
---

# Refine with keyframes

Each keyframe has a **Key Settings** section to configure and fine-tune its behavior within the animation. The **Key Settings** section provides the following options:

* **Transition Duration**: This setting specifies the duration of the keyframe. It controls the time it takes for the animation to transition from one keyframe to the next.
* **Extrapolated pose**: This setting uses ML models to determine the transition between the last existing pose and the next one. For a smooth transition in animation, instead of manually defining the pose, this setting extrapolates and generates a pose that fits well between the existing poses.
* **Loop to first pose**: This setting ensures that your animation ends in the same position as its initial keyframe. It enables your character to return to the same state as the first keyframe. You can apply this option only to the last frame.

To edit the keyframes, follow these steps:

1. Select the keyframe.

   The **Key Settings** menu opens.
2. Move the **Transition Duration** slider control to set the duration of the keyframe. For a small keyframe (faster transition), move the slider control left, and move it right for long keyframes (slower transition).

   > [!NOTE]
   > The last keyframe in the animation has no transition duration as it's the last pose.

3. If you want the ML model to determine how the animation might behave beyond the current keyframe, enable **Extrapolated pose**.
4. If you want your animation's end pose to be same as the initial pose, enable **Loop to first pose**. 

> [!NOTE]
> You can use the mouse scroll wheel to zoom in and out on an animation timeline to closely inspect and fine-tune the keyframe. 

### Add a keyframe

Add a keyframe between two existing keyframes to refine the animation between two poses. This helps to create a smoother transition and lets you adjust attributes, such as position or rotation.

To add a keyframe, follow these steps:

1. Move the playhead to the frame where you want to insert a new keyframe.
2. Select **+** on the playback controls to insert a new keyframe.
3. To view the updated animation, select **Play** on the playback controls.

### Use the context menu

When you right-click on a keyframe, the context menu provides different options to manage keyframes:

* **Copy**: Copies the selected keyframe so that you can paste it elsewhere.

* **Paste and Replace**: Replaces the selected keyframe with the copied keyframe. This option is disabled until you copy a keyframe.

* **Paste Left**: Pastes the copied keyframe to the left of the current keyframe. This option is disabled until you copy a keyframe.

* **Paste Right**: Pastes the copied keyframe to the right of the current keyframe. This option is disabled until you copy a keyframe.

* **Duplicate Left**: Creates a duplicate of the selected keyframe to the left.

* **Duplicate Right**: Creates a duplicate of the selected keyframe to the right.

* **Move Left**: Moves the selected keyframe one position to the left.

* **Move Right**: Moves the selected keyframe one position to the right.

* **Delete**: Deletes the selected keyframe from the playback controls.

## Additional resources

* [Generate animation](generate-animation.md)
* [Manage Library](library.md)
* [Work with playback controls](playback-controls.md)