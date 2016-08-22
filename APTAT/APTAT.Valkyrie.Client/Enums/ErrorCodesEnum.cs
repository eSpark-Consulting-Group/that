using System.ComponentModel;

namespace Valkyrie.WebApiProvider.Enums
{
    public enum ErrorCodesEnum
    {
        [Description("None")]
        None = -1,

        [Description("Success")]
        Success = 0,

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
        FileNotUploadedBefore = 113,

        [Description("INVALID SHA1")]
        InvalidSha1 = 114,

        InvalidNumericValue = 115,

        AptNoStartedScanFound = 116,

        [Description("MANUAL ANALYSIS LIMIT REACHED")]
        ManualAnalysisLimitReached = 117,

        [Description("UNDEFINED MANUAL ANALYSIS LIMIT")]
        UNDEFINED_MANUAL_ANALYSIS_LIMIT = 118,

        [Description("MASTER USER NOT FOUND")]
        MASTER_USER_NOT_FOUND = 119,

        [Description("INVALID_USER_FOR_MANUAL_ANALYSIS")]
        INVALID_USER_FOR_MANUAL_ANALYSIS = 120,

        [Description("MANUAL ANALYSIS LIMIT REACHED")]
        MetadataTokenMissing = 131,

        [Description("METADATA TOKEN ALREADY USED")]
        MetadataTokenAlreadyUsed = 132,

        [Description("INVALID METADATA TYPE")]
        InvalidMetadataType = 133,

        [Description("INVALID METADATA TOKEN PARAMETER")]
        InvalidMetadataTokenParameter = 134,

        [Description("METADATA TOKEN EXPIRED")]
        MetadataTokenExpired = 135,

        [Description("METADATA FILE MISSING")]
        MetadataFileMissing = 136,

        [Description("METADATA FILE IS INVALID")]
        MetadataFileIsInvalid = 137,

        [Description("Component Response Timeout")]
        ComponentResponseTimeout = 201,

        [Description("MANUAL VERDICT NOT GIVEN")]
        ManualVerdictNotGiven = 202,

        [Description("ALREADY_OBJECTED")]
        AlreadyObjected = 203,

        [Description("ANALYSIS NOT FINISHED")]
        AnalysisNotFinished = 204,

        [Description("ANALYSIS NOT FOUND")]
        AnalysisNotFound = 205,

        [Description("ANALYSIS FAILED")]
        AnalysisFailed = 206,

        [Description("COMPONENT ERROR OCCURRED")]
        ComponentErrorOccurred = 207,

        [Description("LICENSE ALREADY ASSOCIATED TO ANOTHER API KEY")]
        LicenseAlreadyAssociatedToAnotherApiKey = 301,

        [Description("INVALID LICENSE ID")]
        InvalidLicenseId = 302,

        [Description("ENDPOINT ID MISSING")]
        EndpointIdMissing = 303,

        [Description("LICENSE EXPIRED")]
        LicenseExpired = 304,

        [Description("NO ACTIVE LICENSE FOUND")]
        NoActiveLicenseFound = 305,

        [Description("CANNOT CONNECT TO CAM LICENSE SERVER")]
        CannotConnectToCamLicenseServer = 306,

        [Description("INVALID CAM CREDENTIALS")]
        InvalidCamCredentials = 307,

        [Description("ALL LICENSES ARE ALREADY CONNECTED TO ANOTHER API KEY")]
        AllLicensesAreAlreadyConnectedToAnotherApiKey = 308,

        [Description("API KEY ALREADY HAS VALID LICENSE")]
        ApiKeyAlreadyHasValidLicense = 309,

        [Description("CAM LICENSE REQUIRED")]
        CamLicenseRequired = 310,

        [Description("ENDPOINT LIMIT REACHED")]
        EndpointLimitReached = 311,

        [Description("NO VALID LICENSE FOUND")]
        NoValidLicenseFound = 312,

        [Description("License limitation: upgrade to Enterprise to obtain file verdict")]
        NonEnterprise = 313,

        [Description("UNEXPECTED RESPONSE RETURNED FROM CAM")]
        UNEXPECTED_RESPONSE_RETURNED_FROM_CAM = 314,

        [Description("Bad Request")]
        BadRequest = 400,

        [Description("Page Not Found")]
        PageNotFound = 404,

        [Description("Internal Server Error")]
        InternalServerError = 500

    }
}
