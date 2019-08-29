using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Assertions;

namespace Saro.BT
{
    public class TimeLimit : Decorator
    {
        private float m_timeLimit;
        private float m_randomVariation;

        public TimeLimit(float timeLimit, Node decorator) : base("TimeLimit", decorator)
        {
            m_timeLimit = timeLimit;
            m_randomVariation = 0f;

            Assert.IsTrue(m_timeLimit > 0f, "timeLimit must greater than 0");
        }

        public TimeLimit(float timeLimit, float randomVariation, Node decorator) : base("TimeLimit", decorator)
        {
            m_timeLimit = timeLimit;
            m_randomVariation = randomVariation;

            Assert.IsTrue(randomVariation >= 0f, "randomVariation must greater than or equal to 0");
            Assert.IsTrue(m_timeLimit > 0f, "timeLimit must greater than 0");
        }

        protected override void InternalStart()
        {
#if UNITY_EDITOR
            m_startTime = Clock.ElapsedTime;
            m_debugTime = m_timeLimit;
#endif

            if (m_randomVariation <= 0f)
            {
                Clock.AddTimer(m_timeLimit, 0, TimeoutReached);
            }
            else
            {
#if UNITY_EDITOR
                var rndValue = UnityEngine.Random.value;
                m_debugTime += (rndValue - 0.5f) * m_randomVariation;
                Clock.AddTimer(m_timeLimit + rndValue, 0, TimeoutReached);
#else
                Clock.AddTimer(m_timeLimit + (UnityEngine.Random.value - 0.5f) * m_randomVariation, 0, TimeoutReached);
#endif
            }
            m_decorated.Start();
        }

        protected override void InternalCancel()
        {
            Clock.RemoveTimer(TimeoutReached);

            if (m_decorated.IsActive)
            {
                m_decorated.Cancel();
            }
            else
            {
                Stopped(false);
            }
        }

        protected override void InternalChildStopped(Node child, bool success)
        {
            Clock.RemoveTimer(TimeoutReached);
            Stopped(success);
        }

        private void TimeoutReached()
        {
            // when time reached, the decorated must be actived
            Assert.IsTrue(m_decorated.IsActive);
            m_decorated.Cancel();
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