using System.Text;
using UnityEngine;

namespace Saro.BT.Sample
{
    public sealed class BTS_FindTarget : BTService
    {
        protected override void ServiceTick()
        {
            if (Blackboard != null)
            {
                var lastValue = Blackboard.GetValue<bool>("hasSeePlayer");
                Blackboard.SetValue("hasSeePlayer", !lastValue);

                Debug.LogError($"set hasSeePlayer: {!lastValue}");
            }
        }

        public override void Description(StringBuilder builder)
        {
            base.Description(builder);

            builder.AppendLine();
            builder.Append("set hasSeePlayer value.");
        }
    }
}
