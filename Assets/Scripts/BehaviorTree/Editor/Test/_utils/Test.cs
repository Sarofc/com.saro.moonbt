namespace Saro.BT
{
    public class Test
    {
        protected Node SUT;
        protected TestRoot Root;
        protected Blackboard Blackboard;
        protected Clock Timer;

        protected TestRoot CreateBehaviorTree(Node sut)
        {
            this.Timer = new Clock();
            this.Blackboard = new Blackboard("",this.Timer);
            this.Root = (TestRoot)new TestRoot(Blackboard, Timer).Decorate(sut);
            this.SUT = sut;
            return Root;
        }
    }
}