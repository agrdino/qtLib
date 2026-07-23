namespace FewClicksDev.TaskList
{
    using FewClicksDev.Core;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Compilation;
    using UnityEditor.SceneManagement;
    using UnityEngine;

    using static FewClicksDev.Core.EditorDrawer;
    using Preferences = TaskListUserPreferences;

    public class TaskListWindow : CustomEditorWindow
    {
        public enum WindowMode
        {
            CurrentTasks = 0,
            Backlog = 1,
            Settings = 2
        }

        public enum AddFromTODOScope
        {
            SingleScript = 0,
            Folder = 1,
            EntireProject = 2
        }

        private const float TOOLBAR_WIDTH = 0.8f;
        private const float LABEL_WIDTH = 140f;
        private const float SETTINGS_LABEL_WIDTH = 220f;
        private const float DATE_WIDTH = 160f;
        private const float INDEX_WIDTH = 30f;
        private const float COLOR_INDICATOR_WIDTH = 8f;
        private const float BUTTON_WIDTH = 60f;

        public const int MAX_VISIBLE_TASKS = 20;
        private static readonly Color SELECT_COLOR = new Color(0.65f, 0.65f, 0.65f, 1f);
        private static readonly GUIContent REMOVE_CONTEXT_CONTENT = new GUIContent(" X ", "Remove the context from the task.");

        private const string VISIBILITY = "Visibility";
        private const string SORT_BY = "Sort by";
        private const string FILTER_BY_LABEL = "Filter by label";
        private const string FILTER_BY_DESCRIPTION = "Filter by description";
        private const string RESET_TO_DEFAULTS = "Reset to defaults";
        private const string CLOSED_STRING = "<b><i>(Closed)</i></b>  ";
        private const string SELECT_ALL = "Select all";
        private const string COLLAPSE_ALL = "Collapse all";
        private const string MOVE_ALL_TO_BACKLOG = "Move all to backlog";
        private const string MOVE_ALL_TO_CURRENT = "Move all to current";
        private const string CREATE_NEW_TASK = "Create a new Task";
        private const string COLORS = "Colors";
        private const string OTHERS = "Others";
        private const string SCENE_NOT_LOADED = "Scene not loaded";
        private const string OBJECT = "Object";
        private const string SCRIPT = "Script";
        private const string PING = "Ping";
        private const string OPEN = "Open";
        private const string LINK = "Link";
        private const string LOAD = "Load";
        private const string CAMERA_POSITION = "Camera position";
        private const string CAMERA_ROTATION = "Camera rotation";
        private const string MAIN_SCENE = "Main scene";
        private const string SET = "Set";
        private const string UPDATE_SCENES_AND_LOCATION = "Update scenes and location";
        private const string LOAD_CONTEXT_SCENES = "Load context scenes";
        private const string ADD_CONTEXT = "Add context";
        private const string TYPE = "Type";
        private const string CONTEXTS = "Contexts";
        private const string LABEL = "Label";
        private const string ADD_TASKS_FROM_TODO_SCOPE = "Adding tasks scope";
        private const string ADD_TASKS_FROM_TODO = "Add tasks from TODOs comments";
        private const string ADD_FROM_TODO_TO_CURRENT = "Add from TODOs to current";
        private const string ADD_FROM_TODO_TO_BACKLOG = "Add from TODOs to backlog";
        private const string SCRIPT_REFERENCE = "Script reference";
        private const string FOLDER_PATH = "Folder path";
        private const string DESCRIPTION = "Description";
        private const string PRIORITY = "Priority";
        private const string CUSTOM_ORDER = "Custom order";
        private const string DELETE_TASK_QUESTION = "Delete task?";
        private const string NO_TASKS_INFO = "No tasks to display. Create a new task using the button above or change the visibility options!";
        private const string EDIT_MODE_CONTROL = "EditModeTextField";

        protected override string windowName => TaskList.NAME;
        protected override string version => TaskList.VERSION;
        protected override Vector2 minWindowSize => new Vector2(600f, 740f);
        protected override Color mainColor => TaskList.MAIN_COLOR;

        protected override bool askForReview => true;
        protected override string reviewURL => TaskList.REVIEW_URL;
        protected override bool hasDocumentation => true;
        protected override string documentationURL => TaskList.DOCUMENTATION_URL;

        public WindowMode CurrentWindowMode => windowMode;
        private WindowMode windowMode = WindowMode.CurrentTasks;
        private Rect tasksRect = Rect.zero;
        private Task taskInEditMode = null;

        private string tasksLabelFilter = string.Empty;
        private string tasksDescriptionFilter = string.Empty;
        private TaskSelectableList sortedAndFilteredTasks = null;

        private string backlogTasksLabelFilter = string.Empty;
        private string backlogTasksDescriptionFilter = string.Empty;
        private TaskSelectableList sortedAndFilteredBacklogTasks = null;

        private AddFromTODOScope addFromTODOScope = AddFromTODOScope.SingleScript;
        private MonoScript scriptReference = null;
        private string folderPathForAdding = string.Empty;

        protected override void OnEnable()
        {
            base.OnEnable();

            Preferences.OnTasksUpdated -= generateTasksLists;
            Preferences.OnTasksUpdated += generateTasksLists;
            Preferences.LoadPreferences();

            CompilationPipeline.compilationStarted -= onCompilationStarted;
            CompilationPipeline.compilationStarted += onCompilationStarted;
        }

        private void OnDestroy()
        {
            Preferences.SavePreferences();
            Preferences.OnTasksUpdated -= generateTasksLists;
            CompilationPipeline.compilationStarted -= onCompilationStarted;
        }

        public override void AddItemsToMenu(GenericMenu _menu)
        {
            base.AddItemsToMenu(_menu);

            GUIContent _exportCurrentListToJSON = new GUIContent("Export current list to JSON");
            _menu.AddSeparator(string.Empty);
            _menu.AddItem(_exportCurrentListToJSON, false, _exportCurrentToJson);

            GUIContent _importTasksFromJSON = new GUIContent("Import tasks from JSON to the current list");
            _menu.AddItem(_importTasksFromJSON, false, _importFromJson);

            GUIContent _saveAllTasksContent = new GUIContent("Save all tasks");
            _menu.AddItem(_saveAllTasksContent, false, _saveAllTasks);

            void _exportCurrentToJson()
            {
                string _path = EditorUtility.SaveFilePanel("Export tasks to JSON", Application.dataPath, "Tasks.json", "json");

                if (_path.IsNullEmptyOrWhitespace())
                {
                    TaskList.LogError("Invalid path provided for exporting the tasks to JSON!");
                    return;
                }

                string _json = windowMode is WindowMode.Backlog ? Preferences.BacklogTasks.ConvertToJson() : Preferences.Tasks.ConvertToJson();
                System.IO.File.WriteAllText(_path, _json);
                EditorUtility.RevealInFinder(_path);
            }

            void _importFromJson()
            {
                string _path = EditorUtility.OpenFilePanel("Import tasks from JSON", Application.dataPath, "json");

                if (_path.IsNullEmptyOrWhitespace())
                {
                    TaskList.LogError("Invalid path provided for importing the tasks from JSON!");
                    return;
                }

                string _json = System.IO.File.ReadAllText(_path);
                var _tasks = JsonUtility.FromJson<TasksContainer>(_json);

                if (_tasks == null || _tasks.Items.IsNullOrEmpty())
                {
                    TaskList.LogError("No tasks were found in the provided JSON file!");
                    return;
                }

                foreach (var _task in _tasks.Items)
                {
                    Preferences.AddTask(_task, windowMode is WindowMode.Backlog ? TaskListType.Backlog : TaskListType.Current);
                }

                Preferences.SavePreferences();
                generateTasksLists();
            }

            void _saveAllTasks()
            {
                Preferences.SavePreferences();
            }
        }

        protected override void drawWindowGUI()
        {
            NormalSpace();
            windowMode = this.DrawEnumToolbar(windowMode, TOOLBAR_WIDTH, mainColor);
            SmallSpace();
            DrawLine();
            SmallSpace();

            switch (windowMode)
            {
                case WindowMode.CurrentTasks:
                    drawCurrentTasksList();
                    break;

                case WindowMode.Backlog:
                    drawBacklogTasks();
                    break;

                case WindowMode.Settings:
                    drawSettings();
                    break;
            }
        }

        public void RefreshTaskLists()
        {
            generateTasksLists();
        }

        private void generateTasksLists()
        {
            if (sortedAndFilteredTasks == null)
            {
                sortedAndFilteredTasks = new TaskSelectableList();
            }

            if (sortedAndFilteredBacklogTasks == null)
            {
                sortedAndFilteredBacklogTasks = new TaskSelectableList();
            }

            sortAndFilterTasks(TaskListType.Current);
            sortAndFilterTasks(TaskListType.Backlog);
        }

        private void drawCurrentTasksList()
        {
            using (new LabelWidthScope(LABEL_WIDTH))
            {
                using (var _changeCheck = new ChangeCheckScope())
                {
                    Preferences.VisibilityMode = (TasksVisibilityMode) EditorGUILayout.EnumFlagsField(VISIBILITY, Preferences.VisibilityMode);
                    Preferences.SortMode = DrawEnumWithOrder(Preferences.SortMode, SORT_BY, ref Preferences.SortOrder, sumOfPaddings);

                    SmallSpace();
                    tasksLabelFilter = EditorGUILayout.TextField(FILTER_BY_LABEL, tasksLabelFilter);
                    tasksDescriptionFilter = EditorGUILayout.TextField(FILTER_BY_DESCRIPTION, tasksDescriptionFilter);

                    if (_changeCheck.changed)
                    {
                        sortAndFilterTasks(TaskListType.Current);
                    }
                }
            }

            SmallSpace();

            using (new HorizontalScope())
            {
                if (DrawBoxButton(SELECT_ALL, FixedWidthAndHeight(thirdSizeButtonWidth, DEFAULT_LINE_HEIGHT)))
                {
                    sortedAndFilteredTasks.SelectAll();
                }

                FlexibleSpace();

                if (DrawBoxButton(COLLAPSE_ALL, FixedWidthAndHeight(thirdSizeButtonWidth, DEFAULT_LINE_HEIGHT)))
                {
                    sortedAndFilteredTasks.CollapseAll();
                }

                FlexibleSpace();

                if (DrawBoxButton(MOVE_ALL_TO_BACKLOG, FixedWidthAndHeight(thirdSizeButtonWidth, DEFAULT_LINE_HEIGHT)))
                {
                    foreach (var _task in sortedAndFilteredTasks.Items)
                    {
                        Preferences.MoveTaskToBacklog(_task);
                    }

                    sortAndFilterTasks(TaskListType.Current);
                    sortAndFilterTasks(TaskListType.Backlog);
                    Preferences.SavePreferences();
                    return;
                }
            }

            SmallSpace();
            DrawLine();
            SmallSpace();
            drawCreateNewTask(TaskListType.Current);
            drawTasksList(sortedAndFilteredTasks);
        }

        private void drawBacklogTasks()
        {
            using (new LabelWidthScope(LABEL_WIDTH))
            {
                using (var _changeCheck = new ChangeCheckScope())
                {
                    Preferences.BacklogVisibilityMode = (TasksVisibilityMode) EditorGUILayout.EnumFlagsField(VISIBILITY, Preferences.BacklogVisibilityMode);
                    Preferences.BacklogSortMode = DrawEnumWithOrder(Preferences.BacklogSortMode, SORT_BY, ref Preferences.BacklogSortOrder, sumOfPaddings);

                    SmallSpace();
                    backlogTasksLabelFilter = EditorGUILayout.TextField(FILTER_BY_LABEL, backlogTasksLabelFilter);
                    backlogTasksDescriptionFilter = EditorGUILayout.TextField(FILTER_BY_DESCRIPTION, backlogTasksDescriptionFilter);

                    if (_changeCheck.changed)
                    {
                        sortAndFilterTasks(TaskListType.Backlog);
                    }
                }
            }

            SmallSpace();

            using (new HorizontalScope())
            {
                if (DrawBoxButton(SELECT_ALL, FixedWidthAndHeight(thirdSizeButtonWidth, DEFAULT_LINE_HEIGHT)))
                {
                    sortedAndFilteredBacklogTasks.SelectAll();
                }

                FlexibleSpace();

                if (DrawBoxButton(COLLAPSE_ALL, FixedWidthAndHeight(thirdSizeButtonWidth, DEFAULT_LINE_HEIGHT)))
                {
                    sortedAndFilteredBacklogTasks.CollapseAll();
                }

                FlexibleSpace();

                if (DrawBoxButton(MOVE_ALL_TO_CURRENT, FixedWidthAndHeight(thirdSizeButtonWidth, DEFAULT_LINE_HEIGHT)))
                {
                    foreach (var _task in sortedAndFilteredBacklogTasks.Items)
                    {
                        Preferences.MoveBacklogTaskToCurrent(_task);
                    }

                    sortAndFilterTasks(TaskListType.Backlog);
                    sortAndFilterTasks(TaskListType.Current);
                    Preferences.SavePreferences();
                    return;
                }
            }

            SmallSpace();
            DrawLine();
            SmallSpace();
            drawCreateNewTask(TaskListType.Backlog);
            drawTasksList(sortedAndFilteredBacklogTasks);
        }

        private void drawCreateNewTask(TaskListType _taskList)
        {
            using (new HorizontalScope())
            {
                FlexibleSpace();
                float _buttonWidth = windowWidthWithPaddings * 0.75f;

                if (DrawClearBoxButton(CREATE_NEW_TASK, TaskList.MAIN_COLOR, FixedWidthAndHeight(_buttonWidth, DEFAULT_LINE_HEIGHT)))
                {
                    createNewTask(_taskList);
                }

                FlexibleSpace();
            }
        }

        private void createNewTask(TaskListType _list)
        {
            foreach (var _task in Preferences.Tasks.Items)
            {
                _task.SetEditMode(false);
            }

            int _totalTasks = sortedAndFilteredTasks.Count + sortedAndFilteredBacklogTasks.Count;

            Task _newTask = new Task(_list, _totalTasks);
            Preferences.AddTask(_newTask, _list);

            switch (_list)
            {
                case TaskListType.Current:
                    sortAndFilterTasks(TaskListType.Current);
                    break;

                case TaskListType.Backlog:
                    sortAndFilterTasks(TaskListType.Backlog);
                    break;
            }

            _newTask.SetEditMode(true);
        }

        private void sortAndFilterTasks(TaskListType _list)
        {
            switch (_list)
            {
                case TaskListType.Current:
                    sortAndFilterTasks(sortedAndFilteredTasks, Preferences.Tasks.Items, TaskListType.Current);
                    break;

                case TaskListType.Backlog:
                    sortAndFilterTasks(sortedAndFilteredBacklogTasks, Preferences.BacklogTasks.Items, TaskListType.Backlog);
                    break;
            }
        }

        private void sortAndFilterTasks(TaskSelectableList _taskList, List<Task> _tasks, TaskListType _list)
        {
            if (_taskList == null)
            {
                return;
            }

            _taskList.Destroy();
            string _labelSearchFilter = _getLabelSearchFilter();
            string _descriptionSearchFilter = _getDescriptionSearchFilter();

            var _newTasks = new List<Task>();

            //First we filter by name to decrease the number of groups to check
            foreach (var _task in _tasks)
            {
                if (_labelSearchFilter.IsNullEmptyOrWhitespace() && _descriptionSearchFilter.IsNullEmptyOrWhitespace())
                {
                    _newTasks.Add(_task);
                    continue;
                }

                string _taskNameToLower = _task.Label.ToLower().Trim();

                if (_labelSearchFilter.IsNullEmptyOrWhitespace() == false && _taskNameToLower.Contains(_labelSearchFilter))
                {
                    _newTasks.Add(_task);
                    continue;
                }

                string _taskDescriptionToLower = _task.Description.ToLower().Trim();

                if (_descriptionSearchFilter.IsNullEmptyOrWhitespace() == false && _taskDescriptionToLower.Contains(_descriptionSearchFilter))
                {
                    _newTasks.Add(_task);
                    continue;
                }
            }

            //Then we filter by visibility
            for (int i = _newTasks.Count - 1; i >= 0; i--)
            {
                if (_newTasks[i].ShouldBeVisible(_getVisibilityMode()) == false)
                {
                    _newTasks.RemoveAt(i);
                    continue;
                }
            }

            //Then we sort
            switch (_getSortMode())
            {
                case TasksSortMode.CreationDate:
                    _newTasks.Sort((x, y) => x.CreationTimeInTicks.CompareTo(y.CreationTimeInTicks));
                    break;

                case TasksSortMode.Priority:
                    _newTasks.Sort((x, y) => x.Priority.CompareTo(y.Priority));
                    break;

                case TasksSortMode.Label:
                    _newTasks.Sort((x, y) => string.Compare(x.Label, y.Label, System.StringComparison.OrdinalIgnoreCase));
                    break;

                case TasksSortMode.Custom:
                    _newTasks.Sort((x, y) => x.CustomOrder.CompareTo(y.CustomOrder));
                    break;

                case TasksSortMode.ClosedDate:
                    _newTasks.Sort((x, y) => x.ClosedTimeInTicks.CompareTo(y.ClosedTimeInTicks));
                    break;
            }

            if (Preferences.KeepClosedTasksSeparately)
            {
                _newTasks = _newTasks.OrderBy(t => t.IsClosed).ThenByDescending(t => t.IsClosed ? t.ClosedTimeInTicks : 0).ToList();
            }

            if (_getSortOrder() == false)
            {
                _newTasks.Reverse();
            }

            _taskList.Init(_newTasks, DEFAULT_LINE_HEIGHT);

            string _getLabelSearchFilter()
            {
                return _list switch
                {
                    TaskListType.Current => tasksLabelFilter.ToLower().Trim(),
                    TaskListType.Backlog => backlogTasksLabelFilter.ToLower().Trim(),
                    _ => string.Empty
                };
            }

            string _getDescriptionSearchFilter()
            {
                return _list switch
                {
                    TaskListType.Current => tasksDescriptionFilter.ToLower().Trim(),
                    TaskListType.Backlog => backlogTasksDescriptionFilter.ToLower().Trim(),
                    _ => string.Empty
                };
            }

            TasksVisibilityMode _getVisibilityMode()
            {
                return _list switch
                {
                    TaskListType.Current => Preferences.VisibilityMode,
                    TaskListType.Backlog => Preferences.BacklogVisibilityMode,
                    _ => TasksVisibilityMode.All
                };
            }

            TasksSortMode _getSortMode()
            {
                return _list switch
                {
                    TaskListType.Current => Preferences.SortMode,
                    TaskListType.Backlog => Preferences.BacklogSortMode,
                    _ => TasksSortMode.CreationDate
                };
            }

            bool _getSortOrder()
            {
                return _list switch
                {
                    TaskListType.Current => Preferences.SortOrder,
                    TaskListType.Backlog => Preferences.BacklogSortOrder,
                    _ => true
                };
            }
        }

        private void drawTasksList(TaskSelectableList _list)
        {
            if (_list == null)
            {
                return;
            }

            NormalSpace();

            if (_list.Items.IsNullOrEmpty())
            {
                EditorGUILayout.HelpBox(NO_TASKS_INFO, MessageType.Info);
                return;
            }

            float _width = windowWidthWithPaddings - 3f;
            int _visibleEntries = MAX_VISIBLE_TASKS + getExtraLines(DEFAULT_LINE_HEIGHT);
            int _numberOfEntries = _list.Count > _visibleEntries ? _visibleEntries : _list.Count;
            float _height = DEFAULT_LINE_HEIGHT * _visibleEntries;
            bool _visibleScroll = _list.TotalElementsHeight > _height;

            Event _currentEvent = Event.current;

            using (var _scrollScope = new ScrollViewScope(_list.ScrollPosition, false, _visibleScroll, FixedWidth(_width)))
            {
                _list.ScrollPosition = _scrollScope.scrollPosition;
                int _index = 0;

                foreach (var _task in _list.Items)
                {
                    if (drawSingleTask(_currentEvent, _list, _task, _index, _height, _visibleScroll))
                    {
                        return;
                    }

                    _index++;
                }
            }

            Rect _lastRect = GetLastRect();
            Rect _finalRect = new Rect(leftPadding, _lastRect.y, _lastRect.width, _numberOfEntries * DEFAULT_LINE_HEIGHT);

            if (_finalRect.width != 0)
            {
                tasksRect = _finalRect;
            }

            bool _mouseInAnyRect = tasksRect.Contains(_currentEvent.mousePosition);

            if (_currentEvent.type is EventType.MouseDown && _currentEvent.button == 0 && _mouseInAnyRect == false)
            {
                _list.UnselectAll();
                _list.ClearFirstSelected();
                taskInEditMode?.SetEditMode(false);
                taskInEditMode = null;
                DeselectGUIElements();
                Repaint();
            }
        }

        private int getExtraLines(float _singleLineHeight)
        {
            float _heightDifference = windowHeight - minWindowSize.y;
            return Mathf.FloorToInt(_heightDifference / _singleLineHeight);
        }

        private bool drawSingleTask(Event _event, TaskSelectableList _list, Task _task, int _index, float _visibleAreaHeight, bool _visibleSlider)
        {
            if (_task == null)
            {
                return false;
            }

            bool _visible = _list.IsVisible(_task, _visibleAreaHeight);

            if (_visible == false)
            {
                EditorGUILayout.LabelField(string.Empty, FixedHeight(_task.StartPositionAndHeight.y));
                return false;
            }

            using (ColorScope.Background(_task.IsSelected ? SELECT_COLOR : Color.white))
            {
                using (new HorizontalScope())
                {
                    GUILayout.Label($"{_index + 1}", Styles.DefaultLabelCenter, FixedWidth(INDEX_WIDTH));

                    using (ColorScope.BackgroundAndContent(_task.IndicatorColor))
                    {
                        GUILayout.Box(string.Empty, Styles.ClearBox, FixedWidthAndHeight(COLOR_INDICATOR_WIDTH, DEFAULT_LINE_HEIGHT));
                    }

                    bool _isExpanded = GUILayout.Toggle(_task.IsExpanded, string.Empty, Styles.FixedZoom(DEFAULT_LINE_HEIGHT), FixedWidthAndHeight(DEFAULT_LINE_HEIGHT));

                    if (_isExpanded != _task.IsExpanded)
                    {
                        _task.ToggleExpandState();
                        DeselectGUIElements();
                    }

                    float _dateWidth = Preferences.ShowCreationDate ? DATE_WIDTH : 0f;
                    float _maxWidth = windowWidthWithPaddings - (_visibleSlider ? VERTICAL_SLIDER_WIDTH : 0f) - INDEX_WIDTH - _dateWidth - (4f * DEFAULT_LINE_HEIGHT) - COLOR_INDICATOR_WIDTH - 6f;

                    if (_task.IsInEditMode)
                    {
                        taskInEditMode = _task;

                        using (new HorizontalScope(Styles.DefaultLabelLeft, FixedWidth(_maxWidth)))
                        {
                            GUI.SetNextControlName(EDIT_MODE_CONTROL);
                            string _newLabel = EditorGUILayout.TextField(_task.Label);
                            GUI.FocusControl(EDIT_MODE_CONTROL);

                            if (_newLabel != _task.Label)
                            {
                                _task.SetLabel(_newLabel);
                            }
                        }

                        if (Event.current.type is EventType.KeyDown && Event.current.keyCode is KeyCode.Return)
                        {
                            _task.SetEditMode(false);
                            Event.current.Use();
                            return false;
                        }
                    }
                    else if (_task.IsClosed && Preferences.ClosedTaskVisualization is not ClosedTaskVisualizationType.None)
                    {
                        switch (Preferences.ClosedTaskVisualization)
                        {
                            case ClosedTaskVisualizationType.LabelAtTheStart:

                                if (GUILayout.Button(CLOSED_STRING + _task.Label, Styles.DefaultLabelLeft, FixedWidth(_maxWidth)))
                                {
                                    _list.HandleSelection(_task, _event,
                                            () => TaskListGenericMenu.ShowForTasks(this, _event, _list.GetSelectedItems(), _list.Items, _getListFromWindow()));
                                    exitEditMode();
                                    return false;
                                }

                                break;

                            case ClosedTaskVisualizationType.Strikethrough:

                                if (GUILayout.Button(_task.Label, Styles.DefaultLabelLeft, FixedWidth(_maxWidth)))
                                {
                                    _list.HandleSelection(_task, _event,
                                            () => TaskListGenericMenu.ShowForTasks(this, _event, _list.GetSelectedItems(), _list.Items, _getListFromWindow()));
                                    exitEditMode();
                                    return false;
                                }

                                Rect _buttonRect = GetLastRect();
                                float y = _buttonRect.y + _buttonRect.height * 0.5f;
                                Rect _lineRect = new Rect(_buttonRect.x + SMALL_SPACE, y, _buttonRect.width - NORMAL_SPACE, 1f);
                                EditorGUI.DrawRect(_lineRect, DEFAULT_GRAY);
                                break;

                            case ClosedTaskVisualizationType.Dimmed:

                                if (GUILayout.Button(_task.Label, Styles.DefaultLabelLeft.WithColor(new Color(0.85f, 0.85f, 0.85f, 0.35f)), FixedWidth(_maxWidth)))
                                {
                                    _list.HandleSelection(_task, _event,
                                            () => TaskListGenericMenu.ShowForTasks(this, _event, _list.GetSelectedItems(), _list.Items, _getListFromWindow()));
                                    exitEditMode();
                                    return false;
                                }

                                break;
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(_task.Label, Styles.DefaultLabelLeft, FixedWidth(_maxWidth)))
                        {
                            _list.HandleSelection(_task, _event,
                                    () => TaskListGenericMenu.ShowForTasks(this, _event, _list.GetSelectedItems(), _list.Items, _getListFromWindow()));
                            exitEditMode();
                            return false;
                        }
                    }

                    if (Preferences.ShowCreationDate)
                    {
                        if (GUILayout.Button(_task.DateTimeString, Styles.DefaultLabelCenter, FixedWidthAndHeight(DATE_WIDTH, DEFAULT_LINE_HEIGHT)))
                        {
                            _list.HandleSelection(_task, _event,
                                    () => TaskListGenericMenu.ShowForTasks(this, _event, _list.GetSelectedItems(), _list.Items, _getListFromWindow()));
                            exitEditMode();
                            return false;
                        }
                    }

                    bool _isClosed = GUILayout.Toggle(_task.IsClosed, string.Empty, Styles.FixedToggle(DEFAULT_LINE_HEIGHT), FixedWidthAndHeight(DEFAULT_LINE_HEIGHT));

                    if (_isClosed != _task.IsClosed)
                    {
                        _task.SetAsClosed(_isClosed);
                        sortAndFilterTasks(_task.ListType);

                        return true;
                    }

                    if (GUILayout.Button(string.Empty, Styles.FixedSettings(DEFAULT_LINE_HEIGHT), FixedWidthAndHeight(DEFAULT_LINE_HEIGHT)))
                    {
                        _task.IsSelected = true;
                        TaskListGenericMenu.ShowForTasks(this, _event, _list.GetSelectedItems(), _list.Items, _task.ListType);
                    }

                    if (GUILayout.Button(string.Empty, Styles.FixedClose(DEFAULT_LINE_HEIGHT), FixedWidthAndHeight(DEFAULT_LINE_HEIGHT)))
                    {
                        if (Preferences.AskBeforeDeletingTasks)
                        {
                            bool _shouldDelete = EditorUtility.DisplayDialog(DELETE_TASK_QUESTION, $"Are you sure you want to delete the task '{_task.Label}'?", YES, NO);

                            if (_shouldDelete == false)
                            {
                                return false;
                            }
                        }

                        Preferences.RemoveTask(_task, _task.ListType);
                        sortAndFilterTasks(_task.ListType);

                        return true;
                    }

                    VerySmallSpace();
                }
            }

            if (_task.IsExpanded)
            {
                bool _changed = drawExpandedTask(_task);

                if (_changed)
                {
                    return true;
                }
            }

            TaskListType _getListFromWindow()
            {
                return windowMode switch
                {
                    WindowMode.CurrentTasks => TaskListType.Current,
                    WindowMode.Backlog => TaskListType.Backlog,
                    _ => TaskListType.Current
                };
            }

            return false;
        }

        private void exitEditMode()
        {
            if (taskInEditMode != null)
            {
                taskInEditMode.SetEditMode(false);
                taskInEditMode = null;
                DeselectGUIElements();
            }
        }

        private bool drawExpandedTask(Task _task)
        {
            using (new ScopeGroup(new LabelWidthScope(LABEL_WIDTH), new HorizontalScope()))
            {
                using (new VerticalScope(Styles.LightButton))
                {
                    using (new HorizontalScope())
                    {
                        LargeSpace();

                        using (new VerticalScope())
                        {
                            bool _collectionChanged = drawExpandedTasksProperties(_task);

                            if (_collectionChanged)
                            {
                                return true;
                            }
                        }

                        NormalSpace();
                    }
                }

                VerySmallSpace();
            }

            return false;
        }

        private bool drawExpandedTasksProperties(Task _task)
        {
            using (var _changeScope = new ChangeCheckScope())
            {
                SmallSpace();
                string _taskLabel = EditorGUILayout.TextField(LABEL, _task.Label);

                if (_taskLabel != _task.Label)
                {
                    _task.SetLabel(_taskLabel);
                }

                GUIStyle _areaWithWrap = new GUIStyle(EditorStyles.textArea);
                _areaWithWrap.wordWrap = true;

                string _taskDescription = EditorGUILayout.TextField(DESCRIPTION, _task.Description, _areaWithWrap, FixedHeight(SingleLineHeight * 3f));

                if (_taskDescription != _task.Description)
                {
                    _task.SetDescription(_taskDescription);
                }

                TaskPriority _priority = (TaskPriority) EditorGUILayout.EnumPopup(PRIORITY, _task.Priority);

                if (_priority != _task.Priority)
                {
                    _task.SetPriority(_priority);
                    sortAndFilterTasks(_task.ListType);

                    return true;
                }

                int _customOrder = EditorGUILayout.IntField(CUSTOM_ORDER, _task.CustomOrder);

                if (_customOrder != _task.CustomOrder)
                {
                    _task.SetCustomOrder(_customOrder);
                    sortAndFilterTasks(_task.ListType);
                    return true;
                }

                if (_task.NumberOfContexts == 0)
                {
                    _drawAddContextButton();
                    return false;
                }

                SmallSpace();
                GUILayout.Label(CONTEXTS, EditorStyles.boldLabel.WithColor(Color.white));
                VerySmallSpace();

                for (int i = 0; i < _task.NumberOfContexts; i++)
                {
                    var _context = _task.GetContextAtIndex(i);

                    if (_context == null)
                    {
                        continue;
                    }

                    using (new VerticalScope(EditorStyles.helpBox))
                    {
                        VerySmallSpace();
                        var _typeOfContext = _context.ContextType;

                        using (new HorizontalScope())
                        {
                            EditorGUILayout.PrefixLabel(TYPE);
                            _typeOfContext = (TaskContextType) EditorGUILayout.EnumPopup(_typeOfContext);

                            VerySmallSpace();

                            if (GUILayout.Button(REMOVE_CONTEXT_CONTENT, FixedWidth(20f)))
                            {
                                _task.RemoveContextAtIndex(i);
                                return false;
                            }
                        }

                        if (_typeOfContext != _context.ContextType)
                        {
                            _context.SetContextType(_typeOfContext);

                            switch (windowMode)
                            {
                                case WindowMode.CurrentTasks:
                                    sortedAndFilteredTasks.RefreshHeights(_task);
                                    break;

                                case WindowMode.Backlog:
                                    sortedAndFilteredBacklogTasks.RefreshHeights(_task);
                                    break;
                            }
                        }

                        switch (_context.ContextType)
                        {
                            case TaskContextType.Object:
                                _drawObjectContext(_context);
                                break;

                            case TaskContextType.Script:
                                _drawScriptContext(_context);
                                break;

                            case TaskContextType.Scenes:
                                _drawSceneContext(_context);
                                break;

                            case TaskContextType.Link:
                                _drawLinkContext(_context);
                                break;

                            case TaskContextType.SceneGameObject:
                                _drawSceneGameObjectContext(_context);
                                break;
                        }

                        VerySmallSpace();
                    }
                }

                if (_changeScope.changed)
                {
                    Preferences.SavePreferences();
                }
            }

            _drawAddContextButton();
            return false;

            void _drawObjectContext(TaskContext _context)
            {
                using (new HorizontalScope())
                {
                    var _object = EditorGUILayout.ObjectField(OBJECT, _context.ObjectReference, typeof(Object), false);

                    if (_object != _context.ObjectReference)
                    {
                        _context.SetObjectReference(_object);
                    }

                    VerySmallSpace();

                    using (new DisabledScope(_context.ObjectReference == null))
                    {
                        if (GUILayout.Button(PING, FixedWidth(BUTTON_WIDTH)))
                        {
                            AssetsUtilities.Ping(_context.ObjectReference);
                        }
                    }
                }
            }

            void _drawScriptContext(TaskContext _context)
            {
                using (new HorizontalScope())
                {
                    var _script = EditorGUILayout.ObjectField(SCRIPT, _context.ScriptReference, typeof(MonoScript), false);

                    if (_script != _context.ScriptReference)
                    {
                        _context.SetScriptReference(_script as MonoScript);
                    }

                    VerySmallSpace();

                    using (new DisabledScope(_context.ScriptReference == null))
                    {
                        if (GUILayout.Button(OPEN, FixedWidth(BUTTON_WIDTH)))
                        {
                            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(_context.ScriptReference.GetAssetPath(), _context.LineOfContext);
                            AssetDatabase.OpenAsset(_context.ScriptReference);
                        }
                    }
                }
            }

            void _drawSceneContext(TaskContext _context)
            {
                using (new HorizontalScope())
                {
                    EditorGUILayout.PrefixLabel(CAMERA_POSITION);

                    using (new DisabledScope())
                    {
                        EditorGUILayout.Vector3Field(string.Empty, _context.SceneReference.CameraPosition);
                    }

                    VerySmallSpace();

                    if (GUILayout.Button(SET, FixedWidth(BUTTON_WIDTH)))
                    {
                        _context.UpdateCameraPosition();
                    }
                }

                using (new HorizontalScope())
                {
                    EditorGUILayout.PrefixLabel(CAMERA_ROTATION);

                    using (new DisabledScope())
                    {
                        EditorGUILayout.Vector3Field(string.Empty, _context.SceneReference.CameraRotation);
                    }

                    VerySmallSpace();

                    if (GUILayout.Button(SET, FixedWidth(BUTTON_WIDTH)))
                    {
                        _context.UpdateCameraRotation();
                    }
                }

                SmallSpace();
                EditorGUILayout.TextField(MAIN_SCENE, _context.SceneReference.MainSceneName);

                if (_context.NumberOfAdditionalScenes > 0)
                {
                    VerySmallSpace();
                    int _index = 1;

                    foreach (var _sceneName in _context.SceneReference.AdditionalScenesNames)
                    {
                        EditorGUILayout.TextField($"Additional scene #{_index}", _sceneName);
                        _index++;
                    }
                }

                SmallSpace();
                float _buttonWidth = windowWidthWithPaddings / 2.5f;

                using (new HorizontalScope())
                {
                    FlexibleSpace();

                    if (GUILayout.Button(UPDATE_SCENES_AND_LOCATION, FixedWidth(_buttonWidth)))
                    {
                        _context.UpdateLoadedScenes();
                    }

                    FlexibleSpace();

                    using (new DisabledScope(_context.SceneReference.MainSceneName.IsNullEmptyOrWhitespace()))
                    {
                        if (GUILayout.Button(LOAD_CONTEXT_SCENES, FixedWidth(_buttonWidth)))
                        {
                            _context.LoadContextScenes();
                        }
                    }

                    FlexibleSpace();
                }
            }

            void _drawLinkContext(TaskContext _context)
            {
                using (new HorizontalScope())
                {
                    var _link = EditorGUILayout.TextField(LINK, _context.LinkReference);

                    if (_link != _context.LinkReference)
                    {
                        _context.SetLinkReference(_link);
                    }

                    VerySmallSpace();

                    using (new DisabledScope(_context.LinkReference.IsNullEmptyOrWhitespace()))
                    {
                        if (GUILayout.Button(OPEN, FixedWidth(BUTTON_WIDTH)))
                        {
                            Application.OpenURL(_context.LinkReference);
                        }
                    }
                }
            }

            void _drawSceneGameObjectContext(TaskContext _context)
            {
                bool _validIDAndSceneNotLoaded = _context.IsValidIDSceneNotLoaded;

                if (_validIDAndSceneNotLoaded)
                {
                    using (new HorizontalScope())
                    {
                        DrawDefaultLabel(SCENE_NOT_LOADED);

                        using (new DisabledScope())
                        {
                            EditorGUILayout.TextField(_context.ContainedObjectScenePath);
                        }
                    }
                }

                using (new HorizontalScope())
                {
                    if (_validIDAndSceneNotLoaded)
                    {
                        DrawDefaultLabel(OBJECT);

                        using (new DisabledScope())
                        {
                            EditorGUILayout.ObjectField(GUIContent.none, _context.SceneObjectReference, typeof(Transform), true);
                        }

                        VerySmallSpace();

                        if (GUILayout.Button(LOAD, FixedWidth(BUTTON_WIDTH)))
                        {
                            EditorSceneManager.OpenScene(_context.ContainedObjectScenePath, OpenSceneMode.Additive);
                        }

                        return;
                    }

                    var _object = EditorGUILayout.ObjectField(OBJECT, _context.SceneObjectReference, typeof(Transform), true) as Transform;

                    if (_object != _context.SceneObjectReference)
                    {
                        _context.SetSceneObjectReference(_object);
                    }

                    VerySmallSpace();

                    using (new DisabledScope(_context.SceneObjectReference == null))
                    {
                        if (GUILayout.Button(PING, FixedWidth(BUTTON_WIDTH)))
                        {
                            AssetsUtilities.Ping(_context.SceneObjectReference);
                        }
                    }
                }
            }

            void _drawAddContextButton()
            {
                SmallSpace();

                using (new HorizontalScope())
                {
                    FlexibleSpace();

                    if (DrawBoxButton(ADD_CONTEXT, FixedWidthAndHeight(windowWidthScaled(0.5f), DEFAULT_LINE_HEIGHT)))
                    {
                        TaskContext _context = new TaskContext();
                        _task.AddContext(_context);
                    }

                    FlexibleSpace();
                }

                SmallSpace();
            }
        }

        private void drawSettings()
        {
            using (new LabelWidthScope(SETTINGS_LABEL_WIDTH))
            {
                drawAddTasksFromTODO();

                using (var _changeCheck = new ChangeCheckScope())
                {
                    DrawHeader(COLORS);
                    Preferences.ImmediatePriorityColor = EditorGUILayout.ColorField(Preferences.IMMEDIATE_COLOR, Preferences.ImmediatePriorityColor);
                    Preferences.HighPriorityColor = EditorGUILayout.ColorField(Preferences.HIGH_COLOR, Preferences.HighPriorityColor);
                    Preferences.NormalPriorityColor = EditorGUILayout.ColorField(Preferences.NORMAL_COLOR, Preferences.NormalPriorityColor);
                    Preferences.LowPriorityColor = EditorGUILayout.ColorField(Preferences.LOW_COLOR, Preferences.LowPriorityColor);

                    SmallSpace();
                    Preferences.ClosedColor = EditorGUILayout.ColorField(Preferences.CLOSED_COLOR, Preferences.ClosedColor);

                    DrawHeader(LOGS);
                    Preferences.PrintLogs = EditorGUILayout.Toggle(Preferences.PRINT_LOGS, Preferences.PrintLogs);

                    DrawHeader(OTHERS);
                    Preferences.DefaultListType = (TaskListType) EditorGUILayout.EnumPopup(Preferences.DEFAULT_LIST, Preferences.DefaultListType);
                    Preferences.ShowCreationDate = EditorGUILayout.Toggle(Preferences.SHOW_CREATION_DATE, Preferences.ShowCreationDate);
                    Preferences.ApplyPositionAndRotationOnLoad = EditorGUILayout.Toggle(Preferences.APPLY_POSITION_AND_ROTATION_ON_LOAD, Preferences.ApplyPositionAndRotationOnLoad);
                    Preferences.AskBeforeDeletingTasks = EditorGUILayout.Toggle(Preferences.ASK_BEFORE_DELETING_TASKS, Preferences.AskBeforeDeletingTasks);
                    Preferences.KeepClosedTasksSeparately = EditorGUILayout.Toggle(Preferences.KEEP_CLOSED_TASKS_SEPARATELY, Preferences.KeepClosedTasksSeparately);
                    Preferences.ClosedTaskVisualization = (ClosedTaskVisualizationType) EditorGUILayout.EnumPopup(Preferences.CLOSED_TASK_VISUALIZATION, Preferences.ClosedTaskVisualization);

                    if (_changeCheck.changed)
                    {
                        Preferences.SavePreferences();
                    }
                }
            }

            NormalSpace();

            using (new HorizontalScope())
            {
                FlexibleSpace();

                if (DrawBoxButton(RESET_TO_DEFAULTS, FixedWidthAndHeight(windowWidthWithPaddings / 2f, DEFAULT_LINE_HEIGHT)))
                {
                    Preferences.ResetToDefaults();
                }

                FlexibleSpace();
            }
        }

        private void drawAddTasksFromTODO()
        {
            SmallSpace();
            DrawCenteredBoldLabel(ADD_TASKS_FROM_TODO);
            SmallSpace();
            addFromTODOScope = (AddFromTODOScope) EditorGUILayout.EnumPopup(ADD_TASKS_FROM_TODO_SCOPE, addFromTODOScope);

            switch (addFromTODOScope)
            {
                case AddFromTODOScope.SingleScript:
                    scriptReference = EditorGUILayout.ObjectField(SCRIPT_REFERENCE, scriptReference, typeof(MonoScript), false) as MonoScript;
                    break;
                case AddFromTODOScope.Folder:
                    folderPathForAdding = DrawFolderPicker(FOLDER_PATH, folderPathForAdding, "Select folder from which Scripts will be searched.", true);
                    break;
            }

            if (_drawButton())
            {
                NormalSpace();

                using (new HorizontalScope())
                {
                    FlexibleSpace();

                    if (DrawBoxButton(ADD_FROM_TODO_TO_CURRENT, FixedWidthAndHeight(halfSizeButtonWidth, DEFAULT_LINE_HEIGHT)))
                    {
                        Preferences.AddTasksFromTODO(TaskListType.Current, _getScripts());
                    }

                    NormalSpace();

                    if (DrawBoxButton(ADD_FROM_TODO_TO_BACKLOG, FixedWidthAndHeight(halfSizeButtonWidth, DEFAULT_LINE_HEIGHT)))
                    {
                        Preferences.AddTasksFromTODO(TaskListType.Backlog, _getScripts());
                    }

                    FlexibleSpace();
                }
            }

            NormalSpace();
            DrawLine();
            SmallSpace();

            bool _drawButton()
            {
                switch (addFromTODOScope)
                {
                    case AddFromTODOScope.SingleScript:
                        return scriptReference != null;

                    case AddFromTODOScope.Folder:
                        return AssetDatabase.IsValidFolder(AssetsUtilities.ConvertAbsolutePathToDataPath(folderPathForAdding));

                    default:
                        return true;
                }
            }

            MonoScript[] _getScripts()
            {
                switch (addFromTODOScope)
                {
                    case AddFromTODOScope.SingleScript:
                        return new MonoScript[] { scriptReference };

                    case AddFromTODOScope.Folder:
                        var _scriptsInFolder = AssetsUtilities.GetAssetsOfType<MonoScript>(string.Empty, AssetsUtilities.ConvertAbsolutePathToDataPath(folderPathForAdding));
                        return _scriptsInFolder;

                    default:
                        var _allScripts = AssetsUtilities.GetAssetsOfType<MonoScript>(string.Empty, "Assets");
                        return _allScripts;
                }
            }
        }

        private void onCompilationStarted(object _obj)
        {
            Preferences.SavePreferences();
        }

        [MenuItem("Window/FewClicks Dev/Task List", priority = 108)]
        private static void ShowWindow()
        {
            var _window = GetWindow<TaskListWindow>();

            switch (Preferences.DefaultListType)
            {
                case TaskListType.Backlog:
                    _window.windowMode = WindowMode.Backlog;
                    break;

                default:
                    _window.windowMode = WindowMode.CurrentTasks;
                    break;
            }

            _window.Show();
        }
    }
}