using System;
using System.Collections;
using System.Collections.Generic;
using Saro.Utility;
using UnityEngine;

namespace Saro.BT
{
    /*
      参考ue的实现

        1. 针对每个类型的键（int、float、object、enum，etc），Operation操作都不一样
        2. blackboard装饰节点，上保存了int、float、string 3种可比较类型
        3. blackboard资源上，不能编辑value，只有key，value则可以通过service节点赋值上去

        5. blackboarddata 是数据，只保存key
        6. Variable是运行时保存的value

        7. 不支持运行时删除黑板键，只支持运行时添加

     */
    public class BTBlackboard
    {
        public BlackboardData Data { get; private set; }

        /// <summary>
        /// 与 <see cref="BlackboardData.entries"/> 一一对应
        /// </summary>
        public List<Variable> Variables { get; private set; }

        public struct ChangedEvent : IEquatable<ChangedEvent>
        {
            public int index;
            public Action callback;

            public bool Equals(ChangedEvent other)
            {
                return index == other.index && callback == other.callback;
            }
        }

        private readonly List<ChangedEvent> m_Observers = new();

        public BTBlackboard(BlackboardData data)
        {
            Data = data;

            Variables = new List<Variable>(Data.entries.Count);

            for (int i = 0; i < Data.entries.Count; i++)
            {
                var entry = Data.entries[i];
                Variables.Add(entry.keyType.CreateVariable());
            }
        }

        public BlackboardEntry GetKeyEntryByName(string keyName) => Data.GetKeyEntryByName(keyName);

        public int GetKeyIndexByName(string keyName)
        {
            var index = Data.GetKeyIndexByName(keyName);

            if (index == -1)
                Debug.LogError("keyName not found: " + keyName);

            return index;
        }

        public Variable GetVariable(string keyName)
        {
            var index = GetKeyIndexByName(keyName);
            return Variables[index];
        }

        public Variable GetVariable(int index)
        {
            return Variables[index];
        }

        public void SetValue(string keyName, Variable value)
        {
            if (value is Variable<int> _int)
            {
                SetValue(keyName, _int.GetValue());
            }
            else if (value is Variable<float> _float)
            {
                SetValue(keyName, _float.GetValue());
            }
            else if (value is Variable<bool> _bool)
            {
                SetValue(keyName, _bool.GetValue());
            }
            else if (value is Variable<string> _string)
            {
                SetValue(keyName, _string.GetValue());
            }
            else if (value is Variable<object> _object)
            {
                SetObjectValue(keyName, _object.GetValue());
            }
            else if (value is Variable<Vector3> _vector3)
            {
                SetValue(keyName, _vector3.GetValue());
            }
        }

        public ref T GetValue<T>(string keyName)
        {
            var index = GetKeyIndexByName(keyName);
            return ref GetValue<T>(index);
        }

        public ref T GetValue<T>(int index)
        {
            if (index >= 0 && index < Variables.Count)
            {
                if (Variables[index] is Variable<T> variable)
                {
                    return ref variable.GetValue();
                }
            }

            return ref MemoryUtility.NullRef<T>();
        }

        public void SetValue<T>(string keyName, T value) where T : IEquatable<T>
        {
            var index = GetKeyIndexByName(keyName);
            SetValue<T>(index, value);
        }

        public void SetValue<T>(int index, T value) where T : IEquatable<T>
        {
            ref var refValue = ref GetValue<T>(index);

            if (MemoryUtility.IsNullRef(ref refValue))
            {
                Debug.LogError("null ref. index: " + index);
                return;
            }

            if (!refValue.Equals(value))
            {
                refValue = value;
                FireChangeEvent(index);
            }
        }

        public void SetObjectValue(string keyName, object value)
        {
            var index = GetKeyIndexByName(keyName);
            SetObjectValue(index, value);
        }

        public void SetObjectValue(int index, object value)
        {
            ref var refValue = ref GetValue<object>(index);

            if (MemoryUtility.IsNullRef(ref refValue))
            {
                Debug.LogError("null ref. index: " + index);
                return;
            }

            if (!refValue.Equals(value))
            {
                refValue = value;
                FireChangeEvent(index);
            }
        }

        public void ResetData()
        {
            foreach (var item in Variables)
            {
                item.Reset();
            }
        }

        public void RegisterChangeEvent(string keyName, Action callback)
        {
            var index = GetKeyIndexByName(keyName);
            RegisterChangeEvent(index, callback);
        }

        public void RegisterChangeEvent(int index, Action callback)
        {
            var evt = new ChangedEvent { index = index, callback = callback };
            Debug.Assert(!m_Observers.Contains(evt), "duplicated event. index: " + index);

            m_Observers.Add(evt);
        }

        public void UnregisterChangeEvent(string keyName, Action callback)
        {
            var index = GetKeyIndexByName(keyName);
            UnregisterChangeEvent(index, callback);
        }

        public void UnregisterChangeEvent(int index, Action callback)
        {
            var evt = new ChangedEvent { index = index, callback = callback };
            var removeAny = m_Observers.Remove(evt);
            Debug.Assert(removeAny, "no event to remove. index: " + index);
        }

        private void FireChangeEvent(int index)
        {
            for (int i = 0; i < m_Observers.Count; i++)
            {
                var observer = m_Observers[i];
                if (observer.index == index)
                {
                    observer.callback?.Invoke();
                }
            }
        }

        public Enumerator GetEnumerator() => new(this);

        public struct Enumerator : IEnumerator<(BlackboardEntry entry, Variable variable)>
        {
            private BTBlackboard m_Blackboard;
            private int m_Index;

            public Enumerator(BTBlackboard blackboard)
            {
                m_Blackboard = blackboard;
                m_Index = -1;
            }

            public (BlackboardEntry entry, Variable variable) Current
                => (m_Blackboard.Data.entries[m_Index], m_Blackboard.Variables[m_Index]);

            object IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext() => ++m_Index < m_Blackboard.Variables.Count;

            public void Reset() => m_Index = -1;
        }
    }
}
