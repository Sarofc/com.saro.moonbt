
using System;
using System.Text;

namespace Saro.BT
{
    /// <summary>
    /// Coding base
    /// </summary>
    public class Condition : ConditionBase
    {
        private Func<bool> m_condition;

        //public Condition(Func<bool> condition)
        //{
        //    m_condition = condition;
        //}


        public Condition(Func<bool> condition, ObserverAborts aborts = ObserverAborts.NONE, float checkInterval = .1f) : base(aborts, checkInterval)
        {
            m_condition = condition;
        }


        protected override bool TickConditionValue()
        {
            return (bool)m_condition?.Invoke();
        }
    }
}