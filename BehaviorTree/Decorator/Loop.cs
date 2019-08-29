using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saro.BT
{
    public class Loop : Decorator
    {
        // equal -1 means infinite loop
        private int m_loopTimes;
        private int m_remainTimes;

        public Loop(int loopTime, Node decorated) : base("Loop", decorated)
        {
            m_loopTimes = loopTime;
        }

        /// <summary>
        /// Infinite Loop
        /// </summary>
        /// <param name="decorated"></param>
        public Loop(Node decorated) : base("Loop", decorated)
        {
            m_loopTimes = -1;
        }

        protected override void InternalStart()
        {
            m_remainTimes = m_loopTimes;

            m_decorated.Start();
        }

        protected override void InternalCancel()
        {
            //Clock.RemoveTimer(RestartDecorated);

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
            if (success)
            {
                if (m_loopTimes == -1)
                {
                    m_decorated.Start();
                }
                else
                {
                    if (--m_remainTimes > 0)
                    {
                        m_decorated.Start();
                        //Clock.AddTimer(0, 0, RestartDecorated);
                    }
                    else
                    {
                        Stopped(true);
                    }
                }
            }
            else
            {
                Stopped(false);
            }
        }

        private void RestartDecorated()
        {
            m_decorated.Start();
        }
    }
}
