using System;

namespace FAT.Application.Models
{
    public interface IScanTimer
    {
        bool Start();
        bool Stop();
        bool ScanActive { get; }
        Action ScanIterationHandler { set; }
    }
}
