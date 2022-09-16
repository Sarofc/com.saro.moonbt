
using System.Text;

namespace Saro.BT
{
    public class TestService : BTService
    {
        protected override void ServiceTick()
        {
            UnityEngine.Debug.Log("test...");
        }

        public override void Description(StringBuilder builder)
        {
            base.Description(builder);

            builder.Append("\ntest service...");
        }
    }
}