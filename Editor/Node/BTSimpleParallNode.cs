using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Saro.BT.Designer
{
    public class BTSimpleParallelNode : BTCompositeNode
    {
        public Port BranchPort { get; private set; }

        protected override void AddChild()
        {
            ChildPort = CreateChildPort();
            ChildPort.portName = "Main";
            outputContainer.Add(ChildPort);

            BranchPort = CreateChildPort();
            BranchPort.portName = "Branch";
            outputContainer.Add(BranchPort);
        }

        protected override Port CreateChildPort()
        {
            var port = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(Port));
            port.portName = "Child";
            return port;
        }

        public override void PostInit(BTGraphView graphView)
        {
            base.PostInit(graphView);

            if (graphView.EdgeConnectorListener != null)
            {
                BranchPort.AddManipulator(new EdgeConnector<Edge>(graphView.EdgeConnectorListener));
            }
        }

        protected override string OnValidate(Stack<BTGraphNode> stack)
        {
            if (!ChildPort.connected || !BranchPort.connected)
            {
                ChildPort.style.backgroundColor = ChildPort.connected ? new StyleColor(StyleKeyword.Null) : Color.red;
                BranchPort.style.backgroundColor = BranchPort.connected ? new StyleColor(StyleKeyword.Null) : Color.red;

                return $"{NodeBehavior.Title}'s main or branch is null";
            }

            foreach (var graphNode in TreeTraversal.PreOrder(GetChildAt(0)))
            {
                if (graphNode.NodeBehavior.IsComposite())
                {
                    style.backgroundColor = Color.red;

                    return $"{NodeBehavior.Title}'s main don't allowed to use Composite node";
                }
            }

            stack.Push(ChildPort.connections.First().input.node as BTGraphNode);
            stack.Push(BranchPort.connections.First().input.node as BTGraphNode);

            ChildPort.style.backgroundColor = new StyleColor(StyleKeyword.Null);
            BranchPort.style.backgroundColor = new StyleColor(StyleKeyword.Null);

            style.backgroundColor = new StyleColor(StyleKeyword.Null);

            return null;
        }

        protected override void OnCommit(Stack<BTGraphNode> stack)
        {
            m_Cache.Clear();

            var children = new List<BTNode>();

            {
                var child = ChildPort.connections.First().input.node as BTGraphNode;

                children.Add(child.NodeBehavior);

                stack.Push(child);
                m_Cache.Add(child);
            }

            {
                var child = BranchPort.connections.First().input.node as BTGraphNode;

                children.Add(child.NodeBehavior);

                stack.Push(child);
                m_Cache.Add(child);
            }

            (NodeBehavior as SimpleParallel).SetChildren(children.ToArray());
        }

        protected override void OnClearStyle()
        {
            foreach (var node in m_Cache)
            {
                node.ClearStyle();
            }
        }

        public override BTGraphNode GetChildAt(int index)
        {
            if (index == 0)
            {
                if (!ChildPort.connected) return null;
                return ChildPort.connections.First().input.node as BTGraphNode;
            }
            else if (index == 1)
            {
                if (!BranchPort.connected) return null;
                return BranchPort.connections.First().input.node as BTGraphNode;
            }

            throw new System.IndexOutOfRangeException("index: " + index);
        }

        public override int ChildCount() => 2;
    }
}