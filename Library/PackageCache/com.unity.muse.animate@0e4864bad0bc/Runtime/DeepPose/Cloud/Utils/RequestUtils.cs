using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Muse.Animate;
using UnityEngine;
using UnityEngine.Networking;

namespace Unity.DeepPose.Cloud
{
    static class RequestUtils
    {
        private static string _sessionId = System.Guid.NewGuid().ToString();

        class AcceptAllCertificates : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData)
            {
                // Always accept certificate
                return true;
            }
        }

        static JsonSerializerSettings s_SerializationSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Include,
            MaxDepth = 50,
            ContractResolver = new UnityContractResolver()
        };

        public static void SendRequest(string apiUrl,
            object data,
            Action<JObject> onSuccess,
            Action<string> onFail,
            List<KeyValuePair<string, string>> headers = null,
            string method = "post")
        {
            var request = CreateRequest(apiUrl, data, headers, method);
            request.certificateHandler = new AcceptAllCertificates();

            var asyncOp = request.SendWebRequest();
            asyncOp.completed += operation =>
            {
                if (asyncOp.webRequest.result != UnityWebRequest.Result.Success)
                {
                    DevLogger.LogError($"Request failed [{apiUrl}]: {asyncOp.webRequest.error}");
                    onFail?.Invoke(asyncOp.webRequest.error);
                }
                else
                {
                    try
                    {
                        var result = asyncOp.webRequest.downloadHandler.text;
                        var jsonObject = JObject.Parse(result);
                        onSuccess?.Invoke(jsonObject);
                    }
                    catch (JsonReaderException e)
                    {
                        onFail?.Invoke(e.Message);
                    }
                }

                request.Dispose();
            };
        }

        public static void SendRequestResponseRaw(string apiUrl,
            object data,
            Action<byte[]> onSuccess,
            Action<string> onFail,
            List<KeyValuePair<string, string>> headers = null,
            string method = "post")
        {
            var request = CreateRequest(apiUrl, data, headers, method);
            request.certificateHandler = new AcceptAllCertificates();

            var asyncOp = request.SendWebRequest();
            asyncOp.completed += _ =>
            {
                if (asyncOp.webRequest.result != UnityWebRequest.Result.Success)
                {
                    DevLogger.LogError($"Request failed [{apiUrl}]: {asyncOp.webRequest.error}");
                    onFail?.Invoke(asyncOp.webRequest.error);
                }
                else
                {
                    var result = asyncOp.webRequest.downloadHandler.data;
                    onSuccess?.Invoke(result);
                }

                request.Dispose();
            };
        }

        
        static UnityWebRequest CreateRequest(string apiUrl,
            object data,
            List<KeyValuePair<string, string>> headers = null,
            string method = "post")
        {
#if UNITY_2022_3_OR_NEWER
            var www = (method == "post") ? UnityWebRequest.PostWwwForm(apiUrl, "") : UnityWebRequest.Get(apiUrl);
#else
            var www = UnityWebRequest.Post(apiUrl, "");
#endif
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Deep-Pose-Session-Id", RequestUtils._sessionId);
            
            if(headers != null)
            {
                headers.ForEach(delegate (KeyValuePair<string, string> header) {
                    www.SetRequestHeader(header.Key, header.Value);
                });
            }

            if (method != "get")
            {
                var jsonString = JsonConvert.SerializeObject(data, Formatting.None, s_SerializationSettings);
                www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonString));
            }

            www.downloadHandler = new DownloadHandlerBuffer();

            return www;
        }

        public static void PostForm(string apiEndPoint,
            ISerializableAsForm data,
            Action<JObject> onSuccess,
            Action<string> onFail,
            List<KeyValuePair<string, string>> headers)
        {
            var request = UnityWebRequest.Post(apiEndPoint, data.Serialize());
            request.certificateHandler = new AcceptAllCertificates();

            headers.ForEach(delegate (KeyValuePair<string, string> header) {
                request.SetRequestHeader(header.Key, header.Value);
            });

            var asyncOp = request.SendWebRequest();
            asyncOp.completed += operation =>
            {
                if (asyncOp.webRequest.result != UnityWebRequest.Result.Success)
                {
                    DevLogger.LogError($"Request failed [{apiEndPoint}]: {asyncOp.webRequest.error}");
                    onFail?.Invoke(asyncOp.webRequest.error);
                }
                else
                {
                    try
                    {
                        var result = asyncOp.webRequest.downloadHandler.text;
                        var jsonObject = JObject.Parse(result);
                        onSuccess?.Invoke(jsonObject);
                    }
                    catch (JsonReaderException e)
                    {
                        onFail?.Invoke(e.Message);
                    }
                }

                request.Dispose();
            };
        }
    }
}