
using System;
using System.Text;
using UnityEngine.Assertions;

namespace Saro.BT
{
    public class Wait : Task
    {
        private Func<float> m_func = null;
        private string m_blackboardKey = null;
        private float m_seconds = -1f;

        public Wait(float seconds) : base("Wait")
        {
            Assert.IsTrue(seconds >= 0);

            m_seconds = seconds;
        }


        public Wait(string blackboardKey) : base("Wait")
        {
            m_blackboardKey = blackboardKey;
        }

        public Wait(Func<float> func) : base("Wait")
        {
            m_func = func;
        }

        protected override void InternalStart()
        {
            float seconds = m_seconds;
            if (m_seconds < 0)
            {
                if (m_blackboardKey != null)
                {
                    seconds = Blackboard.Get<float>(m_blackboardKey);
                }
                else if (m_func != null)
                {
                    seconds = m_func();
                }
            }

            if (seconds <= 0)
            {
                ReachedTime();
            }
            else
            {
                Clock.AddTimer(seconds, 0, OnFireAndRemoveTimer);
            }

#if UNITY_EDITOR
            m_startTime = Clock.ElapsedTime;
            m_debugTime = seconds;
#endif
        }

        protected override void InternalCancel()
        {
            Clock.RemoveTimer(OnFireAndRemoveTimer);
            Stopped(true);
        }

        private void OnFireAndRemoveTimer()
        {
            Clock.RemoveTimer(OnFireAndRemoveTimer);
            ReachedTime();
        }

        private void ReachedTime()
        {
            Stopped(true);
        }

#if UNITY_EDITOR

        private double m_startTime;
        private double m_debugTime;
        private StringBuilder m_sb = new StringBuilder(30);

        public override string ToString()
        {
            m_sb.Clear();
            m_sb.Append(Name);
            m_sb.AppendFormat(":{0:N2}", m_currentState == State.INACTIVE ? 0 : Clock.ElapsedTime - m_startTime);
            m_sb.AppendFormat("/{0:N2}", m_debugTime);
            return m_sb.ToString();
        }
#endif
    }
}