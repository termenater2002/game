using System.Collections.Generic;
using Unity.Muse.Common.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace Unity.Muse.Common
{
    /// <summary>
    /// A loadable image whose source is a URL, with brute force caching (the cache is only invalidated during a domain reload)
    /// </summary>
    internal class WebImage : LoadableImage
    {
        private static readonly Dictionary<string, Texture2D> s_Cache = new();
        private string m_Url;

        UnityWebRequestAsyncOperation m_CurrentOp;

        public string url
        {
            get => m_Url;
            set
            {
                if (m_Url == value)
                    return;
                
                m_Url = value;
                if (value != null && s_Cache.TryGetValue(value, out var t) && t)
                {
                    OnLoaded(t, ImageDisplay.BackgroundImage);
                    return;
                }
                
                OnLoading();
                Load();
            }
        }

        public WebImage(string url)
        {
            AddToClassList("web-image");
            this.url = url;
        }

        void Load()
        {
            Abort();
            if (string.IsNullOrEmpty(url))
            {
                OnLoaded(null, ImageDisplay.BackgroundImage);
            }
            else
            {
                var req = UnityWebRequestTexture.GetTexture(url);
                m_CurrentOp = req.SendWebRequest();
                if (m_CurrentOp.isDone)
                    OnTextureWebRequestCompleted(m_CurrentOp);
                else
                    m_CurrentOp.completed += OnTextureWebRequestCompleted;
            }
        }
        
        void OnTextureWebRequestCompleted(AsyncOperation operation)
        {
            var webOp = (UnityWebRequestAsyncOperation)operation;
            if (webOp.webRequest.result != UnityWebRequest.Result.Success)
            {
                OnError(webOp.webRequest.error);
            }
            else
            {
                var texture = DownloadHandlerTexture.GetContent(webOp.webRequest);
                if(texture)
                    s_Cache[url] = texture;
                OnLoaded(texture, ImageDisplay.BackgroundImage);
            }
        }

        public void Abort()
        {
            if (m_CurrentOp != null)
            {
                m_CurrentOp.completed -= OnTextureWebRequestCompleted;
                m_CurrentOp.webRequest.Abort();
            }
            
            m_CurrentOp = null;
            GenericLoader.SetState(GenericLoader.State.None);
            GenericLoader.SetDisplay(this, false);
        }
    }
}
