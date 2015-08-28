using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APTAT.Valkyrie.Client.Models
{
    public struct ProgressInfo
    {
        private int _totalCount;
        private int _processedCount;
        private int _malwareFileCount;
        public ProgressInfo(int total, int processed, int malware)
        {
            _totalCount = total;
            _processedCount = processed;
            _malwareFileCount = malware;
        }

        public int TotalFileCount
        {
            get
            {
                return _totalCount;
            }
        }
        public int ProcessedFileCount
        {
            get
            {

                return _processedCount;
            }
        }

        public int MalwareFileCount
        {
            get
            {
                return _malwareFileCount;
            }
        }
    }
}
