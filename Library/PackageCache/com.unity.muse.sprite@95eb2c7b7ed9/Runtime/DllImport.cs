#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace Unity.Muse.Sprite
{
    internal static class DllImport
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    public static extern void DownloadFile(byte[] array, int byteLength, string fileName);
#endif
    }
}
