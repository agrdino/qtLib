using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using qtLib.Helper;
using UnityEngine;

namespace qtLib.UI.Base
{
    [DefaultExecutionOrder(-50)]
    public class qtUiManager : MonoBehaviour
    {
        [SerializeField] private UILoader _mainCanvas;
        [SerializeField] private UILoader _overlaySceneCanvas;
        [SerializeField] private UILoader _canvasOnTop;
        [SerializeField] private CanvasGroup _imgSceneFading;
        [SerializeField] private CanvasGroup _imgPopupFading;

        private List<CanvasGroup> _imgPopupFadings = new ();
        public delegate void OnShowed(qtScene view);
        public OnShowed onShow = null;

        public delegate void OnHided(qtScene view);
        public OnHided onHide = null;

        // public static Vector2 CanvasSize = new Vector2(1054, 596);
        //
        // [SerializeField] protected UIViewLoader _viewManager;
        // public UIViewLoader viewManager => _viewManager;
        //
        // [SerializeField] protected UIPopupLoader _popupManager;
        // public UIPopupLoader popupManager => _popupManager;
        //
        // [SerializeField] protected UIPanelLoader _panelManager;
        // public UIPanelLoader panelManager => _panelManager;
        
        private void Awake()
        {
            qtDependencyInjection.Add(this);
        }
        
        public async UniTask BeforeUIHide<TUI>() where TUI : qtUiObject
        {
            string uiName = typeof(TUI).Name;
            UILoader loader = GetLoader<TUI>();
            try
            {
                if (loader.onBeforeHide != null)
                {
                    await loader.onBeforeHide.Invoke();
                }
            }
            catch (Exception e)
            {
                qtDebug.LogError($"{uiName} - Is Error at Show/Request Bundle or Animation Function - {e.Message}");
            }
        }
        
        public async UniTask<TUI> Show<TUI>(bool inactivePreviousScene, object param = null, bool setAsLastSibling = true, System.Action<TUI> result = null) where TUI : qtUiObject
        {
            qtUiObject ui = null;
            string uiName = typeof(TUI).Name;
            var loader = GetLoader<TUI>();
            ui = await loader.GetOrLoad(uiName);
            if (setAsLastSibling)
            {
                ui.transform.SetAsLastSibling();
            }
            if (ui != null)
            {
                result?.Invoke(ui as TUI);

                try
                {
                    if (loader.onBeforeShow != null)
                    {
                        await loader.onBeforeShow.Invoke(loader, ui, inactivePreviousScene);
                    }
                    await ui.Show(param);
                    loader.onAfterShow?.Invoke(loader, ui);
                }
                catch (Exception e)
                {
                    qtDebug.LogError($"{uiName} - Is Error at Show/Request Bundle or Animation Function - {e.Message}");
                }
            }

            return ui as TUI;
        }
        
        public async UniTask<TUI> Load<TUI>(object param = null) where TUI : qtUiObject
        {
            qtUiObject ui = null;
            string uiName = typeof(TUI).Name;

            ui = await GetLoader<TUI>().GetOrLoad(uiName);
            
            return ui as TUI;
        }
        
        public UILoader GetLoader<TUI>()
        {
            if (typeof(TUI).IsSubclassOf(typeof(qtOverlayScene)) || typeof(TUI) == typeof(qtOverlayScene))
            {
                return _overlaySceneCanvas;
            }

            if (typeof(TUI).IsSubclassOf(typeof(qtScene)) || typeof(TUI) == typeof(qtScene))
            {
                return _mainCanvas;
            }

            if (typeof(TUI).IsSubclassOf(typeof(qtPopup)) || typeof(TUI) == typeof(qtPopup))
            {
                return _canvasOnTop;
            }

            return null;
        }
        
        private UILoader GetLoader(qtUiObject instance)
        {
            if (instance.GetType().IsSubclassOf(typeof(qtOverlayScene)) || instance is qtOverlayScene)
            {
                return _overlaySceneCanvas;
            }
            
            if (instance.GetType().IsSubclassOf(typeof(qtScene)) || instance is qtScene)
            {
                return _mainCanvas;
            }
        
            if (instance.GetType().IsSubclassOf(typeof(qtPopup)) || instance is qtPopup)
            {
                return _canvasOnTop;
            }
        
            return null;
        }
        
        public UniTask BeforeUIHide(qtUiObject view)
        {
            if (view == null)
            {
                return UniTask.CompletedTask;
            }

            return UniTask.CompletedTask;
        }
        
        public async UniTask Hide(qtUiObject view, bool inactivePreviousScene = true)
        {
            if (view == null)
            {
                return;
            }

            await GetLoader(view).Hide(view, inactivePreviousScene);
        }
        
        public async UniTask SceneFadingIn(qtScene scene)
        {
            _imgSceneFading.gameObject.SetActive(true);
            _imgSceneFading.transform.SetAsLastSibling();
            await _imgSceneFading.DOFade(1, scene.animOutTime)
                .SetUpdate(UpdateType.Normal, true)
                .ToUniTask();
        }

        public async UniTask SceneFadingOut(qtScene scene)
        {
            if (!_imgSceneFading.gameObject.activeInHierarchy)
            {
                _imgSceneFading.alpha = 1;
                _imgSceneFading.gameObject.SetActive(true);
            }
            await _imgSceneFading.DOFade(0, scene.animInTime)
                .SetUpdate(UpdateType.Normal,true)
                .OnComplete(() => _imgSceneFading.gameObject.SetActive(false))
                .ToUniTask();
        }

        public CanvasGroup PopupFadeIn(qtPopup popup)
        {
            CanvasGroup imgFading = _imgPopupFadings.Find(x => !x.gameObject.activeInHierarchy);
            if (imgFading == null)
            {
                imgFading = Instantiate(_imgPopupFading, _imgPopupFading.transform.parent);
                _imgPopupFadings.Add(imgFading);
            }
            
            imgFading.transform.SetAsLastSibling();
            popup.transform.SetAsLastSibling();
            
            imgFading.alpha = 0;
            imgFading.gameObject.SetActive(true);
            imgFading.DOFade(0.8f, popup.animInTime)
                .SetUpdate(UpdateType.Normal, true);
            return imgFading;
        }

        public void PopupFadeOut(CanvasGroup imgFading, qtPopup popup)
        {
            if (!imgFading.gameObject.activeInHierarchy)
            {
                imgFading.alpha = 0.8f;
                imgFading.gameObject.SetActive(true);
            }

            imgFading.DOFade(0, popup.animOutTime)
                .SetUpdate(UpdateType.Normal, true)
                .OnComplete(() => imgFading.gameObject.SetActive(false));
        }
    }
}