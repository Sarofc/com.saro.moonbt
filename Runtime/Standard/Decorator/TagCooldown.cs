//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Saro.BT
//{
//    /*
//     * TODO 
//     *  
//     *  需要 引用到 GameplayTag 的dll
//     *  还需要 SetTagCooldown 节点配合使用，任务/装饰 节点各一个
//     * 
//     */

//    [BTNode("Cooldown_24x", "TODO “冷却”装饰节点。\n自身条件基于GameplayTag的Timer是否结束。")]
//    public sealed class TagCooldown : BTDecorator
//    {
//        public int gameplayTag;

//        public bool adddToExistingDuration;

//        public override void OnEnter()
//        { }

//        public override void OnExit()
//        { }

//        internal override void OnValidate()
//        {
//            base.OnValidate();

//            abortType = EAbortType.None;

//            UpdateAborts();
//        }
//    }
//}
