using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Unity.Muse.Sprite.Common.Backend
{
    class WebRequest : IWebRequest
    {
        UnityWebRequest m_WebRequest;
        AsyncOperation m_WebOperation;
        Action<IWebRequest> m_OnComplete;
        Dictionary<string, string> m_Header = new();

        public string info => m_WebRequest.url;
        public float downloadProgress => m_WebRequest?.downloadProgress ?? -1;
        public ulong downloadBytes => m_WebRequest?.downloadedBytes ?? ulong.MaxValue;

        public WebRequest(string url, string method)
        {
            m_WebRequest = new UnityWebRequest(url, method);
            m_WebRequest.downloadHandler = new DownloadHandlerBuffer();
        }

        public void SetRequestHeader(string name, string value)
        {
            m_Header.Add(name, value);
            m_WebRequest.SetRequestHeader(name, value);
        }

        public void SetPayload(byte[] payload, string payloadType)
        {
            m_WebRequest.uploadHandler = new UploadHandlerRaw(payload);
            m_WebRequest.uploadHandler.contentType = payloadType;
        }

        public void SendWebRequest(Action<IWebRequest> onComplete)
        {
            m_WebOperation = m_WebRequest.SendWebRequest();
            m_WebOperation.completed += x => onComplete?.Invoke(this);
        }

        public IWebRequest Recreate()
        {
            var webRequest = new WebRequest(m_WebRequest.url, m_WebRequest.method);
            foreach(var header in m_Header)
            {
                webRequest.SetRequestHeader(header.Key, header.Value);
            }
            if(m_WebRequest.uploadHandler != null)
                webRequest.SetPayload(m_WebRequest.uploadHandler.data, m_WebRequest.uploadHandler.contentType);
            return webRequest;
        }

        public void Dispose()
        {
            m_WebRequest?.uploadHandler?.Dispose();
            m_WebRequest?.downloadHandler?.Dispose();
            m_WebRequest?.Dispose();
        }

        public UnityWebRequest.Result result => m_WebRequest.result;
        public long responseCode => m_WebRequest.responseCode;
        public string error => m_WebRequest.error;
        public string errorMessage => m_WebRequest.downloadHandler?.text;
        public string responseText => m_WebRequest.downloadHandler?.text;
        public byte[] responseByte => m_WebRequest.downloadHandler?.data;
        public byte[] uploadData => m_WebRequest.uploadHandler.data;
        public string uploadDataType => m_WebRequest.uploadHandler.contentType;
    }
}