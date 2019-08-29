using UnityEngine;
using System.Collections;
using NUnit.Framework;

namespace Saro.BT
{
    public class SequenceTest : Test
    {
        [Test]
        public void ShouldFail_If_1st_Child_Fails()
        {
            var _1st = new MockNode();
            var _2nd = new MockNode();
            var _3rd = new MockNode();

            var sut = new Sequence(_1st, _2nd, _3rd);
            var bt = CreateBehaviorTree(sut);
            bt.Start();

            _1st.Finish(false);

            Assert.AreEqual(_1st.CurrentState, Node.State.INACTIVE);
            Assert.AreEqual(_2nd.CurrentState, Node.State.INACTIVE);
            Assert.AreEqual(_3rd.CurrentState, Node.State.INACTIVE);

            Assert.IsFalse(sut.debugLastResult);
        }

        [Test]
        public void ShouldFail_If_2nd_Child_Fails()
        {
            var _1st = new MockNode();
            var _2nd = new MockNode();
            var _3rd = new MockNode();

            var sut = new Sequence(_1st, _2nd, _3rd);
            var bt = CreateBehaviorTree(sut);
            bt.Start();

            _1st.Finish(true);
            _2nd.Finish(false);

            Assert.AreEqual(_1st.CurrentState, Node.State.INACTIVE);
            Assert.AreEqual(_2nd.CurrentState, Node.State.INACTIVE);
            Assert.AreEqual(_3rd.CurrentState, Node.State.INACTIVE);

            Assert.IsFalse(sut.debugLastResult);
        }

        [Test]
        public void ShouldFail_If_3rd_Child_Fails()
        {
            var _1st = new MockNode();
            var _2nd = new MockNode();
            var _3rd = new MockNode();

            var sut = new Sequence(_1st, _2nd, _3rd);
            var bt = CreateBehaviorTree(sut);
            bt.Start();

            _1st.Finish(true);
            _2nd.Finish(true);
            _3rd.Finish(false);

            Assert.AreEqual(_1st.CurrentState, Node.State.INACTIVE);
            Assert.AreEqual(_2nd.CurrentState, Node.State.INACTIVE);
            Assert.AreEqual(_3rd.CurrentState, Node.State.INACTIVE);

            Assert.IsFalse(sut.debugLastResult);
        }


        //-----------------------------------------------------------
        [Test]
        public void ShouldSuccess_When_All_Childs_Success()
        {
            var _1st = new MockNode();
            var _2nd = new MockNode();
            var _3rd = new MockNode();

            var sut = new Sequence(_1st, _2nd, _3rd);
            var bt = CreateBehaviorTree(sut);
            bt.Start();

            _1st.Finish(true);
            _2nd.Finish(true);
            _3rd.Finish(true);

            Assert.AreEqual(_1st.CurrentState, Node.State.INACTIVE);
            Assert.AreEqual(_2nd.CurrentState, Node.State.INACTIVE);
            Assert.AreEqual(_3rd.CurrentState, Node.State.INACTIVE);

            Assert.IsTrue(sut.debugLastResult);
        }
    }
}
