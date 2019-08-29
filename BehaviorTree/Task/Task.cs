
namespace Saro.BT
{
    /// <summary>
    /// Leaf Node
    /// </summary>
    public abstract class Task : Node
    {
        public enum Result
        {
            SUCCESS,
            FAILED,
            BLOCKED,
            PROGRESS
        }


        protected Result m_result;

#if UNITY_EDITOR
        public Result ResultType { get => m_result; }
#endif

        public Task(string name) : base(name)
        {
        }
    }
}