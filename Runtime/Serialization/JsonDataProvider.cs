//#define BSON

#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using Saro.Core;
using UnityEngine;
using System.Threading.Tasks;
using Saro.Utility;

namespace Saro.BT
{
    public class JsonDataProvider<T>
#if UNITY_EDITOR
    where T : ScriptableObject
#endif
    {

#if BSON
        private readonly string m_FilePath = $"Assets/ResRaw/Json/{typeof(T).Name}.bson";
#else
        private readonly string m_FilePath = $"Assets/ResRaw/Json/{typeof(T).Name}.json";
#endif

        public List<T> Load()
        {
#if BSON
            return LoadFromBson();
#else
            return LoadFromJson();
#endif
        }

        public async Task<List<T>> LoadAsync()
        {
#if BSON
            return await LoadFromBsonAsync();
#else
            return await LoadFromJsonAsync();
#endif
        }

        public void Save()
        {
#if UNITY_EDITOR
#if BSON
            ToBson(m_FilePath);
#else
            ToJson(m_FilePath);
#endif
#endif
        }

        private List<T> LoadFromJson()
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

        private async Task<List<T>> LoadFromJsonAsync()
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

#if BSON
        private List<T> LoadFromBson()
        {
#if UNITY_EDITOR //&& false
            using var fs = new FileStream(m_FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var configs = JsonHelper.FromBson<List<T>>(fs, true);
#else 
            var bytes = IAssetManager.Current.GetRawFileBytes(m_FilePath);
            using var ms = new MemoryStream(bytes);
            var configs = JsonHelper.FromBson<List<T>>(ms, true);
#endif
            return configs;
        }

        private async Task<List<T>> LoadFromBsonAsync()
        {
#if UNITY_EDITOR //&& false
            using var fs = new FileStream(m_FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var configs = JsonHelper.FromBson<List<T>>(fs, true);
            await Task.CompletedTask;
#else 
            var bytes = await IAssetManager.Current.GetRawFileBytesAsync(m_FilePath);
            using var ms = new MemoryStream(bytes);
            var configs = JsonHelper.FromBson<List<T>>(ms, true);
#endif
            // TODO 检测数据合法性

            return configs;
        }
#endif

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
                //    validator.OnValidate();
            }

            var json = JsonHelper.ToJson(list);
            var directory = Path.GetDirectoryName(file);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(file, json);
        }

#if BSON
        private void ToBson(string file)
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
                //    validator.OnValidate();
            }

            var directory = Path.GetDirectoryName(file);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using var fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            fs.Seek(0, SeekOrigin.Begin);
            fs.SetLength(0);
            JsonHelper.ToBson(fs, list);
        }
#endif

#endif
    }
}
