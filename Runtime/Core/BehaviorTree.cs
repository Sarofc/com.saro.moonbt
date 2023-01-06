#if FIXED_POINT_MATH
using Single = sfloat;
#else
using Single = System.Single;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Serialization;
using Saro.Entities;
using Saro.BT.Utility;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Saro.BT
{
    /*
     * TODO
     *
     * 1. json保存，目前json会剔除so相关的引用，debug时，有一些逻辑问题
     * 3. 清理整个树时，正在执行的action也要调用reset
     *
     */

    public partial class BehaviorTree
    {
        private BTBehaviorIterator m_MainIterator;

        private UpdateList<Timer> m_ActiveTimers;
        [JsonIgnore]
        public BTBlackboard Blackboard { get; private set; }

        [JsonIgnore]
        public EcsEntity actor;

        [JsonIgnore]
        public bool IsTreeInitialized { get; private set; }

        [JsonIgnore]
        public int Height { get; internal set; }

        [JsonIgnore]
        public int ActiveTimerCount => m_ActiveTimers.Data.Count;

        [JsonIgnore]
        public BTNode Root => nodes.Length == 0 ? null : nodes[0];

        #region Serialization

        public string id;

        [FormerlySerializedAs("Nodes")]
        [SerializeReference]
        [JsonProperty]
        internal BTNode[] nodes = { };

        [FormerlySerializedAs("BlackboardData")]
        [JsonProperty]
        [SerializeField]
        internal BlackboardData blackboardData;

        #endregion

        /// <summary>
        /// 初始化树
        /// <code>此时actor未设置</code>
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        public void Initialize()
        {
            if (Root == null)
                throw new NullReferenceException("Root");

            if (blackboardData != null && blackboardData.entries.Count > 0)
                if (Blackboard == null)
                    Blackboard = new(blackboardData);

            m_ActiveTimers = new();

            SetupNodes();

            IsTreeInitialized = true;
        }

        // TODO 清理树，对象池模型，采用 复用整棵树
        public void ResetData()
        {
            if (Blackboard != null)
            {
                // TODO 清理运行时数据，以便复用
                Blackboard.ResetData();
            }

            foreach (var node in nodes)
            {
                node.OnReset();
            }

            m_ActiveTimers.Clear();
        }

        public void Tick(Single deltaTime)
        {
            if (IsTreeInitialized && m_MainIterator.IsRunning)
            {
                TickTimers(deltaTime);
                m_MainIterator.Tick(deltaTime);
            }
        }

        public void BeginTraversal()
        {
            if (IsTreeInitialized && !m_MainIterator.IsRunning)
            {
                m_MainIterator.Traverse(Root);
            }
        }

        public void Interrupt() => Root.Iterator.Interrupt(Root);

        public void AddTimer(Timer timer) => m_ActiveTimers.Add(timer);

        public void RemoveTimer(Timer timer) => m_ActiveTimers.Remove(timer);

        private void TickTimers(Single deltaTime)
        {
            var timers = m_ActiveTimers.Data;
            var count = m_ActiveTimers.Data.Count;
            for (int i = 0; i < count; i++)
            {
                timers[i].Tick(deltaTime);
            }

            m_ActiveTimers.AddAndRemoveQueued();
        }

        public IEnumerable<T> GetNodes<T>() where T : BTNode => nodes.Select(node => node as T).Where(casted => casted != null);

        private void SetupNodes()
        {
            for (int i = 0; i < nodes.Length; i++) // 其实已经是前序了
            {
                BTNode node = nodes[i];
                node.Tree = this; // 需要先设置了，子节点依赖Tree.nodes
                node.preOrder = i;
            }

            foreach ((BTNode _, int level) in TreeTraversal.LevelOrder(Root))
            {
                Height = level;
            }

            m_MainIterator = new BTBehaviorIterator(this);

            for (int i = 0; i < nodes.Length; i++)
            {
                BTNode node = nodes[i];
                node.Tree = this;
                node.Iterator = m_MainIterator;

                // 设置parent
                for (int k = 0; k < node.ChildCount(); k++)
                {
                    var child = node.GetChildAt(k);
                    child.childIndex = k;
                    child.Parent = node;
                }

                //node.OnValidate();
                node.OnInitialize();
            }
        }

        public bool IsRunning() => m_MainIterator != null && m_MainIterator.IsRunning;

        public BTNode.EStatus LastStatus() => m_MainIterator.LastExecutedStatus;

        public BehaviorTree Clone()
        {
            var newTree = new BehaviorTree();
#if UNITY_EDITOR
            newTree.name = name;
#endif
            newTree.id = id;
            newTree.nodes = new BTNode[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                var newNode = newTree.nodes[i] = nodes[i].Clone();

#if UNITY_EDITOR
                newNode.breakPoint = nodes[i].breakPoint;
                newNode.nodePosition = nodes[i].nodePosition;
                newNode.comment = nodes[i].comment;
                newNode.title = nodes[i].title;
#endif
            }
            newTree.blackboardData = blackboardData;
            return newTree;
        }

#if UNITY_EDITOR
        public string DebugName
        {
            get
            {
                if (actor.IsAlive())
                    return $"{id} - {actor}";
                else
                    return id;
            }
        }
#endif
    }
}
