using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Muse.Common.Account;
using Unity.Muse.Common.Api;
using UnityEngine.Networking;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.Muse.Common
{
    class GenerativeAIBackend
    {
        internal static bool skipErrorLogs;

        internal static event Func<long, string, bool> OnServerError;

        internal static class StatusEnum
        {
            public const string waiting = "waiting";
            public const string working = "working";
            public const string done = "done";
            public const string failed = "failed";
        }

        internal enum GeneratorModel
        {
            StableDiffusionV_1_4 = 14,
            //StableDiffusionV_1_5 = 15,
            StableDiffusionV_2_1 = 21
        }

        public delegate void ArtifactProgressCallback(string guid,
                                                        string statusEnum,
                                                        float progress,
                                                        string errorMsg);

        protected static string AccessToken =>
            //TODO: Fix this up when we can get access tokens from cloudlab canvas outside of the editor
#if UNITY_EDITOR
                                    CloudProjectSettings.accessToken;
#else
                                    GameObject.Find("App").GetComponent<RuntimeCloudContext>().accessToken;
#endif

        internal static TexturesUrl TexturesUrl => new() {orgId = AccountInfo.Instance.Organization?.Id};

        static ICloudContext s_ContextInstance;
        internal static ICloudContext context => s_ContextInstance ??= CloudContextFactory.GetCloudContext();

        [Serializable]
        internal class DownloadURLResponse
        {
            public bool success;
            public string url;
        }

        /// <summary>
        /// Download texture image from the Cloud
        /// </summary>
        /// <param name="artifact">The typed artifact identifier to request</param>
        /// <param name="onDone">Callback called when results are received. Callback parameters (Texture2D, byte[], string)
        ///                     represent received Texture2D object, it's original byte stream as PNG file and error string. In case error occured
        ///                     error string is non-null and other parameters are null</param>
        /// <returns>The the reference to the async operation this generates so that it may be cancelled</returns>
        public static UnityWebRequestAsyncOperation DownloadArtifact<TArtifactType>(Artifact<TArtifactType> artifact,  Action<object, string> onDone)
        {
            void HandleRequest(object data, string error)
            {
                var jsonData = JsonUtility.FromJson<DownloadURLResponse>(Encoding.UTF8.GetString((byte[])data));
                DownloadImageRequest(jsonData.url, onDone);
            }

            return SendGetRequest(TexturesUrl.textureAssets(artifact.Guid), null, HandleRequest);
        }

        public static UnityWebRequestAsyncOperation GetArtifactDownloadUrl(string serviceBaseURL, string assetId, Action<DownloadURLResponse, string> onDownloadUrlFetched)
        {
            void HandleRequest(object data, string error)
            {
                if (!string.IsNullOrEmpty(error) || data == null)
                {
                    // Handle cases where there is an error or the data is null
                    onDownloadUrlFetched(null, error ?? "Response data is null");
                    return;
                }

                try
                {
                    // Try to deserialize the data
                    var jsonData = JsonUtility.FromJson<DownloadURLResponse>(Encoding.UTF8.GetString((byte[])data));
                    onDownloadUrlFetched(jsonData, null); // Pass null as error if successful
                }
                catch (Exception ex)
                {
                    // Handle deserialization errors
                    onDownloadUrlFetched(null, $"Deserialization error: {ex.Message}");
                }
            }
            return SendGetRequest($"{serviceBaseURL}/{assetId}", null, HandleRequest);
        }

        public static UnityWebRequestAsyncOperation UploadDataRequest(string uploadURL, byte[] uploadData, Action<object, string> onDone)
        {
            var request = new UnityWebRequest(uploadURL, "PUT");
            request.uploadHandler = new UploadHandlerRaw(uploadData);
            request.uploadHandler.contentType = "application/octet-stream";
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("x-ms-blob-type", "BlockBlob"); // Required for Azure Blob Storage
            return SendRequest(request, onDone, false);
        }

        public static UnityWebRequestAsyncOperation GetArtifactUploadUrl(string serviceBaseURL, string fileExtension, int fileSize, Action<UploadResponse, string> onUploadUrlFetched)
        {
            void HandleRequest(object data, string error)
            {
                if (!string.IsNullOrEmpty(error) || data == null)
                {
                    // Handle cases where there is an error or the data is null
                    onUploadUrlFetched(null, error ?? "Response data is null");
                    return;
                }

                try
                {
                    // Try to deserialize the data
                    var jsonData = JsonUtility.FromJson<UploadResponse>(Encoding.UTF8.GetString((byte[])data));
                    onUploadUrlFetched(jsonData, null); // Pass null as error if successful
                }
                catch (Exception ex)
                {
                    // Handle deserialization errors
                    onUploadUrlFetched(null, $"Deserialization error: {ex.Message}");
                }
            }

            // Create the upload request
            var uploadRequest = new UploadRequest(fileExtension, fileSize);
            return SendJsonRequest(serviceBaseURL, uploadRequest, HandleRequest, "POST");
        }

        public static async Task<bool> CheckArtifactsStatusAsync(string serviceURL, List<string> activeGuids, ItemRequest itemData, Action<string, string> onStatusChange)
        {
            var tcs = new TaskCompletionSource<StatusResponse>();
#pragma warning disable 4014 // We don't want to await this task
            SendGetRequest(serviceURL, itemData, (object respData, string error) =>
            {
                // Handle error
                if (!string.IsNullOrEmpty(error))
                    tcs.SetResult(new()
                    {
                        results = activeGuids
                            .Where(guid => error.Contains(guid))
                            .Select(guid => new StatusResponseItem
                        {
                            guid = guid,
                            status = StatusEnum.failed
                        }).ToArray()
                    });
                else if (respData == null)
                    tcs.SetResult(null);        // Handle no response scenario
                else
                {
                    StatusResponse resp = JsonUtility.FromJson<StatusResponse>(Encoding.UTF8.GetString((byte[])respData));
                    tcs.SetResult(resp);
                }
            });
#pragma warning restore 4014

            bool anyDone = false;
            StatusResponse statusResponse = await tcs.Task;
            if (statusResponse == null)
                return anyDone;

            // Check each result and update status or call callbacks
            foreach (var result in statusResponse.results)
            {
                if (result.status == StatusEnum.done || result.status == StatusEnum.failed)
                {
                    onStatusChange?.Invoke(result.guid, result.status);
                    activeGuids.Remove(result.guid);
                    anyDone = true;
                }
            }
            return anyDone;
        }

        public static string GetBaseJobsUrl(string serviceBaseURL, List<string> guids, ItemRequest itemData = null)
        {
            if (itemData != null)
            {
                if (guids.Count > 1)
                    itemData.parameters = string.Join("&", guids.Skip(1).Select(guid => "job_ids=" + guid));
                else
                    itemData.parameters = null;
            }
            return $"{serviceBaseURL}/jobs/{guids[0]}";
        }

        /// <summary>
        /// Asynchronously polls the status of artifacts until a specified condition is met.
        /// </summary>
        /// <param name="guids">List of GUIDs for artifacts to check status.</param>
        /// <param name="pollInterval">Time in seconds to wait between polls.</param>
        /// <returns></returns>
        public static async Task MonitorArtifactStatusUntilCompletionAsync(string serviceBaseURL, List<string> guids, Action<string, string> onStatusChange, float pollInterval = 0f)
        {
            string serviceURL = null;
            ItemRequest itemData = null;

            List<string> activeGuids = new List<string>(guids);
            while (activeGuids.Any())
            {
                if (itemData == null)
                {
                    itemData = new ItemRequest();
                    serviceURL = GetBaseJobsUrl(serviceBaseURL, activeGuids, itemData);
                }

                bool anyDone = await CheckArtifactsStatusAsync(serviceURL, activeGuids, itemData, onStatusChange);
                if (anyDone)
                    itemData = null;

                if (pollInterval <= 0)
                    break;

                if (activeGuids.Any())
                    await Task.Delay((int)(pollInterval * 1000)); // Convert seconds to milliseconds
            }
        }

        public static UnityWebRequestAsyncOperation SendRequest(UnityWebRequest request, Action<object, string> onDone, bool checkDownloadBytes = true)
        {
            var stackTrace = new System.Diagnostics.StackTrace();

            void PollForRequestCompletion()
            {
                if (!request.isDone)
                {
                    context.RegisterNextFrameCallback(PollForRequestCompletion);
                    return;
                }
                if (!string.IsNullOrEmpty(request.error) || (checkDownloadBytes && request.downloadedBytes == 0))
                {
                    try
                    {
                        var errorMessage = $"Request failed: {request.method} {request.url} -- Failed to download because " +
                            (request.downloadedBytes == 0
                                ? $"response was empty: {request.error}"
                                : request.error + $"\n{request.downloadHandler?.text}");

                        if (request.responseCode >= 400 && errorMessage.Contains("Invalid or expired access_token"))
                        {
#if UNITY_EDITOR
                            UnityConnectUtils.ClearAccessToken();
                            UnityEditor.CloudProjectSettings.RefreshAccessToken(result =>
                            {
                                Debug.LogWarning("Access token has been refreshed. Please try your action again.");
                            });
                            errorMessage += " -- Trying to refresh access token. Please try again after token has been refreshed.";
#endif
                        }

                        if (request.error != "Request aborted")
                        {
                            var handled = OnServerError?.Invoke(request.responseCode, request.error) ?? false;
                            if (!handled && !skipErrorLogs)
                                Debug.LogError("url: "+ request.url + "\n" + errorMessage + "\nStack trace:\n" + stackTrace);
                        }

                        if (onDone != null && onDone.Target != null)
                            onDone(request.downloadHandler?.data, errorMessage);
                    }
                    finally
                    {
                        request.Dispose();
                    }
                }
                else
                {
                    try
                    {
                        byte[] data = request.downloadHandler.data;

                        if (onDone != null && onDone.Target != null)
                            onDone(data, null);
                    }
                    finally
                    {
                        request.Dispose();
                    }
                }
            }

            // Register the update event
            context.RegisterNextFrameCallback(PollForRequestCompletion);

            // Kick off the webrequest
            return request.SendWebRequest();
        }

        protected static UnityWebRequestAsyncOperation SendJsonRequest(string serviceURL, object requestBody, Action<object, string> onDone, string type = "POST")
        {
            var requestJson = JsonUtility.ToJson(requestBody);

            var request = new UnityWebRequest(serviceURL, type);
            request.SetRequestHeader("content-type", "application/json; charset=UTF-8");
            request.SetRequestHeader("authorization", $"Bearer {AccessToken}");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(requestJson));
            request.uploadHandler.contentType = "application/json";
            request.downloadHandler = new DownloadHandlerBuffer();

           return SendRequest(request, onDone);
        }

        public static UnityWebRequestAsyncOperation SendGetRequest(string serviceURL, ItemRequest data, Action<object, string> onDone)
        {
            var url = serviceURL;
            if (!string.IsNullOrEmpty(data?.parameters))
                url = serviceURL + "?" + data.parameters;

            var request = new UnityWebRequest(url, "GET");
            request.SetRequestHeader("content-type", "application/json; charset=UTF-8");
            request.SetRequestHeader("Authorization", $"Bearer {AccessToken}");
            request.downloadHandler = new DownloadHandlerBuffer();

            return SendRequest(request, onDone);
        }

        static UnityWebRequestAsyncOperation DownloadImageRequest(string imageURL, Action<object, string> onDone)
        {
            var request = UnityWebRequestTexture.GetTexture(imageURL);
            return SendRequest(request, onDone);
        }

        /// <summary>
        /// Generic request handler
        /// </summary>
        protected static Action<object, string> RequestHandler<T>(Action<T, string> callback) where T : class
        {
            return (data, error) =>
            {
                if (data != null && string.IsNullOrEmpty(error))
                {
                    var content = Encoding.UTF8.GetString((byte[]) data);
                    T result = null;
                    try
                    {
                        result = JsonUtility.FromJson<T>(content);
                    }
                    catch (Exception exception)
                    {
                        error = $"Error handling request: {content}\nException: {exception.Message}";
                    }

                    callback(result, error);
                    return;
                }

                callback(null, error);
            };
        }

        public static UnityWebRequestAsyncOperation GetEntitlements(Action<SubscriptionResponse, string> onDone)
        {
            return SendGetRequest($"{TexturesUrl.entitlements}?force_reload_cache=True", null, RequestHandler(onDone));
        }

        public static Task<(SubscriptionResponse, string)> GetEntitlements()
        {
            return AsyncUtils.SafeExecute<(SubscriptionResponse, string)>(tcs =>
            {
                GetEntitlements((response, error) => tcs.SetResult((response, error)));
            });
        }

        public static UnityWebRequestAsyncOperation GetStatus(ClientStatusRequest requestData, Action<ClientStatusResponse, string> onDone)
        {
            return SendGetRequest(TexturesUrl.status, requestData, RequestHandler(onDone));
        }

        public static UnityWebRequestAsyncOperation GetUsage(Action<UsageResponse, string> onDone)
        {
            return SendGetRequest(TexturesUrl.usage, null, RequestHandler(onDone));
        }

        public static UnityWebRequestAsyncOperation StartTrial(string orgId, Action<string> onDone)
        {
            return SendJsonRequest(TexturesUrl.startTrial(orgId), new(),
                RequestHandler<StartTrialResponse>((_, error) => onDone?.Invoke(error)));
        }

        public static Task<string> StartTrial(string orgId)
        {
            return AsyncUtils.SafeExecute<string>(tcs =>
            {
                StartTrial(orgId, error => tcs.SetResult(error));
            });
        }

        public static UnityWebRequestAsyncOperation GetLegalConsent(Action<LegalConsentResponse, string> onDone)
        {
            return SendGetRequest(TexturesUrl.legalConsent, null, RequestHandler(onDone));
        }

        public static Task<(LegalConsentResponse, string)> GetLegalConsent()
        {
            return AsyncUtils.SafeExecute<(LegalConsentResponse, string)>(tcs =>
            {
                GetLegalConsent((response, error) => tcs.SetResult((response, error)));
            });
        }

        public static UnityWebRequestAsyncOperation SetLegalConsent(
            LegalConsentRequest settings,
            Action<LegalConsentResponse, string> onDone)
        {
            return SendJsonRequest(TexturesUrl.legalConsent, settings,
                RequestHandler<LegalConsentResponse>((data, error) =>  onDone?.Invoke(data, error)), "PUT");
        }

        public static Task<(LegalConsentResponse, string)> SetLegalConsent(LegalConsentRequest settings)
        {
            return AsyncUtils.SafeExecute<(LegalConsentResponse, string)>(tcs =>
            {
                SetLegalConsent(settings, (response, error) => tcs.SetResult((response, error)));
            });
        }
    }
}