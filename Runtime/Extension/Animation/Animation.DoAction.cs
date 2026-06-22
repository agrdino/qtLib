using UnityEngine;
using UnityEngine.Events;

namespace qtLib.Extension.Animation
{
    public class DoAction : MonoBehaviour
    {
        #region ----- Component Config -----

        [SerializeField] private UnityEvent _action;

        #endregion

        #region ----- Public Function -----

        public void InvokeAction()
        {
           _action?.Invoke();
        }

        #endregion

    }
}