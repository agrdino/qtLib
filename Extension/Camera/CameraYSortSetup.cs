using UnityEngine;

namespace qtLib.Extension.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CameraYSortSetup : MonoBehaviour
    {
        private void Awake()
        {
            UnityEngine.Camera camera = GetComponent<UnityEngine.Camera>(); 
            camera.transparencySortMode = TransparencySortMode.CustomAxis;
            camera.transparencySortAxis = Vector3.up;
        }
    }
}