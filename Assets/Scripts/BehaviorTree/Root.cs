using System;
using System.Text;
using UnityEngine.Assertions;

namespace Saro.BT
{
    public class Root : AuxiliaryNode
    {
        public bool RepeatRoot { get => m_repeatRoot; set => m_repeatRoot = value; }
        private bool m_repeatRoot = true;

        public event Action OnBTStopped;

        public override Blackboard Blackboard => m_blackboard;
        private Blackboard m_blackboard;

        public override Clock Clock => m_clock;


        private Clock m_clock;


        public Root() : base("Root")
        {
            m_clock = UBTContext.Instance.GetClock();
        }

        public Root(Blackboard blackboard) : base("Root")
        {
            //m_mainNode = mainNode;
            m_clock = UBTContext.Instance.GetClock();
            m_blackboard = blackboard;

            //if (subTree) SetRoot(RootNode);
            //else SetRoot(this);
        }

        public Root(Blackboard blackboard, Clock clock) : base("Root")
        {
            //m_mainNode = mainNode;
            m_clock = clock;
            m_blackboard = blackboard;

            //if (subTree) SetRoot(RootNode);
            //else SetRoot(this);
        }

        public new Root Decorate(Node compositeNode)
        {
            m_childNode = compositeNode;
            m_childNode.SetParent(this);

            SetRoot(this);

            return this;
        }

        //public override void SetRoot(Root rootNode)
        //{
        //    if (rootNode == null) return;

        //    base.SetRoot(rootNode);
        //    m_decorated.SetRoot(rootNode);
        //}

        protected override void InternalStart()
        {
            // blackboard can be null
            if (m_blackboard != null) m_blackboard.Enable();
            m_childNode.Start();
        }

        protected override void InternalAbort()
        {
            if (m_childNode.IsActive)
            {
                m_childNode.Abort();
            }
            else
            {
                m_clock.RemoveTimer(m_childNode.Start);
            }
        }

        protected override void InternalChildStopped(Node child, bool? result)
        {
            if (!IsAborted && m_repeatRoot)
            {
                m_clock.AddTimer(0, 0, m_childNode.Start);
            }
            else
            {
                if (m_blackboard != null) m_blackboard.Disable();
                Stopped(result);

                OnBTStopped?.Invoke();
                OnBTStopped = null;
            }
        }

        public override string GetStaticDescription()
        {
            var des = new StringBuilder(10);

            des.Append(Name);
            des.AppendLine();
            if (m_blackboard != null) des.Append(m_blackboard.GetStaticDescription());

            return des.ToString();
        }

#if UNITY_EDITOR
        public override Node[] DebugChildren => new Node[] { m_childNode };
#endif
    }
}