using UnityEngine;

namespace Saro.BT
{
    [System.Serializable]
    public class BlackboardEntry
    {
        public string keyName;
        public string keyDesc;
        //public string guid; // TODO use GUID to ref

        [SerializeReference]
        public BlackboardKeyType keyType;
    }
}
