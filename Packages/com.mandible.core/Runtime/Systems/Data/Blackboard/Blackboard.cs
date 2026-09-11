using System.Collections.Generic;
using System;
using UnityEngine;

namespace Mandible.Core.Data
{
    [System.Serializable]
    public class Blackboard
    {
        [SerializeReference] private Dictionary<object, object> map = new();

        // Key-Value Methods

        public T GetOrCreate<T>(object key) where T : new()
        {
            if (map.TryGetValue(key, out var obj))
                return (T)obj;

            T v = new();
            map[key] = v;
            return v;
        }

        public T Get<T>(object key) where T : class
        {
            map.TryGetValue(key, out var obj);
            return obj as T;
        }

        public T GetValue<T>(object key, T defaultValue = default) where T : struct
        {
            if (map.TryGetValue(key, out var obj) && obj is T castedValue)
            {
                return castedValue;
            }
            return defaultValue;
        }

        public object GetRawContainer(string keyName)
        {
            if (string.IsNullOrEmpty(keyName)) return null;

            if (map.TryGetValue(keyName, out object directMatch))
                return directMatch;

            // Normalize to compiler behavior
            string standardizedKey = keyName.Replace('.', '+'); 
            string shortName = keyName.Contains(".") ? keyName.Substring(keyName.LastIndexOf('.') + 1) : keyName;

            foreach (var kvp in map)
            {
                if (kvp.Key is System.Type typeKey)
                {
                    if (typeKey.Name == keyName || 
                        typeKey.Name == shortName ||
                        typeKey.FullName == keyName || 
                        typeKey.FullName == standardizedKey ||
                        typeKey.FullName.EndsWith(standardizedKey))
                    {
                        return kvp.Value;
                    }
                }

                else if (kvp.Key is string stringKey)
                {
                    if (stringKey == keyName || stringKey == shortName || stringKey.EndsWith(shortName))
                    {
                        return kvp.Value;
                    }
                }
            }
            
            return null;
        }

        public void Set(object key, object value)
        {
            map[key] = value;
        }

        // Type-as-Key Overloads

        /// <summary>
        /// Gets or creates a reference object using its own System.Type as the key.
        /// </summary>
        public T GetOrCreate<T>() where T : new()
        {
            return GetOrCreate<T>(typeof(T));
        }

        /// <summary>
        /// Gets a reference object using its own System.Type as the key.
        /// </summary>
        public T Get<T>() where T : class
        {
            return Get<T>(typeof(T));
        }

        /// <summary>
        /// Stores an object using its own System.Type as the key.
        /// </summary>
        public void Set<T>(T value)
        {
            map[typeof(T)] = value;
        }
    }
}



