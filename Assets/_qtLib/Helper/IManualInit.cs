using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace qtLib.Helper
{
    public interface IManualInit
    {
        public UniTask ManualInit();
    }
}