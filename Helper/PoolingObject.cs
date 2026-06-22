using UnityEngine;

namespace qtLib.Game.Object
{
    public abstract class PoolingObject : MonoBehaviour, IPoolingObject
    {
        public abstract object ObjectPoolID { get; }

        public GameObject GameObject => gameObject;
        
        public virtual void OnGet()
        {
        }

        public virtual void OnRelease()
        {
        }
    }
}
