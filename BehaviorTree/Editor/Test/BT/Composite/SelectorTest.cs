using UnityEngine;
using System.Collections;
using NUnit.Framework;

namespace Saro.BT
{
    public class SelectorTest : Test
    {
        [Test]
        public void Should_DoNext_If_1st_Child_Fails()
        {
            var _1st = new MockNode();
            var _2nd = new MockNode();
            var _3rd = new MockNode();

            var sut = new Selector(_1st, _2nd, _3rd);
            var bt = CreateBehaviorTree(sut);
            bt.Start();

            _1st.Finish(false);
            //_2nd.Finish(true);
            //_3rd.Finish(false);

            Assert.AreEqual(_1st.CurrentState, Node.State.INACTIVE);
            Assert.AreEqual(_2nd.CurrentState, Node.State.ACTIVE);
            Assert.AreEqual(_3rd.CurrentState, Node.State.INACTIVE);

            Assert.AreEqual(sut.debugLastResult, _2nd.debugLastResult);
        }

        [Test]
        public void Should_DoNext_If_2nd_Child_Fails()
        {
            var _1st = new MockNode();
            var _2nd = new MockNode();
            var _3rd = new MockNode();

            var sut = new Selector(_1st, _2nd, _3rd);
            var bt = CreateBehaviorTree(sut);
            bt.Start();

            _1st.Finish(false);
            _2nd.Finish(false);
            //_3rd.Finish(false);

            Assert.AreEqual(_1st.CurrentState, Node.State.INACTIVE);
            Assert.AreEqual(_2nd.CurrentState, Node.State.INACTIVE);
            Assert.AreEqual(_3rd.CurrentState, Node.State.ACTIVE);

            Assert.AreEqual(sut.debugLastResult, _3rd.debugLastResult);
        }
        //----------------------------------------------------------
        [Test]
        public void Should_Fail_when_All_Childs_Fails()
        {
            var _1st = new MockNode();
            var _2nd = new MockNode();
            var _3rd = new MockNode();

            var sut = new Selector(_1st, _2nd, _3rd);
            var bt = CreateBehaviorTree(sut);
            bt.Start();

            _1st.Finish(false);
            _2nd.Finish(false);
            _3rd.Finish(false);

            Assert.AreEqual(_1st.CurrentState, Node.State.INACTIVE);
            Assert.AreEqual(_2nd.CurrentState, Node.State.INACTIVE);
            Assert.AreEqual(_3rd.CurrentState, Node.State.INACTIVE);

            Assert.IsFalse(sut.debugLastResult);
        }
    }
}
