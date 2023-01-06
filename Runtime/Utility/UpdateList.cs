using System;
using System.Collections.Generic;

namespace Saro.BT
{
    internal partial class UpdateList<T>
    {
        private readonly List<T> m_Data = new();
        private readonly List<T> m_AddQueue = new();
        private readonly List<T> m_RemoveQueue = new();

        private readonly Predicate<T> m_IsInRemovealQueue;

        public IReadOnlyList<T> Data => m_Data;

        public UpdateList()
        {
            m_IsInRemovealQueue = (T val) =>
            {
                return m_RemoveQueue.Contains(val);
            };
        }

        public void Add(T item) => m_AddQueue.Add(item);

        public void Remove(T item) => m_RemoveQueue.Add(item);

        public void AddAndRemoveQueued()
        {
            if (m_RemoveQueue.Count != 0)
            {
                m_Data.RemoveAll(m_IsInRemovealQueue);
                m_RemoveQueue.Clear();
            }

            if (m_AddQueue.Count != 0)
            {
                m_Data.AddRange(m_AddQueue);
                m_AddQueue.Clear();
            }
        }

        public void Clear()
        {
            m_Data.Clear();
            m_AddQueue.Clear();
            m_RemoveQueue.Clear();
        }
    }
}