
using System;
using UnityEngine.Assertions;

namespace Saro.BT
{
    // Coding base
    public class Actions : ActionsBase
    {
        private Action m_onInit;
        private Func<bool?> m_onTick;

        public Actions(Action onInit) : base("Actions", true)
        {
            m_onInit = onInit;
        }

        public Actions(Action onInit, Func<bool?> onTick) : base("Actions", false)
        {
            m_onInit = onInit;
            m_onTick = onTick;
        }

        protected override void InitNode()
        {
            m_onInit?.Invoke();
        }

        protected override bool? TickNode()
        {
            return m_onTick?.Invoke();
        }


        #region Old
        //public enum Request
        //{
        //    START,
        //    UPDATE,
        //    CANCEL,
        //}

        //private Func<bool> m_singleFrameFunc = null;
        //private Func<bool, Result> m_multiFrameFunc = null;
        //private Func<Request, Result> m_multiFrameFunc1 = null;
        //private Action m_action = null;

        //private bool m_blocked = false;

        //        public Actions(System.Action action) : base("Actions")
        //        {
        //            this.m_action = action;
        //        }

        //        public Actions(Func<bool> singleFrameFunc) : base("Actions")
        //        {
        //            this.m_singleFrameFunc = singleFrameFunc;
        //        }

        //        public Actions(Func<bool, Result> multiFrameFunc) : base("Actions")
        //        {
        //            this.m_multiFrameFunc = multiFrameFunc;
        //        }

        //        public Actions(Func<Request, Result> multiFrameFunc1) : base("Actions")
        //        {
        //            this.m_multiFrameFunc1 = multiFrameFunc1;
        //        }

        //        protected override void InternalStart()
        //        {
        //            if (m_action != null)
        //            {
        //                m_action.Invoke();
        //                m_result = Result.SUCCESS;
        //                Stopped(true);
        //            }
        //            else if (m_multiFrameFunc != null)
        //            {
        //                m_result = m_multiFrameFunc.Invoke(false);
        //                if (m_result == Result.PROGRESS)
        //                {
        //                    RootNode.Clock.AddUpdateObserver(OnUpdateFunc);
        //                }
        //                else if (m_result == Result.BLOCKED)
        //                {
        //                    m_blocked = true;
        //                    RootNode.Clock.AddUpdateObserver(OnUpdateFunc);
        //                }
        //                else
        //                {
        //                    Stopped(m_result == Result.SUCCESS);
        //                }
        //            }
        //            else if (m_multiFrameFunc1 != null)
        //            {
        //                m_result = m_multiFrameFunc1.Invoke(Request.START);
        //                if (m_result == Result.PROGRESS)
        //                {
        //                    RootNode.Clock.AddUpdateObserver(OnUpdateFunc1);
        //                }
        //                else if (m_result == Result.BLOCKED)
        //                {
        //                    m_blocked = true;
        //                    RootNode.Clock.AddUpdateObserver(OnUpdateFunc1);
        //                }
        //                else
        //                {
        //                    Stopped(m_result == Result.SUCCESS);
        //                }
        //            }
        //            else if (m_singleFrameFunc != null)
        //            {
        //                Stopped(m_singleFrameFunc.Invoke());
        //            }
        //        }

        //        protected override void InternalAbort()
        //        {
        //            if (m_multiFrameFunc != null)
        //            {
        //                m_result = m_multiFrameFunc.Invoke(true);
        //                Assert.AreNotEqual(m_result, Result.PROGRESS, "The Task has to return Result.SUCCESS, or return Result.FAILED/BLOCKED after being cancelled!");
        //                RootNode.Clock.RemoveUpdateObserver(OnUpdateFunc);
        //                Stopped(m_result == Result.SUCCESS);
        //            }

        //            else if (m_multiFrameFunc1 != null)
        //            {
        //                m_result = m_multiFrameFunc1.Invoke(Request.CANCEL);
        //                Assert.AreNotEqual(m_result, Result.PROGRESS, "The Task has to return Result.SUCCESS, or return Result.FAILED/BLOCKED after being cancelled!");
        //                RootNode.Clock.RemoveUpdateObserver(OnUpdateFunc1);
        //                Stopped(m_result == Result.SUCCESS);
        //            }
        //#if UNITY_EDITOR
        //            else
        //            {
        //                Assert.IsTrue(false, "InternalCancel called on a single frame action on " + this);
        //            }
        //#endif
        //        }

        //        private void OnUpdateFunc()
        //        {
        //            m_result = m_multiFrameFunc.Invoke(false);
        //            if (m_result != Result.PROGRESS && m_result != Result.BLOCKED)
        //            {
        //                RootNode.Clock.RemoveUpdateObserver(OnUpdateFunc);
        //                Stopped(m_result == Result.SUCCESS);
        //            }
        //        }

        //        private void OnUpdateFunc1()
        //        {
        //            m_result = m_multiFrameFunc1.Invoke(m_blocked ? Request.START : Request.UPDATE);
        //            if (m_result == Result.BLOCKED)
        //            {
        //                m_blocked = true;
        //            }
        //            else if (m_result == Result.PROGRESS)
        //            {
        //                m_blocked = false;
        //            }
        //            else
        //            {
        //                RootNode.Clock.RemoveUpdateObserver(OnUpdateFunc1);
        //                Stopped(m_result == Result.SUCCESS);
        //            }
        //        }

        #endregion

    }
}