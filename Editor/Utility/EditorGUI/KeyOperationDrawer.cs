using System;
using Saro.SEditor;
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
                BBKey_Bool => (EBasicKeyOperation)keyOperation,
                BBKey_Object => (EBasicKeyOperation)keyOperation,
                BBKey_EcsEntity => (EBasicKeyOperation)keyOperation,
                BBKey_Vector3 => (EBasicKeyOperation)keyOperation,
                BBKey_Single => (EArithmeticKeyOperation)keyOperation,
                BBKey_Int => (EArithmeticKeyOperation)keyOperation,
                BBKey_String => (ETextKeyOperation)keyOperation,
                _ => null,
            };
        }
    }
}
