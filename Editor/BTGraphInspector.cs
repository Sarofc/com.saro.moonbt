using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Saro.BT.Designer
{
    public class BTGraphInspector : Editor
    {
        private BTGraphNode m_SelectedNode;
        private Vector2 m_DrawScroll;

        //private void OnEnable()
        //{
        //    BTGraphView.OnNodeSelection += OnNodeSelection;
        //}

        //private void OnDisable()
        //{
        //    BTGraphView.OnNodeSelection -= OnNodeSelection;
        //}

        internal void OnNodeSelection(BTGraphNode obj)
        {
            m_SelectedNode = obj;
        }

        public override void OnInspectorGUI()
        {
            if (BTGraphEditor.CurretnTree == null)
            {
                GUILayout.Label("No tree selected");
                return;
            }

            if (EditorApplication.isCompiling)
            {
                GUILayout.Label(new GUIContent("Compiling Please Wait..."));
                return;
            }


            if (m_SelectedNode != null)
            {
                if (m_SelectedNode.GraphView.selection.Count > 1)
                {
                    GUILayout.Label("Multi Edit is not supported");
                    return;
                }
                m_DrawScroll = EditorGUILayout.BeginScrollView(m_DrawScroll, false, false);

                EditorGUI.BeginChangeCheck();

                m_SelectedNode.OnNodeInspectorGUI();

                if (EditorGUI.EndChangeCheck())
                {
                    m_SelectedNode.RefreshBehavior();

                    //BTEditorUtils.SetDirty(m_SelectedNode.GraphView.tree);
                    //Debug.LogError("BTGraphInsepctor Changed");
                }

                EditorGUILayout.EndScrollView();
            }
        }
    }
}
