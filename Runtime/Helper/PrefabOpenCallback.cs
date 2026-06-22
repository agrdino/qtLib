// using Game.Object;
// using Model;
// using UnityEditor;
// using UnityEditor.SceneManagement;
// using UnityEngine;
//
// #if UNITY_EDITOR
//
// [InitializeOnLoad]
// public class PrefabOpenCallback
// {
//     static PrefabOpenCallback()
//     {
//         PrefabStage.prefabStageOpened += OnPrefabOpened;
//         PrefabStage.prefabSaved += OnPrefabSaved;
//         PrefabStage.prefabSaving += OnPrefabSaving;
//         PrefabStage.prefabStageClosing += OnPrefabClosing;
//         PrefabStage.prefabStageDirtied += OnPrefabDirtied;
//     }
//
//     private static void OnPrefabDirtied(PrefabStage prefabStage)
//     {
//     }
//
//     private static void OnPrefabClosing(PrefabStage prefabStage)
//     {
//         
//     }
//
//     private static void OnPrefabSaving(GameObject gameObject)
//     {
//         
//     }
//
//     private static void OnPrefabOpened(PrefabStage prefabStage)
//     {
//         var x = prefabStage.prefabContentsRoot.GetComponent<ObjectEnemy>();
//         if (x == null)
//         {
//             return;
//         }
//         x.Get();
//         EditorUtility.SetDirty(x);
//     }
//
//     private static void OnPrefabSaved(GameObject gameObject)
//     {
//     }
// }
//
// #endif
