using System;
using APTAT.Core.Descent.Apt;

namespace FAT.Application.Models
{
    public interface IBadFileItem
    {
        DateTime Accessed { get; set; }
        string Attributes { get; set; }
        CloudVerdict Cloud { get; set; }
        string Copyright { get; set; }
        DateTime Created { get; set; }
        string Description { get; set; }
        string FileSizeFormatted { get; }
        bool IsClean { get; }
        bool IsMalware { get; }
        bool IsUnknown { get; }
        DateTime Modified { get; set; }
        string Name { get; set; }
        string Product { get; set; }
        string Sha1 { get; set; }
        string Signer { get; set; }
        long Size { get; set; }
        string SizeSorting { get; }
        string Version { get; set; }
    }
}