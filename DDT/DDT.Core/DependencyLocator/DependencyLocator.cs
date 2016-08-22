using APTAT.Common;
using DDT.Core.Reports;

namespace DDT.Core
{
    public class DependencyLocator : IDependencyLocatorModule
    {
        public void RegisterDependencies(ICompositionHelper resolver)
        {
            resolver.Register<IFileHelper, FileHelper>(true);
            resolver.Register<IReportRenderer, TelerikReportRenderer>(false);
            resolver.Register<IReportBuilder, ReportBuilder>(false);
        }
    }
}
