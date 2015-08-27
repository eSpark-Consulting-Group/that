using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APTAT.Valkyrie.Client
{
    public class AnalyzeCompletedEventArgs<T> : System.ComponentModel.AsyncCompletedEventArgs
    {
        private T[] analysisResult;

        public AnalyzeCompletedEventArgs(T[] analysisResult, Exception e, bool canceled, object state)
            : base(e, canceled, state)
        {
            this.analysisResult = analysisResult;
        }
        public T[] Result
        {
            get
            {
                return analysisResult;
            }
        }
    }

    public delegate void AnalyzeCompletedEventHandler<T>(object sender, AnalyzeCompletedEventArgs<T> e);

}
