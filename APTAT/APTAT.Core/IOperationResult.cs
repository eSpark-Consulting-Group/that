using System;

namespace APTAT.Core
{
    public interface IOperationResult
    {
        Exception Error { get; set; }
        string ErrorMessage { get; set; }
        bool IsSucceeded { get; set; }
    }
}