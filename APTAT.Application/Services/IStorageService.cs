using System;

namespace APTAT.Application.Services
{
    public class KnownKeys
    {
        public const String CheckedDirectoryItems = "CheckedDirectoryItems";
        public const String Login = "Login";
        public const String Password = "Password";
    }

    public interface IStorageService
    {
        void SetValue(String key, Object value);

        T GetValue<T>(String key, Boolean remove);
    }
}
