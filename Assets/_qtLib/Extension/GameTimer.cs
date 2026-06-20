using System;
using System.Collections.Generic;
using qtLib.Helper;
using UnityEngine;

namespace qtLib.Extension
{
    public class GameTimer : qtSingleton<GameTimer>
    {
        #region ----- Variables -----

        private List<Timer> _timer = new ();
        private Timer _temp;
        
        private bool _isPause;

        #endregion

        #region ----- Public Functions -----

        public long RegisterTimer(float time, bool isDown, Action onComplete)
        {
            return RegisterTimer(time, isDown, null, onComplete);
        }

        public long RegisterTimer(float time, bool isDown, Action<float> onUpdate, Action onComplete)
        {
            long timerID = DateTime.Now.Ticks;
            _timer.Add(new Timer()
            {
                ID = timerID,
                time = time,
                isDown = isDown,
                updateCallback = onUpdate,
                completeCallback = onComplete
            });
            return timerID;
        }

        public void PauseTimer(long timerID, bool isPause)
        {
            for (var i = 0; i < _timer.Count; i++)
            {
                if (_timer[i].ID == timerID)
                {
                    _timer[i].isPause = isPause;
                }
            }
        }

        public void PauseAll(bool isPause)
        {
            _isPause = isPause;
        }

        public void UnRegisterTimer(long timerID)
        {
            for (var i = 0; i < _timer.Count; i++)
            {
                if (_timer[i].ID == timerID)
                {
                    _timer.RemoveAt(i);
                    return;
                }
            }
        }
        
        #endregion

        #region ----- Unity Event -----

        private void Update()
        {
            if (_isPause)
            {
                return;
            }
            for (var i = 0; i < _timer.Count; i++)
            {
                _temp = _timer[i];
                if (_temp.isPause)
                {
                    continue;
                }

                if (_temp.isDown)
                {
                    _temp.time -= Time.deltaTime;
                }
                else
                {
                    _temp.time += Time.deltaTime;
                }
                _temp.updateCallback?.Invoke(_temp.time);
                if (_temp.time <= 0)
                {
                    _temp.completeCallback?.Invoke();
                    if (_timer.Remove(_temp))
                    {
                        i--;
                    }
                }
            }
        }

        #endregion

        private class Timer
        {
            public long ID;
            public bool isPause;
            public float time;
            public bool isDown = true;
            public Action<float> updateCallback;
            public Action completeCallback;
        }
    }
}