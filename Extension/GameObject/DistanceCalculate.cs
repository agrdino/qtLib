using System;
using UnityEngine;

namespace Extension
{
    public class DistanceCalculate : MonoBehaviour
    {
        #region ----- Component Config -----

        [SerializeField] private Transform _target;

        #endregion

        #region ----- Unity Event -----

        private void FixedUpdate()
        {
            if (_target == null)
            {
                return;
            }
            
            Debug.Log($"From {_target.name} to {_target.gameObject.name}: {Vector3.Distance(transform.position, _target.position)}");
        }

        #endregion
    }
}