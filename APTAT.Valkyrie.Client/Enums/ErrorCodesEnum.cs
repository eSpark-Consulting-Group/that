namespace APTAT.Valkyrie.Client.Enums
{
    public enum ErrorCodesEnum
    {
        Success=0,
        AnalysisTokenExpired = 101,
        AnalysisTokenAlreadyUsed = 102,
        InvalidAnalysisToken = 103,
        AnalysisTokenMissing = 104,
        ClientIdMissing = 105,
        FileMissing = 106,
        InvalidRequestType = 107,
        ManualAnalysisNotFound = 108,
        ComponentResponseTimeout = 201,
        BadRequest = 400,
        PageNotFound = 404,
        InternalServerError = 500       

    }
}
