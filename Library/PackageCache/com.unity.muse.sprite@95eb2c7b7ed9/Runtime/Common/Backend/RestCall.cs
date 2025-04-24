using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Unity.Muse.Sprite.Common.Backend
{
    internal record BaseRequest
    {
        public string access_token;
        public string organization_id;
    }

    internal interface IQuarkEndpoint
    {
        public enum EMethod
        {
            GET,
            POST,
            PUT,
            DELETE,
            PATCH
        }

        public string server { get; }
        public string endPoint { get; }
        public EMethod method { get; }
    }
    internal abstract class QuarkRestCall : IDisposable
    {
        public enum EState
        {
            None,
            WaitingForDependency,
            InProgress,
            Completed,
            Error,
            Retrying,
            Forbidden
        }
        List<QuarkRestCall> m_Dependencies = new List<QuarkRestCall>();
        public IReadOnlyList<QuarkRestCall> dependencies => m_Dependencies;

        public event Action<QuarkRestCall> onCompleted = _ => { };


        public void SendRequest()
        {
            // we are already done
            if (isCompleted)
                onCompleted?.Invoke(this);
            if (m_Dependencies.Count > 0)
            {
                restCallState = EState.WaitingForDependency;
                for (int i = 0; i < m_Dependencies.Count; ++i)
                {
                    if (!m_Dependencies[i].isCompleted)
                    {
                        m_Dependencies[i].onCompleted += OnDependencyComplete;
                        m_Dependencies[i].SendRequest();
                    }
                }
            }
            else if (restCallState != EState.InProgress)
            {
                restCallState = EState.InProgress;
                MakeServerRequest();
            }

        }

        void OnDependencyComplete(QuarkRestCall dependency)
        {
            if (dependency.restCallState != EState.Retrying)
                dependency.onCompleted -= OnDependencyComplete;
            int i = 0;
            for (; i < m_Dependencies.Count; ++i)
            {
                if (!m_Dependencies[i].isCompleted)
                    break;
            }

            if ((restCallState == EState.None || restCallState == EState.WaitingForDependency) && i >= m_Dependencies.Count)
            {
                restCallState = EState.InProgress;
                MakeServerRequest();
            }

        }

        protected abstract void MakeServerRequest();

        public EState restCallState { get; set; }
        public bool isCompleted => restCallState == EState.Completed || restCallState == EState.Error;
        public bool isError => restCallState == EState.Error;
        public QuarkRestCall DependOn(QuarkRestCall call)
        {
            if (restCallState != EState.None)
            {
                //since we have started, we need to start the dependency as well
                call.onCompleted += OnDependencyComplete;
                call.SendRequest();
            }
            m_Dependencies.Add(call);
            return this;
        }

        protected void SignalRequestCompleted(EState state)
        {
            restCallState = state;
            onCompleted?.Invoke(this);
        }

        public virtual void Dispose()
        {
            for (int i = 0; i < m_Dependencies.Count; ++i)
            {
                m_Dependencies[i]?.Dispose();
            }

            m_Dependencies?.Clear();
            onCompleted = null;
        }
    }

    internal class QuarkRestCallJob : QuarkRestCall
    {
        List<QuarkRestCall> m_RestCalls = new List<QuarkRestCall>();
        protected override void MakeServerRequest()
        {
            if (m_RestCalls.Count == 0)
                SignalRequestCompleted(EState.Completed);
            else
            {
                for (int i = 0; i < m_RestCalls.Count; ++i)
                {
                    m_RestCalls[i].onCompleted += OnRestCallCompleted;
                    m_RestCalls[i].SendRequest();
                }
            }
        }

        public QuarkRestCallJob AddCall(QuarkRestCall call)
        {
            //if (isCompleted)
            //    throw new Exception("Request already in completed state");
            m_RestCalls.Add(call);
            // we are already in progress, start the call
            if (restCallState != EState.None && !call.isCompleted)
            {
                call.onCompleted += OnRestCallCompleted;
                call.SendRequest();
            }

            return this;
        }

        void OnRestCallCompleted(QuarkRestCall call)
        {
            //check if all calls are completed
            int i = 0;
            bool hasError = false;
            for (; i < m_RestCalls.Count; ++i)
            {
                if (!m_RestCalls[i].isCompleted)
                {
                    if (m_RestCalls[i].isError)
                        hasError = true;
                    break;
                }

            }

            if (i >= m_RestCalls.Count)
            {
                restCallState = hasError ? EState.Error : EState.Completed;
                SignalRequestCompleted(restCallState);
            }
        }
    }

    internal abstract class QuarkRestCall<TRequest, TResponse, TImplementorType> : QuarkRestCall, IQuarkEndpoint
        where TRequest : BaseRequest
        where TImplementorType : QuarkRestCall
    {
        UnityWebRequest.Result m_RequestResult;
        string m_RequestError;
        string m_ErrorMessage;
        TRequest m_Request;
        IWebRequest m_WebRequest;
        int m_Retries = 0;
        int m_MaxRetries = 1;
        float m_RetryDelay = 0.5f;
        long m_ResponseCode;
        public TRequest request
        {
            get => m_Request;
            protected set => m_Request = value;
        }

        public int retries
        {
            get => m_Retries;
            private set => m_Retries = value;
        }

        public int maxRetries
        {
            get => m_MaxRetries;
            set => m_MaxRetries = value;
        }

        protected float retryDelay
        {
            get => m_RetryDelay;
            set => m_RetryDelay = value;
        }

        public bool retriesFailed => m_Retries >= m_MaxRetries;
        public long responseCode => m_ResponseCode;
        public string requestError => m_RequestError;
        public string errorMessage => m_ErrorMessage;
        public UnityWebRequest.Result requestResult => m_RequestResult;
        protected IWebRequest webRequest => m_WebRequest;

        event Action<TImplementorType, TResponse> onSuccess = (_, __) => { };
        event Action<TImplementorType> onFailure = _ => { };
        event Action<TImplementorType> onCleanUp = _ => { };

        protected static string MakeEndPoint(IQuarkEndpoint asset)
        {
            return asset.server + asset.endPoint;
        }

        protected virtual string RequestLog()
        {
            return $"Request:{MakeEndPoint(this)} Payload:{JsonUtility.ToJson(request)}";
        }

        protected virtual string ResponseLog()
        {
            return $"Response:{MakeEndPoint(this)} Error:{m_RequestError} Result:{m_RequestResult}";
        }

        protected override void MakeServerRequest()
        {
            DebugConfig.DebugConfig.LogRequest(RequestLog);
            m_WebRequest = BackendUtilities.SendRequest(MakeEndPoint(this), request.access_token, request, OnRequestComplete, this.method.ToString());
        }

        public override void Dispose()
        {
            foreach (var dependency in dependencies)
            {
                if (dependency is QuarkRestCall<TRequest, TResponse, TImplementorType> quarkRestCall)
                    quarkRestCall.Dispose();
            }

            maxRetries = 0;
            onSuccess = null;
            onFailure = null;
            onCleanUp = null;
            m_WebRequest = null;
            base.Dispose();
        }

        protected virtual TResponse ParseResponse(IWebRequest response)
        {
            return JsonUtility.FromJson<TResponse>(response.responseText);
        }

        protected void OnRequestComplete(IWebRequest r)
        {
            try
            {
                m_RequestResult = r.result;
                if (r.result != UnityWebRequest.Result.Success)
                {
                    m_RequestError = r.error;
                    m_ErrorMessage = r.errorMessage;
                    m_ResponseCode = r.responseCode;
                    m_Retries++;

                    if (m_Retries < m_MaxRetries)
                    {
                        OnRetry();
                        Scheduler.ScheduleCallback(m_RetryDelay, () => MakeServerRequest());
                    }
                    else
                        OnError();
                }
                else
                {
                    m_Retries = 0;
                    OnSuccess(ParseResponse(r));
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                DebugConfig.DebugConfig.LogResonse(ResponseLog);
                SignalRequestCompleted(restCallState);
                CleanupRequest();
            }
        }

        void OnRetry()
        {
            restCallState = EState.Retrying;
            if (onFailure != null)
                onFailure(this as TImplementorType);
        }

        protected virtual void OnSuccess(TResponse response)
        {
            restCallState = EState.Completed;
            if (onSuccess != null)
                onSuccess(this as TImplementorType, response);
        }

        protected virtual void OnForbidden()
        {
            restCallState = EState.Forbidden;
            if (onFailure != null)
                onFailure(this as TImplementorType);
        }

        protected virtual void OnError()
        {
            restCallState = EState.Error;
            if (onFailure != null)
                onFailure(this as TImplementorType);
        }

        protected virtual void CleanupRequest()
        {
            if (onCleanUp != null)
                onCleanUp(this as TImplementorType);
        }

        public QuarkRestCall<TRequest, TResponse, TImplementorType> RegisterOnSuccess(Action<TImplementorType, TResponse> callback)
        {
            onSuccess += callback;
            return this;
        }

        public QuarkRestCall<TRequest, TResponse, TImplementorType> RegisterOnFailure(Action<TImplementorType> callback)
        {
            onFailure += callback;
            return this;
        }

        public QuarkRestCall<TRequest, TResponse, TImplementorType> RegisterOnCleanup(Action<TImplementorType> callback)
        {
            onCleanUp += callback;
            return this;
        }

        public abstract string server { get; }

        public string endPoint => endPoints[0];

        protected abstract string[] endPoints { get; }

        public IQuarkEndpoint.EMethod method
        {
            get
            {
                var version = ServerConfig.serverConfig.apiVersion;
                var methodsList = methods;
                var methodToUse = methodsList[0];
                if (version > 0 && (version - 1) < methods.Length)
                {
                    methodToUse = methodsList[version - 1];
                }
                return methodToUse;
            }
        }

        protected abstract IQuarkEndpoint.EMethod[] methods { get; }

        public string info => m_WebRequest?.info;
    }
}