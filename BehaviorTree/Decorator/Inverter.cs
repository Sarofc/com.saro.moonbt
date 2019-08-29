using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saro.BT
{
    public class Inverter : Decorator
    {
        public Inverter(Node decoratee) : base("Inverter", decoratee)
        {
        }

        protected override void InternalStart()
        {
            m_decorated.Start();
        }

        override protected void InternalCancel()
        {
            m_decorated.Cancel();
        }

        protected override void InternalChildStopped(Node child, bool result)
        {
            Stopped(!result);
        }
    }
}
