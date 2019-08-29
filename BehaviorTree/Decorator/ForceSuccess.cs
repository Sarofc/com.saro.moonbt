using UnityEngine;
using System.Collections;

namespace Saro.BT
{
    public class ForceSuccess : Decorator
    {
        public ForceSuccess(Node decorated) : base("ForceSuccess", decorated)
        {
        }

        protected override void InternalStart()
        {
            m_decorated.Start();
        }

        protected override void InternalCancel()
        {
            m_decorated.Cancel();
        }

        protected override void InternalChildStopped(Node child, bool success)
        {
            // always return success
            Stopped(true);
        }
    }

}