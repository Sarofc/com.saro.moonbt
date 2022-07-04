using System;

namespace Saro.BT
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class BTNodeAttribute : Attribute
    {
        public readonly string iconPath;
        public readonly string nodeDesc;

        public BTNodeAttribute(string texturePath, string nodeDesc = null)
        {
            this.iconPath = texturePath;
            this.nodeDesc = nodeDesc;
        }
    }
}