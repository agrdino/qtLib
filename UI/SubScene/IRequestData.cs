using Cysharp.Threading.Tasks;
using qtLib.UI.UIManager;

namespace qtLib.UI.SubScene
{
    public interface IRequestData<TParam> where TParam : ParamInput
    {
        public UniTask<TParam> RequestData();
    }
}