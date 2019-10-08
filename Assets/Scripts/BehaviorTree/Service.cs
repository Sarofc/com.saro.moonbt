using System;
using System.Text;
using UnityEngine.Assertions;

namespace Saro.BT
{
    /*
     * 
     * 
     */
    public abstract class Service : AuxiliaryNode
    {
        private float m_interval;
        private float m_randomDeviation;

        public Service(float interval, float randomDeviation) : base("Service")
        {
            Assert.IsTrue(interval > 0.001f, "interval must greater than or equal to 0.001f");
            Assert.IsTrue(interval >= 0.00f, "randomDeviation must greater than or equal to 0.00f");

            m_interval = interval;
            m_randomDeviation = randomDeviation;
        }

        public Service() : base("Service")
        {
            m_interval = 0.5f;
            m_randomDeviation = 0.1f;
        }

        protected override void InternalStart()
        {
            if (m_interval <= 0f)
            {
                Clock.AddUpdateObserver(TickService);
                TickService();
            }
            else
            {
                Clock.AddTimer(
                    m_randomDeviation > 0.00f ?
                    UnityEngine.Random.Range(Math.Max(0, m_interval - m_randomDeviation), m_interval + m_randomDeviation) :
                    m_interval,
                    -1,
                    TickService
                );

                TickService();
            }
            m_childNode.Start();
        }

        protected override void InternalAbort()
        {
            m_childNode.Abort();
        }

        protected override void InternalChildStopped(Node child, bool? result)
        {
            if (m_interval <= 0f)
            {
                Clock.RemoveUpdateObserver(TickService);
            }
            else
            {
                Clock.RemoveTimer(TickService);
            }
            Stopped(result);
        }

        protected abstract void TickService();

        public override string GetStaticDescription()
        {
            StringBuilder des = new StringBuilder(30);
            des.Append(Name);
            des.Append(
                m_randomDeviation > 0.00f ?
                string.Format(":every tick {0:N2}s..{1:N2}s", Math.Max(0, m_interval - m_randomDeviation), m_interval + m_randomDeviation) :
                string.Format(":every tick {0:N2}s", m_interval)
            );

            return des.ToString();
        }
    }
}
