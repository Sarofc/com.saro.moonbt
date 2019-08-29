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
        public void ShouldFail_When_Decorated_Fails()
        {
            MockNode failingChild = new MockNode();
            Loop sut = new Loop(-1, failingChild);
            TestRoot behaviorTree = CreateBehaviorTree(sut);

            behaviorTree.Start();
            Assert.AreEqual(Node.State.ACTIVE, sut.CurrentState);

            failingChild.Finish(false);

            Assert.AreEqual(Node.State.INACTIVE, sut.CurrentState);
            Assert.IsTrue(behaviorTree.DidFinish);
            Assert.IsFalse(behaviorTree.WasSuccess);
        }

        [Test]
        public void ShouldSucceed_WhenDecorated_Succeeded_GivenTimes()
        {
            MockNode succeedingChild = new MockNode();
            Loop sut = new Loop(3, succeedingChild);
            TestRoot behaviorTree = CreateBehaviorTree(sut);

            behaviorTree.Start();
            Assert.AreEqual(Node.State.ACTIVE, sut.CurrentState);

            for (int i = 0; i < 2; i++)
            {
                succeedingChild.Finish(true);
                Assert.AreEqual(Node.State.ACTIVE, sut.CurrentState);
                Assert.IsFalse(behaviorTree.DidFinish);
                Timer.Tick(0.01f);
            }

            succeedingChild.Finish(true);
            Assert.AreEqual(Node.State.INACTIVE, sut.CurrentState);
            Assert.IsTrue(behaviorTree.DidFinish);
            Assert.IsTrue(behaviorTree.WasSuccess);
        }

    }
}
