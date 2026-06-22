using Cysharp.Threading.Tasks;
using qtLib.Helper;
using UnityEngine;

namespace qtLib.UI.Base
{
    public class qtPopupMediator: MonoBehaviour
    {
        private qtUiManager _uiManager => qtDependencyInjection.Get<qtUiManager>();

        private void Awake()
        {
            qtDependencyInjection.Add(this);
            
            var viewLoader = _uiManager.GetLoader<qtPopup>();
            viewLoader.onBeforeShow += BeforeViewShow;
            viewLoader.onAfterShow += AfterViewShow;
            viewLoader.onAfterHided += AfterViewHided;
            viewLoader.onBeforeHide += BeforeViewHide;
        }

        private UniTask BeforeViewShow(qtUiLoader<qtUiObject> loader, qtUiObject newUI, bool inactivePreviousScene)
        {
            return UniTask.CompletedTask;
        }

        private void AfterViewShow(qtUiLoader loader, qtUiObject newUI)
        {

        }

        private void AfterViewHided(qtUiLoader<qtUiObject> loader, qtUiObject newUI)
        {
        }
        
        private UniTask BeforeViewHide()
        {
            return UniTask.CompletedTask;
        }
    }
}