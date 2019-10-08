using System;
using UnityEngine.Assertions;

namespace Saro.BT
{
    public abstract class Container : Node
    {
        // TODO remove this after Editor Complete
        public bool Collapse { get => m_collapse; set => m_collapse = value; }
        private bool m_collapse = false;

        public Container(string name) : base(name)
        {
        }


        public void ChildStopped(Node child, bool? result)
        {
            Assert.AreNotEqual(m_currentStatus, NodeStatus.Inactive, "A Child of a Container was stopped while the container was inactive.");
            InternalChildStopped(child, result);
        }

        // be called in Stopped
        protected abstract void InternalChildStopped(Node child, bool? success);

#if UNITY_EDITOR
        public abstract Node[] DebugChildren { get; }
#endif
    }
}