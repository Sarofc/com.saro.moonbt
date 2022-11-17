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
                Blackboard.SetValue(bbKey, value);
            }
        }

        public sealed override EStatus OnExecute(float deltaTime)
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
