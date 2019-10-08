
using UnityEngine.Assertions;

namespace Saro.BT
{
    // TODO, fix follow control

    /* 
     * Warning:
     * 
     * if don't use ObserverAborts, only override InternalStart(), InternalAbort() and InternalChildStopped(). 
     * Otherwise, consider overriding StartObserver(), StopObserver() and IsConditionMet()
     * 
     */

    public abstract class Decorator : AuxiliaryNode
    {

        protected ObserverAborts m_abortType;
        protected bool m_isObserving;

        // TODO
        protected bool m_abortNone = true;
        protected bool m_abortSelf = true;
        protected bool m_abortLowerPriority = true;

        private Composite m_parentCompositeNode;
        private Node m_chidrenOfComposite;


        public Decorator(string name) : base(name)
        {
            m_isObserving = false;
        }


        protected override void InternalStart()
        {
            if (m_abortType != ObserverAborts.NONE)
            {
                if (!m_isObserving)
                {
                    m_isObserving = true;
                    StartObserving();
                }
            }

            if (!IsConditionMet())
            {
                Stopped(false);
            }
            else
            {
                m_childNode.Start();
            }
        }

        protected override void InternalAbort()
        {
            m_childNode.Abort();
        }

        protected override void InternalChildStopped(Node child, bool? result)
        {
            Assert.AreNotEqual(CurrentStatus, NodeStatus.Inactive);

            if (m_abortType == ObserverAborts.SELF)
            {
                if (m_isObserving)
                {
                    m_isObserving = false;
                    StopObserving();
                }
            }
            Stopped(result);
        }

        /*
         * WARNING : remove observer if Parent Composite Node is stopped.
         */
        protected override void InternalParentCompositeStopped(Composite parentComposite)
        {
            if (m_isObserving)
            {
                m_isObserving = false;
                StopObserving();
            }
        }

        protected void Evaluate()
        {
            // abort self
            if (IsActive && !IsConditionMet())
            {
                if (m_abortType == ObserverAborts.SELF || m_abortType == ObserverAborts.BOTH)
                {
                    Abort();
                }
            }
            // abort any nodes to the right of this node
            else if (!IsActive && IsConditionMet())
            {
                if (m_abortType == ObserverAborts.LOWER_PRIORITY || m_abortType == ObserverAborts.BOTH)
                {
                    if (m_isObserving)
                    {
                        m_isObserving = false;
                        StopObserving();
                    }

                    m_parentCompositeNode.AbortTreeNode(m_chidrenOfComposite);
                }
            }
        }

        // TODO
        public void UpdateAbortsType()
        {
            // get parent composite node
            Container parentNode = ParentContainerNode;
            m_chidrenOfComposite = this;
            while (parentNode != null && !(parentNode is Composite))
            {
                m_chidrenOfComposite = parentNode;
                parentNode = parentNode.ParentContainerNode;
            }

            Assert.IsNotNull(parentNode, "ObservingDecorator is only valid when attached to a parent composite");
            Assert.IsNotNull(m_chidrenOfComposite);

            m_parentCompositeNode = parentNode as Composite;

            // update constraint
            if (m_parentCompositeNode == null)
            {
                m_abortType = ObserverAborts.NONE;
                return;
            }

            if (!m_parentCompositeNode.CanAbortSelf || !m_abortSelf)
            {
                if (m_abortType == ObserverAborts.BOTH)
                {
                    m_abortType = m_parentCompositeNode.CanAbortLowerPriority ? ObserverAborts.LOWER_PRIORITY : ObserverAborts.NONE;
                }
                else if (m_abortType == ObserverAborts.SELF)
                {
                    m_abortType = ObserverAborts.NONE;
                }
            }

            if (!m_parentCompositeNode.CanAbortLowerPriority || !m_abortLowerPriority)
            {
                if (m_abortType == ObserverAborts.BOTH)
                {
                    m_abortType = m_parentCompositeNode.CanAbortSelf ? ObserverAborts.SELF : ObserverAborts.NONE;
                }
                else if (m_abortType == ObserverAborts.LOWER_PRIORITY)
                {
                    m_abortType = ObserverAborts.NONE;
                }
            }
        }

        protected virtual bool IsConditionMet()
        {
            // default value
            return true;
        }

        protected virtual void StartObserving()
        {
            // do nothing in base class
        }

        protected virtual void StopObserving()
        {
            // do nothing in base class
        }
    }
}