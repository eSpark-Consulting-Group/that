using System;

namespace APTAT.Core.Valkyrie
{
    public class AnalyzeCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {
        public AnalyzeCompletedEventArgs(Exception e, bool canceled, object state) : base(e, canceled, state)
        {
        }
    }

}
