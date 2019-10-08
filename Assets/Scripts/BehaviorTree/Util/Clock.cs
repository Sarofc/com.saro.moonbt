using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Saro.BT
{
    public class Clock
    {
        public double ElapsedTime => m_elapsedTime;
        private double m_elapsedTime = 0f;

        private List<Action> m_updateObservers = new List<Action>();
        private Dictionary<Action, Timer> m_timers = new Dictionary<Action, Timer>();
        private HashSet<Action> m_removeObservers = new HashSet<Action>();
        private HashSet<Action> m_addObservers = new HashSet<Action>();
        private HashSet<Action> m_removeTimers = new HashSet<Action>();
        private Dictionary<Action, Timer> m_addTimers = new Dictionary<Action, Timer>();
        private bool m_isInUpdate = false;

        private List<Timer> m_timerPool = new List<Timer>();
        private int m_currentTimerPoolIndex = 0;

        private class Timer
        {
            public double scheduledTime = 0f;
            public int repeat = 0;
            public bool used = false;
            public double delay = 0f;

            public void ScheduleAbsoluteTime(double elapsedTime)
            {
                scheduledTime = elapsedTime + delay;
            }
        }

        // repeat = 0, execute once
        // repeat > 0, repeat times + 1
        // repeat < 0, infinite
        public void AddTimer(float time,  int repeat, Action action)
        {
            Timer timer = null;

            if (!m_isInUpdate)
            {
                if (!m_timers.ContainsKey(action))
                {
                    m_timers[action] = TryGetTimer();
                }
                timer = m_timers[action];
            }
            else
            {
                if (!m_addTimers.ContainsKey(action))
                {
                    m_addTimers[action] = TryGetTimer();
                }
                timer = m_addTimers[action];

                if (m_removeTimers.Contains(action))
                {
                    m_removeTimers.Remove(action);
                }
            }

            Assert.IsTrue(timer.used);
            timer.delay = time;
            timer.repeat = repeat;
            timer.ScheduleAbsoluteTime(m_elapsedTime);
        }

        public void RemoveTimer(Action action)
        {
            if (!m_isInUpdate)
            {
                if (m_timers.ContainsKey(action))
                {
                    m_timers[action].used = false;
                    m_timers.Remove(action);
                }
            }
            else
            {
                if (m_timers.ContainsKey(action))
                {
                    m_removeTimers.Add(action);
                }
                if (m_addTimers.ContainsKey(action))
                {
                    Assert.IsTrue(m_addTimers[action].used);
                    m_addTimers[action].used = false;
                    m_addTimers.Remove(action);
                }
            }
        }

        public bool HasTimer(System.Action action)
        {
            if (!m_isInUpdate)
            {
                return m_timers.ContainsKey(action);
            }
            else
            {
                if (m_removeTimers.Contains(action))
                {
                    return false;
                }
                else if (m_addTimers.ContainsKey(action))
                {
                    return true;
                }
                else
                {
                    return m_timers.ContainsKey(action);
                }
            }
        }


        public void AddUpdateObserver(Action action)
        {
            if (!m_isInUpdate)
            {
                m_updateObservers.Add(action);
            }
            else
            {
                if (!m_updateObservers.Contains(action))
                {
                    m_addObservers.Add(action);
                }
                if (m_removeObservers.Contains(action))
                {
                    m_removeObservers.Remove(action);
                }
            }
        }

        public void RemoveUpdateObserver(Action action)
        {
            if (!m_isInUpdate)
            {
                m_updateObservers.Remove(action);
            }
            else
            {
                if (m_updateObservers.Contains(action))
                {
                    m_removeObservers.Add(action);
                }
                if (m_addObservers.Contains(action))
                {
                    m_addObservers.Remove(action);
                }
            }
        }

        public bool HasUpdateObserver(Action action)
        {
            if (!m_isInUpdate)
            {
                return m_updateObservers.Contains(action);
            }
            else
            {
                if (m_removeObservers.Contains(action))
                {
                    return false;
                }
                else if (m_addObservers.Contains(action))
                {
                    return true;
                }
                else
                {
                    return m_updateObservers.Contains(action);
                }
            }
        }

        public void Tick(float deltaTime)
        {
            m_elapsedTime += deltaTime;

            m_isInUpdate = true;

            foreach (Action action in m_updateObservers)
            {
                if (!m_removeObservers.Contains(action))
                {
                    action.Invoke();
                }
            }

            Dictionary<Action, Timer>.KeyCollection keys = m_timers.Keys;
            foreach (Action callback in keys)
            {
                if (m_removeTimers.Contains(callback))
                {
                    continue;
                }

                Timer timer = m_timers[callback];
                if (timer.scheduledTime <= m_elapsedTime)
                {
                    // remove
                    if (timer.repeat == 0)
                    {
                        RemoveTimer(callback);
                    }
                    // reppeat--
                    else if (timer.repeat > 0)
                    {
                        timer.repeat--;
                    }
                    // infinite loop, repeat < 0

                    callback.Invoke();
                    timer.ScheduleAbsoluteTime(m_elapsedTime);
                }
            }

            foreach (Action action in m_addObservers)
            {
                m_updateObservers.Add(action);
            }
            foreach (Action action in m_removeObservers)
            {
                m_updateObservers.Remove(action);
            }
            foreach (Action action in m_addTimers.Keys)
            {
                if (m_timers.ContainsKey(action))
                {
                    Assert.AreNotEqual(m_timers[action], m_addTimers[action]);
                    m_timers[action].used = false;
                }
                Assert.IsTrue(m_addTimers[action].used);
                m_timers[action] = m_addTimers[action];
            }
            foreach (Action action in m_removeTimers)
            {
                Assert.IsTrue(m_timers[action].used);
                m_timers[action].used = false;
                m_timers.Remove(action);
            }
            m_addObservers.Clear();
            m_removeObservers.Clear();
            m_addTimers.Clear();
            m_removeTimers.Clear();

            m_isInUpdate = false;
        }

        private Timer TryGetTimer()
        {
            int i = 0;
            int l = m_timerPool.Count;
            Timer timer = null;
            while (i < l)
            {
                int timerIndex = (i + m_currentTimerPoolIndex) % l;
                if (!m_timerPool[timerIndex].used)
                {
                    m_currentTimerPoolIndex = timerIndex;
                    timer = m_timerPool[timerIndex];
                    break;
                }
                i++;
            }

            if (timer == null)
            {
                timer = new Timer();
                m_currentTimerPoolIndex = 0;
                m_timerPool.Add(timer);
            }

            timer.used = true;
            return timer;
        }


#if UNITY_EDITOR
        public int DebugPoolSize
        {
            get
            {
                return m_timerPool.Count;
            }
        }

        public int NumUpdateObservers
        {
            get
            {
                return m_updateObservers.Count;
            }
        }

        public int NumTimers
        {
            get
            {
                return m_timers.Count;
            }
        }
#endif
    }
}