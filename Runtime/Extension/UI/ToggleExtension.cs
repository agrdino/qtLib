using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace qtLib.Extension.UI
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleExtension : MonoBehaviour
    {
        #region ----- Component Config -----

        private Toggle _toggle;

        public UnityEvent<bool> onValueBecomeTrue;
        public UnityEvent<bool> onValueBecomeFalse;

        #endregion

        #region ----- Unity Event -----

        private void Awake()
        {
            _toggle = GetComponent<Toggle>();
            _toggle.onValueChanged.AddListener(_OnValueChanged);
        }

        private void Start()
        {
            onValueBecomeTrue?.Invoke(_toggle.isOn);
            onValueBecomeFalse?.Invoke(!_toggle.isOn);
        }

        #endregion

        #region ----- Private Function -----

        private void _OnValueChanged(bool value)
        {
            onValueBecomeTrue?.Invoke(value);
            onValueBecomeFalse?.Invoke(!value);
        }

        #endregion
    }
}