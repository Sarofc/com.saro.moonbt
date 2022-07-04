using System;
using System.Collections.Generic;
using Saro.Pool;

namespace Saro.BT
{
    public static class TreeTraversal
    {
        public static IEnumerable<T> PreOrder<T>(T root) where T : IIterableNode<T>
        {
            using (StackPool<T>.Rent(out var stack))
            {
                if (root != null) stack.Push(root);
                while (stack.Count != 0)
                {
                    var top = stack.Pop();
                    if (top == null) continue;
                    yield return top;

                    for (int i = top.ChildCount() - 1; i >= 0; i--)
                    {
                        var child = top.GetChildAt(i);
                        stack.Push(child);
                    }
                }
            }
        }

        public static IEnumerable<T> PostOrder<T>(T root) where T : IIterableNode<T>
        {
            if (root != null)
            {
                using (StackPool<T>.Rent(out var stack))
                {
                    using (HashSetPool<T>.Rent(out var visited))
                    {
                        stack.Push(root);

                        while (stack.Count != 0)
                        {
                            var current = stack.Peek();

                            while (!visited.Contains(current) && current.ChildCount() != 0)
                            {
                                for (int i = current.ChildCount() - 1; i >= 0; i--)
                                {
                                    var child = current.GetChildAt(i);
                                    stack.Push(child);
                                }

                                visited.Add(current);
                                current = stack.Peek();
                            }

                            var top = stack.Pop();
                            if (top == null) continue;
                            yield return top;
                        }
                    }
                }
            }
        }

        public static IEnumerable<(T node, int level)> LevelOrder<T>(T root) where T : IIterableNode<T>
        {
            var currentLevel = -1;
            var queueNodeCount = 0;

            if (root == null) yield break;

            using (QueuePool<T>.Rent(out var queue))
            {
                queue.Enqueue(root);

#if UNITY_EDITOR
                int times = 0;
#endif

                while (queue.Count != 0)
                {
                    if (queueNodeCount > 0)
                    {
                        queueNodeCount -= 1;
                    }
                    if (queueNodeCount == 0)
                    {
                        queueNodeCount = queue.Count;
                        currentLevel += 1;
                    }

#if UNITY_EDITOR
                    if (times++ > 10000) throw new Exception("times out...");
#endif

                    var top = queue.Dequeue();
                    if (top == null) continue;
                    yield return (top, currentLevel);

                    for (int i = 0; i < top.ChildCount(); i++)
                    {
                        var child = top.GetChildAt(i);
                        queue.Enqueue(child);
                    }
                }
            }
        }
    }
}