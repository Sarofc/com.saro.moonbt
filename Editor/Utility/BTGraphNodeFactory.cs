using System;
using UnityEditor.Experimental.GraphView;

namespace Saro.BT.Designer
{
    public static class BTGraphNodeFactory
    {
        public static BTGraphNode CreateGraphNode(Type type, BTGraphView graphView, BTNode nodeBehavior = null)
        {
            if (!typeof(BTNode).IsAssignableFrom(type))
            {
                throw new InvalidOperationException($"{type} is not impl form {nameof(BTNode)}");
            }

            BTGraphNode graphNode;

            // 特殊子类要跑在前面
            if (type == typeof(Root))
            {
                graphNode = new BTRootNode();
            }
            else if (type == typeof(SimpleParallel))
            {
                graphNode = new BTSimpleParallelNode();
            }
            else if (type.IsSubclassOf(typeof(BTComposite)))
            {
                graphNode = new BTCompositeNode();
            }
            else if (type.IsSubclassOf(typeof(BTDecorator)))
            {
                graphNode = new BTDecoratorNode();
            }
            else if (type.IsSubclassOf(typeof(BTService)))
            {
                graphNode = new BTServiceNode();
            }
            else if(type.IsSubclassOf(typeof(BTTask)))
            {
                graphNode = new BTTaskNode();
            }
            else
            {
                throw new Exception($"CreateGraphNode failed. unhandle type: {type}");
            }

            graphNode.PostInit(graphView);

            graphNode.SetBehavior(nodeBehavior ?? Activator.CreateInstance(type) as BTNode);

            return graphNode;
        }
    }
}