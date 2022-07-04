using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Internal;

namespace Saro.BT
{
    [Serializable]
    public abstract class BTComposite : BTNode
    {
        [JsonProperty]
        [SerializeField]
        private int[] m_Children;

        [JsonIgnore]
        protected EStatus m_LastChildExitStatus;

        [JsonIgnore]
        public int CurrentChildIndex { get; private set; } = 0;

        public virtual BTNode CurrentChild()
        {
            if (CurrentChildIndex < m_Children.Length)
            {
                return GetChildAt(CurrentChildIndex);
            }

            return null;
        }

        public override void OnEnter()
        {
            CurrentChildIndex = 0;
            var next = CurrentChild();
            if (next != null)
            {
                Iterator.Traverse(next);
            }
        }

        public void SetChildren(BTNode[] nodes)
        {
            if (m_Children == null || m_Children.Length != nodes.Length)
                m_Children = new int[nodes.Length];

            for (int i = 0; i < m_Children.Length; i++)
            {
                SetChildAt(nodes[i], i);
            }
        }

        public sealed override void OnAbort(int childIndex)
        {
            CurrentChildIndex = childIndex;
        }

        public override void OnChildExit(int childIndex, EStatus status)
        {
            CurrentChildIndex++;
            m_LastChildExitStatus = status;
        }

        public sealed override int MaxChildCount() => int.MaxValue;

        public sealed override int ChildCount() => m_Children.Length;

        public sealed override BTNode GetChildAt(int childIndex)
        {
            var index = m_Children[childIndex];
            return index > 0 ? Tree.nodes[index] : null;
        }

        public sealed override void SetChildAt(BTNode node, int childIndex)
        {
            var index = Array.IndexOf(Tree.nodes, node);
            m_Children[childIndex] = index;
        }
    }
}

