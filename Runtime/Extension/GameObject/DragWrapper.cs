using System;
using UnityEngine;

namespace Extension
{
    public class DragWrapper : MonoBehaviour
    {
        private static DragWrapper _instance;
        private static bool _canDrag = false;

        public static bool CanDrag
        {
            get => _canDrag;
            set
            {
                _canDrag = value;
                if (!value)
                {
                    _instance.ForceEndDrag();
                }
            }
        }

        // Events - Chỉ truyền draggable và vị trí từ Mouse/Touch
        public event Action<IDraggable> OnBeginDrag;
        public event Action<IDraggable, Vector3> OnDragging; // Vector3 = world position từ mouse/touch
        public event Action<IDraggable> OnEndDrag; // dropWorldPos từ mouse/touch

        private IDraggable _currentDraggable;
        private IClickable _currentClickable;
        private Vector3 _dragStartScreenPosition; // Lưu vị trí màn hình lúc bắt đầu
        private int _touchId = -1;
        private bool _isDragging = false;

        private void Awake()
        {
            _instance = this;
        }

        private void Update()
        {
            if (!CanDrag)
            {
                if (_isDragging)
                {
                    ForceEndDrag();
                }
                return;
            }

            if (!_isDragging)
            {
                TryBeginDrag();
            }
            else
            {
                HandleDragging();
            }
        }

        private void TryBeginDrag()
        {
            Vector3 screenPos = Vector3.zero;
            bool inputStarted = false;
            int newTouchId = -1;

            // Lấy vị trí input
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    screenPos = touch.position;
                    newTouchId = touch.fingerId;
                    inputStarted = true;
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                screenPos = Input.mousePosition;
                newTouchId = -1;
                inputStarted = true;
            }

            if (inputStarted)
            {
                IDraggable draggable = GetObjectAtScreenPosition<IDraggable>(screenPos, x => x.CanDrag());

                if (draggable != null)
                {
                    _currentDraggable = draggable;
                    _dragStartScreenPosition = screenPos; // ← Chỉ lưu vị trí Mouse/Touch

                    _isDragging = true;
                    _touchId = newTouchId;

                    OnBeginDrag?.Invoke(_currentDraggable);
                    Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
                    _currentDraggable.OnBeginDrag(worldPos);
                }

                IImmovable immovable = GetObjectAtScreenPosition<IImmovable>(screenPos, x => x.CanDrag());
                if (immovable != null)
                {
                    immovable.OnBeginDrag();
                }
                
                IClickable clickable = GetObjectAtScreenPosition<IClickable>(screenPos, x => x.CanClick());
                if (clickable != null)
                {
                    _isDragging = true;
                    _currentClickable = clickable;
                    clickable.OnBeginClick();
                }
            }
        }

        private void HandleDragging()
        {
            Vector3 screenPos = GetCurrentScreenPosition();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            worldPos.z = -1.5f; // Độ cao khi kéo

            if (_currentClickable != null)
            {
                IClickable clickable = GetObjectAtScreenPosition<IClickable>(screenPos, x => x.CanClick());
                if (clickable == null || clickable != _currentClickable)
                {
                    _currentClickable.OnDragOut();
                    _currentClickable = null;
                }
            }
            // Ring theo vị trí Mouse/Touch
            if (_currentDraggable != null)
            {
                _currentDraggable.OnDrag(worldPos);
            }

            OnDragging?.Invoke(_currentDraggable, worldPos);

            if (ShouldEndDrag())
            {
                EndCurrentDrag();
            }
        }

        private void EndCurrentDrag()
        {
            _isDragging = false;
            // Thông báo ra ngoài: draggable đang kéo + vị trí thả từ mouse/touch + target draggable (nếu có)
            if (_currentClickable != null)
            {
                IClickable clickable = GetObjectAtScreenPosition<IClickable>(GetCurrentScreenPosition(), x => x.CanClick());
                if (clickable == _currentClickable)
                {
                    _currentClickable.OnEndClick();
                    _currentClickable = null;
                }
            }

            if (_currentDraggable != null)
            {
                OnEndDrag?.Invoke(_currentDraggable);
                IImmovable immovable = GetObjectAtWorldPosition<IImmovable>(_currentDraggable.GameObject().transform.position, x => x.CanDrag());
                if (immovable != null)
                {
                    immovable.OnEndDrag();
                }

                _currentDraggable.OnEndDrag();
            }
            
            _currentDraggable = null;
            _touchId = -1;
        }

        private void ForceEndDrag()
        {
            if (_currentDraggable != null)
            {
                _currentDraggable.OnEndDrag(true);
                OnEndDrag?.Invoke(_currentDraggable);
            }

            _isDragging = false;
            _currentDraggable = null;
            _touchId = -1;
        }

        // ====================== Helper ======================
        private T GetObjectAtScreenPosition<T>(Vector3 screenPosition, Func<T, bool> condition) where T : class
        {
            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            RaycastHit2D[] hit = Physics2D.GetRayIntersectionAll(ray);
            for (int i = 0; i < hit.Length; i++)
            {
                if (hit[i].collider == null)
                {
                    continue;
                }

                T temp  = hit[i].collider.GetComponent<T>();
                if (temp == null)
                {
                    continue;
                }

                if (condition.Invoke(temp))
                {
                    return temp;
                }
            }
            return null;
        }

        private T GetObjectAtWorldPosition<T>(Vector3 worldPosition, Func<T, bool> condition) where T : class
        {
            RaycastHit2D[] hit = Physics2D.RaycastAll(worldPosition, Vector3.zero);
            for (int i = 0; i < hit.Length; i++)
            {
                if (hit[i].collider == null)
                {
                    continue;
                }

                T temp  = hit[i].collider.GetComponent<T>();
                if (temp == null)
                {
                    continue;
                }

                if (condition.Invoke(temp))
                {
                    return temp;
                }
            }
            return null;
        }

        private Vector3 GetCurrentScreenPosition()
        {
            if (_touchId >= 0 && Input.touchCount > 0)
            {
                foreach (Touch t in Input.touches)
                    if (t.fingerId == _touchId)
                        return t.position;
            }

            return Input.mousePosition;
        }

        private bool ShouldEndDrag()
        {
            if (_touchId >= 0)
            {
                foreach (Touch t in Input.touches)
                    if (t.fingerId == _touchId)
                        return t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled;
                return true;
            }

            return Input.GetMouseButtonUp(0);
        }
    }
}