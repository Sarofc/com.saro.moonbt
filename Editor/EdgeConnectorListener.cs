using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;

namespace Saro.BT.Designer
{
    class EdgeConnectorListener : IEdgeConnectorListener
    {
        readonly GraphView m_GraphView;
        readonly CreateBTNodeProvider m_CreateBTNodeProvider;
        readonly EditorWindow m_EditorWindow;

        public EdgeConnectorListener(GraphView graph, CreateBTNodeProvider searchWindowProvider, EditorWindow editorWindow)
        {
            m_GraphView = graph;
            m_CreateBTNodeProvider = searchWindowProvider;
            m_EditorWindow = editorWindow;
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
            //Debug.LogError($"OnDropOutsidePort: {edge} {position}");

            //var draggedPort = (edge.output != null ? edge.output.edgeConnector.edgeDragHelper.draggedPort : null) ?? (edge.input != null ? edge.input.edgeConnector.edgeDragHelper.draggedPort : null);

            var draggedPort = edge.output;

            m_CreateBTNodeProvider.ConnectedPort = draggedPort;
            //Debug.LogError($"draggedPort: {draggedPort}");

            var screenMousePos = m_EditorWindow.position.position + position;

            bool result = SearchWindow.Open(new SearchWindowContext(screenMousePos), m_CreateBTNodeProvider);
            if (!result)
            {
                Debug.LogError("failed");
            }
        }

        public void OnDrop(GraphView graphView, Edge edge)
        {
            //Debug.LogError($"OnDrop: {edge}");

            //var leftSlot = edge.output;
            //var rightSlot = edge.input;
            //if (leftSlot != null && rightSlot != null)
            //{
            //    var newEdge = BTGraphView.ConnectPorts(leftSlot, rightSlot);
            //    m_GraphView.AddElement(newEdge);

            //    //m_Graph.owner.RegisterCompleteObjectUndo("Connect Edge");
            //    //m_Graph.Connect(leftSlot.slotReference, rightSlot.slotReference);
            //}
        }
    }
}
