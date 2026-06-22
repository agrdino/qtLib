using System;
using qtLib.Game.Object;
using UnityEngine;

namespace qtLib.Game.Controller
{
    public class SamplePool : SystemPool<PoolingObject>
    {
        #region ----- Component Config -----

        private PoolingObject _prefab;

        #endregion
        
        #region ----- Constructor -----

        public SamplePool(int id) : base()
        {
            throw new NotImplementedException();
        }

        #endregion
        protected override PoolingObject CreatePooledItem()
        {
            PoolingObject go = GameObject.Instantiate(_prefab);
            go.transform.position = Vector3.zero;
            return go;
        }
    }
}