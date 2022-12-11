using System;
using System.Text;

namespace Saro.BT
{
    [BTNode("Simple_Parallel_24x", "“简单并行”节点\n此节点有装饰器局部作用域\n允许主任务和后台任务一起执行，主任务不能包含Composite节点")]
    public sealed class SimpleParallel : BTComposite
    {
        private BTBehaviorIterator[] m_BranchIterator;
        private EStatus[] m_ChildrenStatus;

        public EFinishMode finishMode = EFinishMode.Immediate;

        public enum EFinishMode : byte
        {
            Immediate = 0,
            Delayed = 1
        }

        public override BTNode Clone()
        {
            var newNode = new SimpleParallel();

            newNode.m_BranchIterator = new BTBehaviorIterator[m_BranchIterator.Length];
            Array.Copy(m_BranchIterator, newNode.m_BranchIterator, m_BranchIterator.Length);

            newNode.m_ChildrenStatus = new EStatus[m_ChildrenStatus.Length];
            Array.Copy(m_ChildrenStatus, newNode.m_ChildrenStatus, m_ChildrenStatus.Length);

            newNode.finishMode = finishMode;

            return newNode;
        }

        public override void OnInitialize()
        {
            base.OnInitialize();

            if (ChildCount() != 2) throw new System.Exception("SimpleParallel's children must equal to 2");

            var count = ChildCount();
            m_ChildrenStatus = new EStatus[count];
            m_BranchIterator = new BTBehaviorIterator[count];
            for (int i = 0; i < count; i++)
            {
                m_BranchIterator[i] = new BTBehaviorIterator(Tree);

                foreach (var node in TreeTraversal.PreOrder(GetChildAt(i)))
                {
                    node.Iterator = m_BranchIterator[i];
                }
            }
        }

        public override void OnEnter()
        {
            for (int i = 0; i < ChildCount(); i++)
            {
                m_ChildrenStatus[i] = EStatus.Running;
                m_BranchIterator[i].Traverse(GetChildAt(i));
            }
        }

        public override void OnExit()
        {
            for (int i = 0; i < m_BranchIterator.Length; i++)
            {
                if (m_BranchIterator[i].IsRunning)
                {
                    m_BranchIterator[i].Interrupt(GetChildAt(i));
                }
            }

            // TODO 此节点有 装饰器作用域，离开此节点时，观察中的装饰器 (LowerPriority/Both) 都要取消观察
            // 没有发现文档。。。 描述很模糊，像是 此节点下的装饰器，只有 Self 有效，只能打断自己，不能 打断低优先级 的树

            //foreach (var node in TreeTraversal.PreOrder<BTNode>(this))
            //{
            //    if (node is BTDecorator decorator)
            //    {
            //        if(decorator.abortType == EAbortType.Both)
            //        {
            //            decorator.abortType = EAbortType.Self;
            //        }
            //        else if(decorator.abortType == EAbortType.LowerPriority)
            //        {
            //            // TODO remove observers
            //            decorator.abortType = EAbortType.None;
            //        }
            //    }
            //}
        }

        public override void OnChildExit(int childIndex, EStatus status)
        {
            m_ChildrenStatus[childIndex] = status;
        }

        public override EStatus OnExecute(float deltaTime)
        {
            if (m_ChildrenStatus[0] == EStatus.Failure) return EStatus.Failure;

            if (finishMode == EFinishMode.Immediate)
            {
                if (m_ChildrenStatus[0] != EStatus.Running)
                {
                    return m_ChildrenStatus[0];
                }
            }
            else
            {
                if (m_ChildrenStatus[0] == EStatus.Success &&
                    m_ChildrenStatus[1] != EStatus.Running)
                {
                    return EStatus.Success;
                }
            }

            for (int i = 0; i < m_BranchIterator.Length; i++)
            {
                if (m_BranchIterator[i].IsRunning)
                {
                    m_BranchIterator[i].Tick(deltaTime);
                }
            }

            return EStatus.Running;
        }

        public override void Description(StringBuilder builder)
        {
            base.Description(builder);
            builder.Append("Local scope for observers");
            builder.AppendLine();
            builder.Append("FinishMode: ").Append(finishMode);
        }
    }
}