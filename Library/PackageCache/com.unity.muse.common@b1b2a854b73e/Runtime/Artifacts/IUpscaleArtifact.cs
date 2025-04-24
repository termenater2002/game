namespace Unity.Muse.Common
{
    internal interface IUpscaleArtifact
    {
        public bool CanUpscale();
        public void Upscale(Model model);
        public ContextMenuAction GetContextMenuAction()
        {
            return new ContextMenuAction
            {
                enabled = true,
                id = (int)Actions.Upscale,
                label = "Upscale"
            };
        }
    }
}
