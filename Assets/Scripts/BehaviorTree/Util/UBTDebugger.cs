
using UnityEngine;

namespace Saro.BT
{
    public class UBTDebugger : MonoBehaviour
    {
        public Root behaviorTree;

        private static Blackboard m_globalBlackboard = null;
        public static Blackboard Globalstats
        {
            get
            {
                if (m_globalBlackboard == null)
                {
                    m_globalBlackboard = UBTContext.Instance.GetGlobalBlackboard("_GlobalBlackboard");
                }
                return m_globalBlackboard;
            }
        }

        private Blackboard _customStatus = null;
        public Blackboard CustomStatus
        {
            get
            {
                if (_customStatus == null)
                {
                    _customStatus = new Blackboard("CustomStatus",Globalstats, UBTContext.Instance.GetClock());
                }
                return _customStatus;
            }
        }
    }
}