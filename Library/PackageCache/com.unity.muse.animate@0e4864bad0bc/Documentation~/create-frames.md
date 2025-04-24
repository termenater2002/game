---
uid: frames
---

# Create keyframes

Transform an animation into a keyframe-based animation. After you select the animation, you can convert it into a keyframe-based animation and save it in the **Assets** folder.

You can set how detailed the keyframe conversion will be. Higher sensitivity results in more keyframes that capture finer details of the animation, but it also makes manual editing cumbersome. In contrast, lower sensitivity results in fewer keyframes that capture only the main movements and transitions.

To convert **Generated Take** animations into keyframe-based animations, follow these steps:

1. In the **Sampling** section, enter a **Sensitivity** value. The default value is `50`, which indicates medium sensitivity. 
2. Select **Sample**.

   This converts the original animation into a series of keyframes based on the selected sensitivity. When an animation plays, the playback controls highlight the segment that corresponds to the active keyframe. This shows which part of the animation is currently playing. As the playhead moves, Muse Animate highlights different keyframe sections in real time. The duration of each keyframe segment on the playback controls varies. Keyframes where the animation changes a lot last longer while those with small changes are shorter.

   An [**Editable Take**](library.md#editable-take-animations) animation appears in the **Library**.

## Save the animation

After the sampling is complete, a set of frames that represent your animation are displayed. To save all the frames, select **Convert to Frames**.

Muse Animate saves the **Editable Take** animation in the **Assets** folder of your project. You can access each frame separately for detailed editing and use them in other parts of your project.

## Additional resources

* [Refine with keyframes](keyframes.md)
* [Edit a pose with effectors](effectors.md)
* [Export animation](export-animation.md)