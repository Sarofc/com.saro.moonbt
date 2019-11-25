
using System;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

namespace Saro.BT
{
    // TODO 
    public class MoveTo : ActionsBase
    {
        private NavMeshAgent m_agent;
        private string m_targetKey;

        private float m_sqrStoppingDistance = 1f;

        public MoveTo(NavMeshAgent agent, string targetKey) : base("MoveTo", false)
        {
            m_agent = agent;
            m_targetKey = targetKey;
        }

        protected override void InitNode()
        {
            //Blackboard.AddObserver(m_targetKey, OnDestinationChanged);

        }

        private void OnDestinationChanged(Blackboard.Type type, object data)
        {

        }

        protected override bool? TickNode()
        {
            var valueRef = Blackboard.Get(m_targetKey);

            if (valueRef == null) return true;

            if (valueRef.Value is Transform _transform)
            {
                if ((_transform.position - m_agent.transform.position).sqrMagnitude > m_sqrStoppingDistance * m_sqrStoppingDistance)
                    m_agent.destination = _transform.position;
                else
                    return true;
            }
            else if (valueRef.Value is Vector3 _pos)
            {
                if ((_pos - m_agent.transform.position).sqrMagnitude > m_sqrStoppingDistance * m_sqrStoppingDistance)
                    m_agent.destination = _pos;
                else
                    return true;
            }
            else
            {
                Debug.LogError($"can't support this type : {valueRef.Value.GetType()}");
                return true;
            }

            return null;
        }

        public override string GetStaticDescription()
        {
            return base.GetStaticDescription();
        }

        public override string DescribeRuntimeValues(StringBuilder des)
        {
            return base.DescribeRuntimeValues(des);
        }
    }
}