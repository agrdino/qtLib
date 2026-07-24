using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using qtLib.CustomDebug;
using qtLib.Helper;
using UnityEngine;

namespace qtLib.UI.UIManager
{
    [DefaultExecutionOrder(-49)]
    public class qtSceneMediator : MonoBehaviour
    {
        private readonly List<qtScene> _allViews = new List<qtScene>();
        private UILoader _viewLoader;
        protected qtUiManager UiManager => qtDependencyInjection.Get<qtUiManager>();

        private void Awake()
        {
            qtDependencyInjection.Add(this);

            var manager = UiManager;
            if (manager == null)
            {
                qtDebug.LogError("qtPopupMediator - qtUiManager is not registered.");
                return;
            }

            _viewLoader = manager.GetLoader<qtScene>();
            if (!_viewLoader)
            {
                qtDebug.LogError("qtPopupMediator - Popup loader is not configured.");
                return;
            }

            _viewLoader.onBeforeShow += BeforeViewShow;
            _viewLoader.onAfterShow += AfterViewShow;
            _viewLoader.onAfterHided += AfterViewHidden;
            _viewLoader.onBeforeHide += BeforeViewHide;
        }

        private void OnDestroy()
        {
            if (!_viewLoader)
            {
                return;
            }

            _viewLoader.onBeforeShow -= BeforeViewShow;
            _viewLoader.onAfterShow -= AfterViewShow;
            _viewLoader.onAfterHided -= AfterViewHidden;
            _viewLoader.onBeforeHide -= BeforeViewHide;
            _viewLoader = null;
        }
        
        private async UniTask BeforeViewShow(
            qtUiLoader<qtUiObject> loader,
            qtUiObject newUI)
        {
            if (newUI is not qtScene newView)
            {
                return;
            }

            qtDebug.Log($"<color=yellow>Show: {newView.GetType()}</color>");
            await UniTask.SwitchToMainThread();

            for (var i = _allViews.Count - 1; i >= 0; i--)
            {
                var previousView = _allViews[i];
                if (!previousView || previousView == newView)
                {
                    _allViews.RemoveAt(i);
                    continue;
                }

                await UiManager.Hide(previousView);
            }

            if (!_allViews.Contains(newView))
            {
                _allViews.Add(newView);
            }
        }

        protected virtual void AfterViewShow(
            qtUiLoader<qtUiObject> loader,
            qtUiObject newUI)
        {
        }
        
        private void AfterViewHidden(
            qtUiLoader<qtUiObject> loader,
            qtUiObject hiddenUI)
        {
            if (hiddenUI is not qtScene view)
            {
                return;
            }

            // inactivePrevious=false intentionally leaves the GameObject active.
            if (!view || !view.gameObject.activeInHierarchy)
            {
                _allViews.Remove(view);
            }
        }

        private async UniTask BeforeViewHide()
        {
            await UniTask.SwitchToMainThread();

            for (var i = _allViews.Count - 1; i >= 0; i--)
            {
                var view = _allViews[i];
                if (!view || !view.gameObject)
                {
                    _allViews.RemoveAt(i);
                    continue;
                }

                await UiManager.BeforeUIHide(view);
            }
        }
    }
}
