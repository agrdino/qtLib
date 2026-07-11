using qtLib.UI.Base;

namespace qtLib.UIScripts.Base.Object.SubScene
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