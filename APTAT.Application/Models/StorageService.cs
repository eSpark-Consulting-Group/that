using System;
using System.Collections.Generic;

namespace APTAT.Application.Models
{
    public class StorageService : IStorageService
    {
        private static Dictionary<String, Object> _dictionary = new Dictionary<String, Object>();

        public void SetValue(String key, Object value)
        {
            _dictionary[key] = value;
        }

        public T GetValue<T>(String key)
        {
            if(_dictionary.ContainsKey(key) == false)
            {
                return default(T);
            }

            Object value = _dictionary[key];
            _dictionary.Remove(key);
            return (T) value;
        }
    }
}
