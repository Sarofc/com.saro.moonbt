using System.Text;
using Saro.SEditor;

#if FIXED_POINT_MATH
using Single = sfloat;
#else
using Single = System.Single;
#endif

namespace Saro.BT
{
    public enum EAbortType
    {
        None = 0,
        /// <summary>
        /// 打断比此节点优先级低的节点，perOrder越大优先级越低
        /// </summary>
        LowerPriority = 1,
        /// <summary>
        /// 打断此节点
        /// </summary>
        Self = 2,
        /// <summary>
        /// LowerPriority 和 Self 都有效
        /// </summary>
        Both = 3,
    }

    // 不再提供默认逻辑，子类自己实现

    [BTNode("Conditional_Decorator_24x")]
    public abstract class BTDecorator : BTAuxiliary
    {
        [UnityEngine.Space(20)]
        [Separator]
        public EAbortType abortType = EAbortType.None;

        protected bool m_IsObserving = false;
        protected bool m_IsActive = false;

        public override BTNode Clone()
        {
            var newNode = MemberwiseClone() as BTDecorator;
            return newNode;
        }

        public sealed override void OnEnter()
        {
            m_IsActive = true;

            OnDecoratorEnter();
        }

        public sealed override void OnExit()
        {
            OnDecoratorExit();

            m_IsActive = false;
        }

        protected abstract void OnDecoratorEnter();
        protected abstract void OnDecoratorExit();

        public sealed override void OnParentExit()
        {
            // 装饰器的父组合节点推出后，需要移除所有监听
            ObserverEnd();

            base.OnParentExit();
        }

        protected void ObserverBegin()
        {
            if (!m_IsObserving)
            {
                m_IsObserving = true;
                OnObserverBegin();
            }
        }

        protected void ObserverEnd()
        {
            if (m_IsObserving)
            {
                m_IsObserving = false;
                OnObserverEnd();
            }
        }

        /// <summary>
        /// 注册观察者
        /// </summary>
        protected virtual void OnObserverBegin() { }

        /// <summary>
        /// 反注册观察者
        /// </summary>
        protected virtual void OnObserverEnd() { }

        public override EStatus OnExecute(Single deltaTime)
        {
            return Iterator.LastChildExitStatus.GetValueOrDefault(EStatus.Failure);
        }

        /// <summary>
        /// 评估打断条件
        /// <code>false: AbortCurrentBranch</code>
        /// <code>true: AbortLowerPriorityBranch</code>
        /// </summary>
        /// <returns></returns>
        protected virtual bool EvaluateCondition() => true;

        protected void Evaluate()
        {
            var result = EvaluateCondition();

            if (m_IsActive && !result)
            {
                AbortCurrentBranch();
            }
            else
            {
                AbortLowerPriorityBranch();
            }
        }

        private void AbortLowerPriorityBranch()
        {
            if (abortType == EAbortType.LowerPriority || abortType == EAbortType.Both)
            {
                if (TryGetCompositeParent(this, out var compositeParent, out var branchIndex))
                {
                    bool isLowerPriority = compositeParent.CurrentChildIndex > branchIndex;
                    if (isLowerPriority)
                    {
                        Iterator.AbortRunningChildBranch(compositeParent, branchIndex);
                    }
                }
            }
        }

        private void AbortCurrentBranch()
        {
            if (abortType == EAbortType.Self || abortType == EAbortType.Both)
            {
                Iterator.AbortRunningChildBranch(Parent, ChildOrder);
            }
        }

        /// <summary>
        /// find parent composite node
        /// </summary>
        /// <param name="child"></param>
        /// <param name="parent"></param>
        /// <param name="childIndex"></param>
        private bool TryGetCompositeParent(BTNode child, out BTComposite compositeParent, out int childIndex)
        {
            var parent = child.Parent;
            childIndex = child.childIndex;

            while (parent != null && !parent.IsComposite())
            {
                childIndex = parent.childIndex;
                parent = parent.Parent;
            }

            compositeParent = parent as BTComposite;

            return compositeParent != null;
        }

        protected void UpdateAborts()
        {
            // TODO 运行时，才能生效，编辑器得单独再弄一个方法来校验？

            if (abortType == EAbortType.None) return;

            if (TryGetCompositeParent(this, out var composite, out _))
            {
                if (composite is Sequence) // sequence 只能打断自己
                {
                    if (abortType == EAbortType.Both)
                        abortType = EAbortType.Self;
                    else if (abortType == EAbortType.LowerPriority)
                        abortType = EAbortType.None;
                }
                else if (composite is SimpleParallel) // simpleparallel 不可打断任何流程，降低复杂度
                {
                    abortType = EAbortType.None;
                }
            }
        }

#if UNITY_EDITOR
        public override void Description(StringBuilder builder)
        {
            base.Description(builder);

            if (abortType != EAbortType.None)
                builder.AppendLine($"(abort: {abortType})");
        }
#endif
    }
}