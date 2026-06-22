namespace Extension
{
    public interface IClickable
    {
        public bool CanClick();
        public void OnBeginClick(){}
        public void OnDragOut(){}
        public void OnEndClick(){}
    }
}