
using System;
using System.Text;
using UnityEngine.Assertions;

namespace Saro.BT
{
    public abstract class Node
    {
        public enum NodeStatus
        {
            Active, // running, but do not return result in running time.
            Aborting,
            Inactive, // stopped, then return a result, successed or failed when stopped.
        }

        public Root RootNode { get; set; }

        public NodeStatus CurrentStatus { get => m_currentStatus; }


        public Container ParentContainerNode { get => m_parentContainerNode; }

        /// <summary>
        /// User label
        /// </summary>
        [Obsolete("User Label, TODO")]
        public string Label { get => m_label; set => m_label = value; }
        /// <summary>
        /// Node name
        /// </summary>
        public string Name { get => m_name; set => m_name = value; }


        /// <summary>
        /// use the 'Abort()' method, will change the Node State to 'State.Aborting'
        /// </summary>
        public bool IsAborted { get => m_currentStatus == NodeStatus.Aborting; }
        /// <summary>
        /// running state
        /// </summary>
        public bool IsActive { get => m_currentStatus == NodeStatus.Active; }

        public virtual Blackboard Blackboard { get => RootNode.Blackboard; }

        public virtual Clock Clock { get => RootNode.Clock; }

        protected NodeStatus m_currentStatus = NodeStatus.Inactive;
        private Container m_parentContainerNode;
        private string m_label;
        private string m_name;


        public Node(string name)
        {
            m_name = name;
        }

        public virtual void SetRoot(Root rootNode)
        {
            RootNode = rootNode;
        }

        public void SetParent(Container parent)
        {
            m_parentContainerNode = parent;
        }

        // debug
#if UNITY_EDITOR
        //public float debugLastStopRequestAt = 0f;
        //public float debugLastStoppedAt = 0f;
        //public int debugNumStartCalls = 0;
        //public int debugNumStopCalls = 0;
        //public int debugNumStoppedCalls = 0;
        public bool? debugLastResult = null;
#endif

        public void Start()
        {
            Assert.AreEqual(m_currentStatus, NodeStatus.Inactive, "can only start inactive node");
            m_currentStatus = NodeStatus.Active;

            InternalStart();
        }


        public void Abort()
        {
            Assert.AreEqual(m_currentStatus, NodeStatus.Active, "can only stop active node");
            m_currentStatus = NodeStatus.Aborting;

            InternalAbort();
        }

        /* 
         * Should call this method when a node is stopped.
         * Make sure that BT can stop child node.
         
         * result of execution
         * null     : InProcessing
         * false    : Failed
         * true     : Succeeded
         */
        protected virtual void Stopped(bool? result)
        {
            Assert.AreNotEqual(m_currentStatus, NodeStatus.Inactive, "Called 'Stopped' while in state INACTIVE, something is wrong!");
            m_currentStatus = NodeStatus.Inactive;

#if UNITY_EDITOR
            debugLastResult = result;
#endif

            m_parentContainerNode?.ChildStopped(this, result);
        }

        public virtual void ParentCompositeStopped(Composite composite)
        {
            InternalParentCompositeStopped(composite);
        }


        protected abstract void InternalStart();

        protected abstract void InternalAbort();

        /// <summary>
        /// Be carefull with this! In most case, do not override this!
        /// This is called in Stopped() method. 
        /// </summary>
        /// <param name="composite"></param>
        protected virtual void InternalParentCompositeStopped(Composite composite)
        {
            // Do nothing in base class
        }

        protected string GetPath()
        {
            if (m_parentContainerNode != null)
            {
                return string.Format("{0}/{1}", m_parentContainerNode.GetPath(), m_name);
            }
            return m_name;
        }

        /// <summary>
        /// static description, call once
        /// </summary>
        /// <returns>static string</returns>
        public virtual string GetStaticDescription() { return Name; }

        /// <summary>
        /// runtime values, need to update
        /// </summary>
        /// <param name="des"></param>
        /// <returns>runtime string</returns>
        public virtual string DescribeRuntimeValues(StringBuilder des) { return null; }

    }
}