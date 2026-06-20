using System.Collections.Generic;
using UnityEngine;

namespace qtLib.Extension
{
    public static partial class qtGameExtension
    {
        #region ----- Defines -----

        public enum EColor
        {
            GreenText,
            RedText,
            TextWhite,
            Common,
            Rare,
            Epic,
            Legend,
        }

        #endregion

        #region ----- Private Function -----

        private static Dictionary<EColor, string> _colorKey = new Dictionary<EColor, string>()
        {
            {EColor.RedText, "ff676e"},
            {EColor.GreenText, "65ef52"},
            {EColor.TextWhite, "ffffff"},
            {EColor.Common, "A9B7B7"},
            {EColor.Rare, "84CE5B"},
            {EColor.Epic, "E274D3"},
            {EColor.Legend, "F3BE0D"},
            
        };
        private static Dictionary<EColor, Color> _colors = new Dictionary<EColor, Color>();

        #endregion

        #region ----- Public Function -----

        public static Color HexToUnityColor(string hexColor)
        {
            string hexKey = hexColor;
            
            // Remove '#' if it's in the string
            if (hexKey.StartsWith("#"))
            {
                hexKey = hexKey.Substring(1);
            }

            // Parse the hexadecimal color components
            int red = int.Parse(hexKey.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            int green = int.Parse(hexKey.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            int blue = int.Parse(hexKey.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            // Normalize the values to range 0-1
            float r = (float)red / 255f;
            float g = (float)green / 255f;
            float b = (float)blue / 255f;

            Color color = new Color(r, g, b);
            return color;
        }

        public static Color HexToUnityColor(EColor hexColor)
        {
            if (_colors.TryGetValue(hexColor, out Color color))
            {
                return color;
            }

            string hexKey = _colorKey[hexColor];
            
            // Remove '#' if it's in the string
            if (hexKey.StartsWith("#"))
            {
                hexKey = hexKey.Substring(1);
            }

            // Parse the hexadecimal color components
            int red = int.Parse(hexKey.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            int green = int.Parse(hexKey.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            int blue = int.Parse(hexKey.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            // Normalize the values to range 0-1
            float r = (float)red / 255f;
            float g = (float)green / 255f;
            float b = (float)blue / 255f;

            color = new Color(r, g, b);
            _colors.TryAdd(hexColor, color);
            // Return the Unity Color
            return color;
        }

        public static string GetColorString(EColor hexColor)
        {
            return _colorKey[hexColor];
        }
        
        public static Color GetColor(EColor hexColor)
        {
            return HexToUnityColor(hexColor);
        }

        public static string SetColor(this string target, EColor color)
        {
            return $"<color=#{GetColorString(color)}>{target}</color>";
        }

        #endregion
    }
}
