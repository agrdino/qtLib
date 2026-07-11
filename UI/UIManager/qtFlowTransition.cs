using System;
using Cysharp.Threading.Tasks;
using qtLib.Helper;

namespace qtLib.UI.UIManager
{
    public class qtFlowTransition<TUI, TLogic> : IDisposable
        where TUI : qtUiObject
        where TLogic : qtLogic
    {
        public delegate UniTask fncVL(TUI ui, TLogic logic);

        protected fncVL _beforeUIShow;
        private TUI _preparedView;

        // Public field and lower-case property name are kept for source compatibility.
        public TLogic logic;
        public static qtUiManager uiManager => qtDependencyInjection.Get<qtUiManager>();

        public qtFlowTransition(TLogic logic)
        {
            this.logic = logic ?? throw new ArgumentNullException(nameof(logic));
        }

        public virtual async UniTask<(TUI, TLogic)> Move(object param = null)
        {
            var manager = uiManager;
            if (manager == null)
            {
                throw new InvalidOperationException("qtUiManager is not registered.");
            }

            // Always assign the parameter so a reused logic object cannot retain stale data.
            logic.param = param;

            await BeforeUIHide<TUI>();

            TUI loadedView = null;

            async UniTask LoadView()
            {
                loadedView = await manager.Load<TUI>(param);
            }

            await UniTask.WhenAll(LogicInit(), LoadView());

            if (!loadedView)
            {
                throw new InvalidOperationException(
                    $"Unable to load UI '{typeof(TUI).FullName}'.");
            }

            _preparedView = loadedView;

            // Prepare the per-show result source before configuration callbacks capture it.
            loadedView.PrepareForShow(param);

            try
            {
                if (_beforeUIShow != null)
                {
                    await _beforeUIShow.Invoke(loadedView, logic);
                }

                var shownView = await UIShow<TUI>(param);

                if (!shownView)
                {
                    throw new InvalidOperationException(
                        $"Unable to show UI '{typeof(TUI).FullName}'.");
                }

                if (shownView != loadedView)
                {
                    loadedView.AbortPreparedShow();
                }

                _preparedView = null;
                return (shownView, logic);
            }
            catch
            {
                loadedView.AbortPreparedShow();
                _preparedView = null;
                throw;
            }
        }

        protected virtual UniTask LogicInit()
        {
            return logic.FlowInit();
        }

        protected virtual UniTask BeforeUIHide<TView>() where TView : qtUiObject
        {
            return uiManager.BeforeUIHide<TView>();
        }

        protected virtual UniTask<TView> UIShow<TView>(object data)
            where TView : qtUiObject
        {
            return uiManager.Show<TView>(
                data,
                preparedView: _preparedView as TView);
        }

        public qtFlowTransition<TUI, TLogic> BeforeUIShow(fncVL beforeUIShow)
        {
            _beforeUIShow = beforeUIShow;
            return this;
        }

        public void Dispose()
        {
            _beforeUIShow = null;
            _preparedView = null;
        }
    }
}
