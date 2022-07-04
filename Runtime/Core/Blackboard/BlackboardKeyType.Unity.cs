using UnityEngine;

namespace Saro.BT
{
    public class BBKeyType_Vector3 : BlackboardKeyType<Vector3>
    {
        public override bool TestOperation(Vector3 valueA, byte op, Vector3 _ = default)
        {
            return (EBasicKeyOperation)op == EBasicKeyOperation.Set ? valueA != null : valueA == null;
        }
    }
}
