using System;
using UnityEngine.Assertions;

namespace Saro.BT
{
    public class WaitForCondition : Decorator
    {
        private Func<bool> m_condition;
        private float m_checkInterval;
        private float m_checkVariance;

        public WaitForCondition(Func<bool> condition, float checkInterval, float randomVariance, Node m_decorated) : base("WaitForCondition", m_decorated)
        {
            this.m_condition = condition;

            this.m_checkInterval = checkInterval;
            this.m_checkVariance = randomVariance;

            this.Label = "" + (checkInterval - randomVariance) + "..." + (checkInterval + randomVariance) + "s";
        }

        public WaitForCondition(Func<bool> condition, Node m_decorated) : base("WaitForCondition", m_decorated)
        {
            this.m_condition = condition;
            this.m_checkInterval = 0.0f;
            this.m_checkVariance = 0.0f;
            this.Label = "every tick";
        }

        protected override void InternalStart()
        {
            if (!m_condition.Invoke())
            {
                Clock.AddTimer(m_checkInterval,/* m_checkVariance,*/ -1, checkCondition);
            }
            else
            {
                m_decorated.Start();
            }
        }

        private void checkCondition()
        {
            if (m_condition.Invoke())
            {
                Clock.RemoveTimer(checkCondition);
                m_decorated.Start();
            }
        }

        protected override void InternalCancel()
        {
            Clock.RemoveTimer(checkCondition);
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
            Assert.AreNotEqual(this.CurrentState, State.INACTIVE);
            Stopped(result);
        }

    }
}