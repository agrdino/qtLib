using Cysharp.Threading.Tasks;

namespace qtLib.UI.UIManager
{
    public abstract class qtLogic
    {
        // Kept public for backward compatibility with existing callers.
        public object param;

        public virtual UniTask FlowInit()
        {
            return Initialize();
        }

        public virtual async UniTask Initialize()
        {
            // Preserve the original one-frame asynchronous initialization point.
            await UniTask.Yield();
        }

        public virtual void HideScene()
        {
        }
    }

    public abstract class qtLogic<T> : qtLogic
    {
        public T Args => (T)param;

        public qtLogic()
        {
        }

        public qtLogic(T param)
        {
            this.param = param;
        }
    }
}
