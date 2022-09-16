using UnityEngine;

namespace Saro.BT
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceClassName: "BBKeyType_Vector3")]
    public class BBKey_Vector3 : BlackboardKeyType<Vector3>
    {
        public override bool TestOperation(Vector3 valueA, byte op, Vector3 _ = default)
        {
            return (EBasicKeyOperation)op == EBasicKeyOperation.Set ? valueA != null : valueA == null;
        }
    }
}
