using System;
using UnityEngine;

namespace Unity.Muse.Sprite.Common
{
    internal static class SchedulerGameObject
    {
        public class SchedulerGO : MonoBehaviour
        {
            public System.Action<GameObject> onDestroyingComponent
            {
                get;
                set;
            }
            void OnDestroy() => onDestroyingComponent?.Invoke(gameObject);
            void FixedUpdate()
            {
                Scheduler.ScheduleTick();
            }
        }
    }
}
