using Cysharp.Threading.Tasks;

namespace qtLib.UI.Base
{
    public class qtOverlayScene : qtUiObject
    {
        protected override UniTask _AnimatedShow()
        {
            return UniTask.CompletedTask;
        }

        public override async void ControllerHide()
        {
            await uiManager.BeforeUIHide(this);
            base.ControllerHide();
        }

        protected override UniTask _AnimatedHide()
        {
            return UniTask.CompletedTask;
        }
    }
}