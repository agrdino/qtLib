using Cysharp.Threading.Tasks;
using qtLib.UI.UIManager;

namespace qtLib.UI.SubScene
{
    public abstract class SubSceneMediator<TUI, TLogic> where TUI : UI.SubScene.SubScene where TLogic : SubSceneLogic
    {
        protected TUI _ui;
        protected TLogic _logic;
        
        protected SubSceneMediator(TUI ui, TLogic logic)
        {
            _ui = ui;
            _logic = logic;
        }
        
        protected virtual void ConfigUI()
        {
            _logic.Initialize();
        }
        
        protected virtual void BeforeUIShow()
        {
        }

        protected virtual void RemoveEvent()
        {
        }

        public virtual void Show()
        {
            ConfigUI();
            BeforeUIShow();
        }

        public virtual void Hide()
        {
            RemoveEvent();
        }
    }

    public abstract class SubSceneMediator<TUI, TLogic, TParam> : SubSceneMediator<TUI, TLogic>, UI.SubScene.IRequestData<TParam>
        where TUI : UI.SubScene.SubScene where TLogic : SubSceneLogic where TParam : ParamInput
    {
        protected TParam _args;
        protected SubSceneMediator(TUI ui, TLogic logic, TParam args) : base(ui, logic)
        {
            _args = args;
        }

        public abstract UniTask<TParam> RequestData();

        public override void Show()
        {
            if (_args == null)
            {
                RequestData().Forget();
            }
            base.Show();
        }
    }
}