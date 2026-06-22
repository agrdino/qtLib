using qtLib.Game.Object;
using UnityEngine.Pool;

namespace qtLib.Game.Controller
{
    public abstract class SystemPool<T> : IObjectPool<T> where T : PoolingObject
    {
        #region ----- Component Config -----

        // Collection checks will throw errors if we try to release an item that is already in the pool.
        protected virtual bool CollectionChecks() => true;
        protected virtual int MaxPoolSize() => 20;
        protected virtual int DefaultPoolSize() => 20;

        private IObjectPool<T> _pool;
        public int CountInactive => _pool.CountInactive;

        #endregion
        
        #region ----- Property -----

        protected IObjectPool<T> Pool
        {
            get
            {
                if (_pool == null)
                {
                    _pool = new ObjectPool<T>(CreatePooledItem, _OnTakeFromPool, _OnReturnedToPool,
                        _OnDestroyPoolObject, CollectionChecks(), DefaultPoolSize(), MaxPoolSize());
                }

                return _pool;
            }
        }

        #endregion

        #region ----- Private Function -----

        protected abstract T CreatePooledItem();

        // Called when an item is returned to the pool using Release
        private void _OnReturnedToPool(T obj)
        {
            obj.OnRelease();
        }

        // Called when an item is taken from the pool using Get
        private void _OnTakeFromPool(T obj)
        {
            obj.OnGet();
        }

        // If the pool capacity is reached then any items returned will be destroyed.
        // We can control what the destroy behavior does, here we destroy the GameObject.
        private void _OnDestroyPoolObject(T obj)
        {
            obj.OnRelease();
            UnityEngine.Object.Destroy(obj.GameObject);
        }

        #endregion

        #region ----- Implement Function -----

        public T Get()
        {
            return Pool.Get();
        }

        public PooledObject<T> Get(out T v)
        {
            return Pool.Get(out v);
        }

        public void Release(T element)
        {
            lock (Pool)
            {
                Pool.Release(element);
            }
        }

        public void Clear()
        {
            Pool.Clear();
        }


        #endregion
    }
}