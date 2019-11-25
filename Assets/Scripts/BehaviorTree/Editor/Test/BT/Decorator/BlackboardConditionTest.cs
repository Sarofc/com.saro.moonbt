using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saro.BT
{
    public class BlackboardConditionTest : Test
    {
        public class TestType { }

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Is_Key_Set_Or_Not()
        {
            var child = new MockNode();
            var sut = new BlackboardCondition<TestType>("Key1", Operator.IS_SET).Decorate(new Log("yahaha"));
            var bt = new Root().Decorate(sut);


            bt.Blackboard.AddObserver("Key1", (t, d) => UnityEngine.Debug.Log($"Raise event {t},{d.GetType()}"));
            bt.Start();

            Assert.AreEqual(Node.NodeStatus.Inactive, child.CurrentStatus, "is not set, child should be INACTIVED");
            bt.Blackboard.Set("Key1", new TestType());

            bt.Clock.Tick(1f);
            
            Assert.AreEqual(Node.NodeStatus.Active, child.CurrentStatus, "is set, child should be ACTIVED");
        }


    }
}
