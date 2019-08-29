namespace Saro.BT
{
    public class MockNode : Node
    {
        private bool succedsOnExplictStop;

        public MockNode(bool succedsOnExplictStop = false) : base("MockNode")
        {
            this.succedsOnExplictStop = succedsOnExplictStop;
        }

        override protected void InternalCancel()
        {
            UnityEngine.Debug.Log("cancel");
            this.Stopped(succedsOnExplictStop);
        }

        public void Finish(bool success)
        {
            UnityEngine.Debug.Log("finish");
            this.Stopped(success);
        }

        protected override void InternalStart()
        {
            //throw new System.NotImplementedException();
        }
    }
}