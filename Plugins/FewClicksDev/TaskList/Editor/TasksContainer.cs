namespace FewClicksDev.TaskList
{
    using FewClicksDev.Core;
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public class TasksContainer
    {
        [SerializeField] private List<Task> tasks = new List<Task>();

        public List<Task> Items => tasks;

        public void AddTask(Task _task)
        {
            tasks.Add(_task);
        }

        public void RemoveTask(Task _task)
        {
            if (tasks.Contains(_task))
            {
                tasks.Remove(_task);
            }
        }

        public string ConvertToJson()
        {
            if (tasks.IsNullOrEmpty())
            {
                return string.Empty;
            }

            return JsonUtility.ToJson(this);
        }

        public void UpdateCustomOrderIndices()
        {
            if (tasks == null)
            {
                tasks = new List<Task>();
            }

            foreach (var _task in tasks)
            {
                if (_task.CustomOrder == Task.INVALID_CUSTOM_ORDER)
                {
                    int _highest = Mathf.Clamp(tasks.HighestCustomOrder(), 1, int.MaxValue);
                    _task.SetCustomOrder(_highest + 1);
                }
            }

            tasks.Sort((a, b) => a.CustomOrder.CompareTo(b.CustomOrder));
        }
    }
}
