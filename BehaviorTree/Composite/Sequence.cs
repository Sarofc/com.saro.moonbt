
using UnityEngine.Assertions;

namespace Saro.BT
{
    public class Sequence : Composite
    {
        private int m_currentIdx = -1;
        // don't allow aborting lower priorities, as it breaks sequence order and doesn't makes sense
        public override bool CanAbortLowerPriority => false;

        public Sequence(params Node[] children) : base("Sequence", children)
        {
        }

        protected override void InternalStart()
        {
#if UNITY_EDITOR
            foreach (var node in m_children)
            {
                Assert.AreEqual(node.CurrentState, State.INACTIVE);
            }
#endif

            m_currentIdx = -1;

            ProcessChildren();
        }


        protected override void InternalCancel()
        {
            m_children[m_currentIdx].Cancel();
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
                    node.Cancel();
                    break;
                }
            }
        }

        [System.Obsolete("obsolete")]
        public override void StopLowerPriorityChildrenForChild(Node child, bool immediateRestart)
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
                    if (immediateRestart)
                    {
                        m_currentIdx = idx - 1;
                    }
                    else
                    {
                        m_currentIdx = m_children.Length;
                    }
                    node.Cancel();
                    break;
                }
            }
        }

        protected override void InternalChildStopped(Node child, bool success)
        {
            if (success)
            {
                ProcessChildren();
            }
            else
            {
                Stopped(false);
            }
        }

        public override string ToString()
        {
            return string.Format("{0} [{1}]", /*base.ToString()*/"<b> -> </b>", m_currentIdx);
        }

        private void ProcessChildren()
        {
            if (++m_currentIdx < m_children.Length)
            {
                if (IsCancelled)
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
                Stopped(true);
            }
        }

    }
}