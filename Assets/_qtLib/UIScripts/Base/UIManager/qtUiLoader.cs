using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using PimDeWitte.UnityMainThreadDispatcher;
using qtLib.Helper;
using Unity.Collections;
using UnityEngine;

namespace qtLib.UI.Base
{
    public abstract partial class qtUiLoader<TUI> : qtUiLoader where TUI : qtUiObject
    {
        private Dictionary<string, TUI> _uiElements = new Dictionary<string, TUI>();
        [ReadOnly] [SerializeField] protected List<TUI> _allElemental = new List<TUI>();

        public delegate void OnAddNew(qtUiLoader<TUI> loader, TUI newUI);
        public OnAddNew onAdd;
        
        public delegate void OnShow(qtUiLoader<TUI> loader, TUI newUI);
        public OnShow onAfterShow;
        public OnShow onAfterHided;

        public delegate UniTask OnBeforeShow(qtUiLoader<TUI> loader, TUI newUI, bool inactivePreviousScene);
        [CanBeNull] public OnBeforeShow onBeforeShow;

        public delegate UniTask OnBeforeHide();
        [CanBeNull] public OnBeforeHide onBeforeHide;
        
        public async UniTask<TUI> GetOrLoad(string uiName)
        {
            var uiView = Get(uiName);
            if (uiView)
            {
                // AddView(viewName, _uiView);                
            }
            else
            {
                uiView = await Load(uiName);
                if (uiView)
                {
                    onAdd?.Invoke(this, uiView);
                    Add(uiName, uiView);
                }
                return uiView;
                // _uiView.Hide();
            }

            return uiView;
        }

        public TUI Get(string name)
        {
            if (!_uiElements.ContainsKey(name)) return null;
            if (_uiElements[name].isActive)
            {
                return null;
            }
            return _uiElements[name];
        }
        
        private async UniTask<TUI> Load(string uiName)
        {
            ResourceRequest viewAsset = null;
            TUI ui = null;
            try
            {
                await UniTask.SwitchToMainThread();
                viewAsset = Resources.LoadAsync<TUI>(uiName);
                while (!viewAsset.isDone)
                {
                    if (viewAsset.asset == null)
                    {
                        throw new UnityException("Failed to load UI: " + uiName);
                    }
                    await UniTask.Yield();
                }

                ui = await InstanceUiObject(viewAsset, uiName);
                if (ui == null)
                {
                    return null;
                }
                else
                {
                    return ui;
                }
            }
            catch (Exception e)
            {
                qtDebug.LogError($"{uiName} - {e.Message}");
                return null;
            }
        }
        
        private async UniTask<TUI> InstanceUiObject(ResourceRequest resourceRequest, string uiName)
        {
            if (resourceRequest.asset == null) {
                qtDebug.LogError($"Fail to get {uiName}");
            }
            var ui = Instantiate((TUI)resourceRequest.asset, transform);
            ui.gameObject.SetActive(false);

            ui.PreInit();
            
            await UniTask.Yield();

            ui.transform.name = ui.transform.name.Replace("(Clone)", string.Empty);

            _allElemental.Add(ui);

            Add(resourceRequest.asset.name, ui);

            return ui;
        }
        
        private void Add(string key, TUI view)
        {
            if (_uiElements.ContainsKey(key))
            {
                _uiElements[key] = view;
            }
            else
            {
                _uiElements.Add(key, view);
            }
        }
        
        public async UniTask Hide(TUI ui, bool inactivePreviousScene, Action<TUI> anotherCallback = null)
        {
            if (ui != null)
            {
                // onBeforeHide?.Invoke();
                await ui.Hide(inactivePreviousScene);
                await UniTask.WaitUntil(() => inactivePreviousScene && !ui.gameObject.activeInHierarchy);
                onAfterHided?.Invoke(this, ui);
                anotherCallback?.Invoke(ui);
            }
        }
    }

    public class qtUiLoader : MonoBehaviour
    {
        
    }
}