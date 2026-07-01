namespace qtLib.UIScripts.Base.Object.SubScene
{
    public abstract class SubSceneMediator<TUI, TLogic> where TUI : UIScripts.Base.Object.SubScene.SubScene where TLogic : SubSceneLogic
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
}