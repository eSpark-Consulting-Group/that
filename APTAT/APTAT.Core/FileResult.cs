using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using APTAT.Core.Descent.Apt;
using APTAT.Core.Valkyrie;
using ValkyrieVerdict = APTAT.Core.Valkyrie.ValkyrieVerdict;

namespace APTAT.Core
{
    public class FileResult : IEquatable<FileResult>
    {
        private IValkyrieAnalysisResult _valkyrieAnalysisResult;
        public FileResult(IModule badFile, IValkyrieAnalysisResult valkyrieAnalysisResult)
        {
            Computers = new List<ComputerInformation>();
            Module = badFile;
            ValkyrieAnalysisResult = valkyrieAnalysisResult;
        }

        public string Sha1 { get { return Module.Sha1; } }

        public IModule Module { get; }

        public IValkyrieAnalysisResult ValkyrieAnalysisResult
        {
            get { return _valkyrieAnalysisResult; }
            private set
            {
                if (_valkyrieAnalysisResult == value) return;;
                _valkyrieAnalysisResult = value;
                VerdictModified = DateTime.UtcNow;
            }
        }

        public DateTime VerdictModified { get; private set; }
        
        public List<ComputerInformation> Computers { get; set; }
        
        public bool Equals(FileResult other)
        {
            return Module.Sha1 == other.Module.Sha1;
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Module.Sha1);
        }

        public override bool Equals(object obj)
        {
            var other = obj as FileResult;
            return other != null && Equals(other);
        }
    }
}
