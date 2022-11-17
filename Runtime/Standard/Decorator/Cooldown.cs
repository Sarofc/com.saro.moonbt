using System;
using System.Text;
using Saro.BT.Utility;

namespace Saro.BT
{
    // Cooldown只要没冷却，就会一直跑，但是观者者终止机制，只会在分支下才有效
    [BTNode("Cooldown_24x", "“冷却”装饰节点。\n自身条件基于Timer是否结束。退出此节点时，即子树执行完毕，才会开始计时！")]
    public sealed class Cooldown : BTDecorator
    {
        [BTRunTimeValue]
        public Timer timer = new();

        private Action m_RemoveTimer;
        private Action m_Evaluate;

        public Cooldown()
        {
            m_RemoveTimer = () => Tree.RemoveTimer(timer);
            m_Evaluate = () =>
            {
                Tree.RemoveTimer(timer);
                Evaluate();
            };
        }

        protected override void OnDecoratorEnter()
        {
            if (timer.IsDone)
            {
                Iterator.Traverse(Child);
            }
        }

        protected override void OnDecoratorExit()
        {
            if (abortType == EAbortType.LowerPriority)
                ObserverBegin();

            if (timer.IsDone)
            {
                Tree.AddTimer(timer);
                timer.Start();
            }
        }

        protected override void OnObserverBegin()
        {
            timer.OnTimeout = m_Evaluate;
        }

        protected override void OnObserverEnd()
        {
            timer.OnTimeout = m_RemoveTimer;
        }

        public override EStatus OnExecute(float deltaTime)
        {
            if (!timer.IsDone) return EStatus.Failure;

            return Iterator.LastChildExitStatus.GetValueOrDefault(EStatus.Failure);
        }

        protected override bool EvaluateCondition() => true;

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