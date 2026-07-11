using Cysharp.Threading.Tasks;

namespace qtLib.UI.UIManager
{
    public class qtScene : qtUiObject
    {
        protected override UniTask _AnimatedHide()
        {
            return uiManager.SceneFadingIn(this);
        }

        protected override UniTask _AnimatedShow()
        {
            return uiManager.SceneFadingOut(this);
        }
    }
}
