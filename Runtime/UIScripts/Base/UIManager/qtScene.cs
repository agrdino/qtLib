using Cysharp.Threading.Tasks;

namespace qtLib.UI.Base
{
    public class qtScene : qtUiObject
    {
        #region ----- Implement Function -----

        protected override UniTask _AnimatedHide()
        {
            return uiManager.SceneFadingIn(this);
        }

        protected override UniTask _AnimatedShow()
        {
            return uiManager.SceneFadingOut(this);
        }

        #endregion
    }
}