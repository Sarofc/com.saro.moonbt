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

        public TestRoot(Blackboard blackboard, Clock timer, Node mainNode) :
            base(blackboard, timer, mainNode)
        {
        }

        override protected void InternalStart()
        {
            this.didFinish = false;
            base.InternalStart();
        }

        override protected void InternalChildStopped(Node node, bool success)
        {
            didFinish = true;
            wasSuccess = success;
            Stopped(success);
        }
    }
}