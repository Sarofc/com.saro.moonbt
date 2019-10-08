using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saro.BT
{
   /*
    * 
    *
    */
    public abstract class AuxiliaryNode : Container
    {
        protected Node m_childNode;

        public AuxiliaryNode(string name) : base(name)
        {
        }

        public Node Decorate(Node decorated)
        {
            m_childNode = decorated;
            m_childNode.SetParent(this);
            return this;
        }

        public override void SetRoot(Root rootNode)
        {
            base.SetRoot(rootNode);
            m_childNode.SetRoot(rootNode);
        }

        public override void ParentCompositeStopped(Composite composite)
        {
            base.ParentCompositeStopped(composite);
            m_childNode.ParentCompositeStopped(composite);
        }

#if UNITY_EDITOR
        public override Node[] DebugChildren => new Node[] { m_childNode };
#endif

    }
}
