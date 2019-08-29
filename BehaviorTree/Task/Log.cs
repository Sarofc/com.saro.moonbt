using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            m_result = Result.SUCCESS;
            Stopped(true);
        }

        protected override void InternalCancel()
        {
            m_result = Result.FAILED;
            Stopped(false);
        }
    }
}
