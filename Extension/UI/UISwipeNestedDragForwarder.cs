using _Scripts.UI.Scene.MenuScene;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace qtLib.Extension.UI
{
    [DisallowMultipleComponent]
    public class UISwipeNestedDragForwarder : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("References")] [SerializeField]
        private UISubSceneSwiper swipe;

        [SerializeField] private ScrollRect scrollRect;

        [Header("Optional Conflict Fix")]
        [Tooltip("Nếu ScrollView con có Horizontal = true, tạm tắt horizontal scroll khi user swipe ngang page.")]
        [SerializeField]
        private bool disableChildHorizontalScrollWhenPageSwipe = true;

        [SerializeField] private float detectDirectionPixels = 8f;
        [SerializeField] private float horizontalDominance = 1.1f;

        private Vector2 startPointer;
        private bool directionResolved;
        private bool originalHorizontal;

        private void Reset()
        {
            swipe = GetComponentInParent<UISubSceneSwiper>();
            scrollRect = GetComponent<ScrollRect>();
        }

        private void Awake()
        {
            if (swipe == null)
            {
                swipe = GetComponentInParent<UISubSceneSwiper>();
            }

            if (scrollRect == null)
            {
                scrollRect = GetComponent<ScrollRect>();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (swipe == null)
            {
                return;
            }

            startPointer = eventData.position;
            directionResolved = false;

            if (scrollRect != null)
            {
                originalHorizontal = scrollRect.horizontal;
            }

            swipe.ForwardBeginDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (swipe == null)
            {
                return;
            }

            ResolveScrollRectConflict(eventData);

            swipe.ForwardDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (swipe != null)
            {
                swipe.ForwardEndDrag(eventData);
            }

            if (scrollRect != null)
            {
                scrollRect.horizontal = originalHorizontal;
            }

            directionResolved = false;
        }

        private void ResolveScrollRectConflict(PointerEventData eventData)
        {
            if (!disableChildHorizontalScrollWhenPageSwipe)
            {
                return;
            }

            if (scrollRect == null)
            {
                return;
            }

            if (directionResolved)
            {
                return;
            }

            Vector2 delta = eventData.position - startPointer;

            float absX = Mathf.Abs(delta.x);
            float absY = Mathf.Abs(delta.y);

            if (absX < detectDirectionPixels && absY < detectDirectionPixels)
            {
                return;
            }

            if (absX >= detectDirectionPixels && absX >= absY * horizontalDominance)
            {
                // User đang swipe ngang page.
                // Tắt horizontal scroll của ScrollRect con để tránh conflict.
                scrollRect.horizontal = false;
                directionResolved = true;
                return;
            }

            if (absY >= detectDirectionPixels && absY >= absX * horizontalDominance)
            {
                // User đang scroll dọc trong ScrollView.
                // Giữ ScrollRect hoạt động bình thường.
                directionResolved = true;
            }
        }
    }
}