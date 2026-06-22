using Cysharp.Threading.Tasks;

namespace qtLib.UI.Base
{
    public class qtLogic
    {
        public object param = null;
        
        public virtual async UniTask FlowInit()
        {
            await Initialize();
        }

        public virtual async UniTask Initialize()
        {
            await UniTask.Yield();
        }
        
        public virtual void HideScene(){}
    }
    
    public class qtLogic<T> : qtLogic
    {
        public T Args => (T)this.param;
        public qtLogic() { }
        public qtLogic(T param) => this.param = param;
    }
}