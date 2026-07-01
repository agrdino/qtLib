using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace qtLib.Extension.UI
{
    public class UISubSceneSwiper : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private enum DragMode
        {
            None,
            Horizontal,
            Vertical
        }

        [Header("UI References")] [SerializeField]
        private RectTransform viewport;

        [SerializeField] private RectTransform[] uiObjects;

        [Header("Initial State")] [SerializeField]
        private int initialIndex = 0;

        [Header("Swipe Settings")] [SerializeField]
        private float detectDirectionPixels = 8f;

        [Tooltip("Càng cao thì càng khó bị nhận nhầm swipe ngang khi đang scroll dọc.")] [SerializeField]
        private float horizontalDominance = 1.1f;

        [SerializeField] private float swipeThresholdPercent = 0.2f;
        [SerializeField] private float snapDuration = 0.18f;

        public event Action<int> onSwipeTransitionCompleted;
        public event Action<int> onSwipeTransitionStarted;

        private int currentIndex;
        private int targetIndex = -1;

        // +1 = target nằm bên phải current.
        // -1 = target nằm bên trái current.
        private int targetOffset = 0;

        private float screenWidth;
        private Vector2 startPointer;

        private bool isDragging;
        private bool isAnimating;

        private DragMode dragMode = DragMode.None;

        private CancellationTokenSource animationCts;

        public int CurrentIndex => currentIndex;
        public int PageCount => uiObjects == null ? 0 : uiObjects.Length;

        private void Reset()
        {
            viewport = GetComponent<RectTransform>();
        }

        private void Awake()
        {
            if (viewport == null)
            {
                viewport = GetComponent<RectTransform>();
            }

            currentIndex = initialIndex;

            UpdatePageSizes();
            ClampCurrentIndex();
            ShowOnlyCurrent();
        }

        private void OnEnable()
        {
            UpdatePageSizes();
            ClampCurrentIndex();
            ShowOnlyCurrent();
        }

        private void OnDisable()
        {
            CancelAnimation();

            isDragging = false;
            isAnimating = false;

            dragMode = DragMode.None;
            targetIndex = -1;
            targetOffset = 0;
        }

        private void OnDestroy()
        {
            CancelAnimation();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (viewport == null)
            {
                viewport = GetComponent<RectTransform>();
            }

            UpdatePageSizes();

            if (!isDragging && !isAnimating)
            {
                ShowOnlyCurrent();
            }
        }

        private void UpdatePageSizes()
        {
            if (viewport == null || uiObjects == null || uiObjects.Length == 0)
            {
                return;
            }

            screenWidth = viewport.rect.width;

            if (screenWidth <= 0f)
            {
                screenWidth = Screen.width;
            }

            if (screenWidth <= 0f)
            {
                screenWidth = 1f;
            }

            for (int i = 0; i < uiObjects.Length; i++)
            {
                RectTransform page = uiObjects[i];

                if (page == null)
                    continue;

                page.anchorMin = new Vector2(0f, 0f);
                page.anchorMax = new Vector2(1f, 1f);
                page.pivot = new Vector2(0.5f, 0.5f);

                // Width bằng viewport width, height stretch theo parent.
                // page.sizeDelta = new Vector2(screenWidth, 0f);
                page.anchoredPosition = new Vector2(page.anchoredPosition.x, 0f);
            }
        }

        private void ClampCurrentIndex()
        {
            if (uiObjects == null || uiObjects.Length == 0)
            {
                currentIndex = 0;
                return;
            }

            currentIndex = Mathf.Clamp(currentIndex, 0, uiObjects.Length - 1);
        }

        private void ShowOnlyCurrent()
        {
            if (uiObjects == null || uiObjects.Length == 0)
            {
                return;
            }

            targetIndex = -1;
            targetOffset = 0;

            for (int i = 0; i < uiObjects.Length; i++)
            {
                RectTransform page = uiObjects[i];

                if (page == null)
                    continue;

                bool active = i == currentIndex;

                page.gameObject.SetActive(active);
                SetPageX(i, 0f);
            }
        }

        // Drag trực tiếp trên viewport.
        public void OnBeginDrag(PointerEventData eventData)
        {
            BeginDragInternal(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            DragInternal(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            EndDragInternal(eventData);
        }

        // Drag được forward từ ScrollView con.
        public void ForwardBeginDrag(PointerEventData eventData)
        {
            BeginDragInternal(eventData);
        }

        public void ForwardDrag(PointerEventData eventData)
        {
            DragInternal(eventData);
        }

        public void ForwardEndDrag(PointerEventData eventData)
        {
            EndDragInternal(eventData);
        }

        private void BeginDragInternal(PointerEventData eventData)
        {
            if (uiObjects == null || uiObjects.Length <= 1)
            {
                return;
            }

            if (!IsValidIndex(currentIndex))
            {
                return;
            }

            CancelAnimation();

            UpdatePageSizes();
            ShowOnlyCurrent();

            isDragging = true;
            dragMode = DragMode.None;

            startPointer = GetLocalPointer(eventData);
        }

        private void DragInternal(PointerEventData eventData)
        {
            if (!isDragging)
            {
                return;
            }

            if (!IsValidIndex(currentIndex))
            {
                return;
            }

            Vector2 currentPointer = GetLocalPointer(eventData);
            Vector2 delta = currentPointer - startPointer;

            ResolveDragMode(delta);

            // Nếu đang scroll dọc trong ScrollView con thì không move page.
            if (dragMode != DragMode.Horizontal)
            {
                return;
            }

            // Khi tay quay gần về điểm bắt đầu, bỏ target cũ.
            // Nhờ vậy kéo qua phải xong kéo ngược qua trái vẫn đổi hướng được.
            if (Mathf.Abs(delta.x) < detectDirectionPixels)
            {
                ClearTargetPreview();
                return;
            }

            int desiredOffset = delta.x < 0f ? +1 : -1;

            if (!TryActivateTarget(desiredOffset))
            {
                SetPageX(currentIndex, 0f);
                return;
            }

            float clampedDeltaX = ClampDeltaByTarget(delta.x);

            SetPageX(currentIndex, clampedDeltaX);
            SetPageX(targetIndex, targetOffset * screenWidth + clampedDeltaX);
        }

        private void EndDragInternal(PointerEventData eventData)
        {
            if (!isDragging)
            {
                return;
            }

            isDragging = false;

            Vector2 endPointer = GetLocalPointer(eventData);
            Vector2 delta = endPointer - startPointer;

            ResolveDragMode(delta);

            if (dragMode != DragMode.Horizontal)
            {
                dragMode = DragMode.None;
                ShowOnlyCurrent();
                return;
            }

            if (Mathf.Abs(delta.x) < detectDirectionPixels)
            {
                dragMode = DragMode.None;
                ShowOnlyCurrent();
                return;
            }

            int desiredOffset = delta.x < 0f ? +1 : -1;

            if (!TryActivateTarget(desiredOffset))
            {
                dragMode = DragMode.None;
                ShowOnlyCurrent();
                return;
            }

            float clampedDeltaX = ClampDeltaByTarget(delta.x);

            SetPageX(currentIndex, clampedDeltaX);
            SetPageX(targetIndex, targetOffset * screenWidth + clampedDeltaX);

            float threshold = screenWidth * swipeThresholdPercent;
            bool shouldSwitch = Mathf.Abs(clampedDeltaX) >= threshold;

            dragMode = DragMode.None;

            StartSnap(
                shouldSwitch: shouldSwitch,
                notifyWhenCompleted: true
            );
        }

        private void ResolveDragMode(Vector2 delta)
        {
            if (dragMode != DragMode.None)
            {
                return;
            }

            float absX = Mathf.Abs(delta.x);
            float absY = Mathf.Abs(delta.y);

            if (absX < detectDirectionPixels && absY < detectDirectionPixels)
            {
                return;
            }

            if (absX >= detectDirectionPixels && absX >= absY * horizontalDominance)
            {
                dragMode = DragMode.Horizontal;
                return;
            }

            if (absY >= detectDirectionPixels && absY >= absX * horizontalDominance)
            {
                dragMode = DragMode.Vertical;
                ClearTargetPreview();
            }
        }

        private bool TryActivateTarget(int desiredOffset)
        {
            int desiredIndex = currentIndex + desiredOffset;

            if (!IsValidIndex(desiredIndex))
            {
                ClearTargetPreview();
                return false;
            }

            if (targetIndex == desiredIndex && targetOffset == desiredOffset)
            {
                return true;
            }

            // Đổi hướng trong cùng một lần drag:
            // tắt target cũ, bật target mới.
            ClearTargetPreview();

            targetOffset = desiredOffset;
            targetIndex = desiredIndex;

            onSwipeTransitionStarted?.Invoke(targetIndex);
            uiObjects[targetIndex].gameObject.SetActive(true);

            SetPageX(currentIndex, 0f);
            SetPageX(targetIndex, targetOffset * screenWidth);

            return true;
        }

        private void ClearTargetPreview()
        {
            if (IsValidIndex(targetIndex) && targetIndex != currentIndex)
            {
                uiObjects[targetIndex].gameObject.SetActive(false);
                SetPageX(targetIndex, 0f);
            }

            targetIndex = -1;
            targetOffset = 0;

            if (IsValidIndex(currentIndex))
            {
                SetPageX(currentIndex, 0f);
            }
        }

        private float ClampDeltaByTarget(float deltaX)
        {
            if (targetOffset == +1)
            {
                // Target ở bên phải, kéo từ 0 tới -screenWidth.
                return Mathf.Clamp(deltaX, -screenWidth, 0f);
            }

            if (targetOffset == -1)
            {
                // Target ở bên trái, kéo từ 0 tới +screenWidth.
                return Mathf.Clamp(deltaX, 0f, screenWidth);
            }

            return 0f;
        }

        private void StartSnap(bool shouldSwitch, bool notifyWhenCompleted)
        {
            if (!IsValidIndex(currentIndex) || !IsValidIndex(targetIndex))
            {
                ShowOnlyCurrent();
                return;
            }

            CancellationTokenSource cts = RestartAnimationToken();

            SnapAsync(
                fromIndex: currentIndex,
                toIndex: targetIndex,
                offset: targetOffset,
                shouldSwitch: shouldSwitch,
                notifyWhenCompleted: notifyWhenCompleted,
                cts: cts
            ).Forget();
        }

        private async UniTask SnapAsync(
            int fromIndex,
            int toIndex,
            int offset,
            bool shouldSwitch,
            bool notifyWhenCompleted,
            CancellationTokenSource cts
        )
        {
            bool shouldNotifyAfterCleanup = false;
            int notifyFromIndex = -1;
            int notifyToIndex = -1;

            isAnimating = true;

            try
            {
                if (!IsValidIndex(fromIndex) || !IsValidIndex(toIndex))
                {
                    return;
                }

                CancellationToken token = cts.Token;

                RectTransform fromPage = uiObjects[fromIndex];
                RectTransform toPage = uiObjects[toIndex];

                float width = screenWidth;

                float fromStartX = fromPage.anchoredPosition.x;
                float toStartX = toPage.anchoredPosition.x;

                float fromEndX;
                float toEndX;

                if (shouldSwitch)
                {
                    fromEndX = -offset * width;
                    toEndX = 0f;
                }
                else
                {
                    fromEndX = 0f;
                    toEndX = offset * width;
                }

                if (snapDuration <= 0f)
                {
                    SetPageX(fromIndex, fromEndX);
                    SetPageX(toIndex, toEndX);
                }
                else
                {
                    float timer = 0f;

                    while (timer < snapDuration)
                    {
                        token.ThrowIfCancellationRequested();

                        timer += Time.unscaledDeltaTime;

                        float t = Mathf.Clamp01(timer / snapDuration);

                        // Smooth step.
                        t = t * t * (3f - 2f * t);

                        SetPageX(fromIndex, Mathf.Lerp(fromStartX, fromEndX, t));
                        SetPageX(toIndex, Mathf.Lerp(toStartX, toEndX, t));

                        await UniTask.Yield(PlayerLoopTiming.Update, token);
                    }
                }

                token.ThrowIfCancellationRequested();

                SetPageX(fromIndex, fromEndX);
                SetPageX(toIndex, toEndX);

                int oldIndex = currentIndex;

                if (shouldSwitch)
                {
                    currentIndex = toIndex;
                }

                ShowOnlyCurrent();

                if (shouldSwitch && notifyWhenCompleted && oldIndex != currentIndex)
                {
                    shouldNotifyAfterCleanup = true;
                    notifyFromIndex = oldIndex;
                    notifyToIndex = currentIndex;
                }
            }
            catch (OperationCanceledException)
            {
                // Bị cancel do user drag tiếp, disable object, hoặc GoToIndex mới.
            }
            finally
            {
                if (animationCts == cts)
                {
                    animationCts = null;
                    isAnimating = false;
                }

                cts.Dispose();
            }

            if (shouldNotifyAfterCleanup)
            {
                NotifySwipeTransitionCompleted(notifyFromIndex, notifyToIndex);
            }
        }

        private CancellationTokenSource RestartAnimationToken()
        {
            CancelAnimation();

            animationCts = CancellationTokenSource.CreateLinkedTokenSource(
                this.GetCancellationTokenOnDestroy()
            );

            return animationCts;
        }

        private void CancelAnimation()
        {
            if (animationCts == null)
            {
                isAnimating = false;
                return;
            }

            try
            {
                if (!animationCts.IsCancellationRequested)
                {
                    animationCts.Cancel();
                }
            }
            catch (ObjectDisposedException)
            {
            }

            isAnimating = false;
        }

        private void NotifySwipeTransitionCompleted(int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex)
            {
                return;
            }

            onSwipeTransitionCompleted?.Invoke(toIndex);
            // OnSwipeTransitionCompleted?.Invoke(fromIndex, toIndex);
        }

        private void SetPageX(int index, float x)
        {
            if (!IsValidIndex(index))
            {
                return;
            }

            RectTransform page = uiObjects[index];
            page.anchoredPosition = new Vector2(x, 0f);
        }

        private Vector2 GetLocalPointer(PointerEventData eventData)
        {
            if (viewport == null)
            {
                return eventData.position;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                viewport,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint
            );

            return localPoint;
        }

        private bool IsValidIndex(int index)
        {
            return uiObjects != null
                   && index >= 0
                   && index < uiObjects.Length
                   && uiObjects[index] != null;
        }
        
        // Manual GoToIndex có animation, nhưng KHÔNG notify.
        public void GoToIndex(int index)
        {
            if (uiObjects == null || uiObjects.Length == 0)
            {
                return;
            }

            int newIndex = Mathf.Clamp(index, 0, uiObjects.Length - 1);

            if (!IsValidIndex(newIndex))
            {
                return;
            }

            CancelAnimation();

            isDragging = false;
            dragMode = DragMode.None;

            UpdatePageSizes();
            ClampCurrentIndex();

            if (newIndex == currentIndex)
            {
                ShowOnlyCurrent();
                return;
            }

            ShowOnlyCurrent();
            
            targetIndex = newIndex;

            // Index lớn hơn: target nằm bên phải, trượt sang trái.
            // Index nhỏ hơn: target nằm bên trái, trượt sang phải.
            targetOffset = targetIndex > currentIndex ? +1 : -1;

            uiObjects[targetIndex].gameObject.SetActive(true);

            SetPageX(currentIndex, 0f);
            SetPageX(targetIndex, targetOffset * screenWidth);

            StartSnap(
                shouldSwitch: true,
                notifyWhenCompleted: false
            );
        }

        public void GoNext()
        {
            GoToIndex(currentIndex + 1);
        }

        public void GoPrevious()
        {
            GoToIndex(currentIndex - 1);
        }

        public int GetCurrentIndex()
        {
            return currentIndex;
        }
    }
}