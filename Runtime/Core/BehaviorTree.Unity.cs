#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Saro.BT
{
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
}

#endif
