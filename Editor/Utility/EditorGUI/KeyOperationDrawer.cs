using System;
using UnityEditor;

namespace Saro.BT.Designer
{
    public class KeyOperationDrawer : AttributeDrawer<KeyOperationAttribute>
    {
        public override void OnGUI(string label, ref object instance, object context)
        {
            if (context is BBCondition bbCondition)
            {
                var Enum = GetOperationEnum(bbCondition.bbKey, bbCondition.keyOperation);
                if (Enum != null)
                {
                    instance = (byte)(EArithmeticKeyOperation)EditorGUILayout.EnumPopup("keyOperation", Enum);
                }
                else
                    EditorGUILayout.LabelField("invalid bbKey");
            }
        }

        public Enum GetOperationEnum(BBKeySelector bbKey, byte keyOperation)
        {
            if (bbKey.data == null) return null;

            var entry = bbKey.data.GetKeyEntryByName(bbKey);

            if (entry == null) return null;

            var keyType = entry.keyType;

            return keyType switch
            {
                BBKeyType_Bool => (EBasicKeyOperation)keyOperation,
                BBKeyType_Object => (EBasicKeyOperation)keyOperation,
                BBKeyType_Float => (EArithmeticKeyOperation)keyOperation,
                BBKeyType_Int => (EArithmeticKeyOperation)keyOperation,
                BBKeyType_String => (ETextKeyOperation)keyOperation,
                _ => null,
            };
        }
    }
}
