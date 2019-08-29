

using System.Text;
using UnityEngine.Assertions;

namespace Saro.BT
{
    public class Cooldown : Decorator
    {
        private float m_cooldownTime;
        private float m_randomVariation;

        public Cooldown(float cooldownTime, Node decorator) : base("Cooldown", decorator)
        {
            this.m_cooldownTime = cooldownTime;
            this.m_randomVariation = 0f;
            Assert.IsTrue(cooldownTime > 0f, "cooldownTime has to be set");
        }

        public Cooldown(float cooldownTime, float randomVariation, Node m_decorator) : base("Cooldown", m_decorator)
        {
            this.m_cooldownTime = cooldownTime;
            this.m_randomVariation = randomVariation;
            Assert.IsTrue(cooldownTime > 0f, "limit has to be set");
        }

        protected override void InternalStart()
        {
#if UNITY_EDITOR
            m_startTime = Clock.ElapsedTime;
            m_debugTime = m_cooldownTime;
#endif

            if (m_randomVariation <= 0f)
            {
                Clock.AddTimer(m_cooldownTime, 0, TimeoutReached);
            }
            else
            {
#if UNITY_EDITOR
                var rndValue = UnityEngine.Random.value;
                m_debugTime += (rndValue - 0.5f) * m_randomVariation;
                Clock.AddTimer(m_cooldownTime + rndValue, 0, TimeoutReached);
#else
                Clock.AddTimer(m_cooldownTime + (UnityEngine.Random.value - 0.5f) * m_randomVariation, 0, TimeoutReached);
#endif
            }
        }

        override protected void InternalCancel()
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

        protected override void InternalChildStopped(Node child, bool result)
        {
            Clock.RemoveTimer(TimeoutReached);
            Stopped(result);
        }

        private void TimeoutReached()
        {
            m_decorated.Start();
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