using UnityEngine;
using System.Collections;

namespace Saro.BT
{
    /*
     * 
     * always return success
     * 
     */
    public class ForceSuccess : Decorator
    {
        public ForceSuccess() : base("ForceSuccess")
        {
            m_abortType = ObserverAborts.NONE;

            m_abortNone = false;
            m_abortSelf = false;
            m_abortLowerPriority = false;
        }

        protected override void InternalStart()
        {
            m_childNode.Start();
        }

        protected override void InternalAbort()
        {
            m_childNode.Abort();
        }

        protected override void InternalChildStopped(Node child, bool? result)
        {
            Stopped(true);
        }

        public override string GetStaticDescription()
        {
            return string.Format("{0}:Always Success", Name);
        }
    }

}