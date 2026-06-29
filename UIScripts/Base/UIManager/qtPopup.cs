using Cysharp.Threading.Tasks;
using DG.Tweening;
using Redcode.Extensions;
using UnityEngine;

namespace qtLib.UI.Base
{
    [RequireComponent(typeof(CanvasGroup))]
    public class qtPopup : qtUiObject
    {
        #region ----- Component Config -----
        
        [SerializeField] protected RectTransform _container;
        [SerializeField] protected CanvasGroup _canvasGroup;
        
        #endregion
        
        protected CanvasGroup _currentImageFading;
        
        protected override async UniTask _AnimatedShow()
        {
            _currentImageFading = uiManager.PopupFadeIn(this);
            switch (_animationConfig.animIn)
            {
                case PopupAnimType.Left:
                {
                    _container.anchoredPosition = _container.anchoredPosition.WithX(-0.5f * Screen.width);
                    await _container
                        .DOAnchorPosX(0, _animationConfig.animInTime).SetUpdate(UpdateType.Normal, true)
                        .ToUniTask();
                    break;
                }
                case PopupAnimType.Right:
                {
                    _container.anchoredPosition = _container.anchoredPosition.WithX(1.5f * Screen.width);
                    await _container
                        .DOAnchorPosX(0, _animationConfig.animInTime).SetUpdate(UpdateType.Normal, true)
                        .ToUniTask();
                    break;
                }
                case PopupAnimType.Zoom:
                {
                    _container.localScale = Vector3.zero;
                    await _container
                        .DOScale(Vector3.one, _animationConfig.animInTime)
                        .SetUpdate(UpdateType.Normal, true)
                        .ToUniTask();
                    break;
                }
                case PopupAnimType.Fade:
                {
                    _container.localScale = Vector3.one;
                    _canvasGroup.alpha = 0;
                    await _canvasGroup
                        .DOFade(1, _animationConfig.animInTime)
                        .SetUpdate(UpdateType.Normal, true)
                        .ToUniTask();
                    break;
                }
                case PopupAnimType.Immediately:
                {
                    _container.localScale = Vector3.one;
                    _canvasGroup.alpha = 1;
                    break;
                }
            }
        }

        public override async UniTaskVoid ControllerHide()
        {
            await uiManager.BeforeUIHide(this);
            base.ControllerHide().Forget();
        }

        protected override async UniTask _AnimatedHide()
        {
            uiManager.PopupFadeOut(_currentImageFading, this);
            switch (_animationConfig.animOut)
            {
                case PopupAnimType.Left:
                {
                    await _container
                        .DOAnchorPosX(-0.5f * Screen.width, _animationConfig.animOutTime)
                        .SetUpdate(UpdateType.Normal, true)
                        .ToUniTask();
                    break;
                }
                case PopupAnimType.Right:
                {
                    await _container
                        .DOAnchorPosX(1.5f * Screen.width, _animationConfig.animOutTime)
                        .SetUpdate(UpdateType.Normal, true)
                        .ToUniTask();
                    break;
                }
                case PopupAnimType.Zoom:
                {
                    await _container
                        .DOScale(Vector3.zero, _animationConfig.animOutTime)
                        .SetUpdate(UpdateType.Normal, true)
                        .ToUniTask();
                    break;
                }
                case PopupAnimType.Fade:
                {
                    _canvasGroup.alpha = 1;
                    await _canvasGroup
                        .DOFade(0, _animationConfig.animOutTime)
                        .SetUpdate(UpdateType.Normal, true)
                        .ToUniTask();
                    break;
                }
                case PopupAnimType.Immediately:
                {
                    break;
                }
            }   
        }
    }
}