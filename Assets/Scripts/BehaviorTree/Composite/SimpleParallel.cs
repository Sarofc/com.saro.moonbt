using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Assertions;

namespace Saro.BT
{
    /*
     allow only two children : 
     main task : one which must be a single task node(with optional decorators).
     background tree : and the other of which can be a complete subtree.
     retun main task result
    */

    /// <summary>
    /// 
    /// </summary>
    public class SimpleParallel : Composite
    {
        public enum FinishMode
        {
            IMMEDIATE,  //once the main task finishes, the background tree will be aborted
            DELAYED     //once the main task has finished, the background tree will be permitted to finish
        }

        // don't allow aborting lower priorities & self, doesn't makes sense
        public override bool CanAbortLowerPriority => false;
        public override bool CanAbortSelf => false;

        private FinishMode m_finishMode;
        private int m_runningCount;
        private bool? m_mainTaskResult;
        private bool? m_bgTreeResult;

        public SimpleParallel() : base("SimpleParallel")
        {
            m_finishMode = FinishMode.IMMEDIATE;
        }

        public SimpleParallel(FinishMode finishMode) : base("SimpleParallel")
        {
            m_finishMode = finishMode;
        }

        /*
         * SimpleParallel Node is special, allow only two nodes
         */
        public Composite OpenBranch(Node mainTask, Node subTree = null)
        {
            if (subTree != null)
                return base.InternalOpenBranch(mainTask, subTree);
            return base.InternalOpenBranch(mainTask);
        }

        protected override void InternalStart()
        {
#if UNITY_EDITOR
            foreach (var node in m_children)
            {
                Assert.AreEqual(node.CurrentStatus, NodeStatus.Inactive);
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

        protected override void InternalAbort()
        {
            foreach (var node in m_children)
            {
                if (node.IsActive) node.Abort();
            }
        }

        protected override void InternalChildStopped(Node child, bool? result)
        {
            if (m_runningCount == m_children.Length)
            {
                if (m_finishMode == FinishMode.IMMEDIATE)
                {
                    // main task node is index of 0
                    if (child == m_children[0])
                    {
                        // cancel bgTree
                        if (m_children[1].IsActive) m_children[1].Abort();
                        Stopped(result);
                    }
                }
                else
                {
                    if (child == m_children[0])
                    {
                        m_mainTaskResult = result;
                    }
                    // bgTree
                    else if (child == m_children[1])
                    {
                        m_bgTreeResult = result;
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

        public override string GetStaticDescription()
        {
            return string.Format("<b> ↓↓ </b>({0})", m_finishMode);
        }
    }
}
