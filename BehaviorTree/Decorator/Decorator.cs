
namespace Saro.BT
{
    public abstract class Decorator : Container
    {
        protected Node m_decorated;

        public Decorator(string name, Node decorated) : base(name)
        {
            m_decorated = decorated;
            m_decorated.SetParent(this);
            //UnityEngine.Debug.Log("decorator");
        }

        public override void SetRoot(Root rootNode)
        {
            base.SetRoot(rootNode);
            m_decorated.SetRoot(rootNode);
        }

        public override void ParentCompositeStopped(Composite composite)
        {
            base.ParentCompositeStopped(composite);
            m_decorated.ParentCompositeStopped(composite);
        }

#if UNITY_EDITOR
        public override Node[] DebugChildren => new Node[] { m_decorated };
#endif
    }
}