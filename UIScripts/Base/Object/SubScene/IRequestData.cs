using Cysharp.Threading.Tasks;
using qtLib.UI.Base;

namespace qtLib.UIScripts.Base.Object.SubScene
{
    public interface IRequestData<TParam> where TParam : ParamInput
    {
        public UniTask<TParam> RequestData();
    }
}