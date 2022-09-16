using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Saro.BT.Designer
{
    public class DecorateBTNodeProvider : ScriptableObject, ISearchWindowProvider
    {
        private BTGraphNode m_Node;
        private List<SearchTreeEntry> m_Entries;
        private EditorWindow m_GraphEditor;

        public DecorateBTNodeProvider(EditorWindow graphEditor)
        {
            m_GraphEditor = graphEditor;

            m_Entries = new List<SearchTreeEntry>();
            m_Entries.Add(new SearchTreeGroupEntry(new GUIContent("Select Auxiliary")));

            var decoratorGroup = new List<SearchTreeEntry>();
            var serviceGroup = new List<SearchTreeEntry>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!type.IsAbstract)
                    {
                        if (type.IsSubclassOf(typeof(BTDecorator)))
                            decoratorGroup.Add(new SearchTreeEntry(new GUIContent(type.Name)) { level = 2, userData = type });
                        else if (type.IsSubclassOf(typeof(BTService)))
                            serviceGroup.Add(new SearchTreeEntry(new GUIContent(type.Name)) { level = 2, userData = type });
                    }
                }
            }

            m_Entries.Add(new SearchTreeGroupEntry(new GUIContent("BTDecorator"), 1));
            m_Entries.AddRange(decoratorGroup);

            m_Entries.Add(new SearchTreeGroupEntry(new GUIContent("BTService"), 1));
            m_Entries.AddRange(serviceGroup);
        }

        public void Show(BTGraphNode node, ContextualMenuPopulateEvent evt)
        {
            m_Node = node;
            SearchWindow.Open(new SearchWindowContext(GetScreenMousePosition(evt)), this);
        }

        private Vector2 GetScreenMousePosition(ContextualMenuPopulateEvent evt) => m_GraphEditor.position.position + evt.mousePosition;

        List<SearchTreeEntry> ISearchWindowProvider.CreateSearchTree(SearchWindowContext context)
        {
            return m_Entries;
        }

        bool ISearchWindowProvider.OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var type = searchTreeEntry.userData as Type;

            var graphView = m_Node.GraphView;

            var newNode = BTGraphNodeFactory.CreateGraphNode(type, graphView) as BTAuxiliaryNode;

            var nodePosition = m_Node.GetPosition();
            nodePosition.x -= nodePosition.width + 30f;
            nodePosition.y -= 30f;
            newNode.SetPosition(nodePosition);

            graphView.AddElement(newNode);

            if (m_Node.ParentPort.connected)
            {
                var oldEdge = m_Node.ParentPort.connections.First();
                var parentNode = oldEdge.output.node;

                BTGraphView.DisconnectPorts(oldEdge);
                graphView.RemoveElement(oldEdge);

                switch (parentNode)
                {
                    case BTCompositeNode compositeNode:
                        {
                            //var port = compositeNode.ChildPort.Find(p => p == node.ParentPort);
                            var port = compositeNode.ChildPort;

                            var edge1 = BTGraphView.ConnectPorts(port, newNode.ParentPort);
                            graphView.AddElement(edge1);
                        }
                        break;
                    case BTAuxiliaryNode auxiliaryNode:
                        {
                            var port = auxiliaryNode.ChildPort;

                            var edge = BTGraphView.ConnectPorts(port, newNode.ParentPort);
                            graphView.AddElement(edge);
                        }
                        break;
                    case BTRootNode rootNode:
                        {
                            var port = rootNode.ChildPort;

                            var edge = BTGraphView.ConnectPorts(port, newNode.ParentPort);
                            graphView.AddElement(edge);
                        }
                        break;
                    default:
                        break;
                }
            }

            var edge2 = BTGraphView.ConnectPorts(newNode.ChildPort, m_Node.ParentPort);
            graphView.AddElement(edge2);

            //if (node.parent is StackNode stackNode)
            //{
            //    var index = stackNode.IndexOf(node);
            //    stackNode.InsertElement(index, newNode);
            //}
            //else
            //{
            //    stackNode = new StackNode();
            //    stackNode.AddElement(newNode);
            //    stackNode.AddElement(node);

            //    graphView.AddElement(stackNode);
            //}

            m_Node = null;

            return true;
        }

    }
}