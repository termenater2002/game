namespace Unity.Muse.Common
{
    static class StringExtensions
    {
        /// <summary>
        /// Get the nth index of the given character in a string.
        /// </summary>
        /// <param name="str">The string value.</param>
        /// <param name="element">The char to find in the string.</param>
        /// <param name="nth">A value greater than 0.</param>
        /// <returns>The nth index.</returns>
        internal static int IndexOfNth(this string str, char element, int nth)
        {
            if (nth <= 0 || str == null)
                return -1;

            for (var i = 0; i < str.Length; i++)
            {
                if (str[i] == element)
                {
                    nth--;
                    if (nth == 0)
                        return i;
                }
            }

            return -1;
        }
    }
}
