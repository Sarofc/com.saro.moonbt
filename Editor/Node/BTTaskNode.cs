using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Saro.BT.Designer
{
    public class BTTaskNode : BTGraphNode
    {
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            evt.menu.MenuItems().Add(new BTGraphDropdownMenuAction("Change", (a) =>
            {
                GraphView.OnNodeChangeRequest?.Invoke(this, evt);
            }, (DropdownMenuAction a) => !Application.isPlaying ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled));
        }

        protected override void OnSetBehavior()
        {
            base.OnSetBehavior();

            if (NodeBehavior is RunBehaviour _runBehavior)
            {
                UnregisterCallback<MouseDownEvent>(OnOpenSubTree, TrickleDown.TrickleDown);
                RegisterCallback<MouseDownEvent>(OnOpenSubTree, TrickleDown.TrickleDown);
            }
            else
            {
                UnregisterCallback<MouseDownEvent>(OnOpenSubTree, TrickleDown.TrickleDown);
            }
        }

        private void OnOpenSubTree(MouseDownEvent evt)
        {

            if (evt.clickCount == 2)
            {
                if (NodeBehavior is RunBehaviour _runBehavior)
                {
                    if (_runBehavior.subtreeAsset != null)
                        Selection.activeObject = _runBehavior.subtreeAsset;
                    else
                        Debug.LogError("subTree is null");
                }
            }
        }

        protected override string OnValidate(Stack<BTGraphNode> stack) => null;

        protected override void OnCommit(Stack<BTGraphNode> stack)
        {
        }

        protected override void OnClearStyle()
        {
        }

        public override BTGraphNode GetChildAt(int _) => throw new NotSupportedException("TaskNode shouldn't have child");

        public override int ChildCount() => 0;
    }
}