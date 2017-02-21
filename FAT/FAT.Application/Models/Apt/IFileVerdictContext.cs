using System;
using APTAT.Valkyrie;
using CloudVerdict = APTAT.Core.Descent.Apt.CloudVerdict;

namespace APTAT.Application.Models.Apt
{
    public interface IFileVerdictContext
    {
        string Sha1 { get; }
        CloudVerdict? FlsVerdict { get; }
        DateTime? FlsVerdictModifiedDate { get; }
        ValkyrieAnalysisResult ValkyrieAnalysisResult { get; }
        DateTime? ValkyrieModifiedDate { get; set; }
    }
}