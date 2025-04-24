using System;
using UnityEngine;

namespace Unity.Muse.Common
{
    class DefaultCloudContext : ICloudContext
    {
        void ICloudContext.RegisterNextFrameCallback(ICloudContext.Callback cb) => throw new System.NotImplementedException();
    }
}
