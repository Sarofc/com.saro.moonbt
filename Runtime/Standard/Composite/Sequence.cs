using System;
using System.Collections.Generic;

namespace Saro.BT
{
    [BTNode("Sequence_24x", "“序列”节点\n从上往下执行子节点，如果一个子节点失败，则此节点失败。\n所有子节点执行成功，则此节点成功")]
    public class Sequence : BTComposite
    {
        public override EStatus OnExecute()
        {
            if (m_LastChildExitStatus == EStatus.Failure) return EStatus.Failure;

            var next = CurrentChild();

            if (next == null)
            {
                return EStatus.Success;
            }
            else
            {
                Iterator.Traverse(next);
                return EStatus.Running;
            }
        }
    }
}
