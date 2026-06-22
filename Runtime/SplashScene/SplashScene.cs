using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Redcode.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace qtLib.SplashScene
{
    public class SplashScene : MonoBehaviour
    {
        #region ----- Component Config -----

        [SerializeField] private GameObject _container;
        [SerializeField] private List<GameObject> _controller;

        [Space] 
        [SerializeField] private Animator _animator;
        [SerializeField] private Animator _animatorIconGame;

        [Space] 
        [SerializeField] private SceneReference _splashScene;
        [SerializeField] private SceneReference _gameScene;

        private AsyncOperation _loadSceneOperation;
        private static readonly int LoadSceneDone = Animator.StringToHash("LoadSceneDone");

        #endregion

        private async void Start()
        {
            for (var i = 0; i < _controller.Count; i++)
            {
                await InstantiateAsync(_controller[i], _container.transform);
            }

            _loadSceneOperation = SceneManager.LoadSceneAsync(_gameScene, LoadSceneMode.Single);
            _loadSceneOperation.allowSceneActivation = false;
            
            _animator.SetBool(LoadSceneDone, true);
            _animatorIconGame.SetBool(LoadSceneDone, true);
#if UNITY_EDITOR
            _ChangeScene();
#endif
        }
        
        private void _ChangeScene()
        {
            _loadSceneOperation.allowSceneActivation = true;
        }
    }
}