using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using qtLib.Helper;
using UnityEngine;

namespace qtLib.UI.Base
{
    [DefaultExecutionOrder(-49)]
    public class qtOverlaySceneMediator : MonoBehaviour
    {
        private qtUiManager _uiManager => qtDependencyInjection.Get<qtUiManager>();
        private List<qtOverlayScene> _allViews = new List<qtOverlayScene>();

        private void Awake()
        {
            qtDependencyInjection.Add(this);

            var viewLoader = _uiManager.GetLoader<qtOverlayScene>();
            viewLoader.onBeforeShow += BeforeViewShow;
            viewLoader.onAfterShow += AfterViewShow;
            viewLoader.onAfterHided += AfterViewHided;
            viewLoader.onBeforeHide += BeforeViewHide;
        }

        private async UniTask BeforeViewShow(qtUiLoader<qtUiObject> loader, qtUiObject newUI, bool inactivePreviousScene)
        {
            qtDebug.Log("<color=yellow>Show: " + newUI.GetType().ToString() + "</color>");
            _allViews ??= new List<qtOverlayScene>();
            
            if (newUI != null)
            {
                await UniTask.SwitchToMainThread();
                var uiManager = qtDependencyInjection.Get<qtUiManager>();
                for (int i = 0; i < _allViews.Count; i++)
                {
                    if (_allViews[i] && _allViews[i].gameObject && _allViews[i].Equals(newUI) == false)
                    {
                        await uiManager.Hide(_allViews[i], inactivePreviousScene);
                        if (inactivePreviousScene)
                        {
                            _allViews.RemoveAt(i);
                            i--;
                        }
                    }
                    else
                    {
                        _allViews.RemoveAt(i);
                        i--;
                    }
                }

                if (_allViews.Contains(newUI as qtOverlayScene) == false)
                {
                    if (newUI.GetType().IsSubclassOf(typeof(qtOverlayScene)) || newUI is qtOverlayScene)
                    {
                        _allViews.Add(newUI as qtOverlayScene);
                    }
                }
            }
        }

        private void AfterViewShow(qtUiLoader loader, qtUiObject newUI)
        {

        }
        
        private void AfterViewHided(qtUiLoader<qtUiObject> loader, qtUiObject newUI)
        {
        }
        
        private async UniTask BeforeViewHide()
        {
            _allViews ??= new List<qtOverlayScene>();
            await UniTask.SwitchToMainThread();
            
            var uiManager = qtDependencyInjection.Get<qtUiManager>();
            for (int i = 0; i < _allViews.Count; i++)
            {
                if (_allViews[i] && _allViews[i].gameObject)
                {
                    await uiManager.BeforeUIHide(_allViews[i]);
                }
            }
        }
    }
}
