using UnityEngine;

namespace qtLib.Extension
{
    public static partial class qtGameExtension
    {
        public static T TryGetComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject == null)
            {
                throw new System.ArgumentNullException();
            }

            if (gameObject.TryGetComponent(out T component))
            {
                return component;
            }
            
            return gameObject.AddComponent<T>();
        }

        public static void RemoveComponent<T>(this GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component != null)
            {
                UnityEngine.Object.Destroy(component);
            }
        }
    }
}