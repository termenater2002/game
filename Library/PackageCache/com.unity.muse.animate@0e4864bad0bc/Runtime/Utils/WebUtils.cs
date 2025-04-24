using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Unity.DeepPose.Cloud;

using UnityEngine;

namespace Unity.Muse.Animate
{
    static class WebUtils
    {
        public static void SendRequestWithAuthHeaders<TRequest, TResponse>(this WebAPI webAPI,
            TRequest request,
            Action<TResponse> onSuccess,
            Action<Exception> onError,
            string method = "post")
            where TRequest : ISerializable
            where TResponse : IDeserializable<JObject>, new()
        {
            // Note: Since some of these calls are from jobs,
            // they can happen too late after the app was closed.
            // Therefore, we check if the Application is still available.
            if (!Application.IsInitialized)
                return;
            
            Application.Instance.GetAuthHeaders(headers =>
            {
                webAPI.SendRequest(request, onSuccess, onError, headers.ToList(), method);
            });
        }

        public static void SendRequestWithAuthHeaders<TRequest>(this WebAPI webAPI,
            TRequest request,
            Action<byte[]> onSuccess,
            Action<Exception> onError,
            string method = "post")
            where TRequest : ISerializable
        {
            // Note: Since some of these calls are from jobs,
            // they can happen too late after the app was closed.
            // Therefore, we check if the Application is still available.
            if (!Application.IsInitialized)
                return;
            
            Application.Instance.GetAuthHeaders(headers =>
            {
                webAPI.SendRequest(request, onSuccess, onError, headers.ToList(), method);
            });
        }

        public static async Task<TResponse> SendRequestAsync<TRequest, TResponse>(this WebAPI webAPI,
            TRequest request,
            List<KeyValuePair<string, string>> headers = null,
            string method = "get")
            where TRequest : ISerializable
            where TResponse : IDeserializable<JObject>, new()
        {
            var tcs = new TaskCompletionSource<TResponse>();
            webAPI.SendRequest<TRequest, TResponse>(request,
                tcs.SetResult,
                tcs.SetException,
                headers,
                method);
            return await tcs.Task;
        }
        
        public static async Task<TResponse> SendRequestWithAuthHeadersAsync<TRequest, TResponse>(this WebAPI webAPI,
            TRequest request,
            string method = "post")
            where TRequest : ISerializable
            where TResponse : IDeserializable<JObject>, new()
        {
            var tcs = new TaskCompletionSource<TResponse>();

            Application.Instance.GetAuthHeaders(headers =>
            {
                webAPI.SendRequest<TRequest, TResponse>(request, 
                    tcs.SetResult, 
                    tcs.SetException, 
                    headers.ToList(), method);
            });

            return await tcs.Task;
        }
        
        public static void PostFormWithAuthHeaders<TRequest, TResponse>(this WebAPI webAPI,
            TRequest request,
            Action<TResponse> onSuccess,
            Action<Exception> onError)
            where TRequest : ISerializableAsForm
            where TResponse : IDeserializable<JObject>, new()
        {
            Application.Instance.GetAuthHeaders(headers =>
            {
                webAPI.PostForm(request, onSuccess, onError, headers.ToList());
            });
        }

        public static async Task<TResponse> PostFormAsync<TRequest, TResponse>(this WebAPI webAPI,
            TRequest request,
            List<KeyValuePair<string, string>> headers = null)
            where TRequest : ISerializableAsForm
            where TResponse : IDeserializable<JObject>, new()
        {
            var tcs = new TaskCompletionSource<TResponse>();
            webAPI.PostForm<TRequest, TResponse>(request, tcs.SetResult, tcs.SetException, headers);
            return await tcs.Task;
        }
        
        public static async Task<TResponse> PostFormWithAuthHeadersAsync<TRequest, TResponse>(this WebAPI webAPI,
            TRequest request)
            where TRequest : ISerializableAsForm
            where TResponse : IDeserializable<JObject>, new()
        {
            var tcs = new TaskCompletionSource<TResponse>();

            Application.Instance.GetAuthHeaders(headers =>
            {
                webAPI.PostForm<TRequest, TResponse>(request, tcs.SetResult, tcs.SetException, headers.ToList());
            });

            return await tcs.Task;
        }

