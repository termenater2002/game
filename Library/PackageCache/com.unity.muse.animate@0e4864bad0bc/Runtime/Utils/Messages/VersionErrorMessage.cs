namespace Unity.Muse.Animate
{
    readonly struct VersionErrorMessage
    {
        public readonly string ExpectedVersion;
        public readonly string ActualVersion;
        
        public VersionErrorMessage(string expectedVersion, string actualVersion)
        {
            ExpectedVersion = expectedVersion;
            ActualVersion = actualVersion;
        }
    }
}
