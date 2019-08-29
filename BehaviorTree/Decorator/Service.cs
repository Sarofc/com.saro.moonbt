
using System;

namespace Saro.BT
{
    public class Service : Decorator
    {
        private Action m_serviceMethod;

        private float m_interval = -1.0f;

        public Service(float interval, Action service, Node decoratee) : base("Service", decoratee)
        {
            m_serviceMethod = service;
            m_interval = interval;

            Label = interval + "s";
        }

        public Service(Action service, Node decoratee) : base("Service", decoratee)
        {
            m_serviceMethod = service;
            Label = "every tick";
        }

        protected override void InternalStart()
        {
            if (m_interval <= 0f)
            {
                Clock.AddUpdateObserver(m_serviceMethod);
                m_serviceMethod();
            }
            else
            {
                Clock.AddTimer(m_interval, -1, m_serviceMethod);
                m_serviceMethod();
            }
            m_decorated.Start();
        }

        override protected void InternalCancel()
        {
            m_decorated.Cancel();
        }

        protected override void InternalChildStopped(Node child, bool result)
        {
            if (m_interval <= 0f)
            {
                Clock.RemoveUpdateObserver(m_serviceMethod);
            }
            else
            {
                Clock.RemoveTimer(m_serviceMethod);
            }
            Stopped(result);
        }


        public override string ToString()
        {
            return string.Format("({0})", Name);
        }
    }
}