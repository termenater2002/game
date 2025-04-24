using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.Muse.Sprite.Common
{
    static class Scheduler
    {
        struct ScheduleCallbackObject
        {
            public float timer;
            public DateTime startTime;
            public Action callback;
        }
        static List<ScheduleCallbackObject> s_Schedules = new();
        static SchedulerGameObject.SchedulerGO m_SchedulerGO;

        public static bool IsCallScheduled(Action callback)
        {
            for (var i = 0; i < s_Schedules.Count; ++i)
            {
                if (s_Schedules[i].callback == callback)
                    return true;
            }

            return false;
        }

        public static void ScheduleCallback(float timer, Action callback)
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying)
            {
                if(m_SchedulerGO == null)
                {
                    var go = new GameObject("Scheduler");
                    go.hideFlags = HideFlags.HideAndDontSave;
                    m_SchedulerGO = go.AddComponent<SchedulerGameObject.SchedulerGO>();
                    m_SchedulerGO.onDestroyingComponent += OnHelperDestroyed;
                }
            }
            else
            {
                UnityEditor.EditorApplication.CallbackFunction callbackFunction = ScheduleTick;
                if (!UnityEditor.EditorApplication.update.GetInvocationList().Contains(callbackFunction))
                    UnityEditor.EditorApplication.update += ScheduleTick;
            }
#else
            if (m_SchedulerGO == null)
            {
                var go = new GameObject("Scheduler");
                go.hideFlags = HideFlags.HideAndDontSave;
                m_SchedulerGO = go.AddComponent<SchedulerGameObject.SchedulerGO>();
                m_SchedulerGO.onDestroyingComponent += OnHelperDestroyed;
            }
#endif
            if (timer <= 0)
            {
                callback?.Invoke();
            }
            else
            {
                s_Schedules.Add(new ScheduleCallbackObject()
                {
                    timer = timer,
                    callback = callback,
                    startTime = DateTime.Now
                });
            }
        }

        static void OnHelperDestroyed(GameObject obj)
        {
            m_SchedulerGO = null;
        }

        internal static void ScheduleTick()
        {
            var now = DateTime.Now;
            for (var i = 0; i < s_Schedules.Count; ++i)
            {
                if (s_Schedules[i].timer <= (now - s_Schedules[i].startTime).TotalSeconds)
                {
                    var callback = s_Schedules[i];
                    s_Schedules.RemoveAt(i);
                    callback.callback?.Invoke();
                    --i;
                }
            }
        }
    }
}
