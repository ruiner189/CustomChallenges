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

        public  ReadOnlyDictionary<String, object> GetData()
        {
            return new ReadOnlyDictionary<string, object>(_data);
        }

        public object GetEntry(String key)
        {
            return _data.GetValueOrDefault(key);
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
            return (T)GetEntry(key);
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
