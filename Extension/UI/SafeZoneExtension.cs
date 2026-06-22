using UnityEngine;

namespace Extension
{
    [DefaultExecutionOrder(-1)]
    public class SafeZoneExtension : MonoBehaviour
    {
        private float _top, _bot;

        public const int MaxScreenWidth = 1080;
        public const int MinimumTop = 88;

        private RectTransform _rectTransform;
        private RectTransform _canvasRectTransform;

        [SerializeField] private bool _conformX = true, _conformY = true;
        private bool _isInitialized = false;

        private void OnEnable()
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;
            _rectTransform = GetComponent<RectTransform>();
            _canvasRectTransform = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            Rect safeArea = Screen.safeArea;
            _bot = safeArea.y / 2f;

            if (_bot == 0)
            {
            }
            else
            { 
                _bot = Mathf.Clamp(_bot - 50, 0, _bot);
            }

            _top = (Screen.height - safeArea.y - safeArea.height) / 2f;
            if ((int)_top == 0)
            {
                _top = MinimumTop;
            }
        
            SetHeight();
            SetWidth();
        }
        
        private void SetHeight()
        {
            if (!_conformY)
            {
                return;
            }

            Vector2 sizeDelta = _rectTransform.sizeDelta;
            sizeDelta.y = 0 - _bot - _top;
            _rectTransform.sizeDelta = sizeDelta;

            _rectTransform.localPosition = (_bot - _top) / 2f * Vector2.up;
        }

        private void SetWidth()
        {
            if (!_conformX)
            {
                return;
            }

            Vector2 sizeDelta = _rectTransform.sizeDelta;
            if (_canvasRectTransform.sizeDelta.x >= MaxScreenWidth)
            {
                sizeDelta.x -= (_canvasRectTransform.sizeDelta.x - MaxScreenWidth);
                _rectTransform.sizeDelta = sizeDelta;
            }
        }
    }
}