using System;

namespace APTAT.Valkyrie.Client
{
    public delegate void AnalyzeCompletedEventHandler<T>(object sender, AnalyzeCompletedEventArgs<T> e);
}
