---
uid: unity-object-query
---

# Interact with GameObjects, assets, and Console errors

You can use Muse Chat to interact contextually with GameObjects, assets, and console errors directly within the Unity Editor. You can query and receive information about the selected component using natural language through Muse Chat.

## Prerequisites

These instructions assume that you have integrated and docked Muse Chat in the Unity Editor.

For more information, refer to [Install Muse Chat with the Package Manager](install-chat.md#launch-the-tool).

## Engage with development tools and environment

1. To interact with the GameObjects, assets, or the **Console** logs and errors:

   **For GameObjects**

   In the **Scene** view or **Hierarchy** window, select one or more GameObjects.

   **For assets**

   In the **Project** window, select one or more assets.

   **For Console logs and errors**

   1. To open the **Console** window, in the main menu, go to **Window** > **Panels** > **Console**.
   2. In the **Console** window, select one or more logs and errors.

2. Select **Attach items** in the Muse Chat window.

   The **Attach items** window displays the selected GameObjects (from the **Hierarchy** window), assets (from the **Project** window), and logs and errors (from the **Console** window).

3. For each item you want to use with your prompt, select **Attach** on the right side.

   The attached items display in the **Attach items** section on the Muse Chat window.

4. Alternatively, to attach all the currently selected GameObjects, assets, and Console logs and errors at once, select **Add Editor selection** in the **Attach items** window.

   For GameObjects (from the **Hierarchy** window), assets (from the **Project** window), and Components (from Unity **Inspector** window), you can drag them directly into the Muse Chat window.

5. Select **Remove** next to any items you no longer want to attach or to remove all the attached items, select **Clear Content** in the **Attach items** section.

Muse Chat displays the GameObject, assets, or Console logs and errors you selected and uses them as context to answer queries. You can now interact with Muse Chat as you normally would.

## Additional resources

* [Muse Chat Editor interface](editor-chat-interface.md)
* [Use Muse Chat Editor](use-editor-chat.md)
* [Best practices for using Muse Chat](best-practice-chat.md)