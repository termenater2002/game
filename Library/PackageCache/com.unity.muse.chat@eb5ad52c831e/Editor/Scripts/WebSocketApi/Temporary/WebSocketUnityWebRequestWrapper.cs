using System;
using System.Collections.Generic;
using Unity.Muse.Chat.BackendApi.Utilities;
using UnityEngine.Networking;

namespace Unity.Muse.Chat.WebSocketApi.Temporary
{
    internal class WebSocketUnityWebRequestWrapper: IUnityWebRequest
    {
        public string Text { get; set; }
        public Dictionary<string, string> ResponseHeaders { get; set; } = null;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public string url { get; set; }
        public string method { get; set; }
        public string error { get; }
        public bool isDone { get; }
        public bool isNetworkError { get; }
        public bool isHttpError { get; }
        public long responseCode { get; }
        public float uploadProgress { get; }
        public float downloadProgress { get; }
        public ulong uploadedBytes { get; }
        public ulong downloadedBytes { get; }
        public IUploadHandler uploadHandler { get; set; }

        public IDownloadHandler downloadHandler
        {
            get => new WebSocketDownloadHandlerWrapper(Text);
            set { }
        }

        public ICertificateHandler certificateHandler { get; set; }
        public int timeout { get; set; }
        public int redirectLimit { get; set; }
        public bool useHttpContinue { get; set; }
        public bool disposeDownloadHandlerOnDispose { get; set; }
        public bool disposeUploadHandlerOnDispose { get; set; }
        public void SetRequestHeader(string name, string value)
        {
            throw new NotImplementedException();
        }

        public string GetRequestHeader(string name)
        {
            throw new NotImplementedException();
        }

        public string GetResponseHeader(string name)
        {
            return ResponseHeaders == null ? null : ResponseHeaders[name];
        }

        public Dictionary<string, string> GetResponseHeaders()
        {
            return ResponseHeaders;
        }

        public UnityWebRequestAsyncOperation SendWebRequest()
        {
            throw new NotImplementedException();
        }

        public void Abort()
        {
            throw new NotImplementedException();
        }
    }
}
