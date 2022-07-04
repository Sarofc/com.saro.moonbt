using System;
using System.Collections.Generic;

namespace Saro.BT
{
    [Serializable]
    public partial class BBKeySelector : IEquatable<BBKeySelector>
    {
        public string keyName;

        public static implicit operator string(BBKeySelector keySelector)
        {
            return keySelector.keyName;
        }

        public bool Equals(BBKeySelector other) => keyName == other.keyName;
    }

#if UNITY_EDITOR
    public partial class BBKeySelector
    {
        [NonSerialized]
        public BlackboardData data;

        public BlackboardKeyType GetBlackboardKeyType()
        {
            if (data == null) return null;

            var entry = data.GetKeyEntryByName(this);

            if (entry == null) return null;

            return entry.keyType;
        }

        public static void SetBBField(object obj, BlackboardData data)
        {
            var fields = GetBBKeySelectors(obj);
            for (int i = 0; i < fields.Count; i++)
            {
                BBKeySelector field = fields[i];
                field.data = data;
            }
        }

        public static List<BBKeySelector> GetBBKeySelectors(object obj)
        {
            var results = new List<BBKeySelector>();
            var fields = obj.GetType().GetFields();
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(BBKeySelector))
                {
                    var value = (BBKeySelector)field.GetValue(obj);
                    if (value == null) value = Activator.CreateInstance<BBKeySelector>();
                    results.Add(value);
                }
            }

            return results;
        }
    }
#endif
}
