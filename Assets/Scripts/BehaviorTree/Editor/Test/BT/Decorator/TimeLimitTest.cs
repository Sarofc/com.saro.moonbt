using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saro.BT
{
    public class TimeLimitTest : Test
    {
        [Test]
        public void Timeout()
        {
            for (int i = 0; i < 10; i++)
            {
                var rndTime = UnityEngine.Random.Range(0.01f, 10f);

                MockNode child = new MockNode();
                var sut = new TimeLimit(rndTime).Decorate(child);
                var bt = CreateBehaviorTree(sut);
                bt.Start();

                Assert.AreEqual(sut.CurrentStatus, Node.NodeStatus.Active, "TimeLimit AVTIVE");
                Assert.AreEqual(child.CurrentStatus, Node.NodeStatus.Active, "Start, child should be ACTIVED");

                Timer.Tick(rndTime);

                Assert.IsTrue(bt.DidFinish);
                Assert.IsFalse(bt.WasSuccess);
                //bt.Cancel();
            }
        }

        [Test]
        public void Child_Return_First()
        {
            for (int i = 0; i < 10; i++)
            {
                var rndTime = UnityEngine.Random.Range(.01f, 10f);

                MockNode child = new MockNode();
                var sut = new TimeLimit(rndTime).Decorate(child);
                var bt = CreateBehaviorTree(sut);
                bt.Start();

                Assert.AreEqual(sut.CurrentStatus, Node.NodeStatus.Active, "TimeLimit AVTIVE");
                Assert.AreEqual(child.CurrentStatus, Node.NodeStatus.Active, "Start, child should be ACTIVED");

                Timer.Tick(rndTime * UnityEngine.Random.Range(0, .95f));
                child.Finish(true);

                Assert.AreEqual(child.CurrentStatus, Node.NodeStatus.Inactive, "Child finished, child should be INACTIVED");
                Assert.AreEqual(sut.debugLastResult, child.debugLastResult, "Return result should equal to child's result");

                //bt.Cancel();
            }
        }
    }
}
