using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System;
using UnityEngine.UIElements;

namespace Saro.BT.Designer
{
    public sealed class BTRootNode : BTGraphNode
    {
        public readonly Port ChildPort;

        private BTGraphNode m_Cache;

        public BTRootNode()
        {
            ChildPort = CreateChildPort();
            outputContainer.Add(ChildPort);

            capabilities &= ~Capabilities.Movable;
            capabilities &= ~Capabilities.Deletable;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
        }

        protected override void AddParent()
        {
        }

        protected override void OnSetBehavior()
        {
            (NodeBehavior as Root).UpdateEditor = ClearStyle;
        }

        public override void PostInit(BTGraphView graphView)
        {
            base.PostInit(graphView);

            if (graphView.EdgeConnectorListener != null)
            {
                ChildPort.AddManipulator(new EdgeConnector<Edge>(graphView.EdgeConnectorListener));
            }
        }

        protected override string OnValidate(Stack<BTGraphNode> stack)
        {
            if (!ChildPort.connected)
            {
                ChildPort.style.backgroundColor = Color.red;

                return $"Root's child is null";
            }
            stack.Push(ChildPort.connections.First().input.node as BTGraphNode);

            ChildPort.style.backgroundColor = new StyleColor(StyleKeyword.Null);

            return null;
        }

        protected override void OnCommit(Stack<BTGraphNode> stack)
        {
            var child = ChildPort.connections.First().input.node as BTGraphNode;
            var newRoot = new Root();
            newRoot.Child = child.NodeBehavior;
            newRoot.UpdateEditor = ClearStyle;
            NodeBehavior = newRoot;
            stack.Push(child);
            m_Cache = child;
        }

        public void PreCommit(BehaviorTree tree)
        {
            var child = ChildPort.connections.First().input.node as BTGraphNode;

            var editorNodes = TreeTraversal.PreOrder(child).ToArray();
            var nodes = new List<BTNode>();
            //var order = 0;
            foreach (var node in editorNodes)
            {
                nodes.Add(node.NodeBehavior);
                //node.NodeBehavior.preOrder = order;
                //node.RefreshPreOrder(order);
                //order++;
            }

            tree.graphPosition = GraphView.viewTransform.position;
            tree.graphScale = GraphView.viewTransform.scale;
            tree.SetNodes(nodes);
        }

        protected override void OnClearStyle()
        {
            m_Cache?.ClearStyle();
        }

        public override BTGraphNode GetChildAt(int index)
        {
            if (!ChildPort.connected) return null;
            return ChildPort.connections.First().input.node as BTGraphNode;
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

    /// <summary>
    /// dummy node for editor
    /// </summary>
    [BTNode("Run_Behaviour_24x")]
    internal sealed class Root : BTNode
    {
        private BTNode m_Child;

        public Action UpdateEditor;

        public BTNode Child
        {
            get => m_Child;
            set => m_Child = value;
        }

        public Root()
        {
            nodePosition = new Rect(100, 200, 0, 0);
        }

        public override EStatus OnExecute()
        {
            throw new NotImplementedException();
        }

        public override BTNode GetChildAt(int _ = 0)
        {
            return m_Child;
        }

        public override int ChildCount() => m_Child == null ? 0 : 1;

        public override int MaxChildCount() => 1;

        public override void SetChildAt(BTNode node, int _ = 0)
        {
            m_Child = node;
        }
    }
}
