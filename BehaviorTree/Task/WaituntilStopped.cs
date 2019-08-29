
namespace Saro.BT
{
    public class WaitUntilStopped : Task
    {
        //private bool m_result;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result">result of execution</param>
        public WaitUntilStopped(Result result = Result.FAILED) : base("WaitUntilStopped")
        {
            m_result = result;
        }

        protected override void InternalStart()
        {
            // do nothing
        }

        protected override void InternalCancel()
        {
            Stopped(m_result == Result.SUCCESS);
        }

    }
}