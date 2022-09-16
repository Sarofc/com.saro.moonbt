using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;

namespace Saro.BT.Designer
{
    /// <summary>
    /// BTGraphView 节点 排版
    /// </summary>
    public static class BTGraphFormatter
    {
        public static void PositionNodesNicely(BTGraphNode root, Vector2 anchor)
        {
            var positioning = new FormatPositioning();

            // 1. record position
            foreach (var node in TreeTraversal.PostOrder(root))
            {
                node.nodePosition = node.GetPosition();
            }

            // 2. position vertical
            foreach (var node in TreeTraversal.PostOrder(root))
            {
                PositionVertical(node, positioning);
            }

            // 3. position horizontal
            foreach (BTGraphNode node in TreeTraversal.PreOrder(root))
            {
                PositionHorizontal(node);
            }

            // 4. move root
            SetSubtreePosition(root, TreeTraversal.PreOrder(root).Skip(1), anchor, Vector3.zero);

            // 5. apply position
            foreach (var node in TreeTraversal.PostOrder(root))
            {
                node.SetPosition(node.nodePosition);
            }
        }

        public static void SetSubtreePosition(BTGraphNode root, IEnumerable<BTGraphNode> otherNodes, Vector2 dragPosition, Vector2 offset)
        {
            float min = float.MinValue;

            if (root.ParentNode != null)
            {
                float nodeLeft = root.nodePosition.xMin;
                float parentRight = root.ParentNode.nodePosition.xMax;

                if (nodeLeft < parentRight)
                {
                    min = parentRight;
                }
            }

            Vector2 oldPosition = root.nodePosition.center;

            Vector2 newPosition = dragPosition - offset;
            newPosition.x = Mathf.Clamp(newPosition.x, min, float.MaxValue);

            root.nodePosition.center = newPosition;

            // 有其他node时，也一并移动
            if (otherNodes != null)
            {
                Vector2 pan = root.nodePosition.center - oldPosition;

                foreach (BTGraphNode node in otherNodes)
                {
                    node.nodePosition.center = node.nodePosition.center + pan;
                }
            }
        }

        private static void PositionVertical(BTGraphNode node, FormatPositioning positioning)
        {
            float yCoord;

            int childCount = node.ChildCount();

            if (childCount > 1)
            {
                Vector2 firstChildPos = node.GetChildAt(0).nodePosition.center;
                Vector2 lastChildPos = node.GetChildAt(childCount - 1).nodePosition.center;
                float yMid = (firstChildPos.y + lastChildPos.y) / 2f;

                yCoord = yMid;
                positioning.yIntermediate = yMid;
            }
            else if (childCount == 1)
            {
                yCoord = positioning.yIntermediate;
            }
            else
            {
                float leafHeight = node.nodePosition.size.y;
                positioning.yLeaf += 0.5f * (positioning.lastLeafHeight + leafHeight) + FormatPositioning.yLeafSeparation;

                yCoord = positioning.yLeaf;
                positioning.yIntermediate = positioning.yLeaf;
                positioning.lastLeafHeight = leafHeight;
            }

            node.nodePosition.center = new Vector2(0, yCoord);
        }

        private static void PositionHorizontal(BTGraphNode node)
        {
            if (node.ParentNode != null)
            {
                BTGraphNode parent = node.ParentNode;

                //Debug.LogError($"{node.NodeBehavior.Title}'s parent: {parent.NodeBehavior.Title}");

                float xSeperation = parent.ChildCount() == 1
                  ? FormatPositioning.xLevelSeparation / 2f
                  : FormatPositioning.xLevelSeparation;

                float x = parent.nodePosition.position.x + parent.nodePosition.size.x + xSeperation;
                float y = node.nodePosition.position.y;

                node.nodePosition.position = new Vector2(x, y);
            }
        }

        private class FormatPositioning
        {
            public float yLeaf = 0f;
            public float yIntermediate = 0f;
            public float lastLeafHeight = 0f;

            public const float yLeafSeparation = 20f;
            public const float xLevelSeparation = 50f;
        }
    }
}
