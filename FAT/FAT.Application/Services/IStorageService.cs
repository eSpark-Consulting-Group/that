using System;

namespace APTAT.Application.Services
{
    public class KnownKeys
    {
        public const String CheckedDirectoryItems = "CheckedDirectoryItems";

        public const String UnknownFile = "BadFile";
        public const String OperationGuid = "OperationGuid";
        public const String ReportKind = "ReportKind";
        public const String DevicesSourceDescription = "DevicesSourceDescription";

        public const String CustomScanOptions = "CustomScanOptions";
    }

    public interface IStorageService
    {
        void SetValue(String key, Object value);

        T GetValue<T>(String key, Boolean remove);

        bool ContainsKey(string key);
    }
}
