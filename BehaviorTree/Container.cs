using System;
using UnityEngine.Assertions;

namespace Saro.BT
{
    public abstract class Container : Node
    {
        public bool Collapse { get => m_collapse; set => m_collapse = value; }
        private bool m_collapse = false;

        public Container(string name) : base(name)
        {
        }

        public void ChildStopped(Node child, bool success)
        {
            Assert.AreNotEqual(m_currentState, State.INACTIVE, "A Child of a Container was stopped while the container was inactive.");
            InternalChildStopped(child, success);
        }

        // be called in Stopped
        protected abstract void InternalChildStopped(Node child, bool success);

#if UNITY_EDITOR
        public abstract Node[] DebugChildren { get; }
#endif
    }
}