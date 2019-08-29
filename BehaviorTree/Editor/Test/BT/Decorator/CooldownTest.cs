using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Saro.BT
{
    public class CooldownTest : Test
    {
        [Test]
        public void Cooldown_Test_Process_and_Reached()
        {
            for (int i = 0; i < 10; i++)
            {
                var rndTime = UnityEngine.Random.Range(0, 10f);

                MockNode inactiveChild = new MockNode();
                Cooldown sut = new Cooldown(rndTime, inactiveChild);
                var bt = CreateBehaviorTree(sut);
                bt.Start();

                Timer.Tick(rndTime - UnityEngine.Random.Range(0, rndTime));
                Assert.AreEqual(inactiveChild.CurrentState, Node.State.INACTIVE, "In Cooldown, child should not be ACTIVED");

                Timer.Tick(rndTime);
                Assert.AreEqual(inactiveChild.CurrentState, Node.State.ACTIVE, "Cooldown, child should be ACTIVED");

                bt.Cancel();
            }
        }
    }
}
