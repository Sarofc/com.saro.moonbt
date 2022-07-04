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

            if (type == typeof(Root))
            {
                graphNode = new BTRootNode();
            }
            else if (type == typeof(SimpleParallel))
            {
                graphNode = new BTSimpleParallelNode();
            }
            // 具体子类需要放在前面
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
            else
            {
                graphNode = new BTTaskNode();
            }

            graphNode.PostInit(graphView);

            graphNode.SetBehavior(nodeBehavior ?? Activator.CreateInstance(type) as BTNode);

            return graphNode;
        }
    }
}