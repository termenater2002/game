using System;
using UnityEngine;

namespace Unity.Muse.Animate
{
    [Serializable]
    abstract class TakeModel : LibraryItemModel
    {

        protected TakeModel(
            string title,
            string description,
            LibraryItemType itemType,
            bool hasToBake = true)
            : base(
                title,
                description,
                itemType,
                hasToBake) { }

        public virtual void TrackRequest<T>(BakingRequest<T> request) where T : DenseTake { }

        protected void CopyTo(TakeModel item)
        {
            base.CopyTo(item);
        }
    }
}
