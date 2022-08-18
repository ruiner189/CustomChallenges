using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CustomChallenges
{
    public class DataObject
    {
        protected readonly Dictionary<String, object> _data = new Dictionary<string, object>();

        public static DataObject From(Dictionary<String, object> data)
        {
            DataObject obj = new DataObject();
            foreach(var pair in data)
            {
                obj._data.Add(pair.Key, pair.Value);
            }
            return obj;
        }

        protected static DataObject From(JToken token)
        {
            DataObject data = new DataObject();
            JObject obj = JObject.Load(token.CreateReader());
            foreach (JProperty property in obj.Properties())
            {
                data._data.Add(property.Name, data.GetValue(property.Value));
            }

            return data;
        }

        protected object GetValue(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Array:
                    List<object> values = new List<object>();
                    foreach (JToken child in token)
                    {
                        values.Add(GetValue(child));
                    }
                    return values.ToArray();

                case JTokenType.Object:
                    JObject obj = JObject.Load(token.CreateReader());
                    Dictionary<String, object> dict = new Dictionary<String, object>();
                    foreach (JProperty property in obj.Properties())
                    {
                        dict.Add(property.Name, GetValue(property.Value));
                    }
                    DataObject jsonObj = DataObject.From(dict);
                    return jsonObj;
                case JTokenType.String:
                    return (string)token;
                case JTokenType.Float:
                    return (float)token;
                case JTokenType.Integer:
                    return (int)token;
                case JTokenType.Boolean:
                    return (bool)token;
                default:
                    return null;
            }
        }

        public  ReadOnlyDictionary<String, object> GetData()
        {
            return new ReadOnlyDictionary<string, object>(_data);
        }

        public object GetEntry(String key)
        {
            return _data.GetValueOrDefault(key);
        }

        public T TryGetNestedEntry<T>(params String[] keys)
        {
            DataObject currentObject = this;
            for(int i = 0; i < keys.Length - 1; i++)
            {
                if (currentObject.TryGetEntry<DataObject>(keys[i], out DataObject nextObject) && nextObject != null)
                    currentObject = nextObject;
                else
                    return default;
            }
            if(currentObject.TryGetEntry<T>(keys[keys.Length - 1], out T result))
                return result;

            return default;

        }

        public bool TryGetEntry(String key, out Object result)
        {
            if (ContainsKey(key))
            {
                result = GetEntry(key);
                return true;
            }
            result = default;
            return false;
        }

        public T GetEntry<T>(String key)
        {
            Object result = GetEntry(key);
            if (result is T finalResult)
                return finalResult;
            return default;
        }

        public bool TryGetEntry<T>(String key, out T result)
        {
            if (ContainsKey(key))
            {
                result = GetEntry<T>(key);
                return true;
            }
            result = default;
            return false;
        }

        public T[] GetEntryArray<T>(String key)
        {
            Object entry = GetEntry(key);
            if (entry == null || entry is not object[]) return new T[0];

            Object[] array = (Object[])entry;
            T[] newArray = new T[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                newArray[i] = (T)array[i];
            }
            return newArray;
        }

        public bool TryGetEntryArray<T>(String key, out T[] result)
        {
            if (ContainsKey(key))
            {
                result = GetEntryArray<T>(key);
                return true;
            }
            result = new T[0];
            return false;
        }

        public bool ContainsKey(String key)
        {
            return _data.ContainsKey(key);
        }
    }
}
