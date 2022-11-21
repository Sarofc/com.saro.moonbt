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
     * 1. json保存，目前json会剔除so相关的引用，debug时，有一些逻辑问题
     * 3. 清理整个树时，正在执行的action也要调用reset
     * 
     */

    public partial class BehaviorTree
    {
        private BTBehaviorIterator m_MainIterator;

        private UpdateList<Timer> m_ActiveTimers;

        [JsonIgnore]
        public int ActiveTimerCount => m_ActiveTimers.Data.Count;

        [JsonIgnore]
        public bool IsTreeInitialized { get; private set; }

        [JsonIgnore]
        public BTBlackboard Blackboard { get; private set; }

        [JsonIgnore]
        public EcsEntity actor;

        [JsonIgnore]
        public int Height { get; internal set; }

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

            m_ActiveTimers.Clear();
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

                //node.OnValidate();
                node.OnInitialize();
            }
        }

        public bool IsRunning() => m_MainIterator != null && m_MainIterator.IsRunning;

        public BTNode.EStatus LastStatus() => m_MainIterator.LastExecutedStatus;

        public BehaviorTree Clone()
        {
            var newTree = new BehaviorTree();
            newTree.id = id;
            newTree.nodes = new BTNode[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
                newTree.nodes[i] = nodes[i].Clone();
            newTree.blackboardData = blackboardData;
            return newTree;
        }
    }

    partial class BehaviorTree
    {
        private static Dictionary<string, BehaviorTree> s_Templates;
#if UNITY_EDITOR
        public static Dictionary<string, BehaviorTree> s_Templates_Editor;
#endif
        public static void Load(List<BehaviorTree> list)
        {
#if UNITY_EDITOR
            var finds = AssetDatabase.FindAssets($"t:Saro.BT.BehaviorTree", new[] { "Assets" });

            if (s_Templates_Editor == null)
                s_Templates_Editor = new(finds.Length);
            else
                s_Templates_Editor.Clear();

            foreach (var item in finds)
            {
                var path = AssetDatabase.GUIDToAssetPath(item);
                var tree = AssetDatabase.LoadAssetAtPath<BehaviorTree>(path);
                if (tree)
                {
                    s_Templates_Editor.Add(tree.name, tree);
                }
                else
                {
                    Log.ERROR($"Editor BehaviorTree '{path}' is invalid");
                }
            }
#endif

            if (s_Templates == null)
                s_Templates = new(list.Count);
            else
                s_Templates.Clear();

            foreach (var item in list)
            {
                // TODO 检查序列化的数据是否和so一致，提前在编辑器下发现问题

                s_Templates.Add(item.id, item);
            }
        }

        // TODO 通过id，加载实例，池化策略：以id为准，同一id，数据一致，都是静态的数据
        public static BehaviorTree CreateRuntimeTree(string treeId, EcsEntity actor)
        {
#if UNITY_EDITOR //&& false
            var map = s_Templates_Editor;
#else
            var map = s_Templates;
#endif
            if (map.TryGetValue(treeId, out var tree))
            {
                return CreateRuntimeTree(tree, actor);
            }
            Log.ERROR("CreateRuntimeTree failed. treeId: " + treeId);
            return null;
        }

        public static BehaviorTree CreateRuntimeTree(BehaviorTree template, EcsEntity actor)
        {
            var runtimeTree = template.Clone();
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
            runtimeTree.ResetData();
        }
    }

#if UNITY_EDITOR
    [CreateAssetMenu(menuName = "Gameplay/" + nameof(BehaviorTree))]
    partial class BehaviorTree : ScriptableObject
    {
        [SerializeField]
        [HideInInspector]
        internal Vector3 graphPosition;
        [SerializeField]
        [HideInInspector]
        internal Vector3 graphScale = Vector3.one;

        /*[HideInInspector]*/
        //public List<BTNode> unusedNodes = new List<BTNode>();

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

        internal void OnValidate()
        {
            id = this.name;

            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    node.OnValidate();
                }
            }
        }
    }
#endif
}
