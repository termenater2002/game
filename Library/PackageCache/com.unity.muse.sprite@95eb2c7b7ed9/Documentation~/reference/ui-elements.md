---
uid: ui-elements
---

# UI elements

This page describes the UI elements of the **Muse Sprite** tool.

## Muse Generator window

| **Property** | **Description** |
| --- | --- |
| **Images** | Sets the number of images to be generated. |
| **Generate Sprites** | Starts generating sprites. Generated sprites appear in the **Generations** panel. |
| **Prompt** | Text input used to generate sprites. |
| **Negative Prompt** | Text that describes the elements to exclude from the generated sprites. |
| **Generations** | Previews and provides actions for the generated sprites. |
| **Zoom slider** | Zooms the generated images in and out. |

## Refine panel

| **Property** | **Description** |
| --- | --- |
| **Hand** | Deselects a refine tool. |
| **Paint** | Activates the painting brush you can use to create and edit a mask. |
| **Erase** | Erases the mask.|
| **Clear** | Clears the canvas. |
| **Radius** | Sets the size of the brush or eraser. |

## Parameters panel

| **Property** | **Description** |
| --- | --- |
| **Remove Background** | Removes the background from the generated sprites. |
| **Style Strength** | Determines how closely the generated sprites follow the style of the referenced image or the scribble. Higher values result in more closely aligned styles.|
| **Custom Seed** | If enabled, you can enter a custom seed number to generate the sprite. Otherwise, the tool generates a random seed number. |

## Input Image panel

| **Property** | **Description** |
| --- | --- |
| **Scribble tool** | Draws on the canvas. Only works when there is no reference image. |
| **Eraser tool** | Erases the scribbles. Only works when there is no reference image. |
| **Sprite picker** | Selects a sprite from the **Scene** view, **Hierarchy** panel or the **Project** window to use as the reference image. |
| **Clear tool** | Clears the scribbles or the reference image. |
| **Tightness** | Determines the likeness between the generated sprites and the referenced image. Higher values result in more alike sprites.|

## Image context menu items

The following table describes the context menu (&#8230;) items of a generated sprite:

| **Property** | **Description** |
| --- | --- |
| **Generation Settings** | Displays and reuses the prompt, negative prompt, seed number, and style used in the generation. |
| **Export** | Exports the sprite to the project's `Assets` folder. |

## Unity Muse bar

| **Property** | **Description** |
| --- | --- |
| **Muse Points Used** | Displays the number of [Muse points you have used and the total points you have](https://unity.com/ai/faq). There are no point deductions for unsuccessful generations. |
| **Go to Muse account** | Opens the [Muse account](https://id.unity.com/en/account/edit) page in your browser. |

## Additional resources

* [Generate from a seed](xref:generate-from-seed)
* [Generate from a reference image](xref:generate-from-reference)
* [Generate from a scribble](xref:generate-from-scribble)
* [Manage generated sprites](xref:manage-sprites)
* [Refine with masking](xref:refine)
* [Keyboard shortcuts](xref:keyboard-shortcuts)
