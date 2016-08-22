using APTAT.Common;
using DDT.Core.Reports;

namespace DDT.Core
{
    public class DependencyLocator : IDependencyLocatorModule
    {
        public void RegisterDependencies(ICompositionHelper resolver)
        {
            resolver.RegisterSingleton<IFileHelper, FileHelper>();
            resolver.Register<IReportRenderer, TelerikReportRenderer>();
            resolver.Register<IReportBuilder, ReportBuilder>();
        }
    }
}
