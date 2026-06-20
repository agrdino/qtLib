using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace qtLib.Helper
{
    public static class qtDebug
    {
        private static string Tag = "qtLog";
        [Conditional("ENABLE_LOG")]
        public static void Log(object msg)
        {
            Debug.unityLogger.Log(Tag, msg);
        }

        [Conditional("ENABLE_LOG")]
        public static void LogError(object msg)
        {
            Debug.unityLogger.LogError(Tag, msg);
        }

        [Conditional("ENABLE_LOG")]
        public static void LogWarning(object msg)
        {
            Debug.unityLogger.LogWarning(Tag, msg);
        }
    }
}