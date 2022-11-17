using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using Newtonsoft.Json;
using UnityEngine.Serialization;
using Saro.Entities;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Saro.BT.Utility;

namespace Saro.BT
{
    /*
     * TODO
     * 
     * 1. json保存
     * 2. action执行完，立即调用reset清理接口
     * 3. stop整个树是，正在执行的action也要调用reset
     * 
     */

    [CreateAssetMenu(menuName = "Gameplay/" + nameof(BehaviorTree))]
    public partial class BehaviorTree
        : ScriptableObject // TODO 这个加个editor宏，就可以打包后，直接使用json之类的来加载了，json导出忽略editor下的字段
    {
        private BTBehaviorIterator m_MainIterator;

        private UpdateList<Timer> m_ActiveTimers;

        [JsonIgnore]
        public int ActiveTimerCount => m_ActiveTimers.Data.Count;

        [JsonIgnore]
        public bool IsTreeInitialized { get; private set; }

        [JsonIgnore]
        public BTBlackboard Blackboard { get; private set; }

        /// <summary>
        /// 行为树的拥有者
        /// </summary>
        [JsonIgnore]
        public EcsEntity actor;

        [JsonIgnore]
        public int Height { get; internal set; }

        [JsonIgnore]
        public BTNode Root => nodes.Length == 0 ? null : nodes[0];

        #region Serialization

        [FormerlySerializedAs("Nodes")]
        [SerializeReference]
        [JsonProperty]
        internal BTNode[] nodes = { };

        [FormerlySerializedAs("BlackboardData")]
        [JsonProperty]
        [SerializeField]
        internal BlackboardData blackboardData;

        #endregion

        public static BehaviorTree CreateRuntimeTree(BehaviorTree treeAsset, EcsEntity actor)
        {
            var runtimeTree = Clone(treeAsset);

            if (runtimeTree != null)
            {
#if UNITY_EDITOR
                runtimeTree.name = runtimeTree.name.Replace("(Clone)", "(Runtime)");
#endif
            }
            else
            {
                Debug.LogError("null tree set for.");
            }

            runtimeTree.actor = actor;

            runtimeTree.Initialize();

            return runtimeTree;
        }

        public static void ReleaseRuntimeTree(BehaviorTree runtimeTree)
        {
            // TODO pooling
        }

        /// <summary>
        /// 初始化树
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        public void Initialize()
        {
            if (Root == null)
            {
                throw new NullReferenceException("Root");
            }

            if (blackboardData != null && blackboardData.entries.Count > 0)
            {
                if (Blackboard == null)
                {
                    Blackboard = new BTBlackboard(blackboardData);
                }
            }

            m_ActiveTimers = new UpdateList<Timer>();

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

            // TODO timer
        }

        public void Tick(float deltaTime)
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

        private void TickTimers(float deltaTime)
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

                node.OnInitialize();
            }
        }

        public bool IsRunning() => m_MainIterator != null && m_MainIterator.IsRunning;

        public BTNode.EStatus LastStatus() => m_MainIterator.LastExecutedStatus;

        public static BehaviorTree Clone(BehaviorTree originTree)
        {
            var newTree = ScriptableObject.Instantiate(originTree);
            return newTree;
        }

        public static string ToJson(BehaviorTree tree)
        {
            return JsonHelper.ToJson(tree);
        }

        public static BehaviorTree FromJson(string json)
        {
            return JsonHelper.FromJson(json);
        }


#if UNITY_EDITOR

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var node in nodes)
            {
                sb.AppendLine(node.ToString());
            }

            return sb.ToString();
        }

        internal void SetNodes_Editor(IEnumerable<BTNode> nodes)
        {
            this.nodes = nodes.ToArray();
            for (int i = 0; i < this.nodes.Length; i++)
            {
                BTNode node = this.nodes[i];
                node.Tree = this;
                node.preOrder = i;
            }
        }

        [SerializeField]
        [HideInInspector]
        internal Vector3 graphPosition;
        [SerializeField]
        [HideInInspector]
        internal Vector3 graphScale = Vector3.one;

        /*[HideInInspector]*/
        //public List<BTNode> unusedNodes = new List<BTNode>();
#endif
    }

}
