using UnityEngine.Assertions;

namespace Unity.Muse.Animate
{
    static class AssertUtils
    {
        public static void Fail(string message)
        {
            throw new AssertionException(message, null);
        }
    }
}
