using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using qtLib.Helper;
using UnityEngine;

namespace qtLib.UI.Base
{
    [DefaultExecutionOrder(-49)]
    public class qtSceneMediator : MonoBehaviour
    {
        private qtUiManager _uiManager => qtDependencyInjection.Get<qtUiManager>();
        private List<qtScene> _allViews = new List<qtScene>();

        private void Awake()
        {
            qtDependencyInjection.Add(this);

            var viewLoader = _uiManager.GetLoader<qtScene>();
            viewLoader.onBeforeShow += BeforeViewShow;
            viewLoader.onAfterShow += AfterViewShow;
            viewLoader.onAfterHided += AfterViewHided;
            viewLoader.onBeforeHide += BeforeViewHide;
        }
        
        private async UniTask BeforeViewShow(qtUiLoader<qtUiObject> loader, qtUiObject newUI, bool inactivePreviousScene)
        {
            qtDebug.Log("<color=yellow>Show: " + newUI.GetType().ToString() + "</color>");
            _allViews ??= new List<qtScene>();
            if (newUI != null)
            {
                await UniTask.SwitchToMainThread();
                var uiManager = qtDependencyInjection.Get<qtUiManager>();
                for (int i = 0; i < _allViews.Count; i++)
                {
                    if (_allViews[i] && _allViews[i].gameObject && !_allViews[i].Equals(newUI))
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

                if (_allViews.Contains(newUI as qtScene) == false)
                {
                    if (newUI.GetType().IsSubclassOf(typeof(qtScene)) || newUI is qtScene)
                        _allViews.Add(newUI as qtScene);
                }
            }
        }

        private void AfterViewShow(qtUiLoader loader, qtUiObject newUI)
        {

        }
        
        private async UniTask BeforeViewHide()
        {
            _allViews ??= new List<qtScene>();
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

        private void AfterViewHided(qtUiLoader<qtUiObject> loader, qtUiObject newUI)
        {
            // var _typeUI = newUI.GetType();
            // if (_branches.ContainsKey(_typeUI))
            // {
            //     if (currentSceneBranch != null && _branches[_typeUI].IsBranch(newUI))
            //     {
            //         int index = currentSceneBranch.GetIndex(newUI);
            //     }
            // }
        }
    }
}
