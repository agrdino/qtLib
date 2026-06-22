using System;
using UnityEngine;

namespace qtLib.Extension
{
    public partial class qtGameExtension
    {
        public static string NumberSuffixNotation(this int num)
        {
            if (num >= 1000000)
            {
                return (num / 1000000).ToString("0.#") + "m";
            }
            if (num >= 10000)
            {
                return (num / 1000).ToString("0.#") + "k";
            }
            return num.ToString();
        }

        public static string FormatNumber(this double num)
        {
            if (Math.Abs(num % 1) < double.Epsilon)
            {
                return num.ToString("N0"); // Số nguyên
            }
            else
            {
                return num.ToString("N2"); // Số thập phân
            }
        }
        
        public static string FormatNumber(this float num)
        {
            return ((double)num).FormatNumber();
        }
        
        public static string FormatNumber(this long num)
        {
            return ((double)num).FormatNumber();
        }
        
        public static float RoundToNearest(this float value, float step)
        {
            return (Mathf.Round(value / step) * step);
        }
    }
}