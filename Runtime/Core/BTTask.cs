
namespace Saro.BT
{
#if UNITY_EDITOR
    //[Newtonsoft.Json.JsonConverter(typeof(MissingTaskConverter))]
#endif
    [BTNode("Task_24x")]
    public abstract class BTTask : BTNode
    {
        public sealed override void OnAbort(int childIndex) { }

        public sealed override void OnChildEnter(int childIndex) { }

        public sealed override void OnChildExit(int childIndex, EStatus status) { }

        public sealed override int ChildCount() => 0;

        public sealed override BTNode GetChildAt(int childIndex) => null;

        public sealed override void SetChildAt(BTNode node, int childIndex) { }

        public sealed override int MaxChildCount() => 0;
    }
}

