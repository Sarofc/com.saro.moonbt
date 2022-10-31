using System;
using System.Diagnostics;

namespace Saro.BT
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class BTRunTimeValueAttribute : Attribute
    {
        public readonly string label;

        public BTRunTimeValueAttribute(string label = null)
        {
            this.label = label;
        }
    }
}