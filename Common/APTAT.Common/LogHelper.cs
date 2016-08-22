using System;
using System.Diagnostics;
using System.Reflection;

namespace APTAT.Common
{
    public static class LogHelper
    {
        public static void Trace(TraceEventType severity, string format, params object[] args)
        {
            Trace(Assembly.GetCallingAssembly(), severity, TraceCategory.General, Guid.Empty, null, format, args);
        }

        public static void Trace(TraceEventType severity, TraceCategory traceCategory, string format, params object[] args)
        {
            Trace(Assembly.GetCallingAssembly(), severity, traceCategory, Guid.Empty, null, format, args);
        }

        public static void Trace(Assembly callingAssembly, TraceEventType severity, TraceCategory traceCategory, Guid activityId, Guid? relatedActivityId, string format, params object[] args)
        {
            switch (severity)
            {
                case TraceEventType.Critical:
                    System.Diagnostics.Trace.TraceError(format, args);
                    break;
                case TraceEventType.Error:
                    System.Diagnostics.Trace.TraceError(format, args);
                    break;
                case TraceEventType.Warning:
                    System.Diagnostics.Trace.TraceWarning(format, args);
                    break;
                case TraceEventType.Information:
                    System.Diagnostics.Trace.TraceInformation(format, args);
                    break;
                default:
                    System.Diagnostics.Trace.TraceInformation(format, args);
                    break;
            }
            System.Diagnostics.Trace.Flush();

#if DEBUG
            Console.ResetColor();
#endif
        }





    }

    [Flags]
    public enum TraceCategory
    {
        None = 0,
        General = 0x0001,
        Network = 0x0002,
        Reports = 0x0004,
        Descent = 0x0008,
        Licensing = 0x0010,
        Tasks = 0x0020,
        Storage = 0x0040,
        Facade = 0x0080,
        Policy = 0x0100,
        Directory = 0x0200,
        Push = 0x0400,
    }
}
