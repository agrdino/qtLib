using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace qtLib.UI.UIManager
{
    [RequireComponent(typeof(CanvasGroup))]
    public class qtPopup : qtUiObject
    {
        [SerializeField] protected RectTransform _container;
        [SerializeField] protected CanvasGroup _canvasGroup;

        protected CanvasGroup _currentImageFading;

        protected virtual void Awake()
        {
            ResolveComponents();
        }

        protected override async UniTask _AnimatedShow()
        {
            ResolveComponents();
            KillAnimationTweens();

            _currentImageFading = uiManager.PopupFadeIn(this);
            SetAnchoredPositionX(0f);
            _container.localScale = Vector3.one;
            _canvasGroup.alpha = 1f;

            switch (Animation.animIn)
            {
                case PopupAnimType.Left:
                    SetAnchoredPositionX(-0.5f * Screen.width);
                    await _container
                        .DOAnchorPosX(0f, Animation.animInTime)
                        .SetUpdate(UpdateType.Normal, true)
                        .ToUniTask();
                    break;

                case PopupAnimType.Right:
                    SetAnchoredPositionX(1.5f * Screen.width);
                    await _container
                        .DOAnchorPosX(0f, Animation.animInTime)
                        .SetUpdate(UpdateType.Normal, true)
                        .ToUniTask();
                    break;

                case PopupAnimType.Zoom:
                    _container.localScale = Vector3.zero;
                    await _container
                        .DOScale(Vector3.one, Animation.animInTime)
                        .SetUpdate(UpdateType.Normal, true)
                        .ToUniTask();
                    break;

                case PopupAnimType.Fade:
                    _canvasGroup.alpha = 0f;
                    await _canvasGroup
                        .DOFade(1f, Animation.animInTime)
                        .SetUpdate(UpdateType.Normal, true)
                        .ToUniTask();
                    break;

                case PopupAnimType.Immediately:
                    break;
            }
        }

        protected override async UniTask _AnimatedHide()
        {
            ResolveComponents();
            KillAnimationTweens();
            uiManager.PopupFadeOut(_currentImageFading, this);

            switch (Animation.animOut)
            {
                case PopupAnimType.Left:
                    await _container
                        .DOAnchorPosX(-0.5f * Screen.width, Animation.animOutTime)
                        .SetUpdate(UpdateType.Normal, true)
                        .ToUniTask();
                    break;

                case PopupAnimType.Right:
                    await _container
                        .DOAnchorPosX(1.5f * Screen.width, Animation.animOutTime)
                        .SetUpdate(UpdateType.Normal, true)
                        .ToUniTask();
                    break;

                case PopupAnimType.Zoom:
                    await _container
                        .DOScale(Vector3.zero, Animation.animOutTime)
                        .SetUpdate(UpdateType.Normal, true)
                        .ToUniTask();
                    break;

                case PopupAnimType.Fade:
                    _canvasGroup.alpha = 1f;
                    await _canvasGroup
                        .DOFade(0f, Animation.animOutTime)
                        .SetUpdate(UpdateType.Normal, true)
                        .ToUniTask();
                    break;

                case PopupAnimType.Immediately:
                    break;
            }
        }

        private void ResolveComponents()
        {
            if (!_container)
            {
                _container = transform as RectTransform;
            }

            if (!_canvasGroup)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            if (!_container || !_canvasGroup)
            {
                throw new MissingReferenceException(
                    $"{GetType().Name} requires a RectTransform container and CanvasGroup.");
            }
        }

        private void KillAnimationTweens()
        {
            _container.DOKill();
            _canvasGroup.DOKill();
        }

        private void SetAnchoredPositionX(float x)
        {
            var position = _container.anchoredPosition;
            position.x = x;
            _container.anchoredPosition = position;
        }
    }
}
