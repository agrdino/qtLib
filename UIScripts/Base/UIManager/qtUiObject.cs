using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using qtLib.Helper;
using UnityEngine;

namespace qtLib.UI.Base
{
    public class Args
    {
    }

    public class ParamOutput : Args
    {
    }

    public class ParamInput : Args
    {
    }

    public class NoOutput : ParamOutput
    {
    }

    public class NoInput : ParamInput
    {
    }

    public abstract class qtUiObject : MonoBehaviour
    {
        #region ----- Definitation -----

        public enum PopupAnimType
        {
            Zoom,
            Left,
            Right,
            Fade,
            Immediately
        }

        #endregion
        
        #region ----- Properties -----

        public float animInTime => _animationConfig.animInTime;
        public float animOutTime => _animationConfig.animOutTime;

        #endregion

        [SerializeField] protected AnimationConfig _animationConfig;

        public object parameter { get; protected set; }
        public UniTaskCompletionSource<ParamOutput> uiResult = new();
        protected qtUiManager uiManager => qtDependencyInjection.Get<qtUiManager>();

        public delegate void OnRemoveEvent();

        public OnRemoveEvent onRemoveEvent;

        public delegate void OnBeforeUIHide();

        public OnBeforeUIHide onBeforeUIHide;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        public new CancellationTokenSource destroyCancellationToken = new CancellationTokenSource();
        public Func<qtUiObject, UniTask> overrideAnimShow;
        public Func<qtUiObject, UniTask> overrideAnimHide;

        public bool isActive { get; protected set; }

        public virtual void PreInit()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        #region ----- Show -----

        public async UniTask Show(object param = null)
        {
            parameter = param;
            await _WaitToShowed();
        }

        private async UniTask _WaitToShowed()
        {
            gameObject.SetActive(true);
            isActive = true;
            if (overrideAnimShow != null)
            {
                await overrideAnimShow.Invoke(this);
                overrideAnimShow = null;
            }
            else
            {
                await _AnimatedShow();
            }
        }

        protected abstract UniTask _AnimatedShow();

        #endregion

        #region ----- Hide -----

        public virtual async UniTaskVoid ControllerHide()
        {
            await uiManager.Hide(this, true);
        }

        public async UniTask Hide(bool inactivePreviousScene)
        {
            await _WaitToHide(inactivePreviousScene);
        }

        private async UniTask _WaitToHide(bool inactivePreviousScene)
        {
            // await _BeforeHide();
            if (overrideAnimHide != null)
            {
                await overrideAnimHide.Invoke(this);
                overrideAnimHide = null;
            }
            else
            {
                await _AnimatedHide();
            }

            // Some callback is call after itself destroyed
            if (gameObject == null)
            {
                return;
            }

            if (inactivePreviousScene)
            {
                await _BeforeHide();
                gameObject.SetActive(false);
            }

            isActive = false;

            if (destroyCancellationToken != null)
            {
                destroyCancellationToken.Cancel();
            }

            destroyCancellationToken = new CancellationTokenSource();
        }

        protected virtual UniTask _BeforeHide()
        {
            onBeforeUIHide?.Invoke();
            return UniTask.CompletedTask;
        }

        protected abstract UniTask _AnimatedHide();

        #endregion

        [Serializable]
        public class AnimationConfig
        {
            [SerializeField] private PopupAnimType _animIn = PopupAnimType.Zoom;
            [SerializeField] private float _animInTime = 0.1f;
            [Space] [SerializeField] private PopupAnimType _animOut = PopupAnimType.Zoom;
            [SerializeField] private float _animOutTime = 0.1f;

            public PopupAnimType animIn => _animIn;
            public float animInTime => _animInTime;
            public PopupAnimType animOut => _animOut;
            public float animOutTime => _animOutTime;
        }
    }
}