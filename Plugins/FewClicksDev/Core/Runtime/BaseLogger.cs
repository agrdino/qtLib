namespace FewClicksDev.Core
{
    using UnityEngine;

    public static class BaseLogger
    {
        public static void Log(string _context, string _message)
        {
            Debug.Log($"{_context.ToUpper()} :: {_message}");
        }

        public static void Log(string _context, string _message, Color _color)
        {
            Debug.Log($"<color={_color.GetHexString()}>{_context.ToUpper()}</color> :: {_message}");
        }

        public static void Log(string _context, string _message, Color _color, Object _object)
        {
            Debug.Log($"<color={_color.GetHexString()}>{_context.ToUpper()}</color> :: {_message}", _object);
        }

        public static void Warning(string _context, string _message)
        {
            Debug.LogWarning($"{_context.ToUpper()} :: {_message}");
        }

        public static void Warning(string _context, string _message, Color _color)
        {
            Debug.LogWarning($"<color={_color.GetHexString()}>{_context.ToUpper()}</color> :: {_message}");
        }

        public static void Error(string _context, string _message)
        {
            Debug.LogError($"{_context.ToUpper()} :: {_message}");
        }

        public static void Error(string _context, string _message, Color _color)
        {
            Debug.LogError($"<color={_color.GetHexString()}>{_context.ToUpper()}</color> :: {_message}");
        }
    }
}