using System;
using System.Collections.Generic;
using System.Text;
using Saro.BT.Utility;

namespace Saro.BT
{
    [BTNode("Time_Limit_24x","“时间限制”装饰节点\n定时器归零后，将失败")]
    public class TimeLimit : BTDecorator
    {
        [BTRunTimeValue]
        public Timer timer = new Timer();

        internal protected override void OnValidate()
        {
            abortType = EAbortType.Self;

            UpdateAborts();
        }

        public override void OnInitialize()
        {
            base.OnInitialize();

            timer.OnTimeout += Evaluate;
        }

        public override void OnEnter()
        {
            Tree.AddTimer(timer);
            timer.Start();
            base.OnEnter();
        }

        public override void OnExit()
        {
            Tree.RemoveTimer(timer);
        }

        public override bool Condition()
        {
            return !timer.IsDone;
        }

        public override void Description(StringBuilder builder)
        {
            base.Description(builder);

            builder.AppendFormat("Abort and fail after {0:0.00} s", timer.GetIntervalInfo());
        }
    }
}