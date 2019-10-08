using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Saro.BT
{
    // TODO fix
    public class CooldownTest : Test
    {
        [Test]
        public void Cooldown_Test_Process_and_Reached()
        {
            for (int i = 0; i < 10; i++)
            {
                var rndTime = UnityEngine.Random.Range(0, 10f);

                MockNode child = new MockNode();
                var sut = new Cooldown(rndTime).Decorate(child);
                var bt = CreateBehaviorTree(sut);
                bt.Start();
                Assert.AreEqual(child.CurrentStatus, Node.NodeStatus.Active, "Cooldown, child should be ACTIVED");
                child.Finish(true);

                //Timer.Tick(rndTime - UnityEngine.Random.Range(0, rndTime));

                //Timer.Tick(rndTime);
                Assert.AreEqual(sut.CurrentStatus, Node.NodeStatus.Inactive);
                Assert.AreEqual(child.CurrentStatus, Node.NodeStatus.Inactive, "In Cooldown, child should not be ACTIVED");

            }
        }
    }
}
