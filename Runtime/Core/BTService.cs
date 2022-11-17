using System.Text;

using Saro.BT.Utility;
using UnityEngine;

namespace Saro.BT
{
    /* 
     * TODO
     * 
     * ue的规则如下：
     * 1. composite时，service在composite后执行
     * 2. task时，service在task之前执行
     * 
     * 此行为树的service都是在之前执行，不管composite、还是task
     * 
     */
    [BTNode("Service_24x", "“服务”节点\n当子树在运行时，就会间隔调用 ServiceTick，用于代替 传统并行节点。")]
    public abstract class BTService : BTAuxiliary
    {
        [BTRunTimeValue]
        public Timer timer;
        [Tooltip("每次进入节点时，重新启动计时器")]
        public bool restartTimerOnEnter;

        private void init()
        {
            timer.OnTimeout = ServiceTick;
            timer.AutoRestart = true;
        }

        public override BTNode Clone()
        {
            var newNode = MemberwiseClone() as BTService;
            newNode.timer = new(timer);
            newNode.init();
            return newNode;
        }

        public sealed override void OnEnter()
        {
            Tree.AddTimer(timer);

            if (timer.IsDone || restartTimerOnEnter)
            {
                timer.Start();
            }

            RunChild();
        }

        public sealed override void OnExit()
        {
            Tree.RemoveTimer(timer);
        }

        public sealed override EStatus OnExecute(float deltaTime)
        {
            return Iterator.LastChildExitStatus.GetValueOrDefault(EStatus.Failure);
        }

        protected abstract void ServiceTick();

        public override void Description(StringBuilder builder)
        {
            builder.AppendFormat("Tick every {0:0.00} s", timer.GetIntervalInfo())
                .AppendLine()
                .Append(restartTimerOnEnter ? "Restart timer on enter" : "Resume timer on enter");
        }

        public sealed override void OnChildEnter(int childIndex) { }

        public sealed override void OnChildExit(int childIndex, EStatus status) { }

        public sealed override void OnParentExit() { base.OnParentExit(); }
    }
}