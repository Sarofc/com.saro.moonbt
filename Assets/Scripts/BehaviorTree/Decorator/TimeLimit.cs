using System;
using System.Text;
using UnityEngine.Assertions;

namespace Saro.BT
{
    /*
     * 
     * 
     * 
     */
    public class TimeLimit : Decorator
    {
        private float m_timeLimit;
        private float m_randomDeviation;

        private float m_currentTimeLimit;
        private double m_startTime;

        public TimeLimit(float timeLimit, float randomDeviation = 0f) : base("TimeLimit")
        {
            Assert.IsTrue(randomDeviation >= 0f, "randomVariation must greater than or equal to 0");
            Assert.IsTrue(timeLimit > 0f, "timeLimit must greater than 0");

            m_timeLimit = timeLimit;
            m_randomDeviation = randomDeviation;

            m_abortType = ObserverAborts.SELF;
            m_abortNone = false;
            m_abortLowerPriority = false;
        }


        protected override void InternalStart()
        {
            m_startTime = Clock.ElapsedTime;
            m_currentTimeLimit = m_randomDeviation > 0.00f ?
                    UnityEngine.Random.Range(Math.Max(0, m_timeLimit - m_randomDeviation), m_timeLimit + m_randomDeviation) :
                    m_timeLimit;

            base.InternalStart();
        }

        protected override bool IsConditionMet()
        {
            return Clock.ElapsedTime - m_startTime < m_currentTimeLimit;
        }

        protected override void StartObserving()
        {
            Clock.AddUpdateObserver(Evaluate);
        }

        protected override void StopObserving()
        {
            Clock.RemoveUpdateObserver(Evaluate);
        }


        public override string GetStaticDescription()
        {
            StringBuilder des = new StringBuilder(20);

            des.AppendFormat("{0}:Failed after {1:N1}s", Name, m_timeLimit);

            return des.ToString();
        }

        public override string DescribeRuntimeValues(StringBuilder des)
        {
            // TODO
            return des.ToString();
        }

    }
}