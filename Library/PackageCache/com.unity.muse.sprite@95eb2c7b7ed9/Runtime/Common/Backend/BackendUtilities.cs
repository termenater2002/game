using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Unity.Muse.Common;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Unity.Muse.Sprite.Common.Backend
{
    static class BackendUtilities
    {
        struct RequestQueue
        {
            public IWebRequest request;
            public Action<IWebRequest> onDone;
        }
        static Queue<RequestQueue> s_Requests = new();
        const int k_MaxRequest = 10;
        const int k_ThrottleIncrease = 5;
        const int k_ThrottleDecrease = 1;
        static float s_Throttle = 0;
        public static float delayCall = 1;
        static bool errorOnUnimplemented = true;

        public static IWebRequest SendRequest<T>(string url, string accessToken, T data, Action<IWebRequest> onDone, string method = "POST")
        {
            if (!ServerConfig.serverConfig || ServerConfig.serverConfig.server == null)
                return null; // server has shut down
            var request = ServerConfig.serverConfig.server.CreateWebRequest(url, method);
            if (request == null)
            {
                var notImplementedException = new NotImplementedException($"{url} is not implemented.");
                if (errorOnUnimplemented)
                    throw notImplementedException;
                Debug.LogException(notImplementedException);
            }

            if (!string.IsNullOrEmpty(accessToken))
                request.SetRequestHeader("authorization", $"Bearer {accessToken}");
            if (method == "POST" || method == "PUT")
            {
                var requestJSON = JsonUtility.ToJson(data);
                request.SetRequestHeader("content-type", "application/json; charset=UTF-8");
                request.SetPayload(Encoding.UTF8.GetBytes(requestJSON), "application/json");
            }

            SendRequest(request, onDone);
            return request;
        }

        static void SendRequest(IWebRequest request, Action<IWebRequest> onDone)
        {
            if (RequestCounter.Counter != 0 && (RequestCounter.Counter >= k_MaxRequest || s_Throttle > 0))
            {
                s_Requests.Enqueue(new RequestQueue
                {
                    request = request,
                    onDone = onDone
                });
                if(!Scheduler.IsCallScheduled(SendNextRequest))
                    Scheduler.ScheduleCallback(ServerConfig.serverConfig.webRequestPollRate, SendNextRequest);
            }
            else
            {
                var requestCounter = new RequestCounter();
                // we are sending all calls 1 second later in case there is a domain reload during startup.
                Scheduler.ScheduleCallback(delayCall, () =>
                {
                    request.SendWebRequest(x =>
                    {
                        var dispose = false;
                        try
                        {
                            requestCounter.Dispose();
                            if (x.responseCode == 429)
                            {
                                // we need to wait for a while and try again.
                                s_Requests.Enqueue(new RequestQueue()
                                {
                                    request = request.Recreate(),
                                    onDone = onDone
                                });
                                s_Throttle += k_ThrottleIncrease;
                            }
                            else
                            {
                                s_Throttle = Mathf.Max(0, s_Throttle - k_ThrottleDecrease);
                                dispose = true;
                                onDone(request);
                            }
                        }
                        finally
                        {
                            if (dispose)
                                request.Dispose();
                            Scheduler.ScheduleCallback(ServerConfig.serverConfig.webRequestPollRate, SendNextRequest);
                        }
                    });
                });
            }
        }

        static void SendNextRequest()
        {
            if (s_Requests.Count > 0 && RequestCounter.Counter < k_MaxRequest)
            {
                var newRequest = s_Requests.Dequeue();
                SendRequest(newRequest.request, newRequest.onDone);
            }
        }

        // method is only used by tests
        internal static IEnumerator WaitForEmptyBackend(float timeout = 30f)
        {
            errorOnUnimplemented = false;
            try
            {
                var start = DateTime.UtcNow;
                var timePassedInSeconds = 0f;

                static bool Predicate()
                {
                    return RequestCounter.Counter == 0;
                }

                while (!Predicate() && timePassedInSeconds < timeout)
                {
                    var end = DateTime.UtcNow;
                    var timeDiff = end - start;
                    timePassedInSeconds = (float)timeDiff.TotalSeconds;

                    yield return null;
                }

                if (!Predicate())
                {
                    Debug.LogWarning("Backend not empty after " + timeout + " seconds.");
                }
            }
            finally
            {
                errorOnUnimplemented = true;
            }
        }

        public static Texture2D CreateTemporaryDuplicate(Texture2D original, int width, int height, TextureFormat format = TextureFormat.RGBA32)
        {
            //if (!ShaderUtil.hardwareSupportsRectRenderTexture || !(bool)(UnityEngine.Object)original)
            if (original == null)
                return null;
            var active = RenderTexture.active;
            var temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(original, temporary);
            RenderTexture.active = temporary;
            var flag = width >= SystemInfo.maxTextureSize || height >= SystemInfo.maxTextureSize;
            var temporaryDuplicate = new Texture2D(width, height, format, original.mipmapCount > 1 || flag);
            temporaryDuplicate.ReadPixels(new Rect(0.0f, 0.0f, (float)width, (float)height), 0, 0);
            temporaryDuplicate.Apply();
#if UNITY_EDITOR
            temporaryDuplicate.alphaIsTransparency = original.alphaIsTransparency;
#endif
            RenderTexture.active = active;
            RenderTexture.ReleaseTemporary(temporary);
            return temporaryDuplicate;
        }

        public static Texture2D SaveTexture2DToFile(string fileName, Texture2D texture)
        {
#if UNITY_EDITOR
            if (texture != null)
            {
                if (!texture.isReadable)
                    texture = CreateTemporaryDuplicate(texture, texture.width, texture.height);
                var savedLocation = SaveBytesToFile(fileName, texture.EncodeToPNG());
                return AssetDatabase.LoadAssetAtPath<Texture2D>(savedLocation);
            }
#endif
            return null;
        }

        public static string SaveBytesToFile(string fileName, byte[] bytes)
        {
#if UNITY_EDITOR
            var f = AssetDatabase.GenerateUniqueAssetPath(fileName);
            File.WriteAllBytes(f, bytes);
            AssetDatabase.ImportAsset(f, ImportAssetOptions.Default);
            return f;
#else
            return string.Empty;
#endif

        }

        public static void LogFile(string filename, string log)
        {
#if UNITY_EDITOR
            File.AppendAllText(filename, log);
#endif
        }

        public static Texture2D SpriteAsTexture(UnityEngine.Sprite sprite)
        {
            var texture = sprite.texture;
            Matrix4x4 transform = Matrix4x4.identity;
            var uvs = sprite.GetVertexAttribute<Vector2>(VertexAttribute.TexCoord0);
            Vector2[] vertices = sprite.vertices;
            var triangles = sprite.triangles;
            Vector2 pivot = sprite.pivot;
            var spriteWidth = sprite.rect.width;
            var spriteHeight = sprite.rect.height;

            var restoreRT = RenderTexture.active;
            var renderTexture = new RenderTexture((int)sprite.rect.width, (int)sprite.rect.height, 0, RenderTextureFormat.ARGB32);

            RenderTexture.active = renderTexture;
            var temporary = RenderTexture.GetTemporary(renderTexture.descriptor);
            var copyMaterial = new Material(Shader.Find("Hidden/BlitCopy"));
            copyMaterial.mainTexture = texture;
            copyMaterial.mainTextureScale = Vector2.one;
            copyMaterial.mainTextureOffset = Vector2.zero;
            copyMaterial.SetPass(0);
            GL.Clear(true, true, new Color(1f, 1f, 1f, 0f));
            GL.PushMatrix();
            GL.LoadOrtho();
            GL.Begin(GL.TRIANGLES);
            Color color = Color.white;
            float pixelsToUnits = sprite.rect.width / sprite.bounds.size.x;
            for (int i = 0; i < triangles.Length; ++i)
            {
                ushort index = triangles[i];
                Vector3 vertex = vertices[index];
                vertex = transform.MultiplyPoint(vertex);
                Vector2 uv = uvs[index];
                GL.Color(color);
                GL.TexCoord(new Vector3(uv.x, uv.y, 0));
                GL.Vertex3((vertex.x * pixelsToUnits + pivot.x) / spriteWidth, (vertex.y * pixelsToUnits + pivot.y) / spriteHeight, 0);
            }
            GL.End();
            GL.PopMatrix();

            Texture2D copy = new Texture2D((int)spriteWidth, (int)spriteHeight, TextureFormat.RGBA32, false);
            copy.hideFlags = HideFlags.HideAndDontSave;
            copy.filterMode = texture != null ? texture.filterMode : FilterMode.Point;
            copy.anisoLevel = texture != null ? texture.anisoLevel : 0;
            copy.wrapMode = texture != null ? texture.wrapMode : TextureWrapMode.Clamp;
            copy.ReadPixels(new Rect(0, 0, spriteWidth, spriteHeight), 0, 0);
            copy.Apply();
            RenderTexture.ReleaseTemporary(temporary);

            RenderTexture.active = restoreRT;
            copyMaterial.SafeDestroy();
            renderTexture.SafeDestroy();
            return copy;
        }

        sealed class RequestCounter : IDisposable
        {
            static int s_Counter;

            public static int Counter => s_Counter;

            public RequestCounter()
            {
                Interlocked.Increment(ref s_Counter);
            }

            bool m_Disposed;

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            void Dispose(bool disposing)
            {
                if (!m_Disposed)
                {
                    if (disposing)
                    {
                        Interlocked.Decrement(ref s_Counter);
                    }

                    m_Disposed = true;
                }
            }

            ~RequestCounter()
            {
                Dispose(false);
            }
        }

    }
}
