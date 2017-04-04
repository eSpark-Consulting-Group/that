using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.Application.Models.Valkyrie;
using APTAT.Core;
using APTAT.Valkyrie;

namespace APTAT.Application.Services
{
    public static class MapperHelper
    {
        public static ValkyrieFile ToClient(this FileResult result)
        {
            return new ValkyrieFile(result);
        }
        

        public static ValkyrieAnalyzeProgressChangedEventArgs<ValkyrieFile> ToClient
            (this ValkyrieAnalyzeProgressChangedEventArgs<FileResult> result)
        {
            return new ValkyrieAnalyzeProgressChangedEventArgs<ValkyrieFile>(result.Percent, 
                new ProgressInfo<ValkyrieFile>(result.ProgressInfo.TotalFileCount, result.ProgressInfo.ProcessedFileCount, 
                result.ProgressInfo.Results.Select(r => new ValkyrieFile(r)).ToArray()), result.IsUpdate);
        }

        
    }
}
