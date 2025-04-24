using System;

namespace Unity.Muse.Animate
{
    interface ICopyable<T>
    {
        void CopyTo(T target);
        T Clone();
    }
}
