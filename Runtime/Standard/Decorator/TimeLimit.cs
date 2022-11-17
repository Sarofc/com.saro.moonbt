using System.Text;
using Saro.BT.Utility;

namespace Saro.BT
{
    [BTNode("Time_Limit_24x", "“时间限制”装饰节点\n定时器归零后，将失败")]
    public class TimeLimit : BTDecorator
    {
        [BTRunTimeValue]
        public Timer timer = new();

        private void init()
        {
            timer.OnTimeout = Evaluate;
        }

        public override BTNode Clone()
        {
            var newNode = base.Clone() as TimeLimit;
            newNode.timer = new(timer);
            newNode.init();
            return newNode;
        }

        internal protected override void OnValidate()
        {
            abortType = EAbortType.Self;

            UpdateAborts();
        }

        protected override void OnDecoratorEnter()
        {
            Tree.AddTimer(timer);
            timer.Start();

            RunChild();
        }

        protected override void OnDecoratorExit()
        {
            Tree.RemoveTimer(timer);
        }

        protected override bool EvaluateCondition() => false;

        public override void Description(StringBuilder builder)
        {
            base.Description(builder);

            builder.AppendFormat("Abort and fail after {0:0.00} s", timer.GetIntervalInfo());
        }
    }
}