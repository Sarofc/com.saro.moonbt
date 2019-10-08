

using System.Text;
using UnityEngine.Assertions;

namespace Saro.BT
{
    /*
     * 
     * 
     * 
     */
    public class Cooldown : Decorator
    {
        private double m_cooldownTime;
        private double m_startTime;


        public Cooldown(float cooldownTime, ObserverAborts aborts = ObserverAborts.NONE) : base("Cooldown")
        {
            Assert.IsTrue(cooldownTime > 0f, "cooldownTime has to be set");

            m_cooldownTime = cooldownTime;
            m_startTime = -cooldownTime;

            m_abortType = aborts;
            m_abortSelf = false;
        }

        protected override void InternalStart()
        {
            if (!IsConditionMet())
            {
                Stopped(false);
            }
            else
            {
                m_childNode.Start();
            }
        }

        protected override void InternalChildStopped(Node child, bool? result)
        {
            // start observer after execution
            // cooldown can't abort SELF, so don't need to stop observer

            m_startTime = Clock.ElapsedTime;
            if (m_abortType != ObserverAborts.NONE)
            {
                if (!m_isObserving)
                {
                    m_isObserving = true;
                    StartObserving();
                }
            }
            Stopped(result);
        }

        protected override void StartObserving()
        {
            Clock.AddUpdateObserver(Evaluate);
        }

        protected override void StopObserving()
        {
            Clock.RemoveUpdateObserver(Evaluate);
        }

        protected override bool IsConditionMet()
        {
            return Clock.ElapsedTime - m_startTime >= m_cooldownTime;
        }

        public override string GetStaticDescription()
        {
            StringBuilder des = new StringBuilder(30);

            des.AppendFormat("({0})\n", m_abortType);
            des.AppendFormat("{0}:lock {1:N1}s after execution", Name, m_cooldownTime);

            return des.ToString();
        }

        public override string DescribeRuntimeValues(StringBuilder des)
        {
            var countTime = (Clock.ElapsedTime - m_startTime);
            des.AppendFormat(":{0:N1}", countTime > 0f ? countTime >= m_cooldownTime ? m_cooldownTime : countTime : 0);

            return des.ToString();
        }

    }
}