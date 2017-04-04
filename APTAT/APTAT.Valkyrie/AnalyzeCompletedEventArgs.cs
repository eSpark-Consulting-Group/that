using System;

namespace APTAT.Valkyrie
{
    public class AnalyzeCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {
        public AnalyzeCompletedEventArgs(Exception e, bool canceled, object state) : base(e, canceled, state)
        {
        }
    }

}
