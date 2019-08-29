
using System;
using UnityEngine.Assertions;

namespace Saro.BT
{
    public abstract class Node
    {
        public enum State
        {
            INACTIVE, // stopped, then return a result, successed or failed when stopped.
            ACTIVE, // running, but do not return result in running time.
            CANCELLED, // canceled
        }

        public Root RootNode { get; set; }

        public State CurrentState { get => m_currentState; }


        public Container ParentContainerNode { get => m_parentContainerNode; }
        /// <summary>
        /// Node Label
        /// </summary>
        public string Label { get => m_label; set => m_label = value; }
        /// <summary>
        /// Node Name
        /// </summary>
        public string Name { get => m_name; set => m_name = value; }


        /// <summary>
        /// use the 'Cancel()' method, will change the Node State to 'State.CANCELLED'
        /// </summary>
        public bool IsCancelled { get => m_currentState == State.CANCELLED; }
        /// <summary>
        /// running state
        /// </summary>
        public bool IsActive { get => m_currentState == State.ACTIVE; }

        public virtual Blackboard Blackboard { get => RootNode.Blackboard; }

        public virtual Clock Clock { get => RootNode.Clock; }

        protected State m_currentState = State.INACTIVE;
        private Container m_parentContainerNode;
        private string m_label;
        private string m_name;


        public Node(string name)
        {
            this.m_name = name;
        }

        public virtual void SetRoot(Root rootNode)
        {
            this.RootNode = rootNode;
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
        public bool debugLastResult = false;
#endif

        public void Start()
        {
            Assert.AreEqual(m_currentState, State.INACTIVE, "can only start inactive node");
            m_currentState = State.ACTIVE;

            InternalStart();
        }


        public void Cancel()
        {
            Assert.AreEqual(m_currentState, State.ACTIVE, "can only stop active node");
            m_currentState = State.CANCELLED;

            InternalCancel();
        }

        /// <summary>
        /// This method has to be the last call in your function. Make sure that BT can stop parrent node.
        /// </summary>
        /// <param name="success">result of execution</param>
        protected virtual void Stopped(bool success)
        {
            Assert.AreNotEqual(m_currentState, State.INACTIVE, "Called 'Stopped' while in state INACTIVE, something is wrong!");
            m_currentState = State.INACTIVE;

#if UNITY_EDITOR
            //rootNode.totalNumStoppedCalls++;
            //debugNumStoppedCalls++;
            //debugLastStoppedAt = UnityEngine.Time.time;
            debugLastResult = success;
#endif

            m_parentContainerNode?.ChildStopped(this, success);
        }

        public virtual void ParentCompositeStopped(Composite composite)
        {
            InternalParentCompositeStopped(composite);
        }


        protected abstract void InternalStart();

        protected abstract void InternalCancel();

        /// <summary>
        /// Be carefull with this! In most case, do not override this!
        /// This is called in Stopped() method. 
        /// </summary>
        /// <param name="composite"></param>
        protected virtual void InternalParentCompositeStopped(Composite composite)
        {

        }

        protected string GetPath()
        {
            if (m_parentContainerNode != null)
            {
                return string.Format("{0}/{1}", m_parentContainerNode.GetPath(), m_name);
            }
            return m_name;
        }

        public override string ToString()
        {
            return !string.IsNullOrEmpty(m_label) ? string.Format("{0}[{1}]", m_name, m_label) : m_name;
        }
    }
}