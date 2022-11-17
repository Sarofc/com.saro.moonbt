using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Saro.SEditor;

namespace Saro.BT
{
    public enum EAbortType
    {
        None = 0,
        LowerPriority = 1,
        Self = 2,
        Both = 3,
    }

    // TODO 简化概念，不再提供默认逻辑，子类自己实现

    [BTNode("Conditional_Decorator_24x")]
    public abstract class BTDecorator : BTAuxiliary
    {
        [UnityEngine.Space(20)]
        [Separator]
        public EAbortType abortType = EAbortType.None;

        [JsonIgnore]
        protected bool IsObserving = false;

        [JsonIgnore]
        protected bool IsActive = false;

        public sealed override void OnEnter()
        {
            IsActive = true;

            OnDecoratorEnter();

            // old
            //if (abortType != EAbortType.None)
            //{
            //    ObserverBegin();
            //}

            //if (EvaluateCondition())
            //{
            //    base.OnEnter();
            //}
        }

        public sealed override void OnExit()
        {
            OnDecoratorExit();

            // old
            //if (abortType == EAbortType.None || abortType == EAbortType.Self)
            //{
            //    ObserverEnd();
            //}

            IsActive = false;
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
            if (!IsObserving)
            {
                IsObserving = true;
                OnObserverBegin();
            }
        }

        protected void ObserverEnd()
        {
            if (IsObserving)
            {
                IsObserving = false;
                OnObserverEnd();
            }
        }

        /// <summary>
        /// register event
        /// </summary>
        protected virtual void OnObserverBegin() { }

        /// <summary>
        /// unregister event
        /// </summary>
        protected virtual void OnObserverEnd() { }

        public override EStatus OnExecute(float deltaTime)
        {
            return Iterator.LastChildExitStatus.GetValueOrDefault(EStatus.Failure);
        }

        /// <summary>
        /// <code>false: AbortCurrentBranch</code>
        /// <code>true: AbortLowerPriorityBranch</code>
        /// </summary>
        /// <returns></returns>
        protected virtual bool EvaluateCondition() => true;

        protected void Evaluate()
        {
            var result = EvaluateCondition();

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