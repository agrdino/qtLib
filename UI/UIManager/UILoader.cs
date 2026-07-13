using UnityEngine;

namespace qtLib.UI.UIManager
{
    [RequireComponent(typeof(Canvas))]
    public class UILoader : qtUiLoader<qtUiObject>
    {
#if UNITY_EDITOR
        private void OnValidate()
        {
            _rectCanvas = (RectTransform)GetComponent<Canvas>().transform;
        }
#endif

    }
}
