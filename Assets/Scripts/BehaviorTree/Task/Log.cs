
namespace Saro.BT
{
    public class Log : Task
    {
        private string m_logText;
        public Log(string logText) : base("Log")
        {
            m_logText = logText;
        }

        protected override void InternalStart()
        {
            UnityEngine.Debug.Log(m_logText);
            Stopped(true);
        }

        protected override void InternalAbort()
        {
            Stopped(false);
        }

        public override string GetStaticDescription()
        {
            return base.GetStaticDescription() + " : " + m_logText;
        }
    }
}
