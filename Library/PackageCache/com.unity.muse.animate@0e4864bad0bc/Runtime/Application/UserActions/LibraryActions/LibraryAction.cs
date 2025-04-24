namespace Unity.Muse.Animate.UserActions
{
    abstract class LibraryAction : UserAction
    {
        internal enum LibraryActionType
        {
            AddItem,
            EditItem,
            DeleteItem,
            ModifyItem,
            DuplicateItem
        }

        internal AuthoringModel Authoring { get; private set; }
        internal ApplicationLibraryModel ApplicationLibraryModel { get; private set; }
        internal LibraryActionType ActionType { get; }

        internal LibraryAction(
            int id, int groupId,
            string name,
            LibraryActionType type,
            ApplicationLibraryModel libraryModel,
            AuthoringModel authoring)
            : base(
                name, 
                id, 
                groupId)
        {
            ActionType = type;
            Authoring = authoring;
            ApplicationLibraryModel = libraryModel;
            
            Log("Created()");
        }
        
        public override void Clear()
        {
            Log("Clear()");
            Authoring = null;
            ApplicationLibraryModel = null;
        }
    }
}
