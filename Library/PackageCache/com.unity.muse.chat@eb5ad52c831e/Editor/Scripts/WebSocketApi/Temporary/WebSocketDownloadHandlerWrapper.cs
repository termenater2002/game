using System;
using Unity.Collections;
using Unity.Muse.Chat.BackendApi.Utilities;

namespace Unity.Muse.Chat.WebSocketApi.Temporary
{
    class WebSocketDownloadHandlerWrapper : IDownloadHandler
    {
        public WebSocketDownloadHandlerWrapper(string text)
        {
            this.text = text;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public byte[] data { get; }
        public string text { get; }
        public NativeArray<byte>.ReadOnly nativeData { get; }
        public bool isDone { get; }
        public string error { get; }
    }
}
