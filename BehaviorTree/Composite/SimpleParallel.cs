using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Assertions;

namespace Saro.BT
{
    /// <summary>
    /// allow only two children : 
    /// main task : one which must be a single task node(with optional decorators).
    /// background tree : and the other of which can be a complete subtree.
    /// retun main task result
    /// </summary>
    public class SimpleParallel : Composite
    {
        public enum FinishMode
        {
            IMMEDIATE, //once the main task finishes, the background tree will be aborted
            DELAYED //once the main task has finished, the background tree will be permitted to finish
        }

        // don't allow aborting lower priorities & self, doesn't makes sense
        public override bool CanAbortLowerPriority => false;
        public override bool CanAbortSelf => false;

        private FinishMode m_finishMode;
        private int m_runningCount;
        private bool? m_mainTaskResult;
        private bool? m_bgTreeResult;

        public SimpleParallel(Node mainTask, Node bgTree) : base("SimpleParallel", new Node[] { mainTask, bgTree })
        {
            m_finishMode = FinishMode.IMMEDIATE;
        }

        public SimpleParallel(Node mainTask, Node subTree, FinishMode finishMode) : base("SimpleParallel", new Node[] { mainTask, subTree })
        {
            m_finishMode = finishMode;
        }

        protected override void InternalStart()
        {
#if UNITY_EDITOR
            foreach (var node in m_children)
            {
                Assert.AreEqual(node.CurrentState, State.INACTIVE);
            }
#endif
            // reset value to default
            m_runningCount = 0;
            m_mainTaskResult = null;
            m_bgTreeResult = null;
            foreach (var node in m_children)
            {
                m_runningCount++;
                node.Start();
            }
        }

        protected override void InternalCancel()
        {
            foreach (var node in m_children)
            {
                if (node.IsActive) node.Cancel();
            }
        }

        protected override void InternalChildStopped(Node child, bool success)
        {
            if (m_runningCount == m_children.Length)
            {
                if (m_finishMode == FinishMode.IMMEDIATE)
                {
                    // main task node is index of 0
                    if (child == m_children[0])
                    {
                        // cancel bgTree
                        if (m_children[1].IsActive) m_children[1].Cancel();
                        Stopped(success);
                    }
                }
                else
                {
                    if (child == m_children[0])
                    {
                        m_mainTaskResult = success;
                    }
                    // bgTree
                    else if (child == m_children[1])
                    {
                        m_bgTreeResult = success;
                    }

                    if (m_bgTreeResult.HasValue && m_mainTaskResult.HasValue)
                    {
                        Stopped(m_mainTaskResult.Value);
                    }
                }
            }
        }

        public override void AbortTreeNode(Node child)
        {
            // do nothing
        }


        [System.Obsolete("obsolete")]
        public override void StopLowerPriorityChildrenForChild(Node child, bool immediateRestart)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return string.Format("{0}", /*base.ToString()*/"<b> ↓↓ </b>");
        }
    }
}
