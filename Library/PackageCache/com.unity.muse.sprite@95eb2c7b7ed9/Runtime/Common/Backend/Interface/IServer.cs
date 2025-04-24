using System;
using UnityEngine.Networking;

namespace Unity.Muse.Sprite.Common.Backend
{
    interface IServer
    {
        void Reset();
        void Init();
        IWebRequest CreateWebRequest(string url, string method);

        string organisationID { get; }
    }

    interface IWebRequest
    {
        void SetRequestHeader(string name, string value);
        void SetPayload(byte[] payload, string payloadType);
        void SendWebRequest(Action<IWebRequest> onComplete);
        void Dispose();

        UnityWebRequest.Result result { get; }
        long responseCode { get; }
        string error { get; }
        string errorMessage { get; }
        string responseText { get; }
        byte[] responseByte { get; }
        byte[] uploadData { get; }
        string uploadDataType { get; }
        string info { get; }

        float downloadProgress { get; }
        ulong downloadBytes { get; }

        IWebRequest Recreate()
        {
            return null;
        }
    }
}