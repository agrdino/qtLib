using qtLib.UIScripts.Base.Object.SubScene;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace qtLib.Extension.UI
{
    [DisallowMultipleComponent]
    public class UISwipeNestedDragForwarder : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private enum NestedDragMode
        {
            None,
            PageSwipe,
            ScrollView
        }

        [Header("References")]
        [SerializeField] private UISubSceneSwiper _swipe;
        [SerializeField] private ScrollRect _scrollRect;

        [Header("Optional Conflict Fix")]
        [Tooltip("Nếu ScrollView con có Horizontal = true, tạm tắt horizontal scroll khi user swipe ngang page.")]
        [SerializeField] private bool _disableChildHorizontalScrollWhenPageSwipe = true;

        [Tooltip("Nếu user đã được detect là page swipe ngang, tạm tắt vertical scroll để không kéo ngược sang hướng scroll.")]
        [SerializeField] private bool _disableChildVerticalScrollWhenPageSwipe = true;

        [SerializeField] private float _detectDirectionPixels = 8f;
        [SerializeField] private float _horizontalDominance = 1.1f;

        [Header("Click Guard")]
        [Tooltip("Khi bắt đầu drag/swipe, hủy pending click để tránh trigger Button/action lúc thả tay.")]
        [SerializeField] private bool _cancelClickAfterDrag = true;

        private Vector2 _startPointer;

        private bool _directionResolved;
        private bool _startedForwardingToSwipe;

        private bool _originalHorizontal;
        private bool _originalVertical;
        private bool _hasCachedScrollRectState;

        private NestedDragMode _dragMode = NestedDragMode.None;

        private void Reset()
        {
            _swipe = GetComponentInParent<UISubSceneSwiper>();
            _scrollRect = GetComponent<ScrollRect>();
        }

        private void Awake()
        {
            if (_swipe == null)
            {
                _swipe = GetComponentInParent<UISubSceneSwiper>();
            }

            if (_scrollRect == null)
            {
                _scrollRect = GetComponent<ScrollRect>();
            }
        }

        private void OnDisable()
        {
            RestoreScrollRectState();

            _directionResolved = false;
            _startedForwardingToSwipe = false;
            _dragMode = NestedDragMode.None;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _startPointer = eventData.position;

            _directionResolved = false;
            _startedForwardingToSwipe = false;
            _dragMode = NestedDragMode.None;

            CacheScrollRectState();

            // OnBeginDrag chỉ xảy ra khi Unity đã xem đây là drag,
            // nên có thể hủy pending click ngay từ đây.
            CancelPendingClick(eventData);

            if (_swipe == null)
            {
                return;
            }

            if (_swipe.IsTransitioning)
            {
                return;
            }

            _startedForwardingToSwipe = true;
            _swipe.ForwardBeginDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            CancelPendingClick(eventData);

            ResolveScrollRectConflict(eventData);

            if (_swipe == null)
            {
                return;
            }

            if (_swipe.IsTransitioning)
            {
                return;
            }

            // Nếu đã detect là ScrollView drag dọc,
            // không forward Drag/EndDrag lên swiper nữa.
            if (_dragMode == NestedDragMode.ScrollView)
            {
                return;
            }

            if (!_startedForwardingToSwipe)
            {
                return;
            }

            _swipe.ForwardDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Quan trọng:
            // Hủy pending click trước khi kết thúc drag để tránh trigger Button/action cuối.
            CancelPendingClick(eventData);

            if (_swipe != null && _startedForwardingToSwipe && !_swipe.IsTransitioning)
            {
                if (_dragMode == NestedDragMode.ScrollView)
                {
                    // ScrollView drag thì không trigger EndDrag của swiper.
                    // Chỉ cancel state đã begin trước đó.
                    _swipe.ForwardCancelDrag();
                }
                else
                {
                    _swipe.ForwardEndDrag(eventData);
                }
            }

            RestoreScrollRectState();

            _directionResolved = false;
            _startedForwardingToSwipe = false;
            _dragMode = NestedDragMode.None;
        }

        private void ResolveScrollRectConflict(PointerEventData eventData)
        {
            if (_directionResolved)
            {
                return;
            }

            Vector2 delta = eventData.position - _startPointer;

            float absX = Mathf.Abs(delta.x);
            float absY = Mathf.Abs(delta.y);

            if (absX < _detectDirectionPixels && absY < _detectDirectionPixels)
            {
                return;
            }

            if (absX >= _detectDirectionPixels && absX >= absY * _horizontalDominance)
            {
                // Đã detect user muốn swipe ngang page.
                // Từ đây khóa mode PageSwipe, không cho đổi sang ScrollView drag nữa.
                _dragMode = NestedDragMode.PageSwipe;
                _directionResolved = true;

                DisableChildScrollForPageSwipe();

                return;
            }

            if (absY >= _detectDirectionPixels && absY >= absX * _horizontalDominance)
            {
                // Đã detect user muốn kéo ScrollView.
                // Từ đây khóa mode ScrollView, không cho trigger page swipe ở EndDrag.
                _dragMode = NestedDragMode.ScrollView;
                _directionResolved = true;

                if (_swipe != null && _startedForwardingToSwipe)
                {
                    _swipe.ForwardCancelDrag();
                    _startedForwardingToSwipe = false;
                }
            }
        }

        private void DisableChildScrollForPageSwipe()
        {
            if (_scrollRect == null)
            {
                return;
            }

            if (_disableChildHorizontalScrollWhenPageSwipe)
            {
                _scrollRect.horizontal = false;
            }

            if (_disableChildVerticalScrollWhenPageSwipe)
            {
                _scrollRect.vertical = false;
            }

            _scrollRect.StopMovement();
        }

        private void CacheScrollRectState()
        {
            if (_scrollRect == null)
            {
                _hasCachedScrollRectState = false;
                return;
            }

            _originalHorizontal = _scrollRect.horizontal;
            _originalVertical = _scrollRect.vertical;
            _hasCachedScrollRectState = true;
        }

        private void RestoreScrollRectState()
        {
            if (_scrollRect == null)
            {
                return;
            }

            if (!_hasCachedScrollRectState)
            {
                return;
            }

            _scrollRect.horizontal = _originalHorizontal;
            _scrollRect.vertical = _originalVertical;

            _hasCachedScrollRectState = false;
        }

        private void CancelPendingClick(PointerEventData eventData)
        {
            if (!_cancelClickAfterDrag)
            {
                return;
            }

            if (eventData == null)
            {
                return;
            }

            // Chặn click ở cấp EventSystem.
            // Quan trọng: phải clear từ BeginDrag/Drag, không chỉ EndDrag,
            // vì một số EventSystem xử lý PointerClick trước EndDrag khi thả tay.
            eventData.eligibleForClick = false;
            eventData.pointerPress = null;
            eventData.rawPointerPress = null;
            eventData.clickCount = 0;
            eventData.clickTime = 0f;

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }
}