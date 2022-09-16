using System;
using System.Collections.Generic;
using UnityEngine;

namespace Saro.BT
{
    [Serializable]
    public sealed class BlackboardData /*: ScriptableObject*/
    {
        //public BlackboardData parent; // TODO 看需不需要 父黑板？

        public List<BlackboardEntry> entries = new(0);

        public BlackboardEntry GetKeyEntryByName(string keyName)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry.keyName == keyName)
                    return entry;
            }

            return null;
        }

        public int GetKeyIndexByName(string keyName)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry.keyName == keyName)
                    return i;
            }

            return -1;
        }
    }
}
