using System;
using System.Collections.Generic;

namespace Saro.BT.Designer
{
    [Serializable]
    public class BTCopyPasteData
    {
        public List<BTNode> nodes = new();
        public List<(int indexA, int indexB)> edges = new();
    }
}
