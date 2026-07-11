using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using qtLib.Helper;
using UnityEngine;

namespace qtLib.UI.UIManager
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
        public enum PopupAnimType
        {
            Zoom,
            Left,
            Right,
            Fade,
            Immediately
        }

        [SerializeField] protected AnimationConfig _animationConfig = new AnimationConfig();

        // Legacy names are intentionally preserved to avoid breaking existing UI scripts.
        public object parameter { get; protected set; }
        public UniTaskCompletionSource<ParamOutput> uiResult = new UniTaskCompletionSource<ParamOutput>();
        public bool isActive { get; protected set; }

        public float animInTime => Animation.animInTime;
        public float animOutTime => Animation.animOutTime;

        public object Parameter => parameter;
        public bool IsActive => isActive;
        public CancellationToken ViewCancellationToken =>
            destroyCancellationToken != null ? destroyCancellationToken.Token : CancellationToken.None;

        protected qtUiManager uiManager => qtDependencyInjection.Get<qtUiManager>();
        protected AnimationConfig Animation => _animationConfig ??= new AnimationConfig();

        public delegate void OnRemoveEvent();
        public OnRemoveEvent onRemoveEvent;

        public delegate void OnBeforeUIHide();
        public OnBeforeUIHide onBeforeUIHide;

        // Kept as CancellationTokenSource because existing consumers may cancel or link this source.
        public new CancellationTokenSource destroyCancellationToken = new CancellationTokenSource();

        public Func<qtUiObject, UniTask> overrideAnimShow;
        public Func<qtUiObject, UniTask> overrideAnimHide;

        private bool _isPreparedForShow;

        public virtual void PreInit()
        {
            RenewViewCancellationToken();
            RenewResultSource();
            _isPreparedForShow = false;
            isActive = false;
        }

        internal void PrepareForShow(object param)
        {
            parameter = param;

            if (_isPreparedForShow)
            {
                return;
            }

            RenewResultSource();
            _isPreparedForShow = true;
        }

        internal void AbortPreparedShow()
        {
            if (!_isPreparedForShow)
            {
                return;
            }

            _isPreparedForShow = false;
            uiResult?.TrySetCanceled();
        }

        public async UniTask Show(object param = null)
        {
            PrepareForShow(param);
            _isPreparedForShow = false;

            gameObject.SetActive(true);
            isActive = true;

            try
            {
                var customAnimation = overrideAnimShow;
                overrideAnimShow = null;

                if (customAnimation != null)
                {
                    await customAnimation.Invoke(this);
                }
                else
                {
                    await _AnimatedShow();
                }
            }
            catch
            {
                isActive = false;
                uiResult?.TrySetCanceled();

                if (this != null && gameObject != null)
                {
                    gameObject.SetActive(false);
                }

                throw;
            }
        }

        protected abstract UniTask _AnimatedShow();

        public virtual async UniTaskVoid ControllerHide()
        {
            var manager = uiManager;
            if (manager == null)
            {
                return;
            }

            await manager.Hide(this);
        }

        public async UniTask Hide()
        {
            var customAnimation = overrideAnimHide;
            overrideAnimHide = null;

            if (customAnimation != null)
            {
                await customAnimation.Invoke(this);
            }
            else
            {
                await _AnimatedHide();
            }

            // Unity objects can be destroyed while an animation is awaiting.
            if (this == null || gameObject == null)
            {
                return;
            }

            if (this is qtScene)
            {
                await _BeforeHide();
                gameObject.SetActive(false);

                // A close without an explicit output should still release result waiters.
                // Explicit results submitted by a hide callback win because TrySetResult
                // simply returns false once the source has already completed.
            }

            isActive = false;
            RenewViewCancellationToken();
        }

        protected virtual UniTask _BeforeHide()
        {
            onBeforeUIHide?.Invoke();
            onRemoveEvent?.Invoke();
            return UniTask.CompletedTask;
        }

        protected abstract UniTask _AnimatedHide();

        protected virtual void OnDestroy()
        {
            uiResult?.TrySetCanceled();
            CancelAndDispose(ref destroyCancellationToken);
        }

        private void RenewResultSource()
        {
            uiResult?.TrySetCanceled();
            uiResult = new UniTaskCompletionSource<ParamOutput>();
        }

        private void RenewViewCancellationToken()
        {
            CancelAndDispose(ref destroyCancellationToken);
            destroyCancellationToken = new CancellationTokenSource();
        }

        private static void CancelAndDispose(ref CancellationTokenSource source)
        {
            if (source == null)
            {
                return;
            }

            try
            {
                if (!source.IsCancellationRequested)
                {
                    source.Cancel();
                }
            }
            catch (ObjectDisposedException)
            {
                // The source is public for compatibility, so external code may have disposed it.
            }
            finally
            {
                source.Dispose();
                source = null;
            }
        }

        [Serializable]
        public class AnimationConfig
        {
            [SerializeField] private PopupAnimType _animIn = PopupAnimType.Zoom;
            [SerializeField] private float _animInTime = 0.1f;
            [Space]
            [SerializeField] private PopupAnimType _animOut = PopupAnimType.Zoom;
            [SerializeField] private float _animOutTime = 0.1f;

            public PopupAnimType animIn => _animIn;
            public float animInTime => _animInTime;
            public PopupAnimType animOut => _animOut;
            public float animOutTime => _animOutTime;
        }
    }
}
