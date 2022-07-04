using System;

namespace Saro.BT
{
    public abstract class Variable
    {
        protected Variable() { }

        public abstract object GetRawValue();

        public abstract void SetRawValue(object val);

        public abstract bool Compare(object other);

        public abstract void Reset();
    }

    public class Variable<T> : Variable
    {
        public T value;

        public ref T GetValue() => ref value;

        public void SetValue(T val) => value = val;

        public override void Reset() => value = default;

        public override bool Compare(object other)
        {
            if (other is Variable<T> _other)
            {
                return value.Equals(_other.value);
            }

            return false;
        }

        public override void SetRawValue(object val) => value = (T)val;

        public override object GetRawValue() => value;
    }

    [Serializable]
    public class Variable_Float : Variable<float> { }

    [Serializable]
    public class Variable_Bool : Variable<bool> { }

    [Serializable]
    public class Variable_Int : Variable<int> { }

    [Serializable]
    public class Variable_String : Variable<string> { }

    //public class Variable_Class : Variable<object> { } // 有用？Type?

    [Serializable]
    public class Variable_Enum : Variable<int> { }

    [Serializable]
    public class Variable_Object : Variable<object> { }
}
