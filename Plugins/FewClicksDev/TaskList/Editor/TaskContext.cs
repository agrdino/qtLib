namespace FewClicksDev.TaskList
{
    using FewClicksDev.Core;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    using static FewClicksDev.Core.EditorDrawer;

    [System.Serializable]
    public class TaskContext
    {
        [System.Serializable]
        public struct SceneContext
        {
            public Vector3 CameraPosition;
            public Vector3 CameraRotation;
            public string MainSceneName;
            public string[] AdditionalScenesNames;

            public float GetHeight()
            {
                float _height = SingleLineHeightWithSpacing * 2f; //Position and rotation
                _height += SMALL_SPACE;
                _height += SingleLineHeightWithSpacing; //Main scene
                _height += SMALL_SPACE;
                _height += SingleLineHeightWithSpacing; //Buttons

                if (AdditionalScenesNames.IsNullOrEmpty())
                {
                    return _height;
                }

                return _height + VERY_SMALL_SPACE + (AdditionalScenesNames.Length * SingleLineHeightWithSpacing); //Add additional scenes names
            }
        }

        private const string NO_SCENE_VIEW_ERROR = "There is no active scene view! Camera position and rotation can't be updated.";
        private const string NO_SCENE_VIEW_FOR_POSITION_ERROR = "There is no active scene view! Position can't be updated.";
        private const string NO_SCENE_VIEW_FOR_ROTATION_ERROR = "There is no active scene view! Rotation can't be updated.";

        private const int INVALID_LOCAL_ID = -1;

        [SerializeField] private TaskContextType contextType = TaskContextType.Object;
        [SerializeField] private Object objectReference = null;
        [SerializeField] private MonoScript scriptReference = null;
        [SerializeField] private int lineOfContext = 0;
        [SerializeField] private SceneContext sceneReference = default;
        [SerializeField] private string linkReference = string.Empty;
        [SerializeField] private Transform sceneObjectReference = null;
        [SerializeField] private long localID = -1;
        [SerializeField] private string containedObjectScenePath = string.Empty;

        public TaskContextType ContextType => contextType;
        public Object ObjectReference => objectReference;
        public MonoScript ScriptReference => scriptReference;
        public int LineOfContext => lineOfContext;
        public SceneContext SceneReference => sceneReference;
        public int NumberOfAdditionalScenes => sceneReference.AdditionalScenesNames.IsNullOrEmpty() ? 0 : sceneReference.AdditionalScenesNames.Length;
        public string LinkReference => linkReference;
        public string ContainedObjectScenePath => containedObjectScenePath;
        public bool IsValidID => localID != INVALID_LOCAL_ID;
        public bool IsValidIDSceneNotLoaded => IsValidID && IsSceneLoaded() == false;

        public Transform SceneObjectReference
        {
            get
            {
                if (sceneObjectReference == null && localID != -1)
                {
                    var _scene = EditorSceneManager.GetSceneByPath(containedObjectScenePath);

                    if (_scene.IsValid() == false || _scene.isLoaded == false)
                    {
                        return null;
                    }

                    var _allTransforms = _scene.GetRootGameObjects();

                    foreach (var _root in _allTransforms)
                    {
                        var _allChildren = _root.GetComponentsInChildren<Transform>(true);

                        foreach (var _child in _allChildren)
                        {
                            if (_child.GetLocalIdentifierOfObject() == localID)
                            {
                                sceneObjectReference = _child;
                                break;
                            }
                        }

                        if (sceneObjectReference != null)
                        {
                            break;
                        }
                    }
                }

                return sceneObjectReference;
            }
        }

        public TaskContext() { }

        public TaskContext(MonoScript _scriptReference, int _lineOfContext) 
        {
            scriptReference = _scriptReference;
            lineOfContext = _lineOfContext;
            contextType = TaskContextType.Script;
        }

        public void SetContextType(TaskContextType _contextType)
        {
            contextType = _contextType;
            TaskListUserPreferences.SavePreferences(false);
        }

        public void SetObjectReference(Object _object)
        {
            objectReference = _object;
            TaskListUserPreferences.SavePreferences(false);
        }

        public void SetScriptReference(MonoScript _script)
        {
            scriptReference = _script;
            TaskListUserPreferences.SavePreferences(false);
        }

        public void SetLinkReference(string _link)
        {
            linkReference = _link;
            TaskListUserPreferences.SavePreferences(false);
        }

        public void SetSceneObjectReference(Transform _sceneObject)
        {
            sceneObjectReference = _sceneObject;

            if (sceneObjectReference != null)
            {
                Scene _scene = sceneObjectReference.gameObject.scene;

                if (_scene.IsValid())
                {
                    localID = sceneObjectReference.GetLocalIdentifierOfObject();
                    containedObjectScenePath = _scene.path;
                }
                else
                {
                    sceneObjectReference = null;
                    containedObjectScenePath = string.Empty;
                    localID = INVALID_LOCAL_ID;
                    TaskList.LogError("The selected GameObject is not part of a valid scene!");
                }
            }
            else
            {
                containedObjectScenePath = string.Empty;
            }

            TaskListUserPreferences.SavePreferences(false);
        }

        public bool IsSceneLoaded()
        {
            if (containedObjectScenePath.IsNullEmptyOrWhitespace())
            {
                return false;
            }

            Scene _scene = EditorSceneManager.GetSceneByPath(containedObjectScenePath);
            return _scene.isLoaded;
        }

        public void UpdateCameraPosition()
        {
            SceneView _sceneView = SceneView.lastActiveSceneView;

            if (_sceneView != null)
            {
                sceneReference.CameraPosition = _sceneView.camera.transform.position;
            }
            else
            {
                TaskList.LogError(NO_SCENE_VIEW_FOR_POSITION_ERROR);
            }

            TaskListUserPreferences.SavePreferences(false);
        }

        public void UpdateCameraRotation()
        {
            SceneView _sceneView = SceneView.lastActiveSceneView;

            if (_sceneView != null)
            {
                sceneReference.CameraRotation = _sceneView.camera.transform.eulerAngles;
            }
            else
            {
                TaskList.LogError(NO_SCENE_VIEW_FOR_ROTATION_ERROR);
            }

            TaskListUserPreferences.SavePreferences(false);
        }

        public void UpdateLoadedScenes()
        {
            SceneContext _sceneContext = new SceneContext();

            SceneView _sceneView = SceneView.lastActiveSceneView;

            if (_sceneView != null)
            {
                _sceneContext.CameraPosition = _sceneView.camera.transform.position;
                _sceneContext.CameraRotation = _sceneView.camera.transform.eulerAngles;
            }
            else
            {
                TaskList.LogWarning(NO_SCENE_VIEW_ERROR);
            }

            Scene _mainScene = SceneManager.GetActiveScene();
            _sceneContext.MainSceneName = _mainScene.name;

            List<string> _additionalScenes = new List<string>();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene _scene = SceneManager.GetSceneAt(i);

                if (_scene != _mainScene && _scene.isLoaded)
                {
                    _additionalScenes.Add(_scene.name);
                }
            }

            _sceneContext.AdditionalScenesNames = _additionalScenes.ToArray();
            sceneReference = _sceneContext;
            TaskListUserPreferences.SavePreferences(false);
        }

        public void LoadContextScenes()
        {
            string _mainScenePath = TaskList.FindScenePath(sceneReference.MainSceneName);

            if (_mainScenePath.IsNullEmptyOrWhitespace())
            {
                TaskList.LogError($"Main scene '{sceneReference.MainSceneName}' is not valid or not found. Skipping whole context loading!");
                return;
            }

            EditorSceneManager.OpenScene(_mainScenePath, OpenSceneMode.Single);

            foreach (string _sceneName in sceneReference.AdditionalScenesNames)
            {
                string _scenePath = TaskList.FindScenePath(_sceneName);

                if (_scenePath.IsNullEmptyOrWhitespace())
                {
                    TaskList.LogError($"Additional scene '{_sceneName}' is not valid or not found. Skipping its loading!");
                    continue;
                }

                EditorSceneManager.OpenScene(_scenePath, OpenSceneMode.Additive);
            }

            if (TaskListUserPreferences.ApplyPositionAndRotationOnLoad == false)
            {
                return;
            }

            SceneView _sceneView = SceneView.lastActiveSceneView;

            if (_sceneView != null)
            {
                _sceneView.camera.transform.position = sceneReference.CameraPosition;
                _sceneView.camera.transform.eulerAngles = sceneReference.CameraRotation;
                _sceneView.Repaint();
            }
            else
            {
                TaskList.LogError(NO_SCENE_VIEW_ERROR);
            }
        }

        public float GetHeight()
        {
            float _baseHeight = 2f * 4f; //Help box style has a top and bottom padding of 3 pixels and a 1 pixel frame

            switch (contextType)
            {
                case TaskContextType.Scenes:
                    return _baseHeight + SingleLineHeightWithSpacing + sceneReference.GetHeight();

                case TaskContextType.SceneGameObject:
                    return IsValidIDSceneNotLoaded ? _baseHeight + (3f * SingleLineHeightWithSpacing) : _baseHeight + (2f * SingleLineHeightWithSpacing);

                default:
                    return _baseHeight + (2f * SingleLineHeightWithSpacing); //Type and asset or link reference
            }
        }
    }
}
