using System;

namespace Saro.BT
{
    ///Derive this to create custom attributes to be drawn with an ObjectAttributeDrawer<T>.
    [AttributeUsage(AttributeTargets.Field)]
    abstract public class CustomDrawerAttribute : Attribute { }
}
