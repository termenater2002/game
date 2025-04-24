using UnityEditor;

namespace Unity.Muse.Chat
{
    class UndoHistoryUtils
    {
        internal static void OpenHistory()
        {
            UndoHistoryWindow.OpenUndoHistory();
        }

        internal static void RevertGroupAndOpenHistory(int group)
        {
            Undo.RevertAllDownToGroup(group);
            UndoHistoryWindow.OpenUndoHistory();
        }
    }
}
