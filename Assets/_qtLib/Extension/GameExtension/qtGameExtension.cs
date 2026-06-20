using System;
using System.IO;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using UnityEngine;

namespace qtLib.Extension
{
    public static partial class qtGameExtension
    {
        public static string PathCombine(this string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        public static string CreateUniqueID()
        {
            return Guid.NewGuid().ToString();
        }

        public static float TakeValueWithEasing(float start, float end, float duration, Ease easing, float at)
        {
            // Evaluate easing (EaseManager trả về giá trị từ 0 -> 1 theo easing)
            float eased = EaseManager.Evaluate(easing, null, at, duration, overshootOrAmplitude: 1f,
                period: 0f);

            // Trả về giá trị nội suy giữa start và end theo eased
            return Mathf.Lerp(start, end, eased);
        }

        public static Vector3 TakeValueWithEasing(Vector3 start, Vector3 end, float duration, Ease easing, float at)
        {
            // Evaluate easing (EaseManager trả về giá trị từ 0 -> 1 theo easing)
            float eased = EaseManager.Evaluate(easing, null, at, duration, overshootOrAmplitude: 1f,
                period: 0f);
            
            // Trả về giá trị nội suy giữa start và end theo eased
            return Vector3.Lerp(start, end, eased);
        }

        public static string ToLocaleCode(this SystemLanguage lang)
        {
            switch (lang)
            {
                case SystemLanguage.English: return "en";
                case SystemLanguage.French: return "fr";
                case SystemLanguage.German: return "de";
                case SystemLanguage.Spanish: return "es";
                case SystemLanguage.Portuguese: return "pt";
                case SystemLanguage.Russian: return "ru";
                case SystemLanguage.Chinese: return "zh";
                case SystemLanguage.ChineseSimplified: return "zh-Hans";
                case SystemLanguage.ChineseTraditional: return "zh-Hant";
                case SystemLanguage.Japanese: return "ja";
                case SystemLanguage.Korean: return "ko";
                case SystemLanguage.Vietnamese: return "vi";
                case SystemLanguage.Thai: return "th";
                case SystemLanguage.Indonesian: return "id";
                case SystemLanguage.Italian: return "it";
                case SystemLanguage.Dutch: return "nl";
                case SystemLanguage.Polish: return "pl";
                case SystemLanguage.Turkish: return "tr";
                case SystemLanguage.Ukrainian: return "uk";
                case SystemLanguage.Czech: return "cs";
                case SystemLanguage.Danish: return "da";
                case SystemLanguage.Finnish: return "fi";
                case SystemLanguage.Norwegian: return "no";
                case SystemLanguage.Swedish: return "sv";
                case SystemLanguage.Greek: return "el";
                case SystemLanguage.Hebrew: return "he";
                case SystemLanguage.Arabic: return "ar";
                case SystemLanguage.Romanian: return "ro";
                case SystemLanguage.Hungarian: return "hu";
                case SystemLanguage.Slovak: return "sk";
                case SystemLanguage.Slovenian: return "sl";
                case SystemLanguage.Latvian: return "lv";
                case SystemLanguage.Lithuanian: return "lt";
                case SystemLanguage.Bulgarian: return "bg";
                case SystemLanguage.Catalan: return "ca";
                case SystemLanguage.Estonian: return "et";
                default: return "en"; // fallback
            }
        }
    }
}