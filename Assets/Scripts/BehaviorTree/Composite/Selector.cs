
using System;
using UnityEngine.Assertions;

namespace Saro.BT
{
    public class Selector : Composite
    {
        private int m_currentIdx = -1;

        public Selector() : base("Selector")
        {
        }

        public Composite OpenBranch(params Node[] children)
        {
            return base.InternalOpenBranch(children);
        }

        protected override void InternalStart()
        {
#if UNITY_EDITOR
            foreach (var node in m_children)
            {
                Assert.AreEqual(node.CurrentStatus, NodeStatus.Inactive);
            }
#endif

            m_currentIdx = -1;

            ProcessChildren();
        }


        protected override void InternalAbort()
        {
            m_children[m_currentIdx].Abort();
        }

        protected override void InternalChildStopped(Node child, bool? result)
        {
            if (!result.HasValue) return;

            if (result.Value)
            {
                Stopped(true);
            }
            else
            {
                ProcessChildren();
            }
        }

        public override void AbortTreeNode(Node child)
        {
            int idx = 0;
            bool found = false;
            foreach (var node in m_children)
            {
                if (node == child)
                {
                    found = true;
                }
                else if (!found)
                {
                    idx++;
                }
                else if (found && node.IsActive)
                {
                    m_currentIdx = idx - 1;
                    node.Abort();
                    break;
                }
            }
        }

        

        private void ProcessChildren()
        {
            if (++m_currentIdx < m_children.Length)
            {
                if (IsAborted)
                {
                    Stopped(false);
                }
                else
                {
                    m_children[m_currentIdx].Start();
                }
            }
            else
            {
                Stopped(false);
            }
        }

        public override string GetStaticDescription()
        {
            return string.Format("{0} [{1}]", /*base.ToString()*/"<b> ? </b>", m_currentIdx);
        }
    }
}