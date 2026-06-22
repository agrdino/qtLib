// ══════════════════════════════════════════════════════════════════════════════
//  ObjectInspectorWindow.cs
//  Đặt file này vào:  Assets/Editor/ObjectInspectorWindow.cs
//
//  Mở window:  Menu  Tools ▶ Object Inspector
//
//  Tính năng:
//  • Kéo bất kỳ GameObject từ Hierarchy/Scene vào drop zone
//  • Hiển thị toàn bộ component giống hệt Inspector (foldout, icon, fields)
//  • Mỗi component dùng SerializedObject + SerializedProperty → edit được
//  • Scroll, foldout state lưu theo component type
//  • Nút "Ping" highlight object trong Hierarchy
//  • Nút "Select" chọn object trong Editor
//  • Live update khi giá trị thay đổi (Undo/Redo support)
// ══════════════════════════════════════════════════════════════════════════════

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace _qtLib.Editor
{
    public class ObjectInspectorWindow : EditorWindow
    {
        // ── State ─────────────────────────────────────────────────────────────────
        GameObject       _target;                            // GameObject đang inspect
        ScriptableObject _targetSO;                          // ScriptableObject đang inspect
        UnityEngine.Object _targetObj => _target != null ? _target : (UnityEngine.Object)_targetSO;
        bool _isSO  => _target == null && _targetSO != null;
        bool _hasAny => _target != null || _targetSO != null;

        Component[] _components;
        UnityEditor.Editor      _soEditor;                               // Editor riêng cho SO
        Dictionary<int, bool>   _foldouts = new();
        Dictionary<int, UnityEditor.Editor> _editors  = new();
        Vector2 _scroll;

        // ── Drop zone ─────────────────────────────────────────────────────────────
        bool  _isDragHover;

        // ── Styles (lazy init) ────────────────────────────────────────────────────
        GUIStyle _dropBoxStyle;
        GUIStyle _componentHeaderStyle;
        GUIStyle _goNameStyle;
        bool     _stylesReady;

        // ── Open window ───────────────────────────────────────────────────────────
        [MenuItem("Tools/Object Inspector")]
        public static void Open()
        {
            var win = CreateWindow<ObjectInspectorWindow>("Object Inspector");
            win.minSize = new Vector2(300, 400);
            win.Show();
        }

        // ─────────────────────────────────────────────────────────────────────────
        void OnEnable()
        {
            // Khi Unity reload scripts hoặc mở lại window, xoá cache cũ
            ClearEditorCache();
        }

        void OnDisable()
        {
            ClearEditorCache();
        }

        // ─────────────────────────────────────────────────────────────────────────
        void OnGUI()
        {
            EnsureStyles();

            // ── Toolbar + Object Field ────────────────────────────────────────────
            DrawToolbar();

            // ── Chưa chọn object ─────────────────────────────────────────────────
            if (!_hasAny)
            {
                DrawEmptyState();
                HandleDragDropAnywhere();
                return;
            }

            // ── Header ────────────────────────────────────────────────────────────
            if (_isSO)
                DrawScriptableObjectHeader();
            else
                DrawGameObjectHeader();

            // ── Scrollable content ────────────────────────────────────────────────
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            if (_isSO)
                DrawSOBody();
            else
                DrawAllComponents();
            EditorGUILayout.EndScrollView();

            HandleDragDropAnywhere();
            if (Application.isPlaying) Repaint();
        }

        // ══════════════════════════════════════════════════════════════════════════
        //  TOOLBAR  +  OBJECT FIELD
        // ══════════════════════════════════════════════════════════════════════════
        void DrawToolbar()
        {
            // ── Toolbar row ───────────────────────────────────────────────────────
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Object Inspector", EditorStyles.toolbarButton,
                    GUILayout.ExpandWidth(false));

                GUILayout.FlexibleSpace();

                if (_hasAny)
                {
                    // Ping
                    if (GUILayout.Button(
                            EditorGUIUtility.IconContent("d_ViewToolOrbit On"),
                            EditorStyles.toolbarButton, GUILayout.Width(28)))
                        EditorGUIUtility.PingObject(_targetObj);

                    // Select
                    if (GUILayout.Button("Select", EditorStyles.toolbarButton,
                            GUILayout.Width(50)))
                        Selection.activeObject = _targetObj;

                    // Clear
                    if (GUILayout.Button(
                            EditorGUIUtility.IconContent("winbtn_win_close"),
                            EditorStyles.toolbarButton, GUILayout.Width(28)))
                        SetTarget(null);
                }
            }

            // ── Object Field row ──────────────────────────────────────────────────
            EditorGUILayout.Space(1);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(4);
                EditorGUILayout.LabelField("Target", GUILayout.Width(42));

                EditorGUI.BeginChangeCheck();
                var picked = EditorGUILayout.ObjectField(
                    _targetObj,
                    typeof(UnityEngine.Object),
                    allowSceneObjects: true);
                if (EditorGUI.EndChangeCheck())
                    SetTarget(picked);

                GUILayout.Space(4);
            }
            EditorGUILayout.Space(2);

            // Đường kẻ phân cách mỏng
            Rect sepRect = GUILayoutUtility.GetRect(0, 1f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(sepRect, new Color(0, 0, 0, 0.25f));
        }

        // ══════════════════════════════════════════════════════════════════════════
        //  EMPTY STATE (khi chưa chọn object)
        // ══════════════════════════════════════════════════════════════════════════
        void DrawEmptyState()
        {
            GUILayout.FlexibleSpace();
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                using (new EditorGUILayout.VerticalScope())
                {
                    // Icon
                    var icon = EditorGUIUtility.IconContent("GameObject Icon");
                    float iconSize = 40f;
                    Rect iconRect = GUILayoutUtility.GetRect(iconSize, iconSize);
                    GUI.color = new Color(1, 1, 1, _isDragHover ? 0.9f : 0.3f);
                    GUI.DrawTexture(iconRect, icon.image, ScaleMode.ScaleToFit);
                    GUI.color = Color.white;

                    EditorGUILayout.Space(6);

                    // Hint text
                    var style = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                    {
                        fontSize = 11,
                        wordWrap = true,
                    };
                    style.normal.textColor = _isDragHover
                        ? new Color(0.6f, 0.82f, 1f)
                        : new Color(0.5f, 0.5f, 0.5f);

                    GUILayout.Label(
                        _isDragHover
                            ? "Release to inspect"
                            : "Select a GameObject\nfrom the field above\nor drag one here",
                        style, GUILayout.Width(180));
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.FlexibleSpace();
        }

        // ══════════════════════════════════════════════════════════════════════════
        //  SCRIPTABLEOBJECT HEADER
        // ══════════════════════════════════════════════════════════════════════════
        void DrawScriptableObjectHeader()
        {
            Color headerBg = EditorGUIUtility.isProSkin
                ? new Color(0.22f, 0.22f, 0.22f)
                : new Color(0.76f, 0.76f, 0.76f);

            Rect headerRect = GUILayoutUtility.GetRect(0, 36f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(headerRect, headerBg);
            EditorGUI.DrawRect(new Rect(headerRect.x, headerRect.yMax - 1,
                headerRect.width, 1), new Color(0, 0, 0, 0.3f));

            // SO icon
            var content = EditorGUIUtility.ObjectContent(_targetSO, _targetSO.GetType());
            if (content.image != null)
                GUI.DrawTexture(new Rect(headerRect.x + 8, headerRect.y + 8, 20, 20),
                    content.image, ScaleMode.ScaleToFit);

            // Type badge nhỏ
            GUIStyle typeStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = new Color(0.6f, 0.8f, 1f) }
            };
            GUI.Label(new Rect(headerRect.x + 34, headerRect.y + 4,
                    headerRect.width - 34, 14),
                _targetSO.GetType().Name, typeStyle);

            // Name (asset name)
            GUI.Label(new Rect(headerRect.x + 34, headerRect.y + 18,
                    headerRect.width - 34, 16),
                _targetSO.name, _goNameStyle);
        }

        // ── Render SO fields dùng Editor built-in ─────────────────────────────────
        void DrawSOBody()
        {
            if (_soEditor == null)
                _soEditor = UnityEditor.Editor.CreateEditor(_targetSO);

            // Script reference row (read-only)
            using (new EditorGUI.DisabledScope(true))
            {
                var script = MonoScript.FromScriptableObject(_targetSO);
                EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
            }

            EditorGUILayout.Space(2);
            _soEditor.OnInspectorGUI();
        }

        // ══════════════════════════════════════════════════════════════════════════
        //  GAMEOBJECT HEADER (giống Inspector header)
        // ══════════════════════════════════════════════════════════════════════════
        void DrawGameObjectHeader()
        {
            Color headerBg = EditorGUIUtility.isProSkin
                ? new Color(0.22f, 0.22f, 0.22f)
                : new Color(0.76f, 0.76f, 0.76f);

            Rect headerRect = GUILayoutUtility.GetRect(0, 46f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(headerRect, headerBg);
            EditorGUI.DrawRect(new Rect(headerRect.x, headerRect.yMax - 1,
                headerRect.width, 1), new Color(0,0,0,0.3f));

            // Active toggle
            bool active = GUI.Toggle(
                new Rect(headerRect.x + 6, headerRect.y + 15, 16, 16),
                _target.activeSelf, GUIContent.none);
            if (active != _target.activeSelf)
            {
                Undo.RecordObject(_target, "Toggle Active");
                _target.SetActive(active);
            }

            // GameObject icon
            var goIcon = EditorGUIUtility.ObjectContent(_target, typeof(GameObject));
            if (goIcon.image != null)
                GUI.DrawTexture(new Rect(headerRect.x + 26, headerRect.y + 13, 20, 20),
                    goIcon.image, ScaleMode.ScaleToFit);

            // Name field
            string newName = EditorGUI.TextField(
                new Rect(headerRect.x + 50, headerRect.y + 12, headerRect.width - 130, 20),
                _target.name, _goNameStyle);
            if (newName != _target.name)
            {
                Undo.RecordObject(_target, "Rename GameObject");
                _target.name = newName;
            }

            // Tag + Layer dropdowns (phải)
            float rightX = headerRect.xMax - 120;
            EditorGUI.LabelField(new Rect(rightX, headerRect.y + 6, 35, 16),
                "Tag", EditorStyles.miniLabel);
            string newTag = EditorGUI.TagField(
                new Rect(rightX + 30, headerRect.y + 6, 86, 16), _target.tag);
            if (newTag != _target.tag)
            {
                Undo.RecordObject(_target, "Change Tag");
                _target.tag = newTag;
            }

            EditorGUI.LabelField(new Rect(rightX, headerRect.y + 26, 35, 16),
                "Layer", EditorStyles.miniLabel);
            int newLayer = EditorGUI.LayerField(
                new Rect(rightX + 30, headerRect.y + 26, 86, 16), _target.layer);
            if (newLayer != _target.layer)
            {
                Undo.RecordObject(_target, "Change Layer");
                _target.layer = newLayer;
            }
        }

        // ══════════════════════════════════════════════════════════════════════════
        //  VẼ TẤT CẢ COMPONENT
        // ══════════════════════════════════════════════════════════════════════════
        void DrawAllComponents()
        {
            if (_components == null) return;

            foreach (var comp in _components)
            {
                if (comp == null) continue;
                DrawComponent(comp);
            }
        }

        void DrawComponent(Component comp)
        {
            int id = comp.GetInstanceID();
            System.Type type = comp.GetType();

            // Foldout state mặc định = true
            if (!_foldouts.ContainsKey(id)) _foldouts[id] = true;

            // Lấy / tạo Editor cho component này
            if (!_editors.TryGetValue(id, out UnityEditor.Editor ed) || ed == null)
            {
                ed = UnityEditor.Editor.CreateEditor(comp);
                _editors[id] = ed;
            }

            // ── Component header ──────────────────────────────────────────────────
            bool foldout = _foldouts[id];
            Rect headerRect = GUILayoutUtility.GetRect(0, 22f, GUILayout.ExpandWidth(true));

            Color hBg = EditorGUIUtility.isProSkin
                ? new Color(0.22f, 0.22f, 0.22f)
                : new Color(0.76f, 0.76f, 0.76f);
            EditorGUI.DrawRect(headerRect, hBg);
            EditorGUI.DrawRect(new Rect(headerRect.x, headerRect.yMax - 1,
                headerRect.width, 1), new Color(0,0,0,0.25f));

            // Arrow foldout
            foldout = EditorGUI.Foldout(
                new Rect(headerRect.x + 4, headerRect.y + 4, 16, 14),
                foldout, GUIContent.none, true);
            _foldouts[id] = foldout;

            // Component icon (Unity built-in icon tự động theo type)
            var content = EditorGUIUtility.ObjectContent(comp, type);
            float iconX = headerRect.x + 22f;
            if (content.image != null)
            {
                GUI.DrawTexture(new Rect(iconX, headerRect.y + 3, 16, 16),
                    content.image, ScaleMode.ScaleToFit);
                iconX += 20f;
            }

            // Enabled toggle (với Behaviour / Renderer có .enabled)
            float toggleX = iconX;
            if (comp is Behaviour behaviour)
            {
                behaviour.enabled = GUI.Toggle(
                    new Rect(toggleX, headerRect.y + 4, 16, 14),
                    behaviour.enabled, GUIContent.none);
                toggleX += 18f;
            }
            else if (comp is Renderer renderer)
            {
                renderer.enabled = GUI.Toggle(
                    new Rect(toggleX, headerRect.y + 4, 16, 14),
                    renderer.enabled, GUIContent.none);
                toggleX += 18f;
            }

            // Component type name
            GUI.Label(
                new Rect(toggleX, headerRect.y + 3, headerRect.width - toggleX - 30,
                    headerRect.height),
                ObjectNames.NicifyVariableName(type.Name),
                _componentHeaderStyle);

            // Context menu (⋮)
            Rect menuBtnRect = new Rect(headerRect.xMax - 22, headerRect.y + 2, 20, 18);
            if (GUI.Button(menuBtnRect,
                    EditorGUIUtility.IconContent("d__Menu"),
                    EditorStyles.iconButton))
            {
                ShowComponentContextMenu(comp);
            }

            // Click header để toggle foldout
            if (Event.current.type == EventType.MouseDown
                && headerRect.Contains(Event.current.mousePosition)
                && !menuBtnRect.Contains(Event.current.mousePosition))
            {
                _foldouts[id] = !_foldouts[id];
                Event.current.Use();
            }

            // ── Component body ────────────────────────────────────────────────────
            if (_foldouts[id])
            {
                EditorGUI.indentLevel++;
                try
                {
                    ed.OnInspectorGUI();
                }
                catch { /* component editor lỗi thì bỏ qua */ }
                EditorGUI.indentLevel--;

                EditorGUILayout.Space(2);
            }

            // Đường kẻ phân cách
            EditorGUI.DrawRect(
                GUILayoutUtility.GetRect(0, 1f, GUILayout.ExpandWidth(true)),
                new Color(0, 0, 0, 0.2f));
        }

        // ── Context menu cho từng component ──────────────────────────────────────
        void ShowComponentContextMenu(Component comp)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Remove Component"), false, () =>
            {
                if (EditorUtility.DisplayDialog("Remove Component",
                        $"Remove {comp.GetType().Name} from {_target.name}?",
                        "Remove", "Cancel"))
                {
                    Undo.DestroyObjectImmediate(comp);
                    RefreshTarget();
                }
            });
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Copy Component"), false, () =>
            {
                UnityEditorInternal.ComponentUtility.CopyComponent(comp);
            });
            menu.AddItem(new GUIContent("Paste Component Values"), false, () =>
            {
                UnityEditorInternal.ComponentUtility.PasteComponentValues(comp);
            });
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Move Up"), false, () =>
            {
                UnityEditorInternal.ComponentUtility.MoveComponentUp(comp);
                RefreshTarget();
            });
            menu.AddItem(new GUIContent("Move Down"), false, () =>
            {
                UnityEditorInternal.ComponentUtility.MoveComponentDown(comp);
                RefreshTarget();
            });
            menu.ShowAsContext();
        }

        // ══════════════════════════════════════════════════════════════════════════
        //  DRAG & DROP
        // ══════════════════════════════════════════════════════════════════════════
        void HandleDragDrop(Rect dropRect)
        {
            Event evt = Event.current;
            if (!dropRect.Contains(evt.mousePosition)) return;

            switch (evt.type)
            {
                case EventType.DragUpdated:
                    if (GetDraggedObject() != null)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                        _isDragHover = true;
                        Repaint();
                    }
                    break;

                case EventType.DragPerform:
                    var obj = GetDraggedObject();
                    if (obj != null)
                    {
                        DragAndDrop.AcceptDrag();
                        SetTarget(obj);
                        _isDragHover = false;
                        Repaint();
                    }
                    break;

                case EventType.DragExited:
                    _isDragHover = false;
                    Repaint();
                    break;
            }
        }

        // Nhận drop ở bất kỳ đâu trong window (khi đã có target)
        void HandleDragDropAnywhere()
        {
            Rect windowRect = new Rect(0, 0, position.width, position.height);
            HandleDragDrop(windowRect);
        }

        UnityEngine.Object GetDraggedObject()
        {
            if (DragAndDrop.objectReferences == null
                || DragAndDrop.objectReferences.Length == 0) return null;

            var obj = DragAndDrop.objectReferences[0];
            if (obj is GameObject) return obj;
            if (obj is ScriptableObject) return obj;
            if (obj is Component c) return c.gameObject;
            return null;
        }

        // ══════════════════════════════════════════════════════════════════════════
        //  SET / CLEAR TARGET
        // ══════════════════════════════════════════════════════════════════════════
        void SetTarget(UnityEngine.Object obj)
        {
            ClearEditorCache();

            _target   = null;
            _targetSO = null;

            if (obj is GameObject go)
            {
                _target     = go;
                _components = go.GetComponents<Component>();
            }
            else if (obj is ScriptableObject so)
            {
                _targetSO = so;
            }
            // null → clear all

            _foldouts.Clear();
            _scroll = Vector2.zero;
            titleContent = new GUIContent(obj != null ? obj.name : "Object Inspector");
            Repaint();
        }

        void RefreshTarget()
        {
            ClearEditorCache();
            if (_target != null)
                _components = _target.GetComponents<Component>();
            Repaint();
        }

        void ClearEditorCache()
        {
            foreach (var ed in _editors.Values)
                if (ed != null) DestroyImmediate(ed);
            _editors.Clear();

            if (_soEditor != null) { DestroyImmediate(_soEditor); _soEditor = null; }
        }

        // ══════════════════════════════════════════════════════════════════════════
        //  STYLE INIT
        // ══════════════════════════════════════════════════════════════════════════
        void EnsureStyles()
        {
            if (_stylesReady) return;

            _dropBoxStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize  = 12,
                border    = new RectOffset(4, 4, 4, 4),
            };

            _componentHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize  = 11,
                alignment = TextAnchor.MiddleLeft,
            };

            _goNameStyle = new GUIStyle(EditorStyles.textField)
            {
                fontSize  = 12,
                fontStyle = FontStyle.Bold,
            };

            _stylesReady = true;
        }

        // ── Tạo Texture2D solid color cho drop box border ─────────────────────────
        Texture2D MakeTex(int w, int h, Color fill, Color border)
        {
            var tex = new Texture2D(w, h);
            Color[] pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = fill;
            // viền: không cần vì GUIStyle.border sẽ scale
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        void OnHierarchyChange()
        {
            if (_target != null) RefreshTarget();
        }

        // Khi undo/redo
        void OnUndoRedo()
        {
            if (_target != null) RefreshTarget();
            Repaint();
        }
    }
}