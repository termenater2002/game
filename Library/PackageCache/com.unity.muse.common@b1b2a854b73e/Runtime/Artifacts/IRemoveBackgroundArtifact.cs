namespace Unity.Muse.Common
{
    internal interface IRemoveBackgroundArtifact
    {
        public bool CanRemoveBackground();
        public void RemoveBackground(Model model);
    }
}