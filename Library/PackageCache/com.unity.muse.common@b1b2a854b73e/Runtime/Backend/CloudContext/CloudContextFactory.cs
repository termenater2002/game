using System;
using Unity.Muse.Common;
using UnityEngine;

namespace Unity.Muse.Common
{
    internal class CloudContextFactory
    {
        static Type s_DefaultCloudContextType;
        static ICloudContext s_Context;

        public static void InjectDefaultCloudContextType()
        {
            InjectCloudContextType<DefaultCloudContext>();
        }

        public static void InjectCloudContextType<T>() where T : ICloudContext, new()
        {
            s_DefaultCloudContextType = typeof(T);
        }

        public static void SetCloudContext(ICloudContext context)
        {
            s_Context = context;

            if (s_Context == null)
            {
                // Always ensure something default is available if we clear this context
                ConstructDefaultContext();
            }
        }

        public static ICloudContext GetCloudContext()
        {
            if (s_DefaultCloudContextType == null)
                InjectDefaultCloudContextType();

            ConstructDefaultContext();

            return s_Context;
        }

        static void ConstructDefaultContext()
        {
            if (s_DefaultCloudContextType != null && s_Context == null)
            {
                s_Context = Activator.CreateInstance(s_DefaultCloudContextType) as ICloudContext;
            }
        }
    }
}
