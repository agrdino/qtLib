using System;
using System.Collections.Generic;

namespace qtLib.Helper
{
    public partial class MessageDispatcher : qtSingleton<MessageDispatcher>
    {
        public enum EEvent
        {
            IAPPurchaseSucceeded,
            OnCoinChanged,
            SwitchTab,
        }
        
        #region ----- Component Config -----

        private static Dictionary<EEvent, List<Action<MessageObject>>> _dictListener;

        #endregion

        #region ----- Private Function -----

        protected override void _Init()
        {
            base._Init();
            _dictListener = new Dictionary<EEvent, List<Action<MessageObject>>>();
        }

        #endregion
        
        #region ----- Public Function -----

        public static void Register(EEvent @event, Action<MessageObject> listener)
        {
            if (_dictListener.ContainsKey(@event))
            {
                _dictListener[@event].Add(listener);
            }
            else
            {
                _dictListener.Add(@event, new List<Action<MessageObject>>());
                _dictListener[@event].Add(listener);
            }
        }

        public static void UnRegister(EEvent @event, Action<MessageObject> listener)
        {
            if (_dictListener.ContainsKey(@event))
            {
                _dictListener[@event].Remove(listener);
            }
        }

        public static void SendMessage(EEvent @event, MessageObject param = null)
        {
            if (_dictListener.ContainsKey(@event))
            {
                _dictListener[@event].ForEach(listener => listener.Invoke(param));
            }
        }

        #endregion
        
        public class MessageObject
        {
            
        }
    }
}
