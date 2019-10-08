using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saro.BT
{
    // TODO, Fix bug, can't abort completely
    public class Loop : Decorator
    {
        // equal -1 means infinite loop
        private int m_loopTimes;
        private int m_remainTimes;

        public Loop(int loopTime = -1) : base("Loop")
        {
            m_loopTimes = loopTime;

            m_abortNone = false;
            m_abortSelf = false;
            m_abortLowerPriority = false;
        }


        protected override void InternalStart()
        {
            m_remainTimes = m_loopTimes;

            m_childNode.Start();
        }

        protected override void InternalChildStopped(Node child, bool? result)
        {
            if (!result.HasValue) return;

            if (result.Value)
            {
                if (m_loopTimes == -1)
                {
                    // restart in next frame
                    Clock.AddTimer(0, 0, m_childNode.Start);
                }
                else if (--m_remainTimes > 0)
                {
                    // restart in next frame
                    Clock.AddTimer(0, 0, m_childNode.Start);
                }
                else
                {
                    Stopped(true);
                }
            }
            else
            {
                Clock.RemoveTimer(m_childNode.Start);
                Stopped(false);
            }
        }

        public override string GetStaticDescription()
        {
            StringBuilder des = new StringBuilder(20);
            des.AppendFormat("{0}:loop {1} times", Name, m_loopTimes);
            return des.ToString();
        }

        public override string DescribeRuntimeValues(StringBuilder des)
        {
            des.AppendFormat(" : {0}",m_remainTimes);
            return des.ToString();
        }

    }
}
