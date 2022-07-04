using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Saro.BT.Designer
{
    public class BTGraphMiniMap : MiniMap
    {
        public BTGraphMiniMap(BTGraphView view)
        {
            //var styleSheet = Resources.Load<StyleSheet>("Stylesheets/BTGraphMiniMap");
            //styleSheets.Add(styleSheet);
            graphView = view;
        }
    }
}
