
using System;
using System.Text;
using UnityEngine;

namespace Saro.BT
{
    public class BlackboardCondition<T> : Decorator
    {
        private string m_key;
        private T m_value;
        private Operator m_op;

        public string Key => m_key;
        public T Value => m_value;
        public Operator Operator => m_op;

        public BlackboardCondition(string key, Operator op, T value = default, ObserverAborts aborts = ObserverAborts.NONE) : base("BlackboardCondition")
        {
            m_op = op;
            m_key = key;
            m_value = value;

            m_abortType = aborts;
        }


        protected override void StartObserving()
        {
            Blackboard.AddObserver(m_key, OnValueChanged);
        }

        protected override void StopObserving()
        {
            Blackboard.RemoveObserver(m_key, OnValueChanged);
        }

        private void OnValueChanged(Blackboard.Type type, object newValue)
        {
            Evaluate();
        }

        protected override bool IsConditionMet()
        {
            if (m_op == Operator.ALWAYS_TRUE)
            {
                return true;
            }

            if (!RootNode.Blackboard.IsSet(m_key))
            {
                return m_op == Operator.IS_NOT_SET;
            }

            var o = RootNode.Blackboard.Get<T>(m_key).Value;

            switch (m_op)
            {
                case Operator.IS_SET:
                    return true;
                case Operator.IS_EQUAL:
                    return object.Equals(o, m_value);
                case Operator.IS_NOT_EQUAL:
                    return !object.Equals(o, m_value);
                case Operator.IS_GREATER_OR_EQUAL:
                    if (o is IComparable)
                    {
                        return ((IComparable)o).CompareTo((IComparable)m_value) >= 0;
                    }
                    else
                    {
                        Debug.LogError($"can't compare t1:{o} t2:{m_value}");
                        return false;
                    }
                case Operator.IS_GREATER:
                    if (o is IComparable)
                    {
                        return ((IComparable)o).CompareTo((IComparable)m_value) > 0;
                    }
                    else
                    {
                        Debug.LogError($"can't compare t1:{o} t2:{m_value}");
                        return false;
                    }
                case Operator.IS_SMALLER_OR_EQUAL:
                    if (o is IComparable)
                    {
                        return ((IComparable)o).CompareTo((IComparable)m_value) <= 0;
                    }
                    else
                    {
                        Debug.LogError($"can't compare t1:{o} t2:{m_value}");
                        return false;
                    }
                case Operator.IS_SMALLER:
                    if (o is IComparable)
                    {
                        return ((IComparable)o).CompareTo((IComparable)m_value) < 0;
                    }
                    else
                    {
                        Debug.LogError($"can't compare t1:{o} t2:{m_value}");
                        return false;
                    }
                default:
                    return false;
            }
        }

        public override string GetStaticDescription()
        {
            StringBuilder des = new StringBuilder(30);
            des.Append(m_abortType);
            des.AppendFormat("\n{0}: {1} {2} {3}", Name, m_key, m_op, m_value);

            return des.ToString();
        }
    }


    #region TODO
    //public class BlackboardCondition : Decorator
    //{
    //    private string m_key;
    //    private object m_value;
    //    private Operator m_op;

    //    public string Key => m_key;
    //    public object Value => m_value;
    //    public Operator Operator => m_op;

    //    public BlackboardCondition(string key, Operator op, object value = default, ObserverAborts aborts = ObserverAborts.NONE) : base("BlackboardCondition")
    //    {
    //        m_op = op;
    //        m_key = key;
    //        m_value = value;

    //        m_abortType = aborts;
    //    }


    //    protected override void StartObserving()
    //    {
    //        RootNode.Blackboard.AddObserver(m_key, OnValueChanged);
    //    }

    //    protected override void StopObserving()
    //    {
    //        RootNode.Blackboard.RemoveObserver(m_key, OnValueChanged);
    //    }

    //    private void OnValueChanged(Blackboard.Type type, object newValue)
    //    {
    //        Evaluate();
    //    }

    //    protected override bool IsConditionMet()
    //    {
    //        if (m_op == Operator.ALWAYS_TRUE)
    //        {
    //            return true;
    //        }

    //        if (!RootNode.Blackboard.IsSet(m_key))
    //        {
    //            return m_op == Operator.IS_NOT_SET;
    //        }

    //        var o = RootNode.Blackboard.Get(m_key).Value;

    //        switch (m_op)
    //        {
    //            case Operator.IS_SET:
    //                return true;
    //            case Operator.IS_EQUAL:
    //                return object.Equals(o, m_value);
    //            case Operator.IS_NOT_EQUAL:
    //                return !object.Equals(o, m_value);
    //            case Operator.IS_GREATER_OR_EQUAL:
    //                if (o is IComparable)
    //                {
    //                    return ((IComparable)o).CompareTo((IComparable)m_value) >= 0;
    //                }
    //                else
    //                {
    //                    Debug.LogError($"can't compare t1:{o} t2:{m_value}");
    //                    return false;
    //                }
    //            case Operator.IS_GREATER:
    //                if (o is IComparable)
    //                {
    //                    return ((IComparable)o).CompareTo((IComparable)m_value) > 0;
    //                }
    //                else
    //                {
    //                    Debug.LogError($"can't compare t1:{o} t2:{m_value}");
    //                    return false;
    //                }
    //            case Operator.IS_SMALLER_OR_EQUAL:
    //                if (o is IComparable)
    //                {
    //                    return ((IComparable)o).CompareTo((IComparable)m_value) <= 0;
    //                }
    //                else
    //                {
    //                    Debug.LogError($"can't compare t1:{o} t2:{m_value}");
    //                    return false;
    //                }
    //            case Operator.IS_SMALLER:
    //                if (o is IComparable)
    //                {
    //                    return ((IComparable)o).CompareTo((IComparable)m_value) < 0;
    //                }
    //                else
    //                {
    //                    Debug.LogError($"can't compare t1:{o} t2:{m_value}");
    //                    return false;
    //                }
    //            default:
    //                return false;
    //        }
    //    }

    //    public override string GetStaticDescription()
    //    {
    //        StringBuilder des = new StringBuilder(30);
    //        des.Append(m_abortType);
    //        des.AppendFormat("\n{0}: {1} {2} {3}", Name, m_key, m_op, m_value);

    //        return des.ToString();
    //    }
    //}
    #endregion
}