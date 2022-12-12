using System;
using System.Collections.Generic;
using UnityEngine;

namespace Saro.BT.Designer
{
    [Serializable]
    public class BTCopyPasteData
    {
        public List<BTNode> nodes = new();
        public List<(int indexA, int indexB)> edges = new();
        // 由于 BTNode的nodePosition不再序列化为json，所以新增此变量存储节点位置
        public List<Rect> nodesPosition = new();
    }
}
