using System.Text;

namespace Saro.BT
{
    [BTNode("Force_Success_24x")]
    public sealed class ForceSuccess : BTDecorator
    {
        protected override void OnDecoratorEnter()
        {
        }

        protected override void OnDecoratorExit()
        {
        }

        public override EStatus OnExecute(float deltaTime)
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
