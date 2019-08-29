
namespace Saro.BT
{
    public enum ObserverAborts
    {
        NONE,// do not abort nothing
        SELF,// abort itself, and any sub-trees running under this node
        LOWER_PRIORITY,// abort any nodes to the right of this node
        BOTH,// SELF & LOWER_PRIORITY
    }
}