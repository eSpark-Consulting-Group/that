using System;

namespace APTAT.Core.Valkyrie
{
    public class ValkyrieAnalyzeProgressChangedEventArgs<T> : EventArgs
    {
        public int Percent { get; private set; }
        public ProgressInfo<T> ProgressInfo { get; private set; }

        public bool IsUpdate { get; private set; }
        public bool IsPendingExist { get; private set; }

        public ValkyrieAnalyzeProgressChangedEventArgs(int progressInt, ProgressInfo<T> progressInfo
            , bool isUpdate = false, bool isPendingExist = false)
        {
            Percent = progressInt;
            ProgressInfo = progressInfo;
            IsUpdate = isUpdate;
            IsPendingExist = isPendingExist;
        }
    }
}
