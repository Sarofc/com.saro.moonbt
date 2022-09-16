//using System;
//using System.Collections.Generic;
//using UnityEditor.Experimental.GraphView;
//using UnityEngine;

//namespace Saro.BT.Designer
//{
//    public class BTAuxiliarySearchWindowProvider : ScriptableObject, ISearchWindowProvider
//    {
//        private readonly BTGraphNode node;

//        public BTAuxiliarySearchWindowProvider(BTGraphNode node)
//        {
//            this.node = node;
//        }

//        List<SearchTreeEntry> ISearchWindowProvider.CreateSearchTree(SearchWindowContext context)
//        {
//            var entries = new List<SearchTreeEntry>();
//            entries.Add(new SearchTreeGroupEntry(new GUIContent("Select BTAuxiliary")));

//            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
//            {
//                foreach (var type in assembly.GetTypes())
//                {
//                    if (type.IsSubclassOf(typeof(BTAuxiliary)))
//                    {
//                        entries.Add(new SearchTreeEntry(new GUIContent(type.Name)) { level = 1, userData = type });
//                    }
//                }
//            }

//            return entries;
//        }

//        bool ISearchWindowProvider.OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
//        {
//            var type = searchTreeEntry.userData as System.Type;
//            this.node.SetBehavior(type);
//            return true;
//        }

//    }
//}