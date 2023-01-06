using System;
using System.Collections;
using System.Collections.Generic;

namespace Saro.BT.Utility
{
    internal class FixedSizeStack<T> : IEnumerable<T>, IEnumerable
    {
        private T[] m_InternalArray;

        public int Capacity { get; private set; }
        public int Count { get; private set; }

        public FixedSizeStack(int capacity)
        {
            Count = 0;
            Capacity = capacity;
            m_InternalArray = new T[capacity];
        }

        public void Clear()
        {
            Count = 0;
            for (int i = 0; i < m_InternalArray.Length; i++)
            {
                m_InternalArray[i] = default;
            }
        }

        public void FastClear() => Count = 0;

        public T Peek() => m_InternalArray[Count - 1];

        public T Pop() => m_InternalArray[--Count];

        public void Push(T value) => m_InternalArray[Count++] = value;

        public T GetValueAt(int index) => m_InternalArray[index];

        public Enumerator GetEnumerator() => new(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<T>, IEnumerator // TODO 测试一下
        {
            public T Current => m_Array.GetValueAt(m_Position);

            object IEnumerator.Current => Current;

            readonly FixedSizeStack<T> m_Array;
            private int m_Position;

            internal Enumerator(FixedSizeStack<T> array)
            {
                if (array == null) throw new Exception();
                m_Array = array;
                m_Position = array.Count;
            }

            public void Dispose() { }

            public bool MoveNext() => --m_Position >= 0;

            public void Reset() => m_Position = m_Array.Count;
        }
    }
}

