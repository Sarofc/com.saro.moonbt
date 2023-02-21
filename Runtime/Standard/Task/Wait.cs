﻿using System;
using System.Collections.Generic;
using System.Text;
using Saro.BT.Utility;

#if FIXED_POINT_MATH
using Single = Saro.FPMath.sfloat;
#else
using Single = System.Single;
#endif

namespace Saro.BT
{
    [BTNode("Wait_24x","“等待”任务节点\n执行时，等待指定时间，再返回成功，其余时候处于执行中")]
    public sealed class Wait : BTTask
    {
        [BTRunTimeValue]
        public Timer timer = new();

        public override BTNode Clone()
        {
            var newNode = base.Clone() as Wait;
            newNode.timer = new(timer);
            return newNode;
        }

        public override void OnEnter()
        {
            timer.Start();
        }

        public override EStatus OnExecute(Single deltaTime)
        {
            timer.Tick(deltaTime);

            var status = timer.IsDone ? EStatus.Success : EStatus.Running;

            return status;
        }

        public override void Description(StringBuilder builder)
        {
            builder.AppendFormat("Wait for {0:0.00} s", timer.GetIntervalInfo());
        }
    }
}