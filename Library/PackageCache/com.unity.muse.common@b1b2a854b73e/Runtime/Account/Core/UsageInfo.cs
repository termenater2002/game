using System;
using System.Globalization;
using UnityEngine;

namespace Unity.Muse.Common.Account
{
    [Serializable]
    record UsageInfo
    {
        public int used;
        public int total;
        public string Tooltip => $"{used.ToString("N0", CultureInfo.CurrentCulture.NumberFormat)} / {total.ToString("N0", CultureInfo.CurrentCulture.NumberFormat)}";
        public string Label => $"{PrettyFormat(used)} / {PrettyFormat(total)}";
        public float Progress => total == 0 ? 0 : (float) used / total;
        public bool CanExceed => DateTime.Now <= new DateTime(2024, 01, 15, 0, 0, 0, DateTimeKind.Local);
        static string PrettyFormat(long number)
        {
            string[] suf = { "", "K", "M", "B" };
            if (number == 0)
                return "0";
            var idx = Convert.ToInt32(Math.Floor(Math.Log(number, 1000)));
            idx = Math.Clamp(idx, 0, suf.Length - 1);
            var num = Math.Round(number / Math.Pow(1000, idx), 1);
            return $"{num.ToString(CultureInfo.CurrentCulture)}{suf[idx]}";
        }

        public bool Exceeded => CanExceed && used > total;
    }
}