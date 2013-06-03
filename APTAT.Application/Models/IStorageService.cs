using System;

namespace APTAT.Application.Models
{
    public class KnownKeys
    {
        public const String CheckedDirectoryItems = "CheckedDirectoryItems";
    }

    public interface IStorageService
    {
        void SetValue(String key, Object value);

        T GetValue<T>(String key);
    }
}
