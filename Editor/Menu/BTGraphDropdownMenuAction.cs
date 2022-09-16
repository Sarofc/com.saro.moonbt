using System;
using UnityEngine.UIElements;

namespace Saro.BT.Designer
{
    public class BTGraphDropdownMenuAction : DropdownMenuAction
    {
        public BTGraphDropdownMenuAction(
            string actionName,
            Action<DropdownMenuAction> actionCallback,
            Func<DropdownMenuAction, Status> actionStatusCallback,
            object userData = null
        ) : base(actionName, actionCallback, actionStatusCallback, userData) {
        }

        public BTGraphDropdownMenuAction(
            string actionName,
            Action<DropdownMenuAction> actionCallback
        ) : this(actionName, actionCallback, (e) => DropdownMenuAction.Status.Normal, null) {
        }
    }
}