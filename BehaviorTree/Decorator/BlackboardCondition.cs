
using UnityEngine;

namespace Saro.BT
{
    public class BlackboardCondition : ObservingDecorator
    {
        private string m_key;
        private object m_value;
        private Operator m_op;

        public string Key => m_key;
        public object Value => m_value;
        public Operator Operator => m_op;

        public BlackboardCondition(string key, Operator op, object value, ObserverAborts aborts, Node decoratee) : base("BlackboardCondition", aborts, decoratee)
        {
            m_op = op;
            m_key = key;
            m_value = value;
        }

        public BlackboardCondition(string key, Operator op, ObserverAborts aborts, Node decoratee) : base("BlackboardCondition", aborts, decoratee)
        {
            m_op = op;
            m_key = key;
            m_value = default;
        }

        public BlackboardCondition(string key, Operator op, Node decoratee) : base("BlackboardCondition", ObserverAborts.NONE, decoratee)
        {
            m_op = op;
            m_key = key;
            m_value = default;
        }


        override protected void StartObserving()
        {
            RootNode.Blackboard.AddObserver(m_key, OnValueChanged);
        }

        override protected void StopObserving()
        {
            RootNode.Blackboard.RemoveObserver(m_key, OnValueChanged);
        }

        private void OnValueChanged(Blackboard.Type type, object newValue)
        {
            Evaluate();
        }

        override protected bool IsConditionMet()
        {
            if (m_op == Operator.ALWAYS_TRUE)
            {
                return true;
            }

            if (!RootNode.Blackboard.IsSet(m_key))
            {
                return m_op == Operator.IS_NOT_SET;
            }

            object o = RootNode.Blackboard.Get(m_key);

            switch (m_op)
            {
                case Operator.IS_SET: return true;
                case Operator.IS_EQUAL: return object.Equals(o, m_value);
                case Operator.IS_NOT_EQUAL: return !object.Equals(o, m_value);

                case Operator.IS_GREATER_OR_EQUAL:
                    if (o is float)
                    {
                        return (float)o >= (float)m_value;
                    }
                    else if (o is int)
                    {
                        return (int)o >= (int)m_value;
                    }
                    else
                    {
                        Debug.LogError("Type not compareable: " + o.GetType());
                        return false;
                    }

                case Operator.IS_GREATER:
                    if (o is float)
                    {
                        return (float)o > (float)m_value;
                    }
                    else if (o is int)
                    {
                        return (int)o > (int)m_value;
                    }
                    else
                    {
                        Debug.LogError("Type not compareable: " + o.GetType());
                        return false;
                    }

                case Operator.IS_SMALLER_OR_EQUAL:
                    if (o is float)
                    {
                        return (float)o <= (float)m_value;
                    }
                    else if (o is int)
                    {
                        return (int)o <= (int)m_value;
                    }
                    else
                    {
                        Debug.LogError("Type not compareable: " + o.GetType());
                        return false;
                    }

                case Operator.IS_SMALLER:
                    if (o is float)
                    {
                        return (float)o < (float)m_value;
                    }
                    else if (o is int)
                    {
                        return (int)o < (int)m_value;
                    }
                    else
                    {
                        Debug.LogError("Type not compareable: " + o.GetType());
                        return false;
                    }

                default: return false;
            }
        }

        override public string ToString()
        {
            return string.Format("<b>{0}</b>, <b>{1}</b> ({2}) <b>{3}</b>", m_abortType, m_key, m_op, m_value);
        }
    }
}