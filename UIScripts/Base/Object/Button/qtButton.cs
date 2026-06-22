using System;
using NaughtyAttributes;
using TMPro;
using qtLib.UI.Base;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace qtLib.Object
{
    [RequireComponent(typeof(Button))]
    public class qtButton : MonoBehaviour, IPointerDownHandler
    {
        #region ----- Component Config -----

        [Serializable]
        protected class ButtonSetting
        {
            public Selectable.Transition transition = Selectable.Transition.ColorTint;
            
            [Space] 
            public Material normalMaterial;
            public Material grayScaleMaterial;
        
            public Color normalColor = Color.white;
            public Color grayScaleColor = Color.white;
        }
        
        [SerializeField] protected ButtonSetting _buttonSetting;
        private Button _button;
        private Image _image;
        private TextMeshProUGUI _text;
        private Image[] _imagesInChildren;
        
        [HideInInspector] public Button.ButtonClickedEvent onClick;
        
        private bool _isInitialized = false;

        #endregion

        #region ----- Properties -----

        public Button button
        {
            get
            {
                if (_button == null)
                {
                    _Initialize();
                }
                return _button;
            }
        }

        public Image image
        {
            get
            {
                if (_image == null)
                {
                    _Initialize();
                }
                return _image;
            }
        }

        #endregion
        
        #region ----- Unity Event -----

        protected virtual void Awake()
        {
            _Initialize();
        }

        #endregion

        #region ----- Public Function -----

        public virtual void SetInteractable(bool isInteractable, bool changeTextColor = true, bool changeImageColor = true, bool changeChildImage = true)
        {
            if (!_isInitialized)
            {
                _Initialize();
            }
            _button.interactable = isInteractable;
            
            SetGrayScale(!isInteractable, changeTextColor, changeImageColor, changeChildImage);
        }

        public void SetGrayScale(bool isGrayScale, bool changeTextColor = true, bool changeImageColor = true, bool changeChildImage = true)
        {
            if (!_isInitialized)
            {
                _Initialize();
            }

            if (changeTextColor && _text)
            {
                _text.color = isGrayScale ? _buttonSetting.grayScaleColor : _buttonSetting.normalColor;
            }

            if (changeImageColor)
            {
                _image.material = isGrayScale ? _buttonSetting.grayScaleMaterial : _buttonSetting.normalMaterial;
            }

            if (changeChildImage)
            {
                for (var i = 0; i < _imagesInChildren.Length; i++)
                {
                    _imagesInChildren[i].material = isGrayScale ? _buttonSetting.grayScaleMaterial : _buttonSetting.normalMaterial;
                }
            }
        }

        public virtual void SetEnable(bool isEnable)
        {
            if (!_isInitialized)
            {
                _Initialize();
            }

            _button.enabled = isEnable;
        }

        public void SetText(string text)
        {
            if (!_isInitialized)
            {
                _Initialize();
            }

            if (_text)
            {
                _text.SetText(text);
            }
        }
        
        #endregion

        #region ----- Private Function -----

        protected virtual void _Initialize()
        {
            if (_isInitialized)
            {
                return;
            }
            _isInitialized = true;
            
            _button = GetComponent<Button>();
            _image = GetComponent<Image>();
            _button.onClick.AddListener(_OnButtonClick);

            _text = GetComponentInChildren<TextMeshProUGUI>();
            _imagesInChildren = GetComponentsInChildren<Image>(true);
        }

        protected virtual void _OnButtonClick()
        {
            // _PlaySfx();
            if (qtUiFlow.IsBusy)
            {
                return;
            }

            onClick?.Invoke();
        }

        protected virtual void _OnButtonPress()
        {
            _PlaySfx();
        }

        protected virtual void _PlaySfx()
        {
            // _PlaySfxSoundController.Instance.PlaySfx(SoundController.SoundID.Click,0.1f);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _OnButtonPress();
        }

        #endregion


#if UNITY_EDITOR
        [HideInInspector] [SerializeField] private Material _editorGrayScaleMaterial; 
        
        [Button("Validate", EButtonEnableMode.Editor)]
        private void OnValidate()
        {
            if (_editorGrayScaleMaterial == null)
            {
                _editorGrayScaleMaterial = Resources.Load<Material>("UI_GrayscaleShader");
                _buttonSetting = new ButtonSetting
                {
                    grayScaleMaterial = _editorGrayScaleMaterial
                };
            }

            button.transition = _buttonSetting.transition;
            switch (_buttonSetting.transition)
            {
                case Selectable.Transition.None:
                case Selectable.Transition.ColorTint:
                case Selectable.Transition.SpriteSwap:
                {
                    if (gameObject.TryGetComponent(out Animator temp))
                    {
                        DestroyImmediate(temp, true);
                    }
                    break;
                }
                case Selectable.Transition.Animation:
                {
                    if (!gameObject.TryGetComponent(out Animator temp))
                    {
                        temp = gameObject.AddComponent<Animator>();
                    }
                
                    if (temp.runtimeAnimatorController == null)
                    {
                        temp.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Button_Animation");
                    }
                    break;
                }
            }
        }
#endif
    }
}
