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
            var sut = new SimpleParallel(SimpleParallel.FinishMode.IMMEDIATE).OpenBranch(mainTask, bgTree);
            var bt = CreateBehaviorTree(sut);
            bt.Start();

            Assert.AreEqual(mainTask.CurrentStatus, Node.NodeStatus.Active);
            Assert.AreEqual(bgTree.CurrentStatus, Node.NodeStatus.Active);

            mainTask.Finish(true);

            Assert.AreEqual(mainTask.CurrentStatus, Node.NodeStatus.Inactive);
            Assert.AreEqual(bgTree.CurrentStatus, Node.NodeStatus.Inactive, "Immediate mode, bgTree should be stopped onced that mainTask complete");

            Assert.AreEqual(sut.CurrentStatus, Node.NodeStatus.Inactive, "Immediate mode, simple parallel should be done");
        }

        [Test]
        public void Stopped_Delayed()
        {
            MockNode mainTask = new MockNode();
            MockNode bgTree = new MockNode();
            var sut = new SimpleParallel(SimpleParallel.FinishMode.DELAYED).OpenBranch(mainTask, bgTree);
            var bt = CreateBehaviorTree(sut);
            bt.Start();

            Assert.AreEqual(mainTask.CurrentStatus, Node.NodeStatus.Active);
            Assert.AreEqual(bgTree.CurrentStatus, Node.NodeStatus.Active);

            mainTask.Finish(true);

            Assert.AreEqual(mainTask.CurrentStatus, Node.NodeStatus.Inactive);
            Assert.AreEqual(bgTree.CurrentStatus, Node.NodeStatus.Active, "Delayed mode, bgTree could be permitted to finish onced that mainTask complete");

            Assert.AreEqual(sut.CurrentStatus, Node.NodeStatus.Active,"Delayed mode, wait for bgTree");

            bgTree.Finish(false);

            Assert.AreEqual(sut.CurrentStatus, Node.NodeStatus.Inactive,"");
        }
    }
}
