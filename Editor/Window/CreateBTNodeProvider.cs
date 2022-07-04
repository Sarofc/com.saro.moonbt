using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Saro.BT.Designer
{
    public class CreateBTNodeProvider : ScriptableObject, ISearchWindowProvider
    {
        private BTGraphView m_GraphView;
        private EditorWindow m_GraphEditor;
        private List<SearchTreeEntry> m_Entries;

        public Port ConnectedPort { get; internal set; }

        public CreateBTNodeProvider(BTGraphView graphView, EditorWindow graphEditor)
        {
            m_GraphView = graphView;
            m_GraphEditor = graphEditor;

            m_Entries = new List<SearchTreeEntry>();
            m_Entries.Add(new SearchTreeGroupEntry(new GUIContent("Create BTNode")));

            var compositeGroup = new List<SearchTreeEntry>();
            var taskGroup = new List<SearchTreeEntry>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    var btNodeAttribute = type.GetCustomAttribute<BTNodeAttribute>();
                    if (btNodeAttribute != null)
                    {
                        if (type != typeof(Root) && !type.IsAbstract)
                        {
                            if (type.IsSubclassOf(typeof(BTComposite)))
                                compositeGroup.Add(new SearchTreeEntry(new GUIContent(type.Name)) { level = 2, userData = type });
                            else if (type.IsSubclassOf(typeof(BTTask)))
                                taskGroup.Add(new SearchTreeEntry(new GUIContent(type.Name)) { level = 2, userData = type });

                            // 不允许创建 BTAuxiliary 了，使用 DecoratorNodeProvider
                            //else if (type.IsSubclassOf(typeof(BTAuxiliary)))
                            //    auxiliaryGroup.Add(new SearchTreeEntry(new GUIContent(type.Name)) { level = 2, userData = type });
                        }
                    }
                }
            }

            m_Entries.Add(new SearchTreeGroupEntry(new GUIContent("Composite"), 1));
            m_Entries.AddRange(compositeGroup);

            //m_Entries.Add(new SearchTreeGroupEntry(new GUIContent("Auxiliary"), 1));
            //m_Entries.AddRange(auxiliaryGroup);

            m_Entries.Add(new SearchTreeGroupEntry(new GUIContent("Task"), 1));
            m_Entries.AddRange(taskGroup);
        }

        List<SearchTreeEntry> ISearchWindowProvider.CreateSearchTree(SearchWindowContext context)
        {
            return m_Entries;
        }

        bool ISearchWindowProvider.OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var type = searchTreeEntry.userData as Type;
            var node = BTGraphNodeFactory.CreateGraphNode(type, m_GraphView);

            var worldMousePosition = this.m_GraphEditor.rootVisualElement.ChangeCoordinatesTo(this.m_GraphEditor.rootVisualElement.parent, context.screenMousePosition - this.m_GraphEditor.position.position);
            var localMousePosition = this.m_GraphView.contentViewContainer.WorldToLocal(worldMousePosition);
            node.SetPosition(new Rect(localMousePosition, new Vector2(100, 100)));
            m_GraphView.AddElement(node);

            if (ConnectedPort != null)
            {
                if (ConnectedPort.connected)
                {
                    if (ConnectedPort.node is BTAuxiliaryNode
                        || ConnectedPort.node is BTSimpleParallelNode)
                    {
                        var oldEdge = ConnectedPort.connections.First();
                        BTGraphView.DisconnectPorts(oldEdge);
                        m_GraphView.RemoveElement(oldEdge);
                    }
                }

                var newEdge = BTGraphView.ConnectPorts(ConnectedPort, node.ParentPort);
                m_GraphView.AddElement(newEdge);

                ConnectedPort = null;
            }

            return true;
        }
    }
}