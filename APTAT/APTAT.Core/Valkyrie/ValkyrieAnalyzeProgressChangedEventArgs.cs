using System;

namespace APTAT.Valkyrie
{
    public class ValkyrieAnalyzeProgressChangedEventArgs<T> : EventArgs
    {
        public int Percent { get; }
        public ProgressInfo<T> ProgressInfo { get; }

        public ValkyrieAnalyzeProgressChangedEventArgs(int progressInt, ProgressInfo<T> progressInfo)
        {
            Percent = progressInt;
            ProgressInfo = progressInfo;
        }
    }
}
