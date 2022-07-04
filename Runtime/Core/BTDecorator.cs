using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Saro.BT
{
    public enum EAbortType
    {
        None = 0,
        LowerPriority = 1,
        Self = 2,
        Both = 3,
    }

    [BTNode("Conditional_Decorator_24x")]
    public abstract class BTDecorator : BTAuxiliary
    {
        [UnityEngine.Space(20)]
        [Separator]
        public EAbortType abortType = EAbortType.None;

        [JsonIgnore]
        public bool IsObserving { get; protected set; } = false;

        [JsonIgnore]
        protected bool IsActive { get; set; } = false;

        public override void OnEnter()
        {
            IsActive = true;

            if (abortType != EAbortType.None)
            {
                if (!IsObserving)
                {
                    IsObserving = true;
                    OnObserverBegin();
                }
            }

            if (Condition())
            {
                base.OnEnter();
            }
        }

        public override void OnExit()
        {
            if (abortType == EAbortType.None || abortType == EAbortType.Self)
            {
                if (IsObserving)
                {
                    OnObserverEnd();
                    IsObserving = false;
                }
            }

            IsActive = false;
        }

        public sealed override void OnParentExit()
        {
            // 装饰器的父组合节点推出后，需要移除所有监听

            if (IsObserving)
            {
                IsObserving = false;
                OnObserverEnd();
            }

            base.OnParentExit();
        }

        /// <summary>
        /// register event
        /// </summary>
        public virtual void OnObserverBegin() { }

        /// <summary>
        /// unregister event
        /// </summary>
        public virtual void OnObserverEnd() { }

        public override EStatus OnExecute()
        {
            return Iterator.LastChildExitStatus.GetValueOrDefault(EStatus.Failure);
        }


        public virtual bool Condition() => true;

        protected void Evaluate()
        {
            var result = Condition();

            if (IsActive && !result)
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

        public override void Description(StringBuilder builder)
        {
            base.Description(builder);

            if (abortType != EAbortType.None)
                builder.AppendLine($"(abort: {abortType})");
        }
    }
}