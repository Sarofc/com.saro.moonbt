using System.Text;

#if FIXED_POINT_MATH
using Single = sfloat;
#else
using Single = System.Single;
#endif

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

        public override EStatus OnExecute(Single deltaTime)
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
