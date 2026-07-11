using Cysharp.Threading.Tasks;
using qtLib.UI.Base;

namespace qtLib.UIScripts.Base.Object.SubScene
{
    public abstract class SubSceneMediator<TUI, TLogic> where TUI : SubScene where TLogic : SubSceneLogic
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

    public abstract class SubSceneMediator<TUI, TLogic, TParam> : SubSceneMediator<TUI, TLogic>, IRequestData<TParam>
        where TUI : SubScene where TLogic : SubSceneLogic where TParam : ParamInput
    {
        protected TParam _param;
        protected SubSceneMediator(TUI ui, TLogic logic, TParam param) : base(ui, logic)
        {
        }

        public abstract UniTask<TParam> RequestData();

        public override void Show()
        {
            if (_param == null)
            {
                RequestData().Forget();
            }
            base.Show();
        }

        public override void Hide()
        {
            _param = null;
            base.Hide();
        }
    }
}