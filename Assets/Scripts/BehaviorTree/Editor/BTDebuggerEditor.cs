using UnityEngine;
using UnityEditor;

namespace Saro.BT
{
    [CustomEditor(typeof(UBTDebugger))]
    public class BTDebuggerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Label("BT Debugger", EditorStyles.centeredGreyMiniLabel);

            if (GUILayout.Button("Open BT Debugger Window"))
            {
                BTDebuggerWindow.selectedDebugger = ((UBTDebugger)target);
                BTDebuggerWindow.selectedObject = BTDebuggerWindow.selectedDebugger.transform;
                BTDebuggerWindow.ShowWindow();
            }
        }
    }
}