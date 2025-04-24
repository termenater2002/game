namespace Unity.Muse.Agent.Dynamic
{
#if CODE_LIBRARY_INSTALLED
    public
#else
    internal
#endif
    interface IRunCommand
    {
        public void Execute(CommandAttachment attachments, ExecutionResult result);
        public void BuildPreview(PreviewBuilder result);
    }
}
