using System;

namespace Saro.BT
{
    //public enum EBlackboardKeyOperation : byte
    //{
    //    Basic = 0,
    //    Arithmetic = 1,
    //    Text = 2,
    //}

    public enum EBlackboardCompare : byte
    {
        Less = 0,
        Equal = 1,
        Greater = 2,
        NotEqual = 3,
    }


    public enum EBasicKeyOperation : byte
    {
        Set = 0,
        NotSet = 1,
    }

    public enum EArithmeticKeyOperation : byte
    {
        Equal,
        NotEqual,
        Less,
        LessOrEqual,
        Greater,
        GreaterOrEqual,
    }

    public enum ETextKeyOperation : byte
    {
        Equal,
        NotEqual,
        Contain,
        NotContain,
    }

    public abstract class BlackboardKeyType
    {
        public abstract Variable CreateVariable();

        public abstract Type GetValueType();
    }

    public abstract class BlackboardKeyType<T> : BlackboardKeyType
    {
        public sealed override Variable CreateVariable() => new Variable<T>();

        public sealed override Type GetValueType() => typeof(T);

        public abstract bool TestOperation(T valueA, byte op, T valueB);
    }

    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceClassName: "BBKeyType_Int")]
    public class BBKey_Int : BlackboardKeyType<int>
    {
        public override bool TestOperation(int valueA, byte op, int valueB)
        {
            return (EArithmeticKeyOperation)op switch
            {
                EArithmeticKeyOperation.Equal => valueA == valueB,
                EArithmeticKeyOperation.NotEqual => valueA != valueB,
                EArithmeticKeyOperation.Less => valueA < valueB,
                EArithmeticKeyOperation.LessOrEqual => valueA <= valueB,
                EArithmeticKeyOperation.Greater => valueA > valueB,
                EArithmeticKeyOperation.GreaterOrEqual => valueA >= valueB,
                _ => false,
            };
        }
    }

    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceClassName: "BBKeyType_Float")]
    public class BBKey_Float : BlackboardKeyType<float>
    {
        public override bool TestOperation(float valueA, byte op, float valueB)
        {
            return (EArithmeticKeyOperation)op switch
            {
                EArithmeticKeyOperation.Equal => valueA == valueB,
                EArithmeticKeyOperation.NotEqual => valueA != valueB,
                EArithmeticKeyOperation.Less => valueA < valueB,
                EArithmeticKeyOperation.LessOrEqual => valueA <= valueB,
                EArithmeticKeyOperation.Greater => valueA > valueB,
                EArithmeticKeyOperation.GreaterOrEqual => valueA >= valueB,
                _ => false,
            };
        }
    }

    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceClassName: "BBKeyType_Bool")]
    public class BBKey_Bool : BlackboardKeyType<bool>
    {
        public override bool TestOperation(bool valueA, byte op, bool _ = false)
        {
            return (EBasicKeyOperation)op == EBasicKeyOperation.Set ? valueA : !valueA;
        }
    }

    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceClassName: "BBKeyType_String")]
    public class BBKey_String : BlackboardKeyType<string>
    {
        public override bool TestOperation(string valueA, byte op, string valueB)
        {
            return (ETextKeyOperation)op switch
            {
                ETextKeyOperation.Equal => valueA == valueB,
                ETextKeyOperation.NotEqual => valueA != valueB,
                ETextKeyOperation.Contain => valueA.Contains(valueB),
                ETextKeyOperation.NotContain => !valueA.Contains(valueB),
                _ => false,
            };
        }
    }

    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceClassName: "BBKeyType_Object")]
    public class BBKey_Object : BlackboardKeyType<object>
    {
        public override bool TestOperation(object valueA, byte op, object _ = null)
        {
            return (EBasicKeyOperation)op == EBasicKeyOperation.Set ? valueA != null : valueA == null;
        }
    }
}
