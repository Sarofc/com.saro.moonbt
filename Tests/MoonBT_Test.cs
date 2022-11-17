//using System.Collections;
//using System.Collections.Generic;
//using NUnit.Framework;
//using UnityEngine;

// TODO unittest

//namespace Saro.BT.Test
//{
//    public class MoonBT_Test
//    {
//        [Test]
//        public static void TimeLimit_Timeout()
//        {
//            var timeLimit = new TimeLimit()
//            {
//                timer = new() { interval = 3f, deviation = 0f }
//            };
//            var wait = new Wait()
//            {
//                timer = new() { interval = 4f, deviation = 0f }
//            };
//            var tree = new BehaviorTree();
//            tree.nodes = new BTNode[2]
//            {
//                timeLimit,
//                wait,
//            };
//            tree.Initialize();

//            timeLimit.SetChildAt(wait);

//            tree.BeginTraversal();
//            tree.Tick(3.5f);

//            var result = tree.LastStatus();
//            Assert.IsTrue(result == BTNode.EStatus.Failure);
//        }
//    }
//}
