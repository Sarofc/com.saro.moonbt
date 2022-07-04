using UnityEditor;
using UnityEngine;

namespace Saro.BT.Designer
{
    public class BTGraphBlackboard : Editor
    {
        private Vector2 m_DrawScroll;
        private BTGraphView m_GraphView;

        public void Initialize(BTGraphView graphView)
        {
            m_GraphView = graphView;
        }

        public override void OnInspectorGUI()
        {
            if (m_GraphView.Tree == null)
            {
                GUILayout.Label("No tree selected");
                return;
            }

            if (EditorApplication.isCompiling)
            {
                GUILayout.Label(new GUIContent("Compiling Please Wait..."));
                return;
            }

            if (m_GraphView != null)
            {
                m_DrawScroll = EditorGUILayout.BeginScrollView(m_DrawScroll, false, false);

                if (Application.isPlaying)
                {
                    var bb = m_GraphView.Tree.Blackboard;
                    if (bb != null)
                    {
                        foreach (var (entry, variable) in bb)
                        {
                            EditorGUILayout.LabelField(entry.keyName, variable.GetRawValue().ToString());
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("null Blackboard");
                    }
                }
                else
                {
                    var bbData = m_GraphView.Tree.blackboardData;
                    if (bbData != null)
                    {
                        BTEditorUtils.ListEditor("", bbData.entries);
                        //BTEditorUtils.ShowAutoEditorGUI(bbData);
                    }
                    else
                    {
                        EditorGUILayout.LabelField("null BlackboardData");
                    }
                }

                EditorGUILayout.EndScrollView();
            }
        }
    }
}
