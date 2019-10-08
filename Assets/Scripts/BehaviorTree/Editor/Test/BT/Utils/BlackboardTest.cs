using NUnit.Framework;
namespace Saro.BT
{
#pragma warning disable 618 // deprecation

    public class BlackboardTest
    {
        private Clock clock;
        private Blackboard sut;

        [SetUp]
        public void SetUp()
        {
            this.clock = new Clock();
            this.sut = new Blackboard("", clock);
        }

        [Test]
        public void DontAddObservers()
        {
            //this.sut.AddObserver("test", (Blackboard.Type type, object value) => { });
            this.sut.Set("test", 1);
            sut.Get<int>("test").Value += 2;

            this.clock.Tick(1f);
        }

        [Test]
        public void ShouldNotNotifyObservers_WhenNoClockUpdate()
        {
            bool notified = false;
            this.sut.AddObserver("test", (Blackboard.Type type, object value) =>
            {
                notified = true;
            });

            this.sut.Set("test", 1f);
            Assert.IsFalse(notified);
        }

        [Test]
        public void ShouldNotifyObservers_WhenClockUpdate()
        {
            bool notified = false;
            this.sut.AddObserver("test", (Blackboard.Type type, object value) =>
            {
                notified = true;
            });

            this.sut.Set("test", 1f);
            this.clock.Tick(1f);
            Assert.IsTrue(notified);
        }

        [Test]
        public void ShouldNotNotifyObserver_WhenRemovedDuringOtherObserver()
        {
            bool notified = false;
            System.Action<Blackboard.Type, object> obs1 = null;
            System.Action<Blackboard.Type, object> obs2 = null;

            obs1 = (Blackboard.Type type, object value) =>
            {
                Assert.IsFalse(notified);
                notified = true;
                this.sut.RemoveObserver("test", obs2);
            };
            obs2 = (Blackboard.Type type, object value) =>
            {
                Assert.IsFalse(notified);
                notified = true;
                this.sut.RemoveObserver("test", obs1);
            };
            this.sut.AddObserver("test", obs1);
            this.sut.AddObserver("test", obs2);

            this.sut.Set("test", 1f);
            this.clock.Tick(1f);
            Assert.IsTrue(notified);
        }

        [Test]
        public void ShouldAllowToSetToNull_WhenAlreadySertToNull()
        {
            this.sut.Set("test", 1f);
            Assert.AreEqual(this.sut.Get<float>("test").Value, 1f);
            this.sut.Set<object>("test", null);
            this.sut.Set<object>("test", null);
            Assert.AreEqual(this.sut.Get("test").Value, null);
            this.sut.Set("test", "something");
            Assert.AreEqual(this.sut.Get("test").Value, "something");
        }

        [Test]
        public void NewDefaultValuesShouldBeCompatible()
        {
            Assert.AreEqual(this.sut.Get<bool>("not-existing"), this.sut.Get<bool>("not-existing"));
            Assert.AreEqual(this.sut.Get<int>("not-existing"), this.sut.Get<int>("not-existing"));
            //            Assert.AreEqual(this.sut.Get<float>("not-existing"), this.sut.GetFloat("not-existing"));
            Assert.AreEqual(this.sut.Get<UnityEngine.Vector3>("not-existing"), this.sut.Get<UnityEngine.Vector3>("not-existing"));
        }


        // check for https://github.com/meniku/NPBehave/issues/17
        [Test]
        public void ShouldListenToEvents_WhenUsingChildBlackboard()
        {
            Blackboard rootBlackboard = new Blackboard("rootbb", clock);
            Blackboard blackboard = new Blackboard("childbb", rootBlackboard, clock);

            // our mock nodes we want to query for status
            MockNode firstChild = new MockNode(false); // false -> fail when aborted
            MockNode secondChild = new MockNode(false);

            // conditions for each subtree that listen the BB for events
            var firstCondition = new BlackboardCondition<bool>("branch1", Operator.IS_EQUAL, true, ObserverAborts.BOTH).Decorate(firstChild);
            var secondCondition = new BlackboardCondition<bool>("branch2", Operator.IS_EQUAL, true, ObserverAborts.BOTH).Decorate(secondChild);

            // set up the tree
            var selector = new Selector().OpenBranch(firstCondition, secondCondition);
            var behaviorTree = new TestRoot(blackboard, clock).Decorate(selector);

            // intially we want to activate branch2
            rootBlackboard.Set("branch2", true);
            UnityEngine.Debug.Log(blackboard.Get("branch2").Value);

            // start the tree
            behaviorTree.Start();

            // tick the timer to ensure the blackboard notifies the nodes
            clock.Tick(0.1f);


            // verify the second child is running
            Assert.AreEqual(Node.NodeStatus.Inactive, firstChild.CurrentStatus);
            Assert.AreEqual(Node.NodeStatus.Active, secondChild.CurrentStatus);

            // change keys so the first conditions get true, too
            rootBlackboard.Set("branch1", true);
            UnityEngine.Debug.Log(blackboard.Get("branch1").Value);

            // tick the timer to ensure the blackboard notifies the nodes
            clock.Tick(0.1f);

            // now we should be in branch1
            Assert.AreEqual(Node.NodeStatus.Active, firstChild.CurrentStatus);
            Assert.AreEqual(Node.NodeStatus.Inactive, secondChild.CurrentStatus);
        }
    }
}