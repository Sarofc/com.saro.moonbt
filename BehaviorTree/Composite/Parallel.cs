
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Saro.BT
{
    [System.Obsolete("obsolete", true)]
    public class Parallel : Composite
    {
        public enum Policy
        {
            ONE,
            ALL
        }

        private Policy m_failurePolicy;
        private Policy m_successPolicy;
        private int m_childrenCount = 0;
        private int m_runningCount = 0;
        private int m_succeededCount = 0;
        private int m_failedCount = 0;
        private Dictionary<Node, bool> m_childrenResults;
        private bool m_successState;
        private bool m_childrenAborted;

        public Parallel(Policy successPolicy, Policy failurePolicy, params Node[] children) : base("Parallel", children)
        {
            m_successPolicy = successPolicy;
            m_failurePolicy = failurePolicy;

            m_childrenCount = children.Length;
            m_childrenResults = new Dictionary<Node, bool>();
        }

        protected override void InternalStart()
        {
#if UNITY_EDITOR
            foreach (var node in m_children)
            {
                Assert.AreEqual(node.CurrentState, State.INACTIVE);
            }
#endif

            m_childrenAborted = false;
            m_runningCount = 0;
            m_succeededCount = 0;
            m_failedCount = 0;
            foreach (Node child in m_children)
            {
                m_runningCount++;
                child.Start();
            }
        }

        protected override void InternalCancel()
        {
            Assert.IsTrue(m_runningCount + m_succeededCount + m_failedCount == m_childrenCount);

            foreach (Node child in m_children)
            {
                if (child.IsActive)
                {
                    child.Cancel();
                }
            }
        }

        [System.Obsolete("obsolete")]
        public override void StopLowerPriorityChildrenForChild(Node child, bool immediateRestart)
        {
            if (immediateRestart)
            {
                Assert.IsFalse(child.IsActive);
                if (m_childrenResults[child])
                {
                    m_succeededCount--;
                }
                else
                {
                    m_failedCount--;
                }
                m_runningCount++;
                child.Start();
            }
            else
            {
                // TODO 
                // remove if else ? make sure that stopping parallel node
                throw new System.Exception("On Parallel Nodes all children have the same priority, thus the method does nothing if you pass false to 'immediateRestart'!");
            }
        }

        protected override void InternalChildStopped(Node child, bool result)
        {
            m_runningCount--;
            if (result)
            {
                m_succeededCount++;
            }
            else
            {
                m_failedCount++;
            }
            m_childrenResults[child] = result;

            bool allChildrenStarted = m_runningCount + m_succeededCount + m_failedCount == m_childrenCount;
            if (allChildrenStarted)
            {
                if (m_runningCount == 0)
                {
                    if (!m_childrenAborted) // if children got aborted because rule was evaluated previously, we don't want to override the successState 
                    {
                        if (m_failurePolicy == Policy.ONE && m_failedCount > 0)
                        {
                            m_successState = false;
                        }
                        else if (m_successPolicy == Policy.ONE && m_succeededCount > 0)
                        {
                            m_successState = true;
                        }
                        else if (m_successPolicy == Policy.ALL && m_succeededCount == m_childrenCount)
                        {
                            m_successState = true;
                        }
                        else
                        {
                            m_successState = false;
                        }
                    }
                    Stopped(m_successState);
                }
                else if (!m_childrenAborted)
                {
                    Assert.IsFalse(m_succeededCount == m_childrenCount);
                    Assert.IsFalse(m_failedCount == m_childrenCount);

                    if (m_failurePolicy == Policy.ONE && m_failedCount > 0/* && waitForPendingChildrenRule != Wait.ON_FAILURE && waitForPendingChildrenRule != Wait.BOTH*/)
                    {
                        m_successState = false;
                        m_childrenAborted = true;
                    }
                    else if (m_successPolicy == Policy.ONE && m_succeededCount > 0/* && waitForPendingChildrenRule != Wait.ON_SUCCESS && waitForPendingChildrenRule != Wait.BOTH*/)
                    {
                        m_successState = true;
                        m_childrenAborted = true;
                    }

                    if (m_childrenAborted)
                    {
                        foreach (Node currentChild in m_children)
                        {
                            if (currentChild.IsActive)
                            {
                                currentChild.Cancel();
                            }
                        }
                    }
                }
            }
        }

        public override void AbortTreeNode(Node child)
        {
            throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            return string.Format("{0}", /*base.ToString()*/"<b> ↓↓ </b>");
        }
    }
}