---
uid: manage-sprites
---

# Manage generated sprites

You can manage the generated sprites in the **Generations** panel.

## Reuse generation settings

You can reuse prompt, negative prompt, seed number, and style from a previous generation to generate similar sprites.

1. Right-click the sprite whose settings you want to reuse.
1. Select **Generation Settings**. The **Generation Settings** window appears.
1. Do the following:

   * To reuse all previous settings, select **Use All**. This copies all settings to the **Generation** area.
   * To reuse only some settings, select **Use**. This copies only the selected settings to the **Generation** area.
1. Select **Generate**.

## Star

To mark a sprite as a favorite, select the **Star** icon. To remove the star, select the **Star** icon again.

## Filter

To filter the generated sprites, enter a keyword in the **Search** box. The keyword can be a full or partial match to the prompt you used to generate the sprites.

To show only starred sprites, select the **Star** icon next to the **Search** box.

## Scale

To make the generated images bigger or smaller, move the slider at the bottom of the **Generations** panel.

## Export

You can export a sprite as a .png file in your project and use it in your project like any other sprite.

To export one or more generated sprites:

1. Select the generated sprite you want to save. To select multiple sprites, hold <kbd>Ctrl</kbd> (macOS: <kbd>Cmd</kbd>) and click the sprites.
1. Do one of the following:

    * Drag the sprites to the `Assets` folder in the Project window.
    * Right-click a sprite, and select **Export**.

> [!TIP]
> When you close the **Muse Generator** window, it saves the **Muse Generator** thread with all sprites in it as a generator asset in your project. You can find the generator asset in the `Assets` folder in the Project window. You can reuse the generator asset to generate more sprites.

## Delete

To delete one or more generated sprites:

1. Select the generated sprite you want to delete. To select multiple sprites, hold <kbd>Ctrl</kbd> (macOS: <kbd>Cmd</kbd>) and click the sprites.
1. Right-click the sprite and select **Delete** (or press <kbd>Delete</kbd>).

> [!TIP]
> To delete all generated sprites, close the **Muse Generator** window and select **Discard**.

## Additional resources

* [Write a prompt](xref:write-prompt)
* [Refine with masking](xref:refine)
* [Generate sprites](xref:generate)
