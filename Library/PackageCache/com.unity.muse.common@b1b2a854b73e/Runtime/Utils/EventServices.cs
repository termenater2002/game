using System;
using UnityEngine;

namespace Unity.Muse.Common
{
    /// <summary>
    /// Class EventServices.
    /// </summary>
    static class EventServices
    {
        /// <summary>
        /// Interval debounce.
        ///
        /// Returns an action that will only do something after the given delay. If it's called multiple times, the delay is reset
        /// and the result will only be called once.
        /// </summary>
        /// <param name="func">The function.</param>
        /// <param name="seconds">The seconds.</param>
        /// <returns>An action</returns>
        public static Action IntervalDebounce(Action func, float seconds = 0.3f)
        {
            var lastEditorUpdateTime = Time.realtimeSinceStartup;
            var started = false;

            void OnUpdate()
            {
                var difference = Time.realtimeSinceStartup - lastEditorUpdateTime;
                if (difference >= seconds)
                {
                    func();
                    started = false;
                    lastEditorUpdateTime = Time.realtimeSinceStartup;
                }
                else
                    GenerativeAIBackend.context.RegisterNextFrameCallback(OnUpdate);
            }

            return () =>
            {
                if (started)
                    return;

                started = true;
                OnUpdate();
            };
        }
    }
}
