using System.Text;
using UnityEngine;

namespace Saro.BT
{
    [BTNode("Blackboard_24x", "设置黑板值")]
    public class SetBBValue : BTTask
    {
        public BBKeySelector bbKey = new();

        [SerializeReference]
        public Variable value;

        public sealed override void OnEnter()
        {
            base.OnEnter();

            if (Blackboard != null)
            {
                if (value is Variable<int> _int)
                {
                    Blackboard.SetValue(bbKey, _int.GetValue());
                }
                else if (value is Variable<float> _float)
                {
                    Blackboard.SetValue(bbKey, _float.GetValue());
                }
                else if (value is Variable<bool> _bool)
                {
                    Blackboard.SetValue(bbKey, _bool.GetValue());
                }
                else if (value is Variable<string> _string)
                {
                    Blackboard.SetValue(bbKey, _string.GetValue());
                }
                else if (value is Variable<object> _object)
                {
                    Blackboard.SetObjectValue(bbKey, _object.GetValue());
                }
                else if (value is Variable<Vector3> _vector3)
                {
                    Blackboard.SetValue(bbKey, _vector3.GetValue());
                }
            }
        }

        public sealed override EStatus OnExecute()
        {
            return EStatus.Success;
        }

        public override void Description(StringBuilder builder)
        {
            base.Description(builder);

            builder.Append("Set ")
                .Append("'")
                .Append(bbKey)
#if UNITY_EDITOR
                .Append("'(")
                .Append(bbKey.GetBlackboardKeyType()?.GetValueType().Name)
                .Append(")")
#endif
                .Append("\nto ")
                .Append(value?.GetRawValue());
        }
    }

    //[BTNode("Blackboard_24x", "设置黑板值")]
    //public abstract class _SetBBValue<T> : BTTask
    //{
    //    public BBKeySelector bbKey = new();

    //    public T value;

    //    public sealed override void OnEnter()
    //    {
    //        base.OnEnter();

    //        if (Blackboard != null)
    //        {
    //            ref var variable = ref Blackboard.GetValue<T>(bbKey);
    //            variable = value;
    //        }
    //    }

    //    public sealed override EStatus OnExecute()
    //    {
    //        return EStatus.Success;
    //    }

    //    public override void Description(StringBuilder builder)
    //    {
    //        base.Description(builder);

    //        builder.Append("Set ")
    //            .Append("'")
    //            .Append(bbKey)
    //            .Append("'")
    //            .Append(" : ")
    //            .Append(value);
    //    }
    //}

    //public sealed class SetBBValue_Int : _SetBBValue<int> { }

    //public sealed class SetBBValue_Float : _SetBBValue<float> { }

    //public sealed class SetBBValue_Bool : _SetBBValue<bool> { }

    //public sealed class SetBBValue_Vector3 : _SetBBValue<Vector3> { }
}
