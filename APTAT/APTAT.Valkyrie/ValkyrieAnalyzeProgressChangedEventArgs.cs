using System;

namespace APTAT.Valkyrie
{
    public class ValkyrieAnalyzeProgressChangedEventArgs<T> : EventArgs
    {
        public int Percent { get; }
        public ProgressInfo<T> ProgressInfo { get; }

        public bool IsUpdate { get; }

        public ValkyrieAnalyzeProgressChangedEventArgs(int progressInt, ProgressInfo<T> progressInfo
            , bool isUpdate = false)
        {
            Percent = progressInt;
            ProgressInfo = progressInfo;
            IsUpdate = isUpdate;
        }
    }
}
