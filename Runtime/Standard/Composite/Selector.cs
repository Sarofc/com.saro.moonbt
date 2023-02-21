using System;
using System.Text;

#if FIXED_POINT_MATH
using Single = Saro.FPMath.sfloat;
#else
using Single = System.Single;
#endif

namespace Saro.BT
{
    [Serializable]
    [BTNode("Selector_24x","“选择器”节点\n从上至下执行子节点，如果一个子节点执行成功，则此节点返回成功，后续子节点不再执行。\n如果所有子节点都失败，则此节点也失败。")]
    public class Selector : BTComposite
    {
        public override EStatus OnExecute(Single deltaTime)
        {
            if (m_LastChildExitStatus == EStatus.Success)
            {
                return EStatus.Success;
            }

            var nextChild = CurrentChild();

            if (nextChild == null)
            {
                return EStatus.Failure;
            }
            else
            {
                Iterator.Traverse(nextChild);
                return EStatus.Running;
            }
        }
    }
}