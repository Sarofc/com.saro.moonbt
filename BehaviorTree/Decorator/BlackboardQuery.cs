using UnityEngine;
using System.Collections;

namespace Saro.BT
{
    public class BlackboardQuery : ObservingDecorator
    {

        private string[] m_keys;
        private System.Func<bool> m_query;

        public BlackboardQuery(string[] keys, ObserverAborts aborts, System.Func<bool> query, Node decoratee) : base("BlackboardQuery", aborts, decoratee)
        {
            m_keys = keys;
            m_query = query;
        }

        override protected void StartObserving()
        {
            foreach (string key in m_keys)
            {
                RootNode.Blackboard.AddObserver(key, OnValueChanged);
            }
        }

        override protected void StopObserving()
        {
            foreach (string key in m_keys)
            {
                RootNode.Blackboard.RemoveObserver(key, OnValueChanged);
            }
        }

        private void OnValueChanged(Blackboard.Type type, object newValue)
        {
            Evaluate();
        }

        protected override bool IsConditionMet()
        {
            return m_query();
        }

        System.Text.StringBuilder keys = new System.Text.StringBuilder(20);
        public override string ToString()
        {
            keys.Clear();
            foreach (string key in m_keys)
            {
                keys.Append(string.Format(" <b>{0}</b>", key));
            }

            return keys.Insert(0, Name).ToString();
        }
    }
}
