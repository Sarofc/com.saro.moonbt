using System.Text;
using Saro.BT.Utility;

namespace Saro.BT
{
    [BTNode("Cooldown_24x", "“冷却”装饰节点。\n自身条件基于Timer是否结束。退出此节点时，即子树执行完毕，才会开始计时！")]
    public sealed class Cooldown : BTDecorator
    {
        [BTRunTimeValue]
        public Timer timer = new();

        public override void OnInitialize()
        {
            base.OnInitialize();

            timer.OnTimeout += RemoveOnTimeout;
        }

        public override void OnReset()
        {
            base.OnReset();

            timer.OnTimeout -= RemoveOnTimeout;

            if (IsObserving)
            {
                IsObserving = false;
                OnObserverEnd();
            }
        }

        public override void OnEnter()
        {
            IsActive = true;

            if (timer.IsDone)
            {
                Iterator.Traverse(Child);
            }
        }

        public override void OnExit()
        {
            if (timer.IsDone)
            {
                Tree.AddTimer(timer);
                timer.Start();
            }

            IsActive = false;
        }

        public override EStatus OnExecute()
        {
            if (timer.IsRunning) return EStatus.Failure;

            return Iterator.LastChildExitStatus.GetValueOrDefault(EStatus.Failure);
        }

        public override void OnObserverBegin()
        {
            timer.OnTimeout += Evaluate;
        }

        public override void OnObserverEnd()
        {
            timer.OnTimeout -= Evaluate;
        }

        public override bool Condition()
        {
            return timer.IsDone;
        }

        private void RemoveOnTimeout()
        {
            Tree.RemoveTimer(timer);
        }

        public override void Description(StringBuilder builder)
        {
            base.Description(builder);

            builder.AppendFormat("Lock execution for {0:0.00} s", timer.GetIntervalInfo());
        }

        internal protected override void OnValidate()
        {
            base.OnValidate();

            if (abortType == EAbortType.Self || abortType == EAbortType.Both)
                abortType = EAbortType.LowerPriority;

            UpdateAborts();
        }
    }
}