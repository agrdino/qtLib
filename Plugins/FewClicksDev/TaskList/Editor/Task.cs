namespace FewClicksDev.TaskList
{
    using FewClicksDev.Core.SelectableList;
    using System.Collections.Generic;
    using System.Globalization;
    using UnityEngine;

    using static FewClicksDev.Core.EditorDrawer;

    [System.Serializable]
    public class Task : SelectableItem, System.IEquatable<Task>
    {
        public const int INVALID_CUSTOM_ORDER = -1;

        [SerializeField] private TaskListType listType = TaskListType.Current;
        [SerializeField] private string label = string.Empty;
        [SerializeField] private string description = string.Empty;
        [SerializeField] private TaskPriority priority = TaskPriority.Normal;
        [SerializeField] private bool isClosed = false;
        [SerializeField] private long creationTimeInTicks = 0;
        [SerializeField] private long closedTimeInTicks = 0;
        [SerializeField] private int customOrder = INVALID_CUSTOM_ORDER;
        [SerializeField] private List<TaskContext> contexts = new List<TaskContext>();

        public TaskListType ListType => listType;
        public string Label => label;
        public string Description => description;
        public TaskPriority Priority => priority;
        public bool IsClosed => isClosed;
        public long CreationTimeInTicks => creationTimeInTicks;
        public long ClosedTimeInTicks => closedTimeInTicks;
        public int CustomOrder => customOrder;
        public int NumberOfContexts => contexts != null ? contexts.Count : 0;

        public bool IsInEditMode { get; private set; } = false;

        public Color IndicatorColor => getIndicatorColor();
        public string DateTimeString => new System.DateTime(creationTimeInTicks).ToString("dd MMMM HH:mm", new CultureInfo("en-US"));

        public Task(TaskListType _listType, int _index)
        {
            label = _listType is TaskListType.Current ? $"New task {_index + 1}" : $"New backlog task {_index + 1}";
            listType = _listType;
            creationTimeInTicks = System.DateTime.Now.Ticks;
        }

        public TaskContext GetContextAtIndex(int _index)
        {
            if (_index < 0 || _index >= contexts.Count)
            {
                return null;
            }

            return contexts[_index];
        }

        public void AddContext(TaskContext _context, bool _save = true)
        {
            if (_context == null)
            {
                return;
            }

            contexts.Add(_context);

            if (_save)
            {
                TaskListUserPreferences.SavePreferences(false);
            }
        }

        public void RemoveContextAtIndex(int _index)
        {
            if (_index < 0 || _index >= contexts.Count)
            {
                return;
            }

            contexts.RemoveAt(_index);
            TaskListUserPreferences.SavePreferences(false);
        }

        public bool ShouldBeVisible(TasksVisibilityMode _visibility)
        {
            if (_visibility.HasFlag(TasksVisibilityMode.Open) && isClosed == false)
            {
                return true;
            }

            if (_visibility.HasFlag(TasksVisibilityMode.Closed) == false && isClosed)
            {
                return false;
            }

            if (_visibility.HasFlag(TasksVisibilityMode.Immediate) && priority is TaskPriority.Immediate)
            {
                return true;
            }

            if (_visibility.HasFlag(TasksVisibilityMode.High) && priority is TaskPriority.High)
            {
                return true;
            }

            if (_visibility.HasFlag(TasksVisibilityMode.Normal) && priority is TaskPriority.Normal)
            {
                return true;
            }

            if (_visibility.HasFlag(TasksVisibilityMode.Low) && priority is TaskPriority.Low)
            {
                return true;
            }

            return false;
        }

        public void SetLabel(string _label)
        {
            label = _label;
            TaskListUserPreferences.SavePreferences(false);
        }

        public void SetDescription(string _description)
        {
            description = _description;
            TaskListUserPreferences.SavePreferences(false);
        }

        public void SetPriority(TaskPriority _priority)
        {
            priority = _priority;
            TaskListUserPreferences.SavePreferences(false);
        }

        public void SetListType(TaskListType _listType)
        {
            listType = _listType;
            TaskListUserPreferences.SavePreferences(false);
        }

        public void SetAsClosed(bool _closed)
        {
            isClosed = _closed;
            closedTimeInTicks = isClosed ? System.DateTime.Now.Ticks : 0;
            TaskListUserPreferences.SavePreferences(false);
        }

        public void SetCustomOrder(int _order)
        {
            customOrder = _order;
        }

        public void SetEditMode(bool _isInEditMode)
        {
            IsInEditMode = _isInEditMode;
        }

        public override void RecalculateHeightAndStartPosition(float _currentY)
        {
            if (isExpanded == false)
            {
                startPositionAndHeight = new Vector2(_currentY, DEFAULT_LINE_HEIGHT);
                return;
            }

            float _height = DEFAULT_LINE_HEIGHT;
            _height += SMALL_SPACE;
            _height += SingleLineHeightWithSpacing; //Label
            _height += SingleLineHeightWithSpacing; //CustomOrder
            _height += SingleLineHeight * 3f; //Description
            _height += StandardVerticalSpacing;

            if (NumberOfContexts == 0)
            {
                _height += _getContextButtonHeight();
            }
            else
            {
                _height += SMALL_SPACE;
                _height += SingleLineHeightWithSpacing; //Context label
                _height += VERY_SMALL_SPACE;

                foreach (var _context in contexts)
                {
                    _height += _context.GetHeight();
                }

                _height += _getContextButtonHeight();
            }

            startPositionAndHeight = new Vector2(_currentY, _height);

            float _getContextButtonHeight()
            {
                float _totalHeight = SMALL_SPACE;
                _totalHeight += DEFAULT_LINE_HEIGHT; //Add context button
                _totalHeight += SMALL_SPACE;

                return _totalHeight;
            }
        }

        public bool Equals(Task _other)
        {
            if (_other == null)
            {
                return false;
            }

            return creationTimeInTicks == _other.creationTimeInTicks;
        }

        private Color getIndicatorColor()
        {
            if (isClosed)
            {
                return TaskListUserPreferences.ClosedColor;
            }

            return TaskListUserPreferences.GetColorFromPriority(Priority);
        }
    }
}
