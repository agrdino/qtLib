using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace qtLib.UIScripts.Base.Object.SubScene
{
    public class UISubSceneSwiper : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private enum DragMode
        {
            None,
            Horizontal,
            Vertical
        }

        [Header("UI References")]
        [SerializeField] private RectTransform viewport;
        [SerializeField] private RectTransform[] uiObjects;

        [Header("Initial State")]
        [SerializeField] private int initialIndex = 0;

        [Header("Swipe Settings")]
        [SerializeField] private float detectDirectionPixels = 8f;

        [Tooltip("Càng cao thì càng khó bị nhận nhầm swipe ngang khi đang scroll dọc.")]
        [SerializeField] private float horizontalDominance = 1.1f;

        [SerializeField] private float swipeThresholdPercent = 0.2f;
        [SerializeField] private float snapDuration = 0.18f;

        /// <summary>
        /// Bắn khi target scene được SetActive(true).
        /// Bắn cho cả swipe và GoToIndex.
        /// Param: fromIndex, toIndex.
        /// </summary>
        public event Action<int, int> onNextSubSceneActivated;

        /// <summary>
        /// Bắn khi previous scene đã bị SetActive(false).
        /// Bắn cho cả swipe và GoToIndex.
        /// Param: fromIndex, toIndex.
        /// </summary>
        public event Action<int, int> onPreviousSubSceneHidden;

        /// <summary>
        /// Bắn sau khi chuyển scene hoàn tất.
        /// Bắn cho cả swipe và GoToIndex.
        /// Param: fromIndex, toIndex.
        /// </summary>
        public event Action<int, int> onTransitionCompleted;

        /// <summary>
        /// Event cũ để giữ compatibility.
        /// Bây giờ cũng bắn cho cả swipe và GoToIndex.
        /// </summary>
        public event Action<int, int> onSwipeTransitionCompleted;

        /// <summary>
        /// Event cũ để giữ compatibility.
        /// Chỉ trả về toIndex.
        /// </summary>
        public event Action<int> onActiveNextSubScene;

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
                {
                    continue;
                }

                page.anchorMin = new Vector2(0f, 0f);
                page.anchorMax = new Vector2(1f, 1f);
                page.pivot = new Vector2(0.5f, 0.5f);

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

        /// <summary>
        /// Reset visual.
        /// Không notify trong function này để tránh bắn sai lúc Awake, OnEnable, BeginDrag, swipe fail.
        /// </summary>
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
                {
                    continue;
                }

                page.gameObject.SetActive(i == currentIndex);
                SetPageX(i, 0f);
            }
        }

        /// <summary>
        /// Dùng sau khi transition thành công.
        /// Notify đúng lúc previous scene bị hide.
        /// </summary>
        private void ShowCurrentAndHidePrevious(
            int previousIndex,
            int newIndex,
            bool notifyPreviousHidden
        )
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
                {
                    continue;
                }

                if (i == newIndex)
                {
                    page.gameObject.SetActive(true);
                    SetPageX(i, 0f);
                    continue;
                }

                bool wasActive = page.gameObject.activeSelf;

                page.gameObject.SetActive(false);
                SetPageX(i, 0f);

                if (
                    notifyPreviousHidden &&
                    i == previousIndex &&
                    previousIndex != newIndex &&
                    wasActive
                )
                {
                    NotifyPreviousSubSceneHidden(previousIndex, newIndex);
                }
            }
        }

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

            if (dragMode != DragMode.Horizontal)
            {
                return;
            }

            if (Mathf.Abs(delta.x) < detectDirectionPixels)
            {
                ClearTargetPreview();
                return;
            }

            int desiredOffset = delta.x < 0f ? +1 : -1;

            if (!TryActivateTarget(desiredOffset, notifyActivated: true))
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

            if (!TryActivateTarget(desiredOffset, notifyActivated: true))
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
                notifyTransitionCompleted: true,
                notifyPreviousHidden: true
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

        private bool TryActivateTarget(int desiredOffset, bool notifyActivated)
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

            ClearTargetPreview();

            targetOffset = desiredOffset;
            targetIndex = desiredIndex;

            SetPageX(currentIndex, 0f);

            uiObjects[targetIndex].gameObject.SetActive(true);
            SetPageX(targetIndex, targetOffset * screenWidth);

            if (notifyActivated)
            {
                NotifyNextSubSceneActivated(currentIndex, targetIndex);
            }

            return true;
        }

        private void ClearTargetPreview()
        {
            if (IsValidIndex(targetIndex) && targetIndex != currentIndex)
            {
                // Đây chỉ là target preview bị tắt.
                // Không gọi onPreviousSubSceneHidden ở đây.
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
                return Mathf.Clamp(deltaX, -screenWidth, 0f);
            }

            if (targetOffset == -1)
            {
                return Mathf.Clamp(deltaX, 0f, screenWidth);
            }

            return 0f;
        }

        private void StartSnap(
            bool shouldSwitch,
            bool notifyTransitionCompleted,
            bool notifyPreviousHidden
        )
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
                notifyTransitionCompleted: notifyTransitionCompleted,
                notifyPreviousHidden: notifyPreviousHidden,
                cts: cts
            ).Forget();
        }

        private async UniTask SnapAsync(
            int fromIndex,
            int toIndex,
            int offset,
            bool shouldSwitch,
            bool notifyTransitionCompleted,
            bool notifyPreviousHidden,
            CancellationTokenSource cts
        )
        {
            bool shouldNotifyTransitionCompletedAfterCleanup = false;
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

                    ShowCurrentAndHidePrevious(
                        previousIndex: oldIndex,
                        newIndex: currentIndex,
                        notifyPreviousHidden: notifyPreviousHidden
                    );
                }
                else
                {
                    ShowOnlyCurrent();
                }

                if (shouldSwitch && notifyTransitionCompleted && oldIndex != currentIndex)
                {
                    shouldNotifyTransitionCompletedAfterCleanup = true;
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

            if (shouldNotifyTransitionCompletedAfterCleanup)
            {
                NotifyTransitionCompleted(notifyFromIndex, notifyToIndex);
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

        private void NotifyNextSubSceneActivated(int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex)
            {
                return;
            }

            onNextSubSceneActivated?.Invoke(fromIndex, toIndex);

            // Event cũ.
            onActiveNextSubScene?.Invoke(toIndex);
        }

        private void NotifyPreviousSubSceneHidden(int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex)
            {
                return;
            }

            onPreviousSubSceneHidden?.Invoke(fromIndex, toIndex);
        }

        private void NotifyTransitionCompleted(int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex)
            {
                return;
            }

            onTransitionCompleted?.Invoke(fromIndex, toIndex);

            // Event cũ.
            // Tên là swipe, nhưng giờ cũng notify cho GoToIndex để không break code cũ.
            onSwipeTransitionCompleted?.Invoke(fromIndex, toIndex);
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

        public void GoToIndex(int index)
        {
            GoToIndex(index, true);
        }

        /// <summary>
        /// Manual GoToIndex cũng notify:
        /// 1. onNextSubSceneActivated
        /// 2. onPreviousSubSceneHidden
        /// 3. onTransitionCompleted / onSwipeTransitionCompleted
        /// </summary>
        public void GoToIndex(int index, bool animated)
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

            int oldIndex = currentIndex;

            ShowOnlyCurrent();

            targetIndex = newIndex;
            targetOffset = targetIndex > currentIndex ? +1 : -1;

            SetPageX(currentIndex, 0f);

            uiObjects[targetIndex].gameObject.SetActive(true);
            SetPageX(targetIndex, targetOffset * screenWidth);

            NotifyNextSubSceneActivated(oldIndex, targetIndex);

            if (!animated)
            {
                currentIndex = targetIndex;

                ShowCurrentAndHidePrevious(
                    previousIndex: oldIndex,
                    newIndex: currentIndex,
                    notifyPreviousHidden: true
                );

                NotifyTransitionCompleted(oldIndex, currentIndex);
                return;
            }

            StartSnap(
                shouldSwitch: true,
                notifyTransitionCompleted: true,
                notifyPreviousHidden: true
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