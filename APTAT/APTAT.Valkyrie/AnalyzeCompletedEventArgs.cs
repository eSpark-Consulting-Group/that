using System;

namespace APTAT.Valkyrie
{
    public class AnalyzeCompletedEventArgs<T> : System.ComponentModel.AsyncCompletedEventArgs
    {
      
        public bool IsPendingExist { get; private set; }
        public AnalyzeCompletedEventArgs(T[] analysisResult, Exception e, bool canceled,
            bool containsPending, object state)
            : base(e, canceled, state)
        {
            _analysisResult = analysisResult;
            IsPendingExist = containsPending;
        }
        private readonly T[] _analysisResult;
        public T[] Result
        {
            get
            {
                return _analysisResult;
            }
        }
    }

}
