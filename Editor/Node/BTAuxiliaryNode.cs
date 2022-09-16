using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Saro.BT.Designer
{
    public abstract class BTAuxiliaryNode : BTGraphNode
    {
        public Port ChildPort { protected set; get; }

        private BTGraphNode m_Cache;

        public BTAuxiliaryNode()
        {
            ChildPort = CreateChildPort();
            outputContainer.Add(ChildPort);
        }

        protected override string OnValidate(Stack<BTGraphNode> stack)
        {
            if (!ChildPort.connected)
            {
                ChildPort.style.backgroundColor = Color.red;

                return $"{NodeBehavior.Title}'s child is null";
            }

            var node = ChildPort.connections.First().input.node as BTGraphNode;
            node.NodeBehavior.Parent = NodeBehavior;
            stack.Push(node);

            ChildPort.style.backgroundColor = new StyleColor(StyleKeyword.Null);

            return null;
        }

        protected override void OnCommit(Stack<BTGraphNode> stack)
        {
            if (!ChildPort.connected)
            {
                (NodeBehavior as BTAuxiliary).Child = null;
                m_Cache = null;
                return;
            }
            var child = ChildPort.connections.First().input.node as BTGraphNode;
            (NodeBehavior as BTAuxiliary).Child = child.NodeBehavior;
            stack.Push(child);
            m_Cache = child;
        }

        public override void PostInit(BTGraphView graphView)
        {
            base.PostInit(graphView);

            if (graphView.EdgeConnectorListener != null)
            {
                ChildPort.AddManipulator(new EdgeConnector<Edge>(graphView.EdgeConnectorListener));
            }
        }

        protected override void OnClearStyle()
        {
            m_Cache?.ClearStyle();
        }

        public override BTGraphNode GetChildAt(int index)
        {
            if (ChildPort.connected)
                return ChildPort.connections.First().input.node as BTGraphNode;
            return null;
        }

        //public override void SetChildAt(BTGraphNode node, int childIndex)
        //{
        //    if (ChildPort.connected)
        //    {
        //        var oldEdge = ChildPort.connections.First();
        //        BTGraphView.DisconnectPorts(oldEdge);
        //        GraphView.RemoveElement(oldEdge);
        //    }

        //    var newEdge = BTGraphView.ConnectPorts(ChildPort, node.ParentPort);
        //    GraphView.AddElement(newEdge);
        //}

        public override int ChildCount() => 1;
    }
}