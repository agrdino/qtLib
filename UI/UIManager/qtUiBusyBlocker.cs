using UnityEngine;
using UnityEngine.UI;

namespace qtLib.UI.UIManager
{
    /// <summary>
    /// Optional full-screen raycast blocker driven by qtUiFlow's internal transition
    /// state. The blocker can be disabled without disabling duplicate-request guards.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class qtUiBusyBlocker : MonoBehaviour
    {
        [SerializeField] private Image _blocker;
        
        private void OnEnable()
        {
            qtUiFlow.RaycastBlockStateChanged += HandleRaycastBlockStateChanged;
            ApplyBlockState(qtUiFlow.ShouldBlockRaycasts);
        }

        private void OnDisable()
        {
            qtUiFlow.RaycastBlockStateChanged -= HandleRaycastBlockStateChanged;
            ApplyBlockState(false);
        }

        /// <summary>
        /// Enables or disables automatic raycast blocking globally. This does not
        /// change qtUiFlow's internal busy state or its double-click protection.
        /// </summary>
        public void SetRaycastBlockingEnabled(bool enabled)
        {
            qtUiFlow.SetRaycastBlockingEnabled(enabled);
        }

        public void DisableRaycastBlocking()
        {
            qtUiFlow.DisableRaycastBlocking();
        }

        public void EnableRaycastBlocking()
        {
            qtUiFlow.EnableRaycastBlocking();
        }

        private void HandleRaycastBlockStateChanged(bool shouldBlock)
        {
            ApplyBlockState(shouldBlock);
        }

        private void ApplyBlockState(bool shouldBlock)
        {
            if (!_blocker)
            {
                return;
            }

            _blocker.raycastTarget = shouldBlock;
        }
    }
}