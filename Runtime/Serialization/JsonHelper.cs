using Newtonsoft.Json;
using Newtonsoft.Json.UnityConverters;

namespace Saro.BT
{
    internal static class JsonHelper
    {
        static JsonSerializerSettings s_Settings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            //Formatting = Formatting.Indented,
            Converters = IAutoJsonConverter.GetJsonConverters(),
        };

        public static string ToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj, s_Settings);
        }

        public static T FromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, s_Settings);
        }
    }
}
