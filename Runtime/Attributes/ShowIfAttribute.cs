using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saro.BT
{
    public sealed class ShowIfAttribute : Attribute
    {
        public readonly string requiredPropertyName;
        public readonly object checkObject;

        public ShowIfAttribute(string requiredPropertyName, object checkObject)
        {
            this.requiredPropertyName = requiredPropertyName;
            this.checkObject = checkObject;
        }
    }
}
