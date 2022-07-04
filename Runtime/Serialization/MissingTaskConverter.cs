//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using UnityEngine;

//namespace Saro.BT
//{
//    public class MissingTaskConverter : JsonConverter
//    {
//        public override bool CanConvert(Type objectType)
//        {
//            return true;
//            //return typeof(BTTask).IsAssignableFrom(objectType);
//        }

//        public override bool CanWrite => false;

//        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//        {
//            var jsonObject = JObject.Load(reader);
//            if (jsonObject.Type == JTokenType.Object)
//            {
//                if (jsonObject.TryGetValue("$type", out var token))
//                {
//                    var typeName = token.ToString();

//                    // class丢失
//                    if (Type.GetType(typeName) == null)
//                    {
//                        var missingTask = Activator.CreateInstance<MissingTask>();
//                        missingTask.missingNodeInfo = jsonObject.ToString();
//                        serializer.Populate(jsonObject.CreateReader(), missingTask);

//                        return missingTask;
//                    }

//                    // 返回原对象
//                    return existingValue;
//                }
//            }

//            Debug.LogError($"unhandle type: {reader}");
//            return null;
//        }

//        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//        { }
//    }
//}
