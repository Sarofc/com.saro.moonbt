
using System;

namespace Saro.BT
{
    public class Condition : ObservingDecorator
    {
        private Func<bool> m_condition;
        private float m_checkInterval;

        public Condition(Func<bool> condition, Node decorated) : base("Condition", ObserverAborts.NONE, decorated)
        {
            m_condition = condition;
            m_checkInterval = 0.0f;
        }

        public Condition(Func<bool> condition, ObserverAborts aborts, Node decorated) : base("Condition", aborts, decorated)
        {
            m_condition = condition;
            m_checkInterval = 0.0f;
        }

        public Condition(Func<bool> condition, ObserverAborts aborts, float checkInterval, Node decorated) : base("Condition", aborts, decorated)
        {
            m_condition = condition;
            m_checkInterval = checkInterval;
        }

        protected override void StartObserving()
        {
            RootNode.Clock.AddTimer(m_checkInterval, -1, Evaluate);
        }

        protected override void StopObserving()
        {
            RootNode.Clock.RemoveTimer(Evaluate);
        }

        protected override bool IsConditionMet()
        {
            return m_condition();
        }
    }
}