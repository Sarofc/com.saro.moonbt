using System;
using System.Diagnostics;
using Saro.SEditor;

namespace Saro.BT
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Struct)]
    public class KeyOperationAttribute : CustomDrawerAttribute { }
}
