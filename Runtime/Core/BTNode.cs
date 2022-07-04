using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Saro.BT
{
    [Serializable]
    public abstract class BTNode : IIterableNode<BTNode>
    {
        public enum EStatus : byte
        {
            Success,
            Failure,
            Running
        }

        public const int k_InvalidPreOrder = -1;

        [JsonIgnore]
        public BehaviorTree Tree { get; internal set; }

        [JsonIgnore]
        public int ChildOrder => childIndex;

        //public int LevelOrder => levelOrder;

        [JsonIgnore]
        protected BTBlackboard Blackboard => Tree.Blackboard;

        [JsonIgnore]
        protected object Actor => Tree.actor;

        [JsonIgnore]
        internal int preOrder = k_InvalidPreOrder;

        //internal int postOrderIndex = 0;

        //internal int levelOrder = 0;

        [JsonIgnore]
        public BTNode Parent { get; set; }

        [JsonIgnore]
        public BTBehaviorIterator Iterator { get; internal set; }

        [JsonIgnore]
        protected internal int childIndex = 0;

        /// <summary>
        /// 行为树启动时调用，用于初始化节点数据
        /// </summary>
        public virtual void OnInitialize()
        {
            OnValidate();
        }

        /// <summary>
        /// 行为树停止时调用，用于清理节点数据
        /// </summary>
        public virtual void OnReset() { }

        /// <summary>
        /// 执行节点
        /// </summary>
        /// <returns></returns>
        public abstract EStatus OnExecute();

        /// <summary>
        /// 此节点开始执行时调用
        /// </summary>
        public virtual void OnEnter() { }

        /// <summary>
        /// 此节点结束执行时调用
        /// </summary>
        public virtual void OnExit() { }

        /// <summary>
        /// 此节点被打断时调用，目前仅 Composite 节点调用
        /// </summary>
        /// <param name="childIndex"></param>
        public virtual void OnAbort(int childIndex) { }

        /// <summary>
        /// 此节点的孩子节点开始执行时调用
        /// </summary>
        /// <param name="childIndex"></param>
        public virtual void OnChildEnter(int childIndex) { }

        /// <summary>
        /// 此节点的孩子节点结束执行时调用
        /// </summary>
        /// <param name="childIndex"></param>
        /// <param name="status"></param>
        public virtual void OnChildExit(int childIndex, EStatus status) { }

        /// <summary>
        /// 此节点的父节点，<see cref="BTComposite"/>/<see cref="BTAuxiliary"/> 结束执行时调用
        /// </summary>
        public virtual void OnParentExit() { }

        /// <summary>
        /// 校验此节点的状态是否合法
        /// </summary>
        internal protected virtual void OnValidate() { }

        public abstract BTNode GetChildAt(int childIndex);

        public abstract void SetChildAt(BTNode node, int childIndex);

        public abstract int ChildCount();

        public abstract int MaxChildCount();

        public bool IsComposite() => MaxChildCount() > 1;

        public bool IsAuxiliary() => MaxChildCount() == 1;

        public bool IsTask() => MaxChildCount() == 0;

        public virtual void Description(StringBuilder builder) { }

        // 用 partial 会导致编辑器面板 字段顺序 紊乱 
#if UNITY_EDITOR
        internal enum EStatusEditor
        {
            Success, Failure, Running, None, Aborted, Interruption
        }

        [HideInInspector]
        [SerializeField]
        internal bool breakPoint;

        [HideInInspector]
        [SerializeField]
        [JsonProperty]
        internal Rect nodePosition;

        [Space(20)]
        [Separator]
        [TextArea]
        [SerializeField]
        [JsonProperty]
        internal string comment;

        [SerializeField]
        [JsonProperty]
        internal string title;

        internal string Title => string.IsNullOrEmpty(title) ? GetType().Name : title;

        internal void OnBreakpoint()
        {
            if (breakPoint) UnityEditor.EditorApplication.isPaused = breakPoint;
        }

        internal EStatusEditor StatusEditorResult { get; set; } = EStatusEditor.None;

        [HideInInspector]
        internal Action<EStatusEditor> StatusEditorNotify;

        internal void NotifyStatusEditor(EStatusEditor status)
        {
            StatusEditorResult = status;
            StatusEditorNotify?.Invoke(status);
        }

        public override string ToString() => $"{this.GetType()}: preOrder: {preOrder} parent's preOrder: {Parent.preOrder} ChildCount: {ChildCount()}";
#endif
    }
}