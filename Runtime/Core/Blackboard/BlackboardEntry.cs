using UnityEngine;

namespace Saro.BT
{
    [System.Serializable]
    public class BlackboardEntry
    {
        public string keyName;

#if UNITY_EDITOR
        [SerializeField]
        internal string keyDesc;
#endif

        [SerializeReference]
        public BlackboardKeyType keyType;

        public override string ToString()
        {
            return $"{keyName}";
        }
    }
}
