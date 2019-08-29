using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saro.BT
{
    public class SimpleParallelTest : Test
    {
        [Test]
        public void Stopped_Immediate()
        {
            MockNode mainTask = new MockNode();
            MockNode bgTree = new MockNode();
            var sut = new SimpleParallel(mainTask, bgTree, SimpleParallel.FinishMode.IMMEDIATE);
            var bt = CreateBehaviorTree(sut);
            bt.Start();

            Assert.AreEqual(mainTask.CurrentState, Node.State.ACTIVE);
            Assert.AreEqual(bgTree.CurrentState, Node.State.ACTIVE);

            mainTask.Finish(true);

            Assert.AreEqual(mainTask.CurrentState, Node.State.INACTIVE);
            Assert.AreEqual(bgTree.CurrentState, Node.State.INACTIVE, "Immediate mode, bgTree should be stopped onced that mainTask complete");

            Assert.AreEqual(sut.CurrentState, Node.State.INACTIVE, "Immediate mode, simple parallel should be done");
        }

        [Test]
        public void Stopped_Delayed()
        {
            MockNode mainTask = new MockNode();
            MockNode bgTree = new MockNode();
            var sut = new SimpleParallel(mainTask, bgTree, SimpleParallel.FinishMode.DELAYED);
            var bt = CreateBehaviorTree(sut);
            bt.Start();

            Assert.AreEqual(mainTask.CurrentState, Node.State.ACTIVE);
            Assert.AreEqual(bgTree.CurrentState, Node.State.ACTIVE);

            mainTask.Finish(true);

            Assert.AreEqual(mainTask.CurrentState, Node.State.INACTIVE);
            Assert.AreEqual(bgTree.CurrentState, Node.State.ACTIVE, "Delayed mode, bgTree could be permitted to finish onced that mainTask complete");

            Assert.AreEqual(sut.CurrentState, Node.State.ACTIVE,"Delayed mode, wait for bgTree");

            bgTree.Finish(false);

            Assert.AreEqual(sut.CurrentState, Node.State.INACTIVE,"");
        }
    }
}
