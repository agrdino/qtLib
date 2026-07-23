namespace FewClicksDev.TaskList
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    using static FewClicksDev.Core.EditorDrawer;

    public static class TaskListGenericMenu
    {
        private static readonly GUIContent EDIT_CONTENT = new GUIContent("Edit label", "Edit the selected task label.");
        private static readonly GUIContent DELETE_CONTENT = new GUIContent("Delete", "Delete selected tasks from the list.");
        private static readonly GUIContent MOVE_TO_CURRENT = new GUIContent("Move to Current", "Move tasks to the current task list.");
        private static readonly GUIContent MOVE_TO_BACKLOG = new GUIContent("Move to Backlog", "Move tasks to the backlog task list.");

        private static readonly GUIContent SET_AS_CLOSED = new GUIContent("Set as Closed/True", "Set the selected tasks as closed.");
        private static readonly GUIContent SET_AS_OPEN = new GUIContent("Set as Closed/False", "Set the selected tasks as open.");

        private static readonly GUIContent CHANGE_PRIORITY_IMMEDIATE = new GUIContent("Change Priority/Immediate", "Change the priority of the selected tasks to 'Immediate'.");
        private static readonly GUIContent CHANGE_PRIORITY_HIGH = new GUIContent("Change Priority/High", "Change the priority of the selected tasks to 'High'.");
        private static readonly GUIContent CHANGE_PRIORITY_NORMAL = new GUIContent("Change Priority/Normal", "Change the priority of the selected tasks to 'Normal'.");
        private static readonly GUIContent CHANGE_PRIORITY_LOW = new GUIContent("Change Priority/Low", "Change the priority of the selected tasks to 'Low'.");

        private static readonly GUIContent MOVE_TO_THE_TOP = new GUIContent("Move to the Top", "Move the selected tasks to the top of the list.");
        private static readonly GUIContent MOVE_UP = new GUIContent("Move Up", "Move the selected tasks up in the list.");
        private static readonly GUIContent MOVE_DOWN = new GUIContent("Move Down", "Move the selected tasks down in the list.");
        private static readonly GUIContent MOVE_TO_THE_BOTTOM = new GUIContent("Move to the Bottom", "Move the selected tasks to the bottom of the list.");

        public static void ShowForTasks(TaskListWindow _window, Event _currentEvent, List<Task> _selectedTasks, List<Task> _allTasks, TaskListType _listType)
        {
            GenericMenu _menu = new GenericMenu();

            string _label = _selectedTasks.Count == 1 ? _selectedTasks[0].Label : $"Selected Tasks ({_selectedTasks.Count})";

            _menu.AddDisabledItem(new GUIContent(_label));
            _menu.AddSeparator(string.Empty);

            if (_selectedTasks.Count == 1)
            {
                _menu.AddItem(EDIT_CONTENT, false, _enterEditMode);
            }

            _menu.AddItem(DELETE_CONTENT, false, _delete);

            switch (_listType)
            {
                case TaskListType.Current:
                    _menu.AddItem(MOVE_TO_BACKLOG, false, _moveToBacklog);
                    break;

                case TaskListType.Backlog:
                    _menu.AddItem(MOVE_TO_CURRENT, false, _moveToCurrent);
                    break;
            }

            _menu.AddSeparator(string.Empty);
            _menu.AddItem(SET_AS_CLOSED, false, () => _setAsClosed(true));
            _menu.AddItem(SET_AS_OPEN, false, () => _setAsClosed(false));
            _menu.AddSeparator(string.Empty);
            _menu.AddItem(CHANGE_PRIORITY_IMMEDIATE, false, () => _changePriority(TaskPriority.Immediate));
            _menu.AddItem(CHANGE_PRIORITY_HIGH, false, () => _changePriority(TaskPriority.High));
            _menu.AddItem(CHANGE_PRIORITY_NORMAL, false, () => _changePriority(TaskPriority.Normal));
            _menu.AddItem(CHANGE_PRIORITY_LOW, false, () => _changePriority(TaskPriority.Low));

            bool _addCustomOrderItems = false;

            switch (_window.CurrentWindowMode)
            {
                case TaskListWindow.WindowMode.CurrentTasks:
                    _addCustomOrderItems = TaskListUserPreferences.SortMode is TasksSortMode.Custom;
                    break;

                case TaskListWindow.WindowMode.Backlog:
                    _addCustomOrderItems = TaskListUserPreferences.BacklogSortMode is TasksSortMode.Custom;
                    break;
            }

            if (_addCustomOrderItems)
            {
                _menu.AddSeparator(string.Empty);
                _menu.AddItem(MOVE_TO_THE_TOP, false, _moveToTheTop);
                _menu.AddItem(MOVE_UP, false, _moveUp);
                _menu.AddItem(MOVE_DOWN, false, _moveDown);
                _menu.AddItem(MOVE_TO_THE_BOTTOM, false, _moveToTheBottom);
            }

            _menu.ShowAsContext();
            _currentEvent.Use();

            void _enterEditMode()
            {
                foreach (Task _task in _selectedTasks)
                {
                    if (_task == null)
                    {
                        continue;
                    }

                    _task.SetEditMode(true);
                }
            }

            void _delete()
            {
                if (TaskListUserPreferences.AskBeforeDeletingTasks)
                {
                    bool _delete = EditorUtility.DisplayDialog($"Delete Tasks ({_selectedTasks.Count})", $"Are you sure you want to delete the selected {_selectedTasks.Count} task(s)? This action cannot be undone.", YES, NO);

                    if (_delete == false)
                    {
                        return;
                    }
                }

                foreach (Task _task in _selectedTasks)
                {
                    if (_task == null)
                    {
                        continue;
                    }

                    TaskListUserPreferences.RemoveTask(_task, _listType, false);
                }

                _window.RefreshTaskLists();
                TaskListUserPreferences.SavePreferences();
            }

            void _moveToCurrent()
            {
                foreach (Task _task in _selectedTasks)
                {
                    if (_task == null)
                    {
                        continue;
                    }

                    TaskListUserPreferences.MoveBacklogTaskToCurrent(_task);
                }

                _window.RefreshTaskLists();
                TaskListUserPreferences.SavePreferences();
            }

            void _moveToBacklog()
            {
                foreach (Task _task in _selectedTasks)
                {
                    if (_task == null)
                    {
                        continue;
                    }

                    TaskListUserPreferences.MoveTaskToBacklog(_task);
                }

                _window.RefreshTaskLists();
                TaskListUserPreferences.SavePreferences();
            }

            void _setAsClosed(bool _closed)
            {
                foreach (Task _task in _selectedTasks)
                {
                    if (_task == null)
                    {
                        continue;
                    }

                    _task.SetAsClosed(_closed);
                }

                _window.RefreshTaskLists();
                TaskListUserPreferences.SavePreferences();
            }

            void _changePriority(TaskPriority _newPriority)
            {
                foreach (Task _task in _selectedTasks)
                {
                    if (_task == null)
                    {
                        continue;
                    }

                    _task.SetPriority(_newPriority);
                }

                _window.RefreshTaskLists();
                TaskListUserPreferences.SavePreferences();
            }

            void _moveToTheTop()
            {
                int _minCustomOrder = _allTasks.LowestCustomOrder() - 1;

                foreach (Task _task in _selectedTasks)
                {
                    if (_task == null)
                    {
                        continue;
                    }

                    _task.SetCustomOrder(_minCustomOrder);
                }

                _window.RefreshTaskLists();
                TaskListUserPreferences.SavePreferences();
            }

            void _moveUp()
            {
                int _minCustomOrderFromSelected = _selectedTasks.GetNextLowestOrder(_allTasks) - 1;

                foreach (Task _task in _selectedTasks)
                {
                    if (_task == null)
                    {
                        continue;
                    }

                    _task.SetCustomOrder(_minCustomOrderFromSelected);
                }

                _window.RefreshTaskLists();
                TaskListUserPreferences.SavePreferences();
            }

            void _moveDown()
            {
                int _maxCustomOrderFromSelected = _selectedTasks.GetNextHighestOrder(_allTasks) + 1;

                foreach (Task _task in _selectedTasks)
                {
                    if (_task == null)
                    {
                        continue;
                    }

                    _task.SetCustomOrder(_maxCustomOrderFromSelected);
                }

                _window.RefreshTaskLists();
                TaskListUserPreferences.SavePreferences();
            }

            void _moveToTheBottom()
            {
                int _maxCustomOrder = _allTasks.HighestCustomOrder() + 1;

                foreach (Task _task in _selectedTasks)
                {
                    if (_task == null)
                    {
                        continue;
                    }

                    _task.SetCustomOrder(_maxCustomOrder);
                }

                _window.RefreshTaskLists();
                TaskListUserPreferences.SavePreferences();
            }
        }
    }
}
