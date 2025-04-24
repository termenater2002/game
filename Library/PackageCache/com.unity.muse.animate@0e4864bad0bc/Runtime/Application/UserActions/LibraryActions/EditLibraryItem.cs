namespace Unity.Muse.Animate.UserActions
{
    class EditLibraryItem : LibraryAction
    {
        LibraryItemAsset m_ItemBefore;
        LibraryItemAsset m_ItemToEdit;

        public LibraryItemAsset TargetItemAsset => m_ItemToEdit;

        internal EditLibraryItem(
            int id, 
            int groupId,
            ApplicationLibraryModel libraryModel,
            AuthoringModel authoring,
            LibraryItemAsset itemBefore,
            LibraryItemAsset itemToEdit)
            : base(id, groupId,
                "Edit " + itemToEdit.Title,
                LibraryActionType.EditItem,
                libraryModel,
                authoring)
        {
            m_ItemBefore = itemBefore;
            m_ItemToEdit = itemToEdit;
        }

        public override void Redo()
        {
            Log("Redo()");
            ApplicationLibraryModel.RequestEditLibraryItem(m_ItemToEdit);
        }

        public override void Undo()
        {
            Log("Undo()");
            ApplicationLibraryModel.RequestEditLibraryItem(m_ItemBefore);
        }

        public override void Clear()
        {
            Log("Clear()");
            m_ItemToEdit = null;
            m_ItemBefore = null;
            base.Clear();
        }
    }
}
