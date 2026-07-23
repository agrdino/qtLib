namespace FewClicksDev.TaskList
{
    using FewClicksDev.Core;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Events;

    using static FewClicksDev.Core.EditorDrawer;

    public static class TaskListUserPreferences
    {
        public static event UnityAction OnTasksUpdated = null;

        private const string PREFS_PATH = "FewClicks Dev/Task List";
        private const string LABEL = "Task List";
        private const SettingsScope SETTINGS_SCOPE = SettingsScope.User;

        private static readonly string PREFS_PREFIX = $"{PlayerSettings.productName}.FewClicksDev.{LABEL}.";
        private static readonly string[] KEYWORDS = new string[] { "FewClicks Dev", LABEL, "Task", "List" };
        private static readonly string[] POSSIBLE_TODOS = new string[] { "//todo", "// todo" };
        private static readonly char[] CHARACTERS_TO_TRIM = new char[] { ':', '-', '–', ' ', ',', '.', '_' };

        public static readonly GUIContent PRINT_LOGS_CONTENT = new GUIContent("Print logs", "Flag specifying if logs should be printed to the console");

        public const float LABEL_WIDTH = 250f;
        public const string RESET_TO_DEFAULTS = "Reset to defaults";
        public const string NUMBER_OF_CURRENT_TASKS = "Number of current tasks";
        public const string NUMBER_OF_BACKLOG_TASKS = "Number of backlog tasks";
        public const string IMMEDIATE_COLOR = "Immediate priority";
        public const string HIGH_COLOR = "High priority";
        public const string NORMAL_COLOR = "Normal priority";
        public const string LOW_COLOR = "Low priority";
        public const string CLOSED_COLOR = "Closed";
        public const string PRINT_LOGS = "Print logs";
        public const string OTHERS = "Others";
        public const string DEFAULT_LIST = "Default list";
        public const string DELETE_CURRENT_TASKS = "Delete current tasks";
        public const string DELETE_BACKLOG_TASKS = "Delete backlog tasks";
        public const string SHOW_CREATION_DATE = "Show creation date";
        public const string APPLY_POSITION_AND_ROTATION_ON_LOAD = "Apply position and rotation on load";
        public const string ASK_BEFORE_DELETING_TASKS = "Ask before deleting tasks";
        public const string KEEP_CLOSED_TASKS_SEPARATELY = "Keep closed tasks separately";
        public const string CLOSED_TASK_VISUALIZATION = "Closed task visualization";

        private const TasksVisibilityMode DEFAULT_VISIBILITY_MODE = TasksVisibilityMode.AllOpen;
        private const TasksSortMode DEFAULT_SORT_MODE = TasksSortMode.CreationDate;
        private const bool DEFAULT_SORT_ORDER = true;

        private static readonly Color DEFAULT_IMMEDIATE_COLOR = new Color(0.615686f, 0.254902f, 0.294118f, 1f);
        private static readonly Color DEFAULT_HIGH_COLOR = new Color(0.726415f, 0.552463f, 0.147339f, 1f);
        private static readonly Color DEFAULT_NORMAL_COLOR = new Color(0.193441f, 0.59434f, 0.193441f, 1f);
        private static readonly Color DEFAULT_LOW_COLOR = new Color(0.184674f, 0.354659f, 0.471698f, 1f);
        private static readonly Color DEFAULT_CLOSED_COLOR = new Color(0.301961f, 0.301961f, 0.301961f, 1f);

        private const bool DEFAULT_PRINT_LOGS = false;

        private const TaskListType DEFAULT_LIST_TYPE = TaskListType.Current;
        private const bool DEFAULT_SHOW_CREATION_DATE = true;
        private const bool DEFAULT_APPLY_POSITION_AND_ROTATION_ON_LOAD = true;
        private const bool DEFAULT_ASK_BEFORE_DELETING_TASKS = true;
        private const bool DEFAULT_KEEP_CLOSED_TASKS_SEPARATELY = true;
        private const ClosedTaskVisualizationType DEFAULT_CLOSED_TASK_VISUALIZATION = ClosedTaskVisualizationType.LabelAtTheStart;

        //Current
        public static TasksContainer Tasks = new TasksContainer();
        public static TasksVisibilityMode VisibilityMode = DEFAULT_VISIBILITY_MODE;
        public static TasksSortMode SortMode = DEFAULT_SORT_MODE;
        public static bool SortOrder = DEFAULT_SORT_ORDER;

        //Backlog
        public static TasksContainer BacklogTasks = new TasksContainer();
        public static TasksVisibilityMode BacklogVisibilityMode = DEFAULT_VISIBILITY_MODE;
        public static TasksSortMode BacklogSortMode = DEFAULT_SORT_MODE;
        public static bool BacklogSortOrder = DEFAULT_SORT_ORDER;

        //Settings
        public static Color ImmediatePriorityColor = DEFAULT_IMMEDIATE_COLOR;
        public static Color HighPriorityColor = DEFAULT_HIGH_COLOR;
        public static Color NormalPriorityColor = DEFAULT_NORMAL_COLOR;
        public static Color LowPriorityColor = DEFAULT_LOW_COLOR;
        public static Color ClosedColor = DEFAULT_CLOSED_COLOR;

        //Logs
        public static bool PrintLogs = DEFAULT_PRINT_LOGS;

        //Others
        public static TaskListType DefaultListType = DEFAULT_LIST_TYPE;
        public static bool ApplyPositionAndRotationOnLoad = DEFAULT_APPLY_POSITION_AND_ROTATION_ON_LOAD;
        public static bool AskBeforeDeletingTasks = DEFAULT_ASK_BEFORE_DELETING_TASKS;
        public static bool ShowCreationDate = DEFAULT_SHOW_CREATION_DATE;
        public static ClosedTaskVisualizationType ClosedTaskVisualization = DEFAULT_CLOSED_TASK_VISUALIZATION;
        public static bool KeepClosedTasksSeparately = DEFAULT_KEEP_CLOSED_TASKS_SEPARATELY;

        private static bool arePrefsLoaded = false;

        static TaskListUserPreferences()
        {
            LoadPreferences();
        }

        [SettingsProvider]
        public static SettingsProvider PreferencesSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(PREFS_PATH, SETTINGS_SCOPE)
            {
                label = LABEL,
                guiHandler = (searchContext) =>
                {
                    OnGUI();
                },

                keywords = new HashSet<string>(KEYWORDS)
            };

            return provider;
        }

        public static void OnGUI()
        {
            using (new IndentScope())
            {
                using (new LabelWidthScope(LABEL_WIDTH))
                {
                    if (arePrefsLoaded == false)
                    {
                        LoadPreferences();
                    }

                    SmallSpace();

                    using (new HorizontalScope())
                    {
                        DrawDefaultLabel(NUMBER_OF_CURRENT_TASKS);

                        using (new DisabledScope())
                        {
                            EditorGUILayout.TextField(Tasks.Items.Count.ToString());
                        }
                    }

                    using (new HorizontalScope())
                    {
                        DrawDefaultLabel(NUMBER_OF_BACKLOG_TASKS);

                        using (new DisabledScope())
                        {
                            EditorGUILayout.TextField(BacklogTasks.Items.Count.ToString());
                        }
                    }

                    DrawHeader(SETTINGS);
                    ImmediatePriorityColor = EditorGUILayout.ColorField(IMMEDIATE_COLOR, ImmediatePriorityColor);
                    HighPriorityColor = EditorGUILayout.ColorField(HIGH_COLOR, HighPriorityColor);
                    NormalPriorityColor = EditorGUILayout.ColorField(NORMAL_COLOR, NormalPriorityColor);
                    LowPriorityColor = EditorGUILayout.ColorField(LOW_COLOR, LowPriorityColor);

                    SmallSpace();
                    ClosedColor = EditorGUILayout.ColorField(CLOSED_COLOR, ClosedColor);

                    DrawHeader(LOGS);
                    PrintLogs = EditorGUILayout.Toggle(PRINT_LOGS_CONTENT, PrintLogs);

                    DrawHeader(OTHERS);
                    DefaultListType = (TaskListType) EditorGUILayout.EnumPopup(DEFAULT_LIST, DefaultListType);
                    ShowCreationDate = EditorGUILayout.Toggle(SHOW_CREATION_DATE, ShowCreationDate);
                    ApplyPositionAndRotationOnLoad = EditorGUILayout.Toggle(APPLY_POSITION_AND_ROTATION_ON_LOAD, ApplyPositionAndRotationOnLoad);
                    AskBeforeDeletingTasks = EditorGUILayout.Toggle(ASK_BEFORE_DELETING_TASKS, AskBeforeDeletingTasks);
                    KeepClosedTasksSeparately = EditorGUILayout.Toggle(KEEP_CLOSED_TASKS_SEPARATELY, KeepClosedTasksSeparately);
                    ClosedTaskVisualization = (ClosedTaskVisualizationType) EditorGUILayout.EnumPopup(CLOSED_TASK_VISUALIZATION, ClosedTaskVisualization);

                    NormalSpace();

                    using (new HorizontalScope())
                    {
                        FlexibleSpace();

                        if (DrawBoxButton(RESET_TO_DEFAULTS, FixedWidthAndHeight(EditorGUIUtility.currentViewWidth / 2f, DEFAULT_LINE_HEIGHT)))
                        {
                            ResetToDefaults();
                        }

                        FlexibleSpace();
                    }

                    SmallSpace();

                    using (new HorizontalScope())
                    {
                        FlexibleSpace();

                        if (DrawBoxButton(DELETE_CURRENT_TASKS, FixedWidthAndHeight(EditorGUIUtility.currentViewWidth / 2f, DEFAULT_LINE_HEIGHT)))
                        {
                            Tasks.Items.Clear();
                            SavePreferences();
                        }

                        FlexibleSpace();
                    }

                    SmallSpace();

                    using (new HorizontalScope())
                    {
                        FlexibleSpace();

                        if (DrawBoxButton(DELETE_BACKLOG_TASKS, FixedWidthAndHeight(EditorGUIUtility.currentViewWidth / 2f, DEFAULT_LINE_HEIGHT)))
                        {
                            BacklogTasks.Items.Clear();
                            SavePreferences();
                        }

                        FlexibleSpace();
                    }

                    if (GUI.changed == true)
                    {
                        SavePreferences();
                    }
                }
            }
        }

        public static void AddTask(Task _task, TaskListType _list, bool _save = true)
        {
            switch (_list)
            {
                case TaskListType.Current:
                    addTask(_task);
                    break;

                case TaskListType.Backlog:
                    addBacklogTask(_task);
                    break;
            }

            if (_save)
            {
                SavePreferences();
            }
        }

        public static void RemoveTask(Task _task, TaskListType _list, bool _save = true)
        {
            switch (_list)
            {
                case TaskListType.Current:
                    removeTask(_task);
                    break;

                case TaskListType.Backlog:
                    removeBacklogTask(_task);
                    break;
            }

            if (_save)
            {
                SavePreferences();
            }
        }

        public static void MoveTaskToBacklog(Task _task)
        {
            if (_task == null || _task.ListType != TaskListType.Current)
            {
                return;
            }

            removeTask(_task);
            _task.SetListType(TaskListType.Backlog);
            addBacklogTask(_task);
        }

        public static void MoveBacklogTaskToCurrent(Task _task)
        {
            if (_task == null || _task.ListType != TaskListType.Backlog)
            {
                return;
            }

            removeBacklogTask(_task);
            _task.SetListType(TaskListType.Current);
            addTask(_task);
        }

        public static Color GetColorFromPriority(TaskPriority _priority)
        {
            return _priority switch
            {
                TaskPriority.Immediate => ImmediatePriorityColor,
                TaskPriority.High => HighPriorityColor,
                TaskPriority.Normal => NormalPriorityColor,
                TaskPriority.Low => LowPriorityColor,
                _ => Color.white
            };
        }

        public static void SavePreferences(bool _printLog = true)
        {
            //Current
            EditorPrefs.SetInt(PREFS_PREFIX + nameof(VisibilityMode), (int) VisibilityMode);
            EditorPrefs.SetInt(PREFS_PREFIX + nameof(SortMode), (int) SortMode);
            EditorPrefs.SetBool(PREFS_PREFIX + nameof(SortOrder), SortOrder);

            string _tasksJson = Tasks.ConvertToJson();
            EditorPrefs.SetString(PREFS_PREFIX + nameof(Tasks), _tasksJson);

            //Backlog
            EditorPrefs.SetInt(PREFS_PREFIX + nameof(BacklogVisibilityMode), (int) BacklogVisibilityMode);
            EditorPrefs.SetInt(PREFS_PREFIX + nameof(BacklogSortMode), (int) BacklogSortMode);
            EditorPrefs.SetBool(PREFS_PREFIX + nameof(BacklogSortOrder), BacklogSortOrder);

            string _backlogTasksJson = BacklogTasks.ConvertToJson();
            EditorPrefs.SetString(PREFS_PREFIX + nameof(BacklogTasks), _backlogTasksJson);

            //Settings
            EditorPrefs.SetString(PREFS_PREFIX + nameof(ImmediatePriorityColor), EditorExtensions.GetStringFromColor(ImmediatePriorityColor));
            EditorPrefs.SetString(PREFS_PREFIX + nameof(HighPriorityColor), EditorExtensions.GetStringFromColor(HighPriorityColor));
            EditorPrefs.SetString(PREFS_PREFIX + nameof(NormalPriorityColor), EditorExtensions.GetStringFromColor(NormalPriorityColor));
            EditorPrefs.SetString(PREFS_PREFIX + nameof(LowPriorityColor), EditorExtensions.GetStringFromColor(LowPriorityColor));
            EditorPrefs.SetString(PREFS_PREFIX + nameof(ClosedColor), EditorExtensions.GetStringFromColor(ClosedColor));

            //Logs
            EditorPrefs.SetBool(PREFS_PREFIX + nameof(PrintLogs), PrintLogs);

            //Others
            EditorPrefs.SetInt(PREFS_PREFIX + nameof(DefaultListType), (int) DefaultListType);
            EditorPrefs.SetBool(PREFS_PREFIX + nameof(ShowCreationDate), ShowCreationDate);
            EditorPrefs.SetBool(PREFS_PREFIX + nameof(ApplyPositionAndRotationOnLoad), ApplyPositionAndRotationOnLoad);
            EditorPrefs.SetBool(PREFS_PREFIX + nameof(AskBeforeDeletingTasks), AskBeforeDeletingTasks);
            EditorPrefs.SetBool(PREFS_PREFIX + nameof(KeepClosedTasksSeparately), KeepClosedTasksSeparately);
            EditorPrefs.SetInt(PREFS_PREFIX + nameof(ClosedTaskVisualization), (int) ClosedTaskVisualization);

            if (_printLog)
            {
                TaskList.Log("Preferences and tasks saved!");
            }
        }

        public static void LoadPreferences()
        {
            if (arePrefsLoaded)
            {
                OnTasksUpdated?.Invoke();
                return;
            }

            //Current
            VisibilityMode = (TasksVisibilityMode) EditorPrefs.GetInt(PREFS_PREFIX + nameof(VisibilityMode), (int) DEFAULT_VISIBILITY_MODE);
            SortMode = (TasksSortMode) EditorPrefs.GetInt(PREFS_PREFIX + nameof(SortMode), (int) DEFAULT_SORT_MODE);
            SortOrder = EditorPrefs.GetBool(PREFS_PREFIX + nameof(SortOrder), DEFAULT_SORT_ORDER);

            string _tasksJson = EditorPrefs.GetString(PREFS_PREFIX + nameof(Tasks), string.Empty);
            Tasks = _tasksJson.IsNullEmptyOrWhitespace() == false ?
                JsonUtility.FromJson<TasksContainer>(_tasksJson) :
                new TasksContainer();

            //Backlog
            BacklogVisibilityMode = (TasksVisibilityMode) EditorPrefs.GetInt(PREFS_PREFIX + nameof(BacklogVisibilityMode), (int) DEFAULT_VISIBILITY_MODE);
            BacklogSortMode = (TasksSortMode) EditorPrefs.GetInt(PREFS_PREFIX + nameof(BacklogSortMode), (int) DEFAULT_SORT_MODE);
            BacklogSortOrder = EditorPrefs.GetBool(PREFS_PREFIX + nameof(BacklogSortOrder), DEFAULT_SORT_ORDER);

            string _backlogTasksJson = EditorPrefs.GetString(PREFS_PREFIX + nameof(BacklogTasks), string.Empty);
            BacklogTasks = _backlogTasksJson.IsNullEmptyOrWhitespace() == false ?
                JsonUtility.FromJson<TasksContainer>(_backlogTasksJson) :
                new TasksContainer();

            //Settings
            ImmediatePriorityColor = EditorExtensions.LoadColor(PREFS_PREFIX + IMMEDIATE_COLOR, DEFAULT_IMMEDIATE_COLOR);
            HighPriorityColor = EditorExtensions.LoadColor(PREFS_PREFIX + HIGH_COLOR, DEFAULT_HIGH_COLOR);
            NormalPriorityColor = EditorExtensions.LoadColor(PREFS_PREFIX + NORMAL_COLOR, DEFAULT_NORMAL_COLOR);
            LowPriorityColor = EditorExtensions.LoadColor(PREFS_PREFIX + LOW_COLOR, DEFAULT_LOW_COLOR);
            ClosedColor = EditorExtensions.LoadColor(PREFS_PREFIX + CLOSED_COLOR, DEFAULT_CLOSED_COLOR);

            //Logs
            PrintLogs = EditorPrefs.GetBool(PREFS_PREFIX + nameof(PrintLogs), DEFAULT_PRINT_LOGS);

            //Others
            DefaultListType = (TaskListType) EditorPrefs.GetInt(PREFS_PREFIX + nameof(DefaultListType), (int) DEFAULT_LIST_TYPE);
            ShowCreationDate = EditorPrefs.GetBool(PREFS_PREFIX + nameof(ShowCreationDate), DEFAULT_SHOW_CREATION_DATE);
            ApplyPositionAndRotationOnLoad = EditorPrefs.GetBool(PREFS_PREFIX + nameof(ApplyPositionAndRotationOnLoad), DEFAULT_APPLY_POSITION_AND_ROTATION_ON_LOAD);
            AskBeforeDeletingTasks = EditorPrefs.GetBool(PREFS_PREFIX + nameof(AskBeforeDeletingTasks), DEFAULT_ASK_BEFORE_DELETING_TASKS);
            KeepClosedTasksSeparately = EditorPrefs.GetBool(PREFS_PREFIX + nameof(KeepClosedTasksSeparately), DEFAULT_KEEP_CLOSED_TASKS_SEPARATELY);
            ClosedTaskVisualization = (ClosedTaskVisualizationType) EditorPrefs.GetInt(PREFS_PREFIX + nameof(ClosedTaskVisualization), (int) DEFAULT_CLOSED_TASK_VISUALIZATION);

            arePrefsLoaded = true;
            OnTasksUpdated?.Invoke();

            Tasks.UpdateCustomOrderIndices();
            BacklogTasks.UpdateCustomOrderIndices();
        }

        public static void AddTasksFromTODO(TaskListType _listType, MonoScript[] _scripts)
        {
            if (_scripts.IsNullOrEmpty())
            {
                TaskList.LogWarning($"No scripts provided to add tasks from TODO comments to {_listType} list!");
                return;
            }

            int _addedTasks = 0;

            foreach (var script in _scripts)
            {
                if (script == null)
                {
                    continue;
                }

                var _allLines = script.text.Split('\n');

                for (int i = 0; i < _allLines.Length; i++)
                {
                    string _line = _allLines[i];

                    foreach (var _todo in POSSIBLE_TODOS)
                    {
                        if (_line.ToLower().Contains(_todo))
                        {
                            int _currentTasks = GetNumberOfTasks(_listType);
                            var _task = new Task(_listType, _currentTasks + 1);
                            string _taskName = _trimName(_line.Substring(_line.ToLower().IndexOf(_todo) + _todo.Length).Trim()).FirstLetterToUpperCase();
                            var _context = new TaskContext(script, i + 1);

                            if (_taskName.IsNullEmptyOrWhitespace())
                            {
                                continue;
                            }

                            _task.SetLabel(_taskName);
                            _task.AddContext(_context, false);

                            AddTask(_task, _listType, false);
                            _addedTasks++;
                            break;
                        }
                    }
                }
            }

            if (_addedTasks > 0)
            {
                TaskList.Log($"Added {_addedTasks} tasks from TODO comments to {_listType} list!");
                SavePreferences();
                OnTasksUpdated?.Invoke();
            }
            else
            {
                TaskList.LogWarning($"No TODO comments found in the provided scripts [{_scripts.Length}] to add to {_listType} list!");
            }

            string _trimName(string _name)
            {
                if (_name.IsNullEmptyOrWhitespace())
                {
                    return _name;
                }

                int i = 0;

                while (i < _name.Length && Array.IndexOf(CHARACTERS_TO_TRIM, _name[i]) >= 0)
                {
                    i++;
                }

                return _name.Substring(i);
            }
        }

        public static int GetNumberOfTasks(TaskListType _listType)
        {
            return _listType switch
            {
                TaskListType.Current => Tasks.Items.Count,
                TaskListType.Backlog => BacklogTasks.Items.Count,
                _ => 0,
            };
        }

        public static void ResetToDefaults()
        {
            //Current 
            VisibilityMode = DEFAULT_VISIBILITY_MODE;
            SortMode = DEFAULT_SORT_MODE;
            SortOrder = DEFAULT_SORT_ORDER;

            //Backlog
            BacklogVisibilityMode = DEFAULT_VISIBILITY_MODE;
            BacklogSortMode = DEFAULT_SORT_MODE;
            BacklogSortOrder = DEFAULT_SORT_ORDER;

            //Settings
            ImmediatePriorityColor = DEFAULT_IMMEDIATE_COLOR;
            HighPriorityColor = DEFAULT_HIGH_COLOR;
            NormalPriorityColor = DEFAULT_NORMAL_COLOR;
            LowPriorityColor = DEFAULT_LOW_COLOR;
            ClosedColor = DEFAULT_CLOSED_COLOR;

            //Logs
            PrintLogs = DEFAULT_PRINT_LOGS;

            //Others
            DefaultListType = DEFAULT_LIST_TYPE;
            ShowCreationDate = DEFAULT_SHOW_CREATION_DATE;
            ApplyPositionAndRotationOnLoad = DEFAULT_APPLY_POSITION_AND_ROTATION_ON_LOAD;
            AskBeforeDeletingTasks = DEFAULT_ASK_BEFORE_DELETING_TASKS;
            KeepClosedTasksSeparately = DEFAULT_KEEP_CLOSED_TASKS_SEPARATELY;
            ClosedTaskVisualization = DEFAULT_CLOSED_TASK_VISUALIZATION;

            SavePreferences();
        }

        private static void addTask(Task _task)
        {
            if (Tasks == null)
            {
                Tasks = new TasksContainer();
            }

            Tasks.AddTask(_task);
        }

        private static void addBacklogTask(Task _task)
        {
            if (BacklogTasks == null)
            {
                BacklogTasks = new TasksContainer();
            }

            BacklogTasks.AddTask(_task);
        }

        private static void removeTask(Task _task)
        {
            Tasks.RemoveTask(_task);
        }

        public static void removeBacklogTask(Task _task)
        {
            BacklogTasks.RemoveTask(_task);
        }
    }
}