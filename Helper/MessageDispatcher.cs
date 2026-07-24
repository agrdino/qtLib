using System;
using System.Collections.Generic;
using UnityEngine;

namespace qtLib.Helper
{
    public partial class MessageDispatcher : MonoBehaviour
    {
        #region ----- Component Config -----

        private static Dictionary<int, List<Action<MessageObject>>> _dictListener;

        #endregion

        #region ----- Private Function -----

        private void Awake()
        {
            _dictListener = new Dictionary<int, List<Action<MessageObject>>>();
        }
        
        #endregion
        
        #region ----- Public Function -----

        public static void Register(int eventID, Action<MessageObject> listener)
        {
            if (!_dictListener.ContainsKey(eventID))
            {
                _dictListener.Add(eventID, new List<Action<MessageObject>>());
            }

            _dictListener[eventID].Add(listener);
        }

        public static void UnRegister(int eventID, Action<MessageObject> listener)
        {
            if (_dictListener.ContainsKey(eventID))
            {
                _dictListener[eventID].Remove(listener);
            }
        }

        public static void SendMessage(int eventID, MessageObject param = null)
        {
            if (_dictListener.TryGetValue(eventID, out var listeners))
            {
                foreach (Action<MessageObject> listener in listeners)
                {
                    listener.Invoke(param);
                }
            }
        }

        #endregion
        
        public class MessageObject
        {
            
        }
        
        public static partial class EventID
        {
            public const int None = 0;
        }
    }
}
