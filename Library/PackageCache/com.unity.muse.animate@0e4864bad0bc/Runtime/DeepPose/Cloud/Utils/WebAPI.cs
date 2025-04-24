using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.DeepPose.Cloud
{
    /// <summary>
    /// A type that is convertible to a json-serializable object.
    /// </summary>
    interface ISerializable
    {
        object Serialize();
    }

    /// <summary>
    /// A type that is convertible to a multipart form.
    /// </summary>
    interface ISerializableAsForm
    {
        WWWForm Serialize();
    }

    /// <summary>
    /// A type that is convertible from a json-serializable object.
    /// </summary>
    /// <typeparam name="TData">A type that is deserialized from JSON</typeparam>
    interface IDeserializable<in TData>
    {
        void Deserialize(TData data);
    }
    
    class WebRequestException : Exception
    {
        public WebRequestException(string message) : base(message)
        {
        }
    }
    
    class ApiVersionMismatchException : Exception
    {
        public string ExpectedVersion { get; }
        public string ActualVersion { get; }
        
        public ApiVersionMismatchException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Convenience wrapper for <see cref="RequestUtils"/> operations.
    /// </summary>
    class WebAPI
    {
        readonly string m_ApiEndPoint;
        readonly string m_HostAddress;

        public string HostAddress => m_HostAddress;
        public string ApiEndPoint => m_ApiEndPoint;

        public WebAPI(string hostAddress, string apiName)
        {
            m_HostAddress = hostAddress;
            m_ApiEndPoint = $"{hostAddress.TrimEnd('/')}/{apiName.Trim('/')}";
        }

        public void SendRequest<TRequest, TResponse>(TRequest request,
            Action<TResponse> onSuccess,
            Action<Exception> onError,
            List<KeyValuePair<string, string>> headers = null,
            string method = "post")
            where TRequest : ISerializable
            where TResponse : IDeserializable<JObject>, new()
        {
            // Wrap connection-related errors
            var onErrorWrapper = new Action<string>(message => onError?.Invoke(new WebRequestException(message)));
            
            RequestUtils.SendRequest(m_ApiEndPoint, request.Serialize(), jsonObject =>
            {
                try
                {
                    var response = new TResponse();
                    response.Deserialize(jsonObject);
                    onSuccess?.Invoke(response);
                }
                catch (JsonSerializationException ex)
                {
                    // TODO: we need the backend to return some kind of version information, and we need to check
                    // it against the current version of the package.
                    onError?.Invoke(new ApiVersionMismatchException(ex.Message));
                }
                catch (Exception ex)
                {
                    onError?.Invoke(ex);
                }
            }, onErrorWrapper, headers, method);
        }

        public void SendRequest<TRequest>(TRequest request,
            Action<byte[]> onSuccess,
            Action<Exception> onError,
            List<KeyValuePair<string, string>> headers = null,
            string method = "post") where TRequest : ISerializable
        {
            // Wrap connection-related errors
            var onErrorWrapper = new Action<string>(message => onError?.Invoke(new WebRequestException(message)));
            
            RequestUtils.SendRequestResponseRaw(m_ApiEndPoint, request.Serialize(), data =>
                {
                    try
                    {
                        onSuccess?.Invoke(data);
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke(ex);
                    }
                },
                onErrorWrapper,
                headers,
                method);
        }

        public void PostForm<TRequest, TResponse>(TRequest request,
            Action<TResponse> onSuccess,
            Action<Exception> onError,
            List<KeyValuePair<string, string>> headers = null)
            where TRequest : ISerializableAsForm
            where TResponse : IDeserializable<JObject>, new()
        {
            // Wrap connection-related errors
            var onErrorWrapper = new Action<string>(message => onError?.Invoke(new WebRequestException(message)));
            RequestUtils.PostForm(m_ApiEndPoint, request, jsonObject =>
            {
                try
                {
                    var response = new TResponse();
                    response.Deserialize(jsonObject);
                    onSuccess?.Invoke(response);
                }
                catch (JsonSerializationException ex)
                {
                    // TODO: we need the backend to return some kind of version information, and we need to check
                    // it against the current version of the package.
                    onError?.Invoke(new ApiVersionMismatchException(ex.Message));
                }
                catch (Exception ex)
                {
                    onError?.Invoke(ex);
                }
            }, onErrorWrapper, headers);
        }
    }
}
