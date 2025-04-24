namespace Unity.Muse.Common
{
    internal interface ICloudContext
    {
        internal delegate void Callback();
        void RegisterNextFrameCallback(Callback cb);
    }
}
