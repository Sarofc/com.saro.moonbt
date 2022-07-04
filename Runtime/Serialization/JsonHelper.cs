using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.UnityConverters;
using Saro.Utility;

namespace Saro.BT
{
    internal static class JsonHelper
    {
        static JsonSerializerSettings s_Settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Converters = ReflectionUtility.GetSubClassTypesAllAssemblies(typeof(PartialConverter)).Select(t => Activator.CreateInstance(t) as JsonConverter).ToArray(),
        };

        public static string ToJson(BehaviorTree tree)
        {
            return JsonConvert.SerializeObject(tree, s_Settings);
        }

        public static BehaviorTree FromJson(string json)
        {
            return JsonConvert.DeserializeObject<BehaviorTree>(json, s_Settings);
        }

        public static string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj, s_Settings);
        }

        public static T DeserializeObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, s_Settings);
        }
    }
}
