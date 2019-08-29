
using UnityEngine.Assertions;

namespace Saro.BT
{
    public abstract class ObservingDecorator : Decorator
    {
        protected ObserverAborts m_abortType;
        private bool m_isObserving;

        private Composite m_parentCompositeNode;
        private Node m_chidrenOfComposite;

        public ObservingDecorator(string name, ObserverAborts aborts, Node decorated) : base(name, decorated)
        {
            //UnityEngine.Debug.Log("observing decorator");
            m_isObserving = false;
            m_abortType = aborts;
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
                m_decorated.Start();
            }
        }

        override protected void InternalCancel()
        {
            m_decorated.Cancel();
        }

        protected override void InternalChildStopped(Node child, bool success)
        {
            Assert.AreNotEqual(CurrentState, State.INACTIVE);

            if (m_abortType == ObserverAborts.SELF)
            {
                if (m_isObserving)
                {
                    m_isObserving = false;
                    StopObserving();
                }
            }
            Stopped(success);
        }

        /// <summary>
        /// WARNNING : remove observer
        /// </summary>
        /// <param name="parentComposite"></param>
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
                    Cancel();
                }
            }
            // abort any nodes to the right of this node
            else if (!IsActive && IsConditionMet())
            {
                if (m_abortType == ObserverAborts.LOWER_PRIORITY || m_abortType == ObserverAborts.BOTH)
                {
                    //Container parentNode = ParentContainerNode;
                    //Node childNode = this;
                    //while (parentNode != null && !(parentNode is Composite))
                    //{
                    //    childNode = parentNode;
                    //    parentNode = parentNode.ParentContainerNode;
                    //}

                    //Assert.IsNotNull(m_parentCompositeNode, "ObservingDecorator is only valid when attached to a parent composite");
                    //Assert.IsNotNull(this);

                    if (m_isObserving)
                    {
                        m_isObserving = false;
                        StopObserving();
                    }

                    m_parentCompositeNode.AbortTreeNode(m_chidrenOfComposite);
                }
            }
        }

        public void UpdateAbortsType()
        {
            //UnityEngine.Debug.Log(ParentContainerNode);
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

            if (m_parentCompositeNode == null)
            {
                m_abortType = ObserverAborts.NONE;
                return;
            }

            if (!m_parentCompositeNode.CanAbortSelf)
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

            if (!m_parentCompositeNode.CanAbortLowerPriority)
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

        protected abstract void StartObserving();

        protected abstract void StopObserving();

        protected abstract bool IsConditionMet();
    }
}