using System;
using System.Collections.Generic;
using System.Linq;
using Saro.Pool;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Saro.BT.Designer
{
    public class BTCompositeNode : BTGraphNode
    {
        public Port ChildPort { protected set; get; }

        protected List<BTGraphNode> m_Cache = new List<BTGraphNode>();

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            evt.menu.MenuItems().Add(new BTGraphDropdownMenuAction("Change", (a) =>
            {
                GraphView.OnNodeChangeRequest?.Invoke(this, evt);
            }, (DropdownMenuAction a) => !Application.isPlaying ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled));
        }

        public BTCompositeNode()
        {
            AddChild();
        }

        protected virtual void AddChild()
        {
            ChildPort = CreateChildPort();
            outputContainer.Add(ChildPort);
        }

        public override void PostInit(BTGraphView graphView)
        {
            base.PostInit(graphView);

            if (graphView.EdgeConnectorListener != null)
            {
                ChildPort.AddManipulator(new EdgeConnector<Edge>(graphView.EdgeConnectorListener));
            }
        }

        protected override Port CreateChildPort()
        {
            var port = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Port));
            port.portName = "Child";
            return port;
        }

        protected override string OnValidate(Stack<BTGraphNode> stack)
        {
            //if (ChildPort.Count <= 0) return false;
            var connCount = ChildPort.connections.Count();
            if (connCount <= 0)
            {
                ChildPort.style.backgroundColor = Color.red;

                return $"{NodeBehavior.Title}'s child count <= 0";
            }

            foreach (var conn in ChildPort.connections)
            {
                var graphNode = conn.input.node as BTGraphNode;
                graphNode.NodeBehavior.Parent = NodeBehavior;
                stack.Push(graphNode);
            }

            ChildPort.style.backgroundColor = new StyleColor(StyleKeyword.Null);

            return null;
        }

        protected override void OnCommit(Stack<BTGraphNode> stack)
        {
            m_Cache.Clear();

            var children = new List<BTNode>();

            for (int i = 0; i < ChildCount(); i++)
            {
                var child = GetChildAt(i);

                children.Add(child.NodeBehavior);

                stack.Push(child);
                m_Cache.Add(child);
            }

            (NodeBehavior as BTComposite).SetChildren(children.ToArray());
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
            // ChildPort.connections 使用的是无序容器 HashSet，需要排序

            if (ChildPort.connected)
            {
                using (ListPool<BTGraphNode>.Rent(out var nodes))
                {
                    foreach (var connection in ChildPort.connections)
                    {
                        if (connection.input.node is BTGraphNode graphNode)
                        {
                            nodes.Add(graphNode);
                        }
                    }

                    //nodes.Sort((up, down) => up.GetPosition().y.CompareTo(down.GetPosition().y));
                    nodes.Sort((up, down) => up.style.top.value.value.CompareTo(down.style.top.value.value)); // GetPosition 的值不一定在这一帧更新，所以使用 style.top

                    //Debug.LogError($"{NodeBehavior.Title}'s children:{ string.Join(", ", nodes)}");

                    return nodes[index];
                }
            }

            return null;
        }

        public override int ChildCount() => ChildPort.connections.Count();
    }
}