using System;
using System.Threading.Tasks;
using UnityEngine;

#pragma warning disable 1998

namespace Unity.Muse.Common
{
    static class AsyncUtils
    {
        public static async void SafeExecute(Func<Task> task)
        {
            SafeExecute(task());
        }

        /// <summary>
        /// Run an async task while ensuring to catch errors.
        /// </summary>
        public static async void SafeExecute(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        public static Task<T> SafeExecute<T>(Action<TaskCompletionSource<T>> callback)
        {
            var tcs = new TaskCompletionSource<T>();
            try
            {
                callback(tcs);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            return tcs.Task;
        }
    }
}
