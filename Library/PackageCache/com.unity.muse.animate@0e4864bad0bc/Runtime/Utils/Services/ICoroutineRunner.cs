using System.Collections;

namespace Unity.Muse.Animate
{
    interface ICoroutineRunner
    {
        public void StartCoroutine(IEnumerator routine);
    }
}
