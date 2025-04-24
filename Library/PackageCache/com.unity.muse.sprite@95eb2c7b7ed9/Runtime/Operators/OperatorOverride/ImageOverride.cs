using System;
using Unity.Muse.Sprite.Common.Events;
using UnityEngine;

namespace Unity.Muse.Sprite.Operators
{
    internal class OperatorReferenceImageOverride : BaseOperatorDataOverride<OperatorReferenceImageOverride>
    {
        public byte[] bytes;
    }

    internal class OperatorDoodleImageOverride : BaseOperatorDataOverride<OperatorDoodleImageOverride>
    {
        public byte[] bytes;
    }

    internal class OperatorMaskImageOverride : BaseOperatorDataOverride<OperatorMaskImageOverride>
    {
        public byte[] bytes;
    }
}