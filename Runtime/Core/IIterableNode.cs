
namespace Saro.BT
{
    public interface IIterableNode<T>
    {
        T GetChildAt(int childIndex);

        void SetChildAt(T node, int childIndex);

        int ChildCount();
    }
}