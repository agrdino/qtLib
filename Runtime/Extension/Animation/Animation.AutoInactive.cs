using Cysharp.Threading.Tasks;
using UnityEngine;

namespace qtLib.Extension.Animation
{
    public class AutoInactive : MonoBehaviour
    {
        #region ----- Component Config -----

        [SerializeField] private float _delay;

        #endregion

        #region ----- Public Function -----

        public async UniTaskVoid Inactive()
        {
            await UniTask.Delay((int)(_delay * 1000));
            gameObject.SetActive(false);
        }

        #endregion
    }
}