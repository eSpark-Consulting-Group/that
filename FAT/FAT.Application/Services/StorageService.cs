using System;
using System.Collections.Generic;

using Composition = APTAT.Common.CompositionHelper;

namespace APTAT.Application.Services
{

    public class StorageService : IStorageService
    {
        public StorageService()
        { 
            _dictionary = new Dictionary<String, Object>(); 
        }
        
        private readonly Dictionary<String, Object> _dictionary;

        public void SetValue(String key, Object value)
        {
            _dictionary[key] = value;
        }

        public bool ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        public T GetValue<T>(String key, Boolean remove)
        {
            if(_dictionary.ContainsKey(key) == false)
            {
                return default(T);
            }

            Object value = _dictionary[key];
            if(remove)
            {
                _dictionary.Remove(key);    
            }
            return (T) value;
        }
    }
}
