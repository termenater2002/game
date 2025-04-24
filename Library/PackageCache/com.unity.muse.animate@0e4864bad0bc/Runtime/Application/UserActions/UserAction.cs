using UnityEngine;

namespace Unity.Muse.Animate.UserActions
{
    abstract class UserAction
    {
        public int Id { get; }
        public string Name { get; }
        public int GroupId { get; }
        public string FullName { get; }
        public ScriptableObject DummyObject { get; }

        internal UserAction(string name, int id, int groupId)
        {
            Id = id;
            Name = name;
            GroupId = groupId;
            FullName = $"{Id}: {Name} ({GroupId})";
            
            DummyObject = ScriptableObject.CreateInstance<ScriptableObject>();
            DummyObject.hideFlags = HideFlags.HideAndDontSave;
        }

        public abstract void Redo();
        public abstract void Undo();
        public abstract void Clear();

        public void Log(string message)
        {
            if (ApplicationConstants.DebugUndoRedo)
                Debug.Log($"[{FullName}] -> {message}");
        }
    }
}
