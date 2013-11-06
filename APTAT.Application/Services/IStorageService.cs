using System;

namespace APTAT.Application.Services
{
    public class KnownKeys
    {
        public const String CheckedDirectoryItems = "CheckedDirectoryItems";
        public const String Login = "Login";
        public const String Password = "Password";
        public const String Domain = "Domain";
        public const String Workgroup = "Workgroup";
        public const String UnknownFile = "BadFile";
        public const String OperationGuid = "OperationGuid";
        public const String ReportKind = "ReportKind";
        public const String DevicesSourceDescription = "DevicesSourceDescription";
    }

    public interface IStorageService
    {
        void SetValue(String key, Object value);

        T GetValue<T>(String key, Boolean remove);
    }
}
