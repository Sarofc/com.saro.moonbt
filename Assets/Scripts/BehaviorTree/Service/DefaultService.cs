
using System;
using System.Text;
using UnityEngine.Assertions;

namespace Saro.BT
{
    /// <summary>
    /// Coding
    /// </summary>
    public class DefaultService : Service    {
        private Action m_serviceMethod;

        public DefaultService(float interval, float randomDeviation, Action service) : base(interval, randomDeviation)
        {
            Assert.IsTrue(interval > 0.001f, "interval must greater than or equal to 0.001f");
            Assert.IsTrue(interval >= 0.00f, "randomDeviation must greater than or equal to 0.00f");

            m_serviceMethod = service;
        }

        public DefaultService(Action service)
        {
            m_serviceMethod = service;
        }

        protected override void TickService()
        {
            m_serviceMethod?.Invoke();
        }
    }
}