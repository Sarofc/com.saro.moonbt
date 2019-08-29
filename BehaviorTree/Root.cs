using UnityEngine.Assertions;

namespace Saro.BT
{
    public class Root : Decorator
    {
        public bool RepeatWhenFinished { get => m_repeatWhenFinished; set => m_repeatWhenFinished = value; }
        private bool m_repeatWhenFinished = true;

        public override Blackboard Blackboard => m_blackboard;
        private Blackboard m_blackboard;

        public override Clock Clock => m_clock;


        private Clock m_clock;

        private Node m_mainNode;

        public Root(Node mainNode, bool subTree = false) : base("Root", mainNode)
        {
            this.m_mainNode = mainNode;
            m_clock = UBTContext.Instance.GetClock();
            m_blackboard = new Blackboard(m_clock);

            if (subTree) SetRoot(RootNode);
            else SetRoot(this);
        }

        public Root(Blackboard blackboard, Node mainNode, bool subTree = false) : base("Root", mainNode)
        {
            this.m_mainNode = mainNode;
            m_clock = UBTContext.Instance.GetClock();
            m_blackboard = blackboard;

            if (subTree) SetRoot(RootNode);
            else SetRoot(this);
        }

        public Root(Blackboard blackboard, Clock clock, Node mainNode, bool subTree = false) : base("Root", mainNode)
        {
            this.m_mainNode = mainNode;
            this.m_clock = clock;
            this.m_blackboard = blackboard;

            if (subTree) SetRoot(RootNode);
            else SetRoot(this);
        }

        public override void SetRoot(Root rootNode)
        {
            if (rootNode == null) return;

            base.SetRoot(rootNode);
            m_mainNode.SetRoot(rootNode);
        }

        protected override void InternalStart()
        {
            m_blackboard.Enable();
            m_mainNode.Start();
        }

        protected override void InternalCancel()
        {
            if (m_mainNode.IsActive)
            {
                m_mainNode.Cancel();
            }
            else
            {
                m_clock.RemoveTimer(m_mainNode.Start);
            }
        }

        protected override void InternalChildStopped(Node child, bool success)
        {
            if (!IsCancelled && m_repeatWhenFinished)
            {
                m_clock.AddTimer(0, 0, m_mainNode.Start);
            }
            else
            {
                m_blackboard.Disable();
                Stopped(success);
            }
        }
    }
}