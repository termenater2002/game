using System;
using Unity.Muse.Sprite.Common.Events;
using UnityEngine.Events;

namespace Unity.Muse.Sprite.Operators
{
    internal class BaseOperatorDataOverride<T> : BaseEvent<T> where T : BaseEvent{ }

    interface IOperatorOverridePublisher
    {
        void RegisterOperatorOverrideEvent<T>(UnityAction<T> callback) where T : class,IEvent, new();
        void UnRegisterOperatorOverrideEvent<T>(UnityAction<T> callback) where T : class,IEvent, new();
        T RequestCurrentOperatorOverride<T>() where T : class,IEvent, new();
        bool SupportOverrideType<T>() where T : class,IEvent, new();
        string name { get; }
    }
}