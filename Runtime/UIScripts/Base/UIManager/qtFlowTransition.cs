using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using qtLib.Helper;

namespace qtLib.UI.Base
{
    public class qtFlowTransition<TUI, TLogic> : IDisposable
        where TUI : qtUiObject 
        where TLogic : qtLogic
    {
        public delegate UniTask fncVL(TUI ui, TLogic logic);
        protected fncVL _beforeUIShow = null;

        public TLogic logic;
        
        public static qtUiManager uiManager => qtDependencyInjection.Get<qtUiManager>();

        public qtFlowTransition(TLogic logic)
        {
            this.logic = logic;
        }

        public virtual async UniTask<(TUI, TLogic)> Move(bool inactivePreviousScene, object param = null, bool setAsLastSibling = true)
        {
            TUI ui = null;

            if (param != null)
            {
                logic.param = param;
            }
            
            await BeforeUIHide<TUI>();
            var allTaskNeed = new List<UniTask>
            {
                //Clone scene
                LogicInit(),
                uiManager.Load<TUI>().ContinueWith(result =>
                {
                    ui = result;
                })
            };

            await UniTask.WhenAll(allTaskNeed);
            if (_beforeUIShow != null)
            {
                await _beforeUIShow.Invoke(ui, logic);
                await UniTask.Yield();
            }
            
            //Hide ở đây
            ui = await UIShow<TUI>(inactivePreviousScene, param, setAsLastSibling);

            return (ui, logic as TLogic);
        }

        protected async UniTask LogicInit()
        {
            await this.logic.FlowInit();
        }

        protected virtual async UniTask BeforeUIHide<tUI>() where tUI : qtUiObject
        {
            await uiManager.BeforeUIHide<tUI>();
        }
        
        protected virtual async UniTask<tUI> UIShow<tUI>(bool inactivePreviousScene, object data, bool setAsLastSibling = true) where tUI : qtUiObject
        {
            var ui =  await uiManager.Show<tUI>(inactivePreviousScene, data, setAsLastSibling);

            return ui;
        }

        public qtFlowTransition<TUI, TLogic> BeforeUIShow(fncVL beforeUIShow)
        {
            _beforeUIShow = beforeUIShow; 
            return this;
        }

        public void Dispose()
        {
        }
    }
}