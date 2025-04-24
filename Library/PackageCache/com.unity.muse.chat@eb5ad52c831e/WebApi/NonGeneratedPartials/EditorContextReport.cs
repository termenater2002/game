using System.Collections.Generic;

namespace Unity.Muse.Chat.BackendApi.Model
{
    partial class EditorContextReport
    {
        internal IEnumerable<ContextItem> AllContext
        {
            get
            {
                foreach (var item in AttachedContext)
                {
                    yield return item;
                }

                foreach (var item in ExtractedContext)
                {
                    yield return item;
                }
            }
        }

        internal void Sort()
        {
            AttachedContext.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            ExtractedContext.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }
    }
}