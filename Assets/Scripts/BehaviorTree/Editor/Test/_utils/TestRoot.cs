namespace Saro.BT
{
    public class TestRoot : Root
    {
        private bool didFinish = false;
        private bool wasSuccess = false;

        public bool DidFinish
        {
            get { return didFinish; }
        }

        public bool WasSuccess
        {
            get { return wasSuccess; }
        }

        public TestRoot(Blackboard blackboard, Clock timer) :
            base(blackboard, timer)
        {
        }

        override protected void InternalStart()
        {
            this.didFinish = false;
            base.InternalStart();
        }

        override protected void InternalChildStopped(Node node, bool? result)
        {
            if (!result.HasValue) return;

            didFinish = true;
            wasSuccess = result.Value;
            Stopped(result);
        }
    }
}