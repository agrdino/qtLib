using qtLib.UI.UIManager;

namespace qtLib.UI.SubScene
{
    public class SubSceneLogic
    {
        public virtual void Initialize(){}
    }
    
    public class SubSceneLogic<TParam> : SubSceneLogic where TParam : ParamInput
    {
        protected TParam _param;
    }
}