using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Saro.BT.Designer
{
    public class BTExecuteLogConsole : EditorWindow
    {
        private Vector2 m_DrawScroll;
        private float m_LastScrollPosY;

        [MenuItem("Gameplay/BTExecuteLogConsole")]
        static void Init()
        {
            var window = GetWindow<BTExecuteLogConsole>();
            window.titleContent = new GUIContent("BTExecuteLogConsole");
        }

        private static GUIStyle s_FontStyle;
        private static GUIStyle FontStyle
        {
            get
            {
                if (s_FontStyle == null)
                    s_FontStyle = new GUIStyle() { richText = true };

                return s_FontStyle;
            }
        }

        private string m_SearchText;

        public void OnGUI()
        {
            if (EditorApplication.isCompiling)
            {
                GUILayout.Label(new GUIContent("Compiling Please Wait..."));
                return;
            }

            var buttonWidth = 48f;
            var toolbarRect = EditorGUILayout.GetControlRect();
            var searchRecat = toolbarRect;
            searchRecat.width -= buttonWidth;
            m_SearchText = EditorGUI.TextField(searchRecat, m_SearchText);

            var clearRect = toolbarRect;
            clearRect.width = buttonWidth;
            clearRect.x += searchRecat.width;
            if (GUI.Button(clearRect, "Clear"))
            {
                BTBehaviorIterator.s_LogCache.Clear();
            }

            BTEditorUtils.Separator();

            bool search = !string.IsNullOrEmpty(m_SearchText);

            m_DrawScroll = EditorGUILayout.BeginScrollView(m_DrawScroll, false, false);

            m_LastScrollPosY = 0;

            foreach (var msg in BTBehaviorIterator.s_LogCache)
            {
                if (!search || msg.Contains(m_SearchText, System.StringComparison.OrdinalIgnoreCase))
                {
                    EditorGUILayout.LabelField(msg, FontStyle);
                    m_LastScrollPosY += EditorGUIUtility.singleLineHeight;
                }
            }

            EditorGUILayout.EndScrollView();

            //if (Mathf.Abs(m_DrawScroll.y - m_LastScrollPosY) <= position.height)
            //{
            //    m_DrawScroll.y = float.MaxValue;
            //}

            this.Repaint();
        }
    }
}
