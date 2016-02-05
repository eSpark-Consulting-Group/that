using System.ComponentModel;

namespace Valkyrie.WebApiProvider.Enums
{
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

        [Description("Timeout extracting archive file")]
        TimeoutExtractingArchiveFile = 111, //CORRUPTED_OR_PROTECTED_ARCHIVE
        
        [Description("No exe in archive file")]
        NoExeInArchiveFile = 112,

        [Description("Component Response Timeout")]
        ComponentResponseTimeout = 201,

        [Description("ENDPOINT ID MISSING ")]
        EndpointIdMissing = 303,
        [Description("Bad Request")]
        BadRequest = 400,

        [Description("Page Not Found")]
        PageNotFound = 404,

        [Description("Internal Server Error")]
        InternalServerError = 500       

    }
}
