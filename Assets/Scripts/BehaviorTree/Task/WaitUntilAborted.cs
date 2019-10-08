
namespace Saro.BT
{
    // is usefull ???
    
    public class WaitUntilAborted : Task
    {
        private  bool m_result;

        public WaitUntilAborted(bool result = false) : base("WaitUntilAborted")
        {
            m_result = result;
        }

        protected override void InternalStart()
        {
            // do nothing
        }

        protected override void InternalAbort()
        {
            Stopped(m_result);
        }
    }
}