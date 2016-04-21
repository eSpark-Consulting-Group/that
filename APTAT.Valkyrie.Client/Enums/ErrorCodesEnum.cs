using System.ComponentModel;

namespace Valkyrie.WebApiProvider.Enums
{
    public enum ErrorCodesEnum
    {
        [Description("None")]
        None = -1,

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

        [Description("FILE NOT UPLOADED BEFORE")]
        FILE_NOT_UPLOADED_BEFORE = 113, 

        [Description("INVALID SHA1")]
        INVALID_SHA1 = 114,

        INVALID_NUMERIC_VALUE = 115, 

        APT_NO_STARTED_SCAN_FOUND = 116,

        [Description("MANUAL ANALYSIS LIMIT REACHED")]
        MANUAL_ANALYSIS_LIMIT_REACHED = 117,

        [Description("MANUAL ANALYSIS LIMIT REACHED")]
        METADATA_TOKEN_MISSING = 131, 

        [Description("METADATA TOKEN ALREADY USED")]
        METADATA_TOKEN_ALREADY_USED = 132,

        [Description("INVALID METADATA TYPE")]
        INVALID_METADATA_TYPE = 133, 

        [Description("INVALID METADATA TOKEN PARAMETER")]
        INVALID_METADATA_TOKEN_PARAMETER = 134,

        [Description("METADATA TOKEN EXPIRED")]
        METADATA_TOKEN_EXPIRED = 135,

        [Description("METADATA FILE MISSING")]
        METADATA_FILE_MISSING = 136,

        [Description("METADATA FILE IS INVALID")]
        METADATA_FILE_IS_INVALID = 137,

        [Description("Component Response Timeout")]
        ComponentResponseTimeout = 201,

        [Description("MANUAL VERDICT NOT GIVEN")]
        MANUAL_VERDICT_NOT_GIVEN = 202,

        [Description("ALREADY_OBJECTED")]
        ALREADY_OBJECTED = 203,

        [Description("ANALYSIS NOT FINISHED")]
        ANALYSIS_NOT_FINISHED = 204,

        [Description("ANALYSIS NOT FOUND")]
        ANALYSIS_NOT_FOUND = 205,

        [Description("ANALYSIS FAILED")]
        ANALYSIS_FAILED = 206,

        [Description("COMPONENT ERROR OCCURRED")]
        COMPONENT_ERROR_OCCURRED = 207,

        [Description("LICENSE ALREADY ASSOCIATED TO ANOTHER API KEY")]
        LICENSE_ALREADY_ASSOCIATED_TO_ANOTHER_API_KEY = 301,

        [Description("INVALID LICENSE ID")]
        INVALID_LICENSE_ID = 302,

        [Description("ENDPOINT ID MISSING")]
        EndpointIdMissing = 303,

        [Description("LICENSE EXPIRED")]
        LICENSE_EXPIRED = 304,

        [Description("NO ACTIVE LICENSE FOUND")]
        NO_ACTIVE_LICENSE_FOUND = 305,

        [Description("CANNOT CONNECT TO CAM LICENSE SERVER")]            
        CANNOT_CONNECT_TO_CAM_LICENSE_SERVER = 306, 

        [Description("INVALID CAM CREDENTIALS")]
        INVALID_CAM_CREDENTIALS = 307,

        [Description("ALL LICENSES ARE ALREADY CONNECTED TO ANOTHER API KEY")]
        ALL_LICENSES_ARE_ALREADY_CONNECTED_TO_ANOTHER_API_KEY = 308,

        [Description("API KEY ALREADY HAS VALID LICENSE")]
        API_KEY_ALREADY_HAS_VALID_LICENSE = 309,

        [Description("CAM LICENSE REQUIRED")]
        CAM_LICENSE_REQUIRED = 310,

        [Description("ENDPOINT LIMIT REACHED")]
        ENDPOINT_LIMIT_REACHED = 311,

        [Description("NO VALID LICENSE FOUND")]
        NO_VALID_LICENSE_FOUND = 312,

        
        [Description("Bad Request")]
        BadRequest = 400,

        [Description("Page Not Found")]
        PageNotFound = 404,

        [Description("Internal Server Error")]
        InternalServerError = 500       

    }
}
