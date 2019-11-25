using System;
using UnityEngine.Assertions;

namespace Saro.BT
{
    public class WaitForCondition : Decorator
    {
        private Func<bool> m_condition;
        private float m_checkInterval;
        private float m_randomDeviation;

        public WaitForCondition(Func<bool> condition, float checkInterval, float randomVariance/*, Node m_decorated*/) : base("WaitForCondition"/*, m_decorated*/)
        {
            this.m_condition = condition;

            this.m_checkInterval = checkInterval;
            this.m_randomDeviation = randomVariance;

            this.Label = "" + (checkInterval - randomVariance) + "..." + (checkInterval + randomVariance) + "s";

            m_abortNone = false;
            m_abortSelf = false;
            m_abortLowerPriority = false;
        }

        public WaitForCondition(Func<bool> condition/*, Node m_decorated*/) : base("WaitForCondition"/*, m_decorated*/)
        {
            this.m_condition = condition;
            this.m_checkInterval = 0.1f;
            this.m_randomDeviation = 0.1f;
            //this.Label = "every tick";

            m_abortNone = false;
            m_abortSelf = false;
            m_abortLowerPriority = false;
        }

        protected override void InternalStart()
        {
            if (!m_condition.Invoke())
            {
                Clock.AddTimer(m_checkInterval, -1, checkCondition);
            }
            else
            {
                m_childNode.Start();
            }
        }

        private void checkCondition()
        {
            if (m_condition.Invoke())
            {
                Clock.RemoveTimer(checkCondition);
                m_childNode.Start();
            }
        }

        protected override void InternalAbort()
        {
            Clock.RemoveTimer(checkCondition);
            Stopped(false);

            //if (m_decorated.IsActive)
            //{
            //    m_decorated.Abort();
            //}
            //else
            //{
            //    Stopped(false);
            //}
        }

        protected override void InternalChildStopped(Node child, bool? result)
        {
            Assert.AreNotEqual(this.CurrentStatus, NodeStatus.Inactive);
            Stopped(result);
        }

        public override string GetStaticDescription()
        {
            return Name;
        }
    }
}