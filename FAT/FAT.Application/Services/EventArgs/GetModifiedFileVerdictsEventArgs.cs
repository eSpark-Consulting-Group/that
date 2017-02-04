using FAT.Application.Models;
using FAT.Application.Services;
using System;
using APTAT.Application.Models.Apt;

namespace APTAT.Application.Services.EventArgs
{
    public class GetModifiedFileVerdictsEventArgs : System.EventArgs
    {
        public GetModifiedFileVerdictsEventArgs(FileVerdictContext[] fileVerdict)
        {
            Results = fileVerdict;
        }

        public FileVerdictContext[] Results { get; }
    }
}