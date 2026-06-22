namespace Extension
{
    public interface IImmovable
    {
        public bool CanDrag();
        public void OnBeginDrag();
        public void OnEndDrag();
    }
}