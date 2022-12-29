using System;
using System.Collections.Generic;
using System.Linq;
using Saro.Utility;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Saro.BT.Designer
{
    /*
     *      serializeGraphElements = SerializeGraphElementsImplementation;
     *      canPasteSerializedData = CanPasteSerializedDataImplementation;
     *      unserializeAndPaste = UnserializeAndPasteImplementation;
     *      deleteSelection = DeleteSelectionImplementation;
     *      elementsInsertedToStackNode = ElementsInsertedToStackNode;
     *      RegisterCallback<DragUpdatedEvent>(OnDragUpdatedEvent);
     *      RegisterCallback<DragPerformEvent>(OnDragPerformEvent);
     *      RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
     *
     *      // TODO
     *      2. 需要实时刷新 preOrder
     */
    public class BTGraphView : AbstractGraphView
    {
        private readonly struct EdgePair
        {
            public readonly BTNode NodeBehavior;
            public readonly Port ParentOutputPort;

            public EdgePair(BTNode nodeBehavior, Port parentOutputPort)
            {
                NodeBehavior = nodeBehavior;
                ParentOutputPort = parentOutputPort;
            }
        }

        internal BehaviorTree Tree { get; private set; }

        internal BTRootNode Root { get; private set; }

        public Action<BTGraphNode> OnNodeSelection { get; set; }

        public Action<BTGraphNode, ContextualMenuPopulateEvent> OnNodeChangeRequest { get; set; }
        public Action<BTGraphNode, ContextualMenuPopulateEvent> OnNodeDecorateRequest { get; set; }
        internal EdgeConnectorListener EdgeConnectorListener { get; set; }

        public BTGraphView() : this(null)
        { }

        public BTGraphView(BehaviorTree tree)
        {
            var miniMap = new BTGraphMiniMap(this) { anchored = true };
            Add(miniMap);
            miniMap.SetPosition(new Rect(0, 0, 100, 30f));

            serializeGraphElements = SerializeGraphElementsImplementation;
            //canPasteSerializedData = CanPasteSerializedDataImplementation;
            unserializeAndPaste = UnserializeAndPasteImplementation;

            OnNodeSelection += HighlightAbortNodes;

            UpdateView(tree);

            //graphViewChanged += OnGraphChange; // 需要参考shadergraph
        }

        public void OnBehaviorTreeChanged(BehaviorTree tree)
        {
            ClearNodesAndEdges();

            if (tree == null)
            {
                Tree = null;
                return;
            }

            UpdateView(tree);
        }

        private void UpdateView(BehaviorTree tree)
        {
            if (tree == null) return;

            Tree = tree;

            UpdateViewTransform(tree.graphPosition, tree.graphScale);

            ConstructTree();

            UpdatePreOrder();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            var remainTargets = evt.menu.MenuItems().FindAll(e =>
            {
                switch (e)
                {
                    case BTGraphDropdownMenuAction: return true;
                    //case DropdownMenuSeparator: return true;
                    case DropdownMenuAction a: return a.name == "Create Node";
                    default: return false;
                }
            });

            //Remove needless default actions .
            evt.menu.MenuItems().Clear();
            remainTargets.ForEach(evt.menu.MenuItems().Add);

            if (evt.target is GraphView || evt.target is Node || evt.target is Group)
            {
                evt.menu.AppendAction("Cut", delegate
                {
                    CutSelectionCallback();
                }, (DropdownMenuAction a) => !Application.isPlaying && canCutSelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }

            if (evt.target is GraphView || evt.target is Node || evt.target is Group)
            {
                evt.menu.AppendAction("Copy", delegate
                {
                    CopySelectionCallback();
                }, (DropdownMenuAction a) => !Application.isPlaying && canCopySelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }

            if (evt.target is GraphView)
            {
                evt.menu.AppendAction("Paste", delegate
                {
                    PasteCallback();
                }, (DropdownMenuAction a) => !Application.isPlaying && canPaste ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }

            if (evt.target is GraphView || evt.target is Node || evt.target is Group || evt.target is Edge)
            {
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("Delete", delegate
                {
                    DeleteSelectionCallback(AskUser.DontAskUser);
                }, (DropdownMenuAction a) => !Application.isPlaying && canDeleteSelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }

            if (evt.target is GraphView || evt.target is Node || evt.target is Group)
            {
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("Duplicate", delegate
                {
                    DuplicateSelectionCallback();
                }, (DropdownMenuAction a) => !Application.isPlaying && canDuplicateSelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                evt.menu.AppendSeparator();
            }
        }

        public override List<Port> GetCompatiblePorts(Port startAnchor, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            foreach (var port in ports.ToList())
            {
                if (startAnchor.node == port.node ||
                    startAnchor.direction == port.direction ||
                    startAnchor.portType != port.portType)
                {
                    continue;
                }

                compatiblePorts.Add(port);
            }
            return compatiblePorts;
        }

        private void ConstructTree()
        {
            var stack = new Stack<EdgePair>();

            var dummpNode = new Root();
            dummpNode.Child = Tree.Root;
            stack.Push(new EdgePair(dummpNode, null));

            var timeout = Time.realtimeSinceStartup + 10f;

            while (stack.Count > 0)
            {
                if (Time.realtimeSinceStartup > timeout)
                {
                    Debug.LogError("CustructTree failed. timeout");
                    break;
                }

                // create node
                var edgePair = stack.Pop();
                if (edgePair.NodeBehavior == null)
                {
                    continue;
                }

                var node = BTGraphNodeFactory.CreateGraphNode(edgePair.NodeBehavior.GetType(), this, edgePair.NodeBehavior);
                AddElement(node);
                node.SetPosition(edgePair.NodeBehavior.nodePosition);

                // connect parent
                if (edgePair.ParentOutputPort != null)
                {
                    var edge = ConnectPorts(edgePair.ParentOutputPort, node.ParentPort);
                    AddElement(edge);
                }

                // seek child
                switch (edgePair.NodeBehavior)
                {
                    case Root nb:
                        {
                            Root = node as BTRootNode;
                            if (nb.Child != null)
                            {
                                stack.Push(new EdgePair(nb.Child, Root.ChildPort));
                            }
                        }
                        break;
                    case SimpleParallel nb:
                        {
                            var simpleNode = node as BTSimpleParallelNode;
                            stack.Push(new EdgePair(nb.GetChildAt(0), simpleNode.ChildPort));
                            stack.Push(new EdgePair(nb.GetChildAt(1), simpleNode.BranchPort));
                        }
                        break;
                    case BTComposite nb:
                        {
                            var compositeNode = node as BTCompositeNode;
                            for (var i = 0; i < nb.ChildCount(); i++)
                            {
                                stack.Push(new EdgePair(nb.GetChildAt(i), compositeNode.ChildPort));
                            }
                            break;
                        }
                    case BTAuxiliary nb:
                        {
                            var decoratorNode = node as BTAuxiliaryNode;
                            stack.Push(new EdgePair(nb.Child, decoratorNode.ChildPort));
                            break;
                        }
                }
            }
        }

        public string Save()
        {
            if (Application.isPlaying)
            {
                return "Only save in editor mode";
            }

            if (Tree == null)
            {
                return "BehaviorTree is null";
            }

            var result = Validate();

            if (string.IsNullOrEmpty(result))
            {
                Commit();
            }

            return result;
        }

        public void Format()
        {
            BTGraphFormatter.PositionNodesNicely(Root, Root.GetPosition().center);
        }

        private string Validate()
        {
            //validate nodes by DFS.
            var stack = new Stack<BTGraphNode>();
            stack.Push(Root);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                var result = node.Validate(stack);
                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }
            }
            return null;
        }

        private void Commit()
        {
            // 设置 Tree.nodes
            Root.PreCommit(Tree);

            var stack = new Stack<BTGraphNode>();
            stack.Push(Root);

            while (stack.Count > 0)
            {
                var node = stack.Pop();
                node.Commit(stack);
            }

            EditorUtility.SetDirty(Tree);

            AssetDatabase.SaveAssetIfDirty(Tree);
        }

        public static Edge ConnectPorts(Port output, Port input)
        {
            var tempEdge = new Edge
            {
                output = output,
                input = input
            };
            tempEdge.input.Connect(tempEdge);
            tempEdge.output.Connect(tempEdge);
            return tempEdge;
        }

        public static void DisconnectPorts(Edge edge)
        {
            edge.input.Disconnect(edge);
            edge.output.Disconnect(edge);
        }

        #region Nodes

        internal void UpdatePreOrder()
        {
            var preOrder = 0;
            foreach (var node in TreeTraversal.PreOrder<BTGraphNode>(Root).Skip(1))
            {
                node.RefreshPreOrder(preOrder++);
            }
        }

        private void HighlightAbortNodes(BTGraphNode selectionNode)
        {
            if (Application.isPlaying) return; // 运行时，不要处理

            if (selectionNode is BTDecoratorNode decoratorNode)
            {
                if (TryGetCompositeParent(decoratorNode, out var compositeParent))
                {
                    var decorator = decoratorNode.NodeBehavior as BTDecorator;
                    ref var abortType = ref decorator.abortType;

                    if (compositeParent.NodeBehavior is Sequence)
                    {
                        if (abortType == EAbortType.Both) abortType = EAbortType.Self;
                        else if (abortType == EAbortType.LowerPriority) abortType = EAbortType.None;
                    }
                    else if (compositeParent.NodeBehavior is SimpleParallel)
                    {
                        abortType = EAbortType.None;
                    }

                    if (abortType == EAbortType.Self || abortType == EAbortType.Both)
                    {
                        foreach (var node in TreeTraversal.PreOrder<BTGraphNode>(decoratorNode))
                        {
                            if (node == null) continue;
                            node.SetBorderColor(Color.red);
                        }
                    }

                    if (abortType == EAbortType.LowerPriority || abortType == EAbortType.Both)
                    {
                        var abortIndex = false;
                        for (int i = 0; i < compositeParent.ChildCount(); i++)
                        {
                            var child = compositeParent.GetChildAt(i);
                            if (child == decoratorNode)
                            {
                                abortIndex = true;
                                continue;
                            }

                            if (abortIndex)
                            {
                                foreach (var node in TreeTraversal.PreOrder<BTGraphNode>(child))
                                {
                                    if (node == null) continue;
                                    node.SetBorderColor(Color.blue);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var node in TreeTraversal.PreOrder<BTGraphNode>(Root))
                {
                    if (node == null) continue;
                    node.SetBorderColor(Color.clear);
                }
            }
        }

        private bool TryGetCompositeParent(BTGraphNode child, out BTCompositeNode compositeParent)
        {
            var parent = child.ParentNode;

            while (parent != null && parent is not BTCompositeNode)
            {
                parent = parent.ParentNode;
            }

            compositeParent = parent as BTCompositeNode;

            return compositeParent != null;
        }

        #endregion

        #region Copy and Paste

        private string SerializeGraphElementsImplementation(IEnumerable<GraphElement> elements)
        {
            var graphNodes = elements.OfType<BTGraphNode>().Where(n => n is not BTRootNode).ToList();

            var copyData = new BTCopyPasteData();

            var nodes = copyData.nodes;
            var edges = copyData.edges;
            var nodePosition = copyData.nodesPosition;

            foreach (var graphNode in graphNodes)
            {
                nodes.Add(graphNode.NodeBehavior);
                nodePosition.Add(graphNode.GetPosition());
                var childCount = graphNode.ChildCount();
                for (int k = 0; k < childCount; k++)
                {
                    var child = graphNode.GetChildAt(k);
                    var childIndex = graphNodes.IndexOf(child);
                    if (childIndex >= 0)
                        edges.Add((graphNodes.IndexOf(graphNode), childIndex));
                }
            }
            if (nodes.Count > 0)
            {
                return JsonHelper.ToJson(copyData);
            }

            return null;
        }

        // 默认实现就是这个
        //private bool CanPasteSerializedDataImplementation(string data)
        //{
        //    return data.Contains("application/vnd.unity.graphview.elements ");
        //}

        private void UnserializeAndPasteImplementation(string operationName, string data)
        {
            var copyData = JsonHelper.FromJson<BTCopyPasteData>(data);

            var nodes = copyData.nodes;
            var edges = copyData.edges;
            var nodesPosition = copyData.nodesPosition;

            if (nodes.Count > 0)
            {
                ClearSelection();

                var graphNodes = new List<BTGraphNode>(nodes.Count);
                for (int i = 0; i < nodes.Count; i++)
                {
                    BTNode node = nodes[i];
                    var newNode = BTGraphNodeFactory.CreateGraphNode(node.GetType(), this, node);
                    var nodePosition = node.nodePosition = nodesPosition[i];
                    nodePosition.x += 200;
                    nodePosition.y += 200;
                    newNode.SetPosition(nodePosition);
                    AddElement(newNode);

                    graphNodes.Add(newNode);

                    AddToSelection(newNode);
                }

                foreach (var (indexA, indexB) in edges)
                {
                    var nodeA = graphNodes[indexA];
                    var nodeB = graphNodes[indexB];

                    if (nodeA is BTSimpleParallelNode simpleParallelNode)
                    {
                        Edge newEdge;
                        if (!simpleParallelNode.ChildPort.connected)
                            newEdge = BTGraphView.ConnectPorts(simpleParallelNode.ChildPort, nodeB.ParentPort);
                        else
                            newEdge = BTGraphView.ConnectPorts(simpleParallelNode.BranchPort, nodeB.ParentPort);
                        AddElement(newEdge);
                    }
                    else if (nodeA is BTCompositeNode compositeNode)
                    {
                        var newEdge = BTGraphView.ConnectPorts(compositeNode.ChildPort, nodeB.ParentPort);
                        AddElement(newEdge);
                    }
                    else if (nodeA is BTAuxiliaryNode auxiliaryNode)
                    {
                        var newEdge = BTGraphView.ConnectPorts(auxiliaryNode.ChildPort, nodeB.ParentPort);
                        AddElement(newEdge);
                    }
                }
            }
        }

        #endregion
    }
}