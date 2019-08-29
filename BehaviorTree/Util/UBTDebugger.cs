
namespace Saro.BT
{
    public class UBTDebugger : UBehaviour
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

        private Blackboard _customStats = null;
        public Blackboard CustomStats
        {
            get
            {
                if (_customStats == null)
                {
                    _customStats = new Blackboard(Globalstats, UBTContext.Instance.GetClock());
                }
                return _customStats;
            }
        }
    }
}