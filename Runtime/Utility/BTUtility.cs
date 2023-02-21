using Saro.Utility;
using UnityEngine;

namespace Saro.BT
{
    internal class BTUtility
    {
        public static int StringToHash(string value)
        {
            return (int)HashUtility.GetCrc32(value);
        }

        public static int StringToHash(ScriptableObject so)
        {
            return so != null ? StringToHash(so.name) : 0;
        }
    }
}
