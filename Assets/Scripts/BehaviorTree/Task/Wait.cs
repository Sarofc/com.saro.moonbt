
using System;
using System.Text;
using UnityEngine.Assertions;

namespace Saro.BT
{
    public class Wait : Task
    {
        private float m_waitTime;
        private float m_randomDeviation;

        private double m_startTime;

        public Wait(float waitTime, float randomDeviation = 0f) : base("Wait")
        {
            Assert.IsTrue(waitTime >= 0);
            Assert.IsTrue(randomDeviation >= 0);

            m_waitTime = waitTime;
            m_randomDeviation = randomDeviation;
        }

        protected override void InternalStart()
        {
            var currentTime = m_randomDeviation > 0f ?
                UnityEngine.Random.Range(UnityEngine.Mathf.Max(0f, m_waitTime - m_randomDeviation), m_waitTime + m_randomDeviation) :
                m_waitTime;

            if (currentTime <= 0f)
            {
                Stopped(true);
            }
            else
            {
                Clock.AddTimer(currentTime, 0, OnFireAndRemoveTimer);
            }

            m_startTime = Clock.ElapsedTime;
        }


        protected override void InternalAbort()
        {
            Clock.RemoveTimer(OnFireAndRemoveTimer);
            Stopped(false);
        }

        private void OnFireAndRemoveTimer()
        {
            Clock.RemoveTimer(OnFireAndRemoveTimer);
            Stopped(true);
        }


        public override string GetStaticDescription()
        {
            StringBuilder des = new StringBuilder(30);
            if (m_randomDeviation > 0f)
                des.AppendFormat("{0}:Wait {1}+-{2}s", Name, m_waitTime, m_randomDeviation);
            else
                des.AppendFormat("{0}:Wait {1}s", Name, m_waitTime);
            return des.ToString();
        }

        // don't calculate random deviation
        public override string DescribeRuntimeValues(StringBuilder des)
        {
            des.AppendFormat(" {0:N1}",
                CurrentStatus == NodeStatus.Active ?
                Clock.ElapsedTime - m_startTime:
                0
            );
            return des.ToString();
        }

    }
}