using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Saro.BT.Designer
{
    public sealed class ChangeBTCompositeProvider : ChangeBTNodeProvider
    {
        public ChangeBTCompositeProvider(EditorWindow graphEditor) : base(graphEditor)
        {
        }

        protected override bool ValidNodeType(Type type)
        {
            return type.IsSubclassOf(typeof(BTComposite)) && type != typeof(SimpleParallel);
        }
    }

    public sealed class ChangeBTTaskProvider : ChangeBTNodeProvider
    {
        public ChangeBTTaskProvider(EditorWindow graphEditor) : base(graphEditor)
        {
        }

        protected override bool ValidNodeType(Type type)
        {
            return type.IsSubclassOf(typeof(BTTask));
        }
    }

    public sealed class ChangeBTDecoratorProvider : ChangeBTNodeProvider
    {
        public ChangeBTDecoratorProvider(EditorWindow graphEditor) : base(graphEditor)
        {
        }

        protected override bool ValidNodeType(Type type)
        {
            return type.IsSubclassOf(typeof(BTDecorator));
        }
    }

    public sealed class ChangeBTServiceProvider : ChangeBTNodeProvider
    {
        public ChangeBTServiceProvider(EditorWindow graphEditor) : base(graphEditor)
        {
        }

        protected override bool ValidNodeType(Type type)
        {
            return type.IsSubclassOf(typeof(BTService));
        }
    }

    public abstract class ChangeBTNodeProvider : ScriptableObject, ISearchWindowProvider
    {
        private BTGraphNode m_Node;
        private List<SearchTreeEntry> m_Entries;
        private EditorWindow m_GraphEditor;

        public ChangeBTNodeProvider(EditorWindow graphEditor)
        {
            m_GraphEditor = graphEditor;

            m_Entries = new List<SearchTreeEntry>();
            m_Entries.Add(new SearchTreeGroupEntry(new GUIContent("Change Node")));

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (ValidNodeType(type))
                    {
                        m_Entries.Add(new SearchTreeEntry(new GUIContent(type.Name)) { level = 1, userData = type });
                    }
                }
            }
        }

        public void Show(BTGraphNode node, ContextualMenuPopulateEvent evt)
        {
            m_Node = node;
            SearchWindow.Open(new SearchWindowContext(GetScreenMousePosition(evt)), this);
        }

        private Vector2 GetScreenMousePosition(ContextualMenuPopulateEvent evt) => m_GraphEditor.position.position + evt.mousePosition;

        protected abstract bool ValidNodeType(Type type);

        List<SearchTreeEntry> ISearchWindowProvider.CreateSearchTree(SearchWindowContext context)
        {
            return m_Entries;
        }

        bool ISearchWindowProvider.OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            if (m_Node == null)
            {
                Debug.LogError("SetNode first");
                return false;
            }

            var type = searchTreeEntry.userData as Type;
            m_Node.SetBehavior(Activator.CreateInstance(type) as BTNode);

            m_Node = null; // Ω‚“˝”√

            return true;
        }

    }
}