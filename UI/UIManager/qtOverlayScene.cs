using Cysharp.Threading.Tasks;

namespace qtLib.UI.UIManager
{
    public class qtOverlayScene : qtUiObject
    {
        protected override UniTask _AnimatedShow()
        {
            return UniTask.CompletedTask;
        }

        protected override UniTask _AnimatedHide()
        {
            return UniTask.CompletedTask;
        }
    }
}
