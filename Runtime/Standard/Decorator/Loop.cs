﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Saro.BT
{
    [BTNode("Loop_24x", "“循环”装饰节点\n自身终止条件是 计数器是否耗尽。\n-1代表无限循环")]
    public sealed class Loop : BTDecorator
    {
        [Tooltip("loop times. -1 means loop infinite.")]
        public int loopCount = 1;

        [BTRunTimeValue]
        private int m_LoopCounter;

        public override void OnEnter()
        {
            m_LoopCounter = 0;
        }

        public override EStatus OnExecute()
        {
            if (loopCount == -1)
            {
                Iterator.Traverse(Child);
                return EStatus.Running;
            }
            else
            {
                if (m_LoopCounter < loopCount)
                {
                    m_LoopCounter++;
                    Iterator.Traverse(Child);
                    return EStatus.Running;
                }
                else
                {
                    return Iterator.LastChildExitStatus.GetValueOrDefault(EStatus.Failure);
                }
            }
        }

        public override void Description(StringBuilder builder)
        {
            base.Description(builder);

            if (loopCount == -1)
            {
                builder.Append("Loop infinite");
            }
            else if (loopCount < 1)
            {
                builder.Append("Don't loop");
            }
            else if (loopCount > 1)
            {
                builder.AppendFormat("Loop {0} times", loopCount);
            }
            else
            {
                builder.Append("Loop once");
            }
        }
    }

}