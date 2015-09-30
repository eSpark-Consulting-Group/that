using System;
using System.ComponentModel;
using System.Reflection;

namespace APTAT.Valkyrie.Client.Enums
{
    public static class EnumExt
    {
        public static string GetEnumDescription(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            if(fi == null) throw new InvalidOperationException("Not supported enum value "+value);
            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes.Length > 0)
                return attributes[0].Description;
            return value.ToString();
        }
    }

    public enum ErrorCodesEnum
    {
        [Description("Success")]
        Success =0,

        [Description("Analysis Token Expired")]
        AnalysisTokenExpired = 101,

        [Description("Analysis Token Already Used")]
        AnalysisTokenAlreadyUsed = 102,

        [Description("Invalid Analysis Token")]
        InvalidAnalysisToken = 103,

        [Description("Analysis Token Missing")]
        AnalysisTokenMissing = 104,

        [Description("Client Id Missing")]
        ClientIdMissing = 105,

        [Description("File Missing")]
        FileMissing = 106,

        [Description("Invalid Request Type")]
        InvalidRequestType = 107,

        [Description("Manual Analysis Not Found")]
        ManualAnalysisNotFound = 108,

        [Description("API Key is invalid")]
        InvalidApiKey = 109,

        [Description("Invalid file type")]
        InvalidFileType = 110,
        
        [Description("Corrupted or protected archive")]
        CorruptedOrProtectedArchive = 111, //CORRUPTED_OR_PROTECTED_ARCHIVE
        
        [Description("No exe in archive file")]
        NoExeInArchiveFile = 112,

        [Description("Component Response Timeout")]
        ComponentResponseTimeout = 201,

        [Description("Bad Request")]
        BadRequest = 400,

        [Description("Page Not Found")]
        PageNotFound = 404,

        [Description("Internal Server Error")]
        InternalServerError = 500       

    }
}
