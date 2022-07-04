using System;
using System.Collections;
using System.Collections.Generic;

namespace Saro.BT.Utility
{
    public class FixedSizeStack<T> : IEnumerable<T>, IEnumerable
    {
        private readonly T[] m_Container;

        public int Capacity { get; private set; }
        public int Count { get; private set; }

        public FixedSizeStack(int capacity)
        {
            Count = 0;
            Capacity = capacity;
            this.m_Container = new T[capacity];
        }

        public void Clear()
        {
            Count = 0;
            for (int i = 0; i < m_Container.Length; i++)
            {
                m_Container[i] = default;
            }
        }

        public void FastClear() => Count = 0;

        public T Peek() => m_Container[Count - 1];

        public T Pop() => m_Container[--Count];

        public void Push(T value) => m_Container[Count++] = value;

        public T this[int index] => m_Container[index];

        public T GetValueAt(int index) => m_Container[index];

        public Enumerator GetEnumerator() => new(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            public T Current => m_Array[m_Position];

            object IEnumerator.Current => m_Array[m_Position];

            readonly FixedSizeStack<T> m_Array;
            private int m_Position;

            internal Enumerator(FixedSizeStack<T> array)
            {
                if (array == null) throw new Exception();
                this.m_Array = array;
                m_Position = -1;
            }

            void IDisposable.Dispose() { }

            public bool MoveNext() => ++m_Position < m_Array.Count;

            void IEnumerator.Reset() => m_Position = -1;
        }
    }
}

