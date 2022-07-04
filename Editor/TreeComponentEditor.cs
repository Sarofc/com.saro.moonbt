using UnityEditor;
using UnityEngine;

namespace Saro.BT.Designer
{
    [CustomEditor(typeof(TreeComponent))]
    public class TreeComponentEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

            if (GUILayout.Button("Open BTGraphEditor"))
            {
                var treeComponent = (TreeComponent)target;

                var tree = treeComponent.RuntimeTree != null ? treeComponent.RuntimeTree : treeComponent.TreeAsset;

                if (tree != null)
                {
                    BTGraphEditor.Init().SetBehaviorTree(tree);
                }
                else
                {
                    Debug.LogError("No tree asset！");
                }
            }
        }
    }
}
