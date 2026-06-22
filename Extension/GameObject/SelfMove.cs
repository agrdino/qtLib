using System;
using DG.Tweening;
using UnityEngine;

namespace Extension
{
    public class SelfMove : MonoBehaviour
    {
        #region ----- Component Config -----

        [SerializeField] private Vector3 _from;
        [SerializeField] private Vector3 _target;
        [SerializeField] private float _time;

        [SerializeField] private int _loop;
        [SerializeField] private LoopType _loopType = LoopType.Restart;

        [SerializeField] private Ease _ease = Ease.Linear;

        #endregion

        private void Start()
        {
            transform.position = _from;
            transform.DOMove(_target, _time).SetEase(_ease).SetLoops(_loop, _loopType);
        }
    }
}