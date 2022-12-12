using Saro.Entities;
using Saro.Pool;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Saro.BT
{
    partial class BehaviorTree
    {
        private static Dictionary<string, BehaviorTree> s_Templates;
#if UNITY_EDITOR
        private static Dictionary<string, BehaviorTree> s_Templates_Editor;
#endif

        private readonly static Dictionary<string, ObjectPool<BehaviorTree>> s_CachedRuntimeTrees = new();

        public bool Poolable { get; private set; }

        public static void Load(List<BehaviorTree> list)
        {
            Load_Editor();

            if (s_Templates == null)
                s_Templates = new(list.Count);
            else
                s_Templates.Clear();

            foreach (var item in list)
                s_Templates.Add(item.id, item);
        }

        [Conditional("UNITY_EDITOR")]
        private static void Load_Editor()
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
                    s_Templates_Editor.Add(tree.name, tree);
                else
                    Log.ERROR($"Editor BehaviorTree '{path}' is invalid");
            }
#endif
        }

        /// <summary>
        /// 通过 [id] 获取 或者 创建 运行时对象，内部池化
        /// </summary>
        /// <param name="treeId"></param>
        /// <param name="actor"></param>
        /// <returns></returns>
        public static BehaviorTree CetOrCreateRuntimeTree(string treeId, EcsEntity actor)
        {
            if (!s_CachedRuntimeTrees.TryGetValue(treeId, out var pool))
            {
                pool = new ObjectPool<BehaviorTree>(
                    onCreate: () =>
                    {
                        return CreateRuntimeTree(treeId, actor);
                    },
                    onGet: (tree) =>
                    {
                        tree.Poolable = true;
                    },
                    onRelease: (tree) =>
                    {
                        tree.ResetData();
                        tree.Poolable = false;
                    });
                s_CachedRuntimeTrees[treeId] = pool;
            }

            var runtimeTree = pool.Rent();
            Log.Assert(runtimeTree != null, $"CreateRuntimeTree failed. treeId: {treeId}");
            runtimeTree.actor = actor;

            Check(runtimeTree);

            return runtimeTree;
        }

        /// <summary>
        /// 通过 [id] 生成一个新的运行时对象
        /// </summary>
        /// <param name="treeId"></param>
        /// <param name="actor"></param>
        /// <returns></returns>
        public static BehaviorTree CreateRuntimeTree(string treeId, EcsEntity actor)
        {
            if (TryGetTemplateTree(treeId, out var template))
                return CreateRuntimeTree(template, actor);
            return null;
        }

        /// <summary>
        /// 通过 [模板对象] 生成一个新的运行时对象
        /// </summary>
        /// <param name="template"></param>
        /// <param name="actor"></param>
        /// <returns></returns>
        public static BehaviorTree CreateRuntimeTree(BehaviorTree template, EcsEntity actor)
        {
            var runtimeTree = template.Clone();
            runtimeTree.Initialize();
            runtimeTree.actor = actor;
            return runtimeTree;
        }

        /// <summary>
        /// 通过 [id] 获取模板对象
        /// </summary>
        /// <param name="treeId"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        public static bool TryGetTemplateTree(string treeId, out BehaviorTree template)
        {
#if UNITY_EDITOR //&& false
            var map = s_Templates_Editor;
#else
            var map = s_Templates;
#endif
            return map.TryGetValue(treeId, out template);
        }

        public static void ReleaseRuntimeTree(BehaviorTree runtimeTree)
        {
            if (runtimeTree.Poolable)
            {
                if (s_CachedRuntimeTrees.TryGetValue(runtimeTree.id, out var pool))
                {
                    pool.Return(runtimeTree);
                }
            }
        }

        /// <summary>
        /// TODO 将runtimeTree对象和template对象对比，提前发现池化bug
        /// </summary>
        /// <param name="runtimeTree"></param>
        [Conditional("UNITY_EDITOR")]
        static void Check(BehaviorTree runtimeTree)
        {
            if (TryGetTemplateTree(runtimeTree.id, out var templateTree))
            {
                // check nodes
                for (int i = 0; i < runtimeTree.nodes.Length; i++)
                {
                    var node = runtimeTree.nodes[i];
                    var node1 = templateTree.nodes[i];
                    var result = CompareFileds(node, node1);
                    if (!string.IsNullOrEmpty(result))
                    {
                        Log.ERROR($"[BT] treeId: {runtimeTree.id} node not equal. node index: {i}.\n {node.GetType()} {result}");
                    }
                }

                // check blackboard
            }
            else
            {
                Log.ERROR("[BT] fatal error");
            }
        }

        /// <summary>
        /// 判断两个相同引用类型的对象的属性值是否相等
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj1">对象1</param>
        /// <param name="obj2">对象2</param>
        /// <returns></returns>
        static string CompareFileds<T>(T obj1, T obj2)
        {
            //为空判断
            if (obj1 == null && obj2 == null)
                return null;
            else if (obj1 == null || obj2 == null)
            {
                if (obj1 != null)
                {
                    if (IgnoreCompare(obj1.GetType()))
                        return null;
                }

                if (obj2 != null)
                {
                    if (IgnoreCompare(obj2.GetType()))
                        return null;
                }

                return $"{obj1} or {obj2} is null";
            }

            var type1 = obj1.GetType();
            var type2 = obj2.GetType();
            if (type1 != type2)
                return $"{obj1.GetType().Name} or {obj2.GetType().Name} type not equal";

            if (IgnoreCompare(type1))
                return null;

            var field = type1.GetFields();
            foreach (var po in field)
            {
                if (IsCanCompare(po.FieldType))
                {
                    var v1 = po.GetValue(obj1);
                    var v2 = po.GetValue(obj2);
                    //if (!po.GetValue(obj1).Equals(po.GetValue(obj2)))
                    if (!Object.Equals(v1, v2))
                    {
                        return $"field {po.Name} not equal. obj1: {v1}  obj2: {v2}";
                    }
                }
                else
                {
                    var b = CompareFileds(po.GetValue(obj1), po.GetValue(obj2));
                    if (!string.IsNullOrEmpty(b))
                        return b;
                }
            }

            return null;
        }

        /// <summary>
        /// 该类型是否可直接进行值的比较
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        static bool IsCanCompare(Type t)
        {
            if (t.IsValueType)
            {
                return true;
            }
            else
            {
                //String是特殊的引用类型，它可以直接进行值的比较
                if (t.FullName == typeof(string).FullName)
                {
                    return true;
                }
                return false;
            }
        }

        static bool IgnoreCompare(Type t)
        {
            return typeof(Delegate).IsAssignableFrom(t);
        }
    }
}
