using System.Text;

namespace Saro.BT
{
    [BTNode("Force_Success_24x")]
    public sealed class ForceSuccess : BTDecorator
    {
        public override EStatus OnExecute()
        {
            return EStatus.Success;
        }

        public override void Description(StringBuilder builder)
        {
            base.Description(builder);

            builder.Append("Always Success");
        }
    }
}
