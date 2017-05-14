using System;
using System.Collections.Generic;
using APTAT.Core.Descent.Copy;
using APTAT.Core.Valkyrie;

namespace APTAT.Core.DescentCopy
{
    public interface ICopyHelper : IDisposable
    {
        IReadOnlyCollection<ICopyFileResult> RemoteFilePathes { get; }
        Guid RequestFiles(IFileInfo[] filePathes, string login, string password);
        bool CheckResults(Guid operationUid, out ICopyFileResult[] results);
        event EventHandler<CopyDescentCompletedEventArgs> CopyCompleted;
    }
    public interface ICopyFileResult : IDisposable
    {
        string RemoteFilePath { get; }
    }
}