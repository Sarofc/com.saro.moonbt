using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saro.BT
{
    public abstract class ConditionBase : Decorator
    {
        protected float m_checkInterval;

        public ConditionBase(ObserverAborts aborts = ObserverAborts.NONE,float checkInterval = .1f) : base("Condition")
        {
            m_checkInterval = checkInterval;

            m_abortType = aborts;
        }

        protected override void StartObserving()
        {
            Clock.AddTimer(m_checkInterval, -1, Evaluate);
        }

        protected override void StopObserving()
        {
            Clock.RemoveTimer(Evaluate);
        }

        protected override bool IsConditionMet()
        {
            return TickConditionValue();
        }

        protected abstract bool TickConditionValue();

        // TODO, make info more clearly
        public override string GetStaticDescription()
        {
            StringBuilder des = new StringBuilder(20);
            des.Append(m_abortType);
            des.AppendLine();
            des.AppendFormat("{0}:{1}", Name, "Conditon Descrition");
            return des.ToString();
        }
    }
}
