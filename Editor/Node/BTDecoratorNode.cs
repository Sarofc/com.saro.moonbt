using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Saro.BT.Designer
{
    public class BTDecoratorNode : BTAuxiliaryNode
    {
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            evt.menu.MenuItems().Add(new BTGraphDropdownMenuAction("Change", (a) =>
            {
                GraphView.OnNodeChangeRequest?.Invoke(this, evt);
            }, (DropdownMenuAction a) => !Application.isPlaying ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled));
        }

        protected override string OnValidate(Stack<BTGraphNode> stack)
        {
            return base.OnValidate(stack);
        }
    }
}