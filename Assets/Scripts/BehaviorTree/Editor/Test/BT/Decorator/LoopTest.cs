using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saro.BT
{
    public class LoopTest : Test
    {
        [Test]
        public void LoopAnyTimes()
        {
            var times = new int[] { 5, 33, 88, 2486, 35896 };

            var count = new int[times.Length];

            for (int i = 0; i < times.Length; i++)
            {
                var child = new Actions(() => ++count[i]);

                var sut = new Loop(times[i]).Decorate(child);
                Root behaviorTree = new Root().Decorate(sut);

                behaviorTree.RepeatRoot = false;
                behaviorTree.Start();

                for (int j = 0; j < times[i]; j++)
                {
                    behaviorTree.Clock.Tick(0.01f);
                }

                Assert.AreEqual(count[i], times[i]);
            }
        }

        [Test]
        public void ShouldFail_When_Decorated_Fails()
        {
            MockNode failingChild = new MockNode();
            var sut = new Loop().Decorate(failingChild);
            TestRoot behaviorTree = CreateBehaviorTree(sut);

            behaviorTree.Start();
            Assert.AreEqual(Node.NodeStatus.Active, sut.CurrentStatus);

            failingChild.Finish(false);

            Assert.AreEqual(Node.NodeStatus.Inactive, sut.CurrentStatus);
            Assert.IsTrue(behaviorTree.DidFinish);
            Assert.IsFalse(behaviorTree.WasSuccess);
        }

        [Test]
        public void ShouldSucceed_WhenDecorated_Succeeded_GivenTimes()
        {
            MockNode succeedingChild = new MockNode();
            var sut = new Loop(3).Decorate(succeedingChild);
            TestRoot behaviorTree = CreateBehaviorTree(sut);

            behaviorTree.Start();
            Assert.AreEqual(Node.NodeStatus.Active, sut.CurrentStatus);

            for (int i = 0; i < 2; i++)
            {
                succeedingChild.Finish(true);
                Assert.AreEqual(Node.NodeStatus.Active, sut.CurrentStatus);
                Assert.IsFalse(behaviorTree.DidFinish);
                Timer.Tick(0.01f);
            }

            succeedingChild.Finish(true);
            Assert.AreEqual(Node.NodeStatus.Inactive, sut.CurrentStatus);
            Assert.IsTrue(behaviorTree.DidFinish);
            Assert.IsTrue(behaviorTree.WasSuccess);
        }

    }
}
