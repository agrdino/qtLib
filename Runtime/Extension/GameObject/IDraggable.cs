using UnityEngine;

namespace Extension
{
    public interface IDraggable
    {
        public GameObject GameObject();
        public bool CanDrag();
        public void OnBeginDrag(Vector3 worldPosition);
        public void OnDrag(Vector3 worldPosition);
        public void OnEndDrag(bool force = false);
    }
}