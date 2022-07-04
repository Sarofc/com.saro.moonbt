using System;
using Saro.Pool;
using UnityEditor;

namespace Saro.BT.Designer
{
    public class BBEntrySelectorDrawer : ObjectDrawer<BBKeySelector>
    {
        public override void OnGUI(string label, ref BBKeySelector instance, object context)
        {
            if (instance.data == null)
            {
                EditorGUILayout.LabelField("blackboardData is null");
                return;
            }
            var entries = instance.data.entries;
            if (entries == null || entries.Count <= 0)
            {
                EditorGUILayout.LabelField("blackboardData.entries is null");
                return;
            }

            using (ListPool<string>.Rent(out var list))
            {
                foreach (var entry in entries)
                {
                    list.Add(entry.keyName);
                }

                var keyName = instance.keyName;
                var index = entries.FindIndex(e => e.keyName == keyName);
                if (index == -1)
                {
                    //if (string.IsNullOrEmpty(keyName))
                    //    instance.keyName = entries[0].keyName;
                    //else
                    EditorGUILayout.LabelField($"*missing ({keyName})");
                }

                string newLabel;
                var keyType = instance.GetBlackboardKeyType();

                if (keyType != null)
                    newLabel = $"{label}({instance.GetBlackboardKeyType()?.GetValueType().Name})";
                else
                    newLabel = label;

                var newIndex = EditorGUILayout.Popup(newLabel, index, list.ToArray());

                if (newIndex != index)
                {
                    instance.keyName = entries[newIndex].keyName;
                }
            }

            return;
        }
    }
}
