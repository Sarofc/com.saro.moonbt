using UnityEngine;

namespace Saro.BT.Sample
{
    public class TreeRunner : MonoBehaviour
    {
        void Start()
        {
            var treeComp = GetComponent<TreeComponent>();
            treeComp.Init(this);
            treeComp.Run();
        }
    }
}