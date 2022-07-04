using System;

namespace Saro.BT
{
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