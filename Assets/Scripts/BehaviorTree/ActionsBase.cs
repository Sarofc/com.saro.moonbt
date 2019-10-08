
namespace Saro.BT
{
    public abstract class ActionsBase : Task
    {
        private bool m_singleFrame;

        public ActionsBase(string name, bool singleFrame) : base(name)
        {
            m_singleFrame = singleFrame;
        }

        protected override void InternalStart()
        {
            InitNode();

            if (m_singleFrame)
            {
                Stopped(true);
            }
            else
            {
                Clock.AddUpdateObserver(Tick);
            }
        }

        protected override void InternalAbort()
        {
            if (!m_singleFrame)
            {
                Clock.RemoveUpdateObserver(Tick);
            }
            Stopped(false);
        }

        protected void Tick()
        {
            var result = TickNode();

            if (result.HasValue)
            {
                Clock.RemoveUpdateObserver(Tick);
                Stopped(result);
            }
        }

        // single frame action
        protected virtual void InitNode() { }

        // multi frame action, default value is false.
        // true     : task successed
        // false    : task failed
        // null     : running
        protected virtual bool? TickNode() { return false; }
    }
}
