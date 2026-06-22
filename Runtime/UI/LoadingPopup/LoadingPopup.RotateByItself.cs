using DG.Tweening;
using UnityEngine;

namespace _Scripts.Popup.LoadingPopup
{
    public class RotateByItself : MonoBehaviour
    {
        private void Start()
        {
            transform.DORotate(-360 * Vector3.forward, 1f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental);
        }
    }
}