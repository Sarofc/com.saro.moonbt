using System;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Saro.BT
{
    public abstract class BTAuxiliary : BTNode
    {
        [JsonProperty]
        [SerializeField]
        private int m_Child;

        [JsonIgnore]
        internal BTNode Child
        {
            get => GetChildAt();
            set => SetChildAt(value);
        }

        public override void OnEnter()
        {
            RunChild();
        }

        protected void RunChild()
        {
            if (Child != null)
                Iterator.Traverse(Child);
        }

        public sealed override void OnAbort(int childIndex) { }

        public override void OnParentExit()
        {
            if (Child != null && Child.IsAuxiliary())
                Child.OnParentExit();
        }

        public sealed override BTNode GetChildAt(int _ = 0)
            => m_Child > 0 ? Tree.nodes[m_Child] : null; // 前序下，孩子节点不可能是0号元素

        public sealed override void SetChildAt(BTNode node, int _ = 0)
            => m_Child = Array.IndexOf(Tree.nodes, node);

        public sealed override int ChildCount() => Child != null ? 1 : 0;

        public sealed override int MaxChildCount() => 1;
    }
}