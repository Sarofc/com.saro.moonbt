using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saro.BT
{
    public class Random : Decorator
    {
        private float probability;

        public Random(float probability, Node decoratee) : base("Random", decoratee)
        {
            this.probability = probability;
        }

        protected override void InternalStart()
        {
            if (UnityEngine.Random.value <= this.probability)
            {
                m_decorated.Start();
            }
            else
            {
                Stopped(false);
            }
        }

        override protected void InternalCancel()
        {
            m_decorated.Cancel();
        }

        protected override void InternalChildStopped(Node child, bool result)
        {
            Stopped(result);
        }
    }
}