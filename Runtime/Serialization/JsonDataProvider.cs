using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using Saro.Core;
using UnityEngine;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Saro.BT
{
    public class JsonDataProvider<T>
#if UNITY_EDITOR
    where T : ScriptableObject
#endif
    {
        private readonly string m_FilePath = $"Assets/ResRaw/Json/{typeof(T).Name}.json";

        public List<T> Load()
        {
#if UNITY_EDITOR //&& false
            var json = File.ReadAllText(m_FilePath);
#else 
            var bytes = IAssetManager.Current.GetRawFileBytes(m_FilePath);
            var json = Encoding.UTF8.GetString(bytes);
#endif
            var configs = JsonHelper.FromJson<List<T>>(json);

            // TODO 检测数据合法性

            return configs;
        }

        public async Task<List<T>> LoadAsync()
        {
#if UNITY_EDITOR
            var json = await File.ReadAllTextAsync(m_FilePath);
#else
            var bytes = await IAssetManager.Current.GetRawFileBytesAsync(m_FilePath);
            Log.INFO($"GetRawFileAsync 2: {m_FilePath} {bytes.Length}"); // 执行了
            var json = Encoding.UTF8.GetString(bytes);
            Log.INFO($"{typeof(T).Name} {json}"); // 执行了
#endif
            var configs = JsonHelper.FromJson<List<T>>(json); // 游戏画面卡死了，cpu100%
            Log.INFO($"{typeof(T).Name} count: {configs.Count}"); // 未执行！

            // TODO 检测数据合法性

            return configs;
        }

        public void Save()
        {
#if UNITY_EDITOR
            ToJson(m_FilePath);
#endif
        }

#if UNITY_EDITOR
        private void ToJson(string file)
        {
            //Debug.LogError($"t:{typeof(T).Name}");
            var list = new List<T>();
            var finds = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { "Assets" });

            foreach (var item in finds)
            {
                var path = AssetDatabase.GUIDToAssetPath(item);
                var config = AssetDatabase.LoadAssetAtPath<T>(path);
                list.Add(config);

                //if (config is IDataValidator validator)
                //{
                //    validator.OnValidate();
                //}
            }

            var json = JsonHelper.ToJson(list);
            var directory = Path.GetDirectoryName(file);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(file, json);
        }
#endif
    }
}
