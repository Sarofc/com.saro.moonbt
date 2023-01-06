#if UNITY_EDITOR
#define DEBUG_MOON_BT // debug
#endif

#if FIXED_POINT_MATH
using Single = sfloat;
#else
using Single = System.Single;
#endif

using System;
using System.Collections.Generic;
using Saro.BT.Utility;

namespace Saro.BT
{
    public sealed partial class BTBehaviorIterator
    {
        private readonly FixedSizeStack<int> m_Traversal;
        private readonly Queue<int> m_RequestedTraversals;
        public BTNode.EStatus? LastChildExitStatus { get; private set; }
        public BTNode.EStatus LastExecutedStatus { get; private set; }

        private readonly BehaviorTree m_Tree;
        public event Action OnCompleted;

        public bool IsRunning => m_Traversal.Count != 0;
        public int CurrentIndex => m_Traversal.Count == 0 ? BTNode.k_InvalidPreOrder : m_Traversal.Peek();

        //public int FirstInTraversal => m_Traversal.GetValueAt(0);

        public BTBehaviorIterator(BehaviorTree tree)
        {
            m_Tree = tree;

            var maxTraversalLen = this.m_Tree.Height + 1;
            m_Traversal = new FixedSizeStack<int>(maxTraversalLen);
            m_RequestedTraversals = new Queue<int>(maxTraversalLen);
        }

        public void Tick(Single deltaTime)
        {
            CallOnEnterOnQueuedNodes();

            var index = m_Traversal.Peek();
            var node = m_Tree.nodes[index];
            var status = node.OnExecute(deltaTime);

            LastExecutedStatus = status;

#if UNITY_EDITOR
            node.NotifyStatusEditor((BTNode.EStatusEditor)status);
#endif

            if (status != BTNode.EStatus.Running)
            {
                PopNode();
                OnChildExit(node, status);
            }

            if (m_Traversal.Count == 0)
            {
                OnCompleted?.Invoke();

#if UNITY_EDITOR
                INFO($"[{m_Tree.name}] iterator done!");
#endif
            }
        }

        private void CallOnEnterOnQueuedNodes()
        {
            while (m_RequestedTraversals.Count != 0)
            {
                var i = m_RequestedTraversals.Dequeue();

#if UNITY_EDITOR
                if (i >= m_Tree.nodes.Length)
                {
                    ERROR($"[{m_Tree.name}] index out of range: {i} >= {m_Tree.nodes.Length}");
                    return;
                }
#endif

                var node = m_Tree.nodes[i];

#if UNITY_EDITOR
                node.OnBreakpoint();
#endif

                node.OnEnter();

#if UNITY_EDITOR
                INFO($"[{m_Tree.name}] enter <color=green>{node.GetType().Name}: {node.preOrder}</color>");
#endif

                OnChildEnter(node);
            }
        }

        private void OnChildEnter(BTNode node)
        {
            if (node.Parent != null)
            {
                LastChildExitStatus = null;
                node.Parent.OnChildEnter(node.childIndex);
            }
        }

        private void OnChildExit(BTNode node, BTNode.EStatus status)
        {
            if (node.Parent != null)
            {
                node.Parent.OnChildExit(node.childIndex, status);
                LastChildExitStatus = status;
            }
        }

        public void Traverse(BTNode child)
        {
            var i = child.preOrder;
            m_Traversal.Push(i);
            m_RequestedTraversals.Enqueue(i);

#if UNITY_EDITOR
            child.NotifyStatusEditor(BTNode.EStatusEditor.Running);
#endif
        }

        public void AbortRunningChildBranch(BTNode parent, int abortBranchIndex)
        {
            if (IsRunning && parent != null)
            {
                var terminatingIndex = parent.preOrder;

                while (m_Traversal.Count != 0 && m_Traversal.Peek() != terminatingIndex)
                {
                    StepBackAbort();
                }

                if (parent.IsComposite())
                {
                    parent.OnAbort(abortBranchIndex);
                }

                m_RequestedTraversals.Clear();

                Traverse(parent.GetChildAt(abortBranchIndex));

#if UNITY_EDITOR
                INFO($"[{m_Tree.name}] <color=red>abort</color> *{parent.GetType().Name}: {parent.preOrder}* branch index: {abortBranchIndex}");
                INFO($"[{m_Tree.name}] ------traversal abort: {string.Join(",", m_Traversal)}");
#endif
            }
        }

        private void StepBackAbort()
        {
            var node = PopNode();

#if UNITY_EDITOR
            node.NotifyStatusEditor(BTNode.EStatusEditor.Aborted);
#endif
        }

        internal void Interrupt(BTNode subtree)
        {
            if (subtree != null)
            {
                var parentIndex = subtree.Parent != null ? subtree.Parent.preOrder : BTNode.k_InvalidPreOrder;
                while (m_Traversal.Count != 0 && m_Traversal.Peek() != parentIndex)
                {
                    var node = PopNode();

#if UNITY_EDITOR
                    node.NotifyStatusEditor(BTNode.EStatusEditor.Interruption);
#endif
                }
                m_RequestedTraversals.Clear();
            }
        }

        private BTNode PopNode()
        {
            var index = m_Traversal.Pop();
            var node = m_Tree.nodes[index];

            if (node.IsComposite())
            {
                for (int i = 0; i < node.ChildCount(); i++)
                {
                    node.GetChildAt(i).OnParentExit();
                }
            }

            node.OnExit();

#if UNITY_EDITOR
            INFO($"[{m_Tree.name}] exit *{LastExecutedStatus}* <color=green>{node.GetType().Name}: {node.preOrder}</color>");
            INFO($"[{m_Tree.name}] ------traversal pop: {string.Join(",", m_Traversal)}");
#endif

            return node;
        }

#if DEBUG_MOON_BT
        internal static List<string> s_LogCache = new(4096);
#endif

        [System.Diagnostics.Conditional("DEBUG_MOON_BT")]
        private static void INFO(string msg)
        {
#if DEBUG_MOON_BT
            s_LogCache.Add($"{DateTime.Now.ToLongTimeString()}   {msg}\n");
#endif

            //UnityEngine.Debug.Log(msg);
        }

        private static void ERROR(string msg)
        {
            var output = $"{DateTime.Now.ToLongTimeString()}   <color=red>[BT]</color> {msg}";

#if DEBUG_MOON_BT
            s_LogCache.Add(output + "\n");
#endif

            //UnityEngine.Debug.LogError(output);
        }
    }
}
