using System;

namespace ValkyrieHashAnalyzer
{
    public class AnalyzeCompletedEventArgs<T> : System.ComponentModel.AsyncCompletedEventArgs
    {
      

        public AnalyzeCompletedEventArgs(T[] analysisResult, Exception e, bool canceled, object state)
            : base(e, canceled, state)
        {
            _analysisResult = analysisResult;
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