        public static async Task<TResponse> PostJobRequestFormAsync<TRequest, TResponse>(this WebAPI webAPI,
            TRequest request,
            int checkIntervalMs = 2000,
            List<KeyValuePair<string, string>> headers = null,
            CancellationToken cancellationToken = default)
            where TRequest : ISerializableAsForm
            where TResponse : IDeserializable<JObject>, new()
        {
            var jobInfo = await PostFormAsync<TRequest, JobAPI.Response>(webAPI, request, headers);
            if (jobInfo.Guid == null)
            {
                throw new Exception("No job GUID");
            }

            var statusJobAPI = new WebAPI(webAPI.HostAddress, $"{JobAPI.ApiStatusName}{jobInfo.Guid}");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var status = await statusJobAPI.SendRequestAsync<JobAPI.Request, JobAPI.Response>(new JobAPI.Request(), headers, "get");
                if (status.Status == JobAPI.JobStatus.done.ToString())
                {
                    break;
                }
                if (status.Status == JobAPI.JobStatus.failed.ToString())
                {
                    throw new Exception("Compute job failed");
                }
                await Task.Delay(checkIntervalMs, cancellationToken);
            }

            var resultJobAPI = new WebAPI(webAPI.HostAddress, $"{JobAPI.ApiStatusName}{jobInfo.Guid}{JobAPI.ApiResultName}");
            var jobResult = await resultJobAPI.SendRequestAsync<JobAPI.Request, TResponse>(new JobAPI.Request(), headers, "get");
            return jobResult;
        }
        
        public static async Task<TResponse> PostJobRequestFormWithAuthHeadersAsync<TRequest, TResponse>(this WebAPI webAPI,
            TRequest request,
            int checkIntervalMs = 2000,
            CancellationToken cancellationToken = default)
            where TRequest : ISerializableAsForm
            where TResponse : IDeserializable<JObject>, new()
        {
            var tcs = new TaskCompletionSource<Dictionary<string, string>>();

            Application.Instance.GetAuthHeaders(headers =>
            {
                tcs.SetResult(headers);
            });

            var headers = await tcs.Task;
            return await PostJobRequestFormAsync<TRequest, TResponse>(webAPI,
                request,
                checkIntervalMs,
                headers.ToList(),
                cancellationToken);
        }

        public static void SendJobRequestWithAuthHeaders<TRequest, TResponse>(this WebAPI webAPI,
            TRequest request,
            Action<TResponse> onSuccess,
            Action<Exception> onError,
            string method = "post")
            where TRequest : ISerializable
            where TResponse : IDeserializable<JObject>, new()
        {
            // Note: Since some of these calls are from jobs,
            // they can happen too late after the app was closed.
            // Therefore, we check if the Application is still available.
            if (!Application.IsInitialized)
                return;
            
            WebAPI statusJobAPI = null;
            WebAPI resultJobAPI = null;
            string jobGuid = "";

            Application.Instance.GetAuthHeaders(headers =>
            {
                webAPI.SendRequest<TRequest, JobAPI.Response>(
                    request,
                    HandleJobRequest,
                    onError,
                    headers.ToList(),
                    method
                );
            });

            async void HandleJobRequest(JobAPI.Response response) {
                if (response.Guid != null) {
                    if (response.Status == null) {
                        jobGuid = response.Guid;

                        statusJobAPI = new WebAPI(webAPI.HostAddress, $"{JobAPI.ApiStatusName}{jobGuid}");
                        resultJobAPI = new WebAPI(webAPI.HostAddress, $"{JobAPI.ApiStatusName}{jobGuid}{JobAPI.ApiResultName}");
                    } else {
                        if (response.Status == JobAPI.JobStatus.done.ToString()) {
                            resultJobAPI.SendRequestWithAuthHeaders<JobAPI.Request, TResponse>(new JobAPI.Request(),
                                onSuccess,
                                onError,
                                "get");

                            return;
                        } else if (response.Status == JobAPI.JobStatus.failed.ToString()) {
                            onError?.Invoke(new Exception("Compute job failed"));

                            return;
                        }
                    }

                    await Task.Delay(500);

                    statusJobAPI.SendRequestWithAuthHeaders<JobAPI.Request, JobAPI.Response>(new JobAPI.Request(),
                        response => HandleJobRequest(response),
                        onError,
                        "get");
                } else {
                    onError?.Invoke(new Exception("No job GUID"));
                }
            }
        }

        public static string BackendUrl => Locator.TryGet(out IBackendSettings settings)
            ? settings.Url
            : ApplicationConstants.CloudInferenceHost;
    }
}
