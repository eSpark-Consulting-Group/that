using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace APTAT.Core
{
	public static class LogHelper
	{
		public static class TraceCategoryName
		{
			public const string General	  = "GEN";
			public const string Network   = "NET";
			public const string Reports   = "REP";
			public const string Descent   = "DSC";
			public const string Licensing = "LIC";
			public const string Tasks     = "TSK";
			public const string Storage   = "STO";
			public const string Facade    = "FAS";
			public const string Policy    = "POL";
			public const string Directory = "DIR";
			public const string Push      = "PSH";
		}

		private static Guid GetLogicalOperationUid()
		{
			try
			{
				object id = System.Diagnostics.Trace.CorrelationManager.LogicalOperationStack.Peek();
				if( id is Guid )
				{
					return ( Guid )id;
				}
			}
			catch( InvalidOperationException ) { }

			return Guid.Empty;
		}

		public static void Trace(TraceEventType severity, string format, params object[] args)
		{
			Trace(Assembly.GetCallingAssembly(), severity, TraceCategory.General, Guid.Empty, null, format, args);
		}

		public static void Trace(TraceEventType severity, TraceCategory traceCategory, string format, params object[] args)
		{
			Trace(Assembly.GetCallingAssembly(), severity, traceCategory, Guid.Empty, null, format, args);
		}

		public static void Trace(TraceEventType severity, TraceCategory traceCategory, Guid activityId, Guid? relatedActivityId, string format, params object[] args)
		{
			Trace(Assembly.GetCallingAssembly(), severity, traceCategory, activityId, relatedActivityId, format, args);
		}

		public static IDisposable StartLogicalOperation( Guid uid )
		{
			//var manager = EnterpriseLibraryContainer.Current.GetInstance<TraceManager>();
			//return manager.StartTrace( null, uid );
            throw new NotImplementedException();
		}

		public static IDisposable StartLogicalOperation()
		{
            //var manager = EnterpriseLibraryContainer.Current.GetInstance<TraceManager>();
            //return manager.StartTrace( null, Guid.NewGuid() );
            throw new NotImplementedException();
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
		    

            //var writer = EnterpriseLibraryContainer.Current.GetInstance<LogWriter>();
            //if (writer.IsLoggingEnabled())
            //{
            //    var logEntry = new LogEntry
            //    {
            //        Categories = GetCategories(traceCategory),
            //        Message = String.Format(format, args),
            //        Severity = severity,
            //        ActivityId = activityId,
            //        RelatedActivityId = relatedActivityId,
            //        Title = callingAssembly.GetName().Name,
            //        MachineName = Environment.MachineName,
            //        ManagedThreadName = Thread.CurrentThread.Name,
            //        Win32ThreadId = GetCurrentThreadId().ToString(),
            //        TimeStamp = DateTime.UtcNow,
            //    };

            //    logEntry.ExtendedProperties["ManagedThreadId"] = Thread.CurrentThread.ManagedThreadId;

            //    if (writer.ShouldLog(logEntry))
            //        writer.Write(logEntry);
            //}

			#if DEBUG
			Console.ResetColor();
			#endif
		}

		public static void TraceInColor(ConsoleColor foregroundColor, TraceEventType severity, string format, params object[] args)
		{
			TraceInColor(Assembly.GetCallingAssembly(), foregroundColor, severity, TraceCategory.General, Guid.Empty, null, format, args);
		}

		public static void TraceInColor(ConsoleColor foregroundColor, TraceEventType severity, TraceCategory traceCategory, string format, params object[] args)
		{
			TraceInColor(Assembly.GetCallingAssembly(), foregroundColor, severity, traceCategory, Guid.Empty, null, format, args);
		}

		public static void TraceInColor(ConsoleColor foregroundColor, TraceEventType severity, TraceCategory traceCategory, Guid activityId, Guid? relatedActivityId, string format, params object[] args)
		{
			TraceInColor(Assembly.GetCallingAssembly(), foregroundColor, severity, traceCategory, activityId, relatedActivityId, format, args);
		}

		private static void TraceInColor(Assembly callingAssembly, ConsoleColor foregroundColor, TraceEventType severity, TraceCategory traceCategory, Guid activityId, Guid? relatedActivityId, string format, params object[] args)
		{
			#if DEBUG
			Console.ForegroundColor = foregroundColor;
			#endif

			Trace(callingAssembly, severity, traceCategory, activityId, relatedActivityId, format, args);

			#if DEBUG
			Console.ResetColor();
			#endif
		}

		public static void AddListener(string categoryName, TraceListener listener)
		{
            //var writer = EnterpriseLibraryContainer.Current.GetInstance<LogWriter>();
            //var entry = new LogEntry { Categories = { categoryName } };
            //foreach (var source in writer.GetMatchingTraceSources(entry))
            //{
            //    if (!source.Listeners.Contains(listener))
            //        source.Listeners.Add(listener);
            //}
            throw new NotImplementedException();
		}

		/*
		public static void TraceException(TraceEventType severity, string message, Exception x)
		{
			TraceException(severity, TraceCategory.General, Guid.Empty, null, message, x);
		}

		public static void TraceException(TraceEventType severity, TraceCategory traceCategory, string message, Exception x)
		{
			TraceException(severity, traceCategory, Guid.Empty, null, message, x);
		}

		public static void TraceException(TraceEventType severity, TraceCategory traceCategory, Guid activityId, Guid? relatedActivityId, string message, Exception x)
		{
			var writer = EnterpriseLibraryContainer.Current.GetInstance<LogWriter>();
			if (writer.IsLoggingEnabled())
			{
				var text = (message ?? String.Empty) + x;
			//	var sb = new StringBuilder(message);
			//	do
			//	{
			//		sb.Append(x.GetType().Name).Append(' ');
			//		sb.Append(x.TargetSite.DeclaringType.Name).Append('.');
			//		sb.Append(x.TargetSite.Name).Append(": ");
			//		sb.Append(x.Message);
			//		x = x.InnerException;
			//		if (x != null)
			//			sb.Append(" --> ");
			//	}
			//	while (x != null);
			//	var text = sb.ToString();
			//
				var logEntry = new LogEntry
				{
					Categories = GetCategories(traceCategory),
					Message = text,
					Severity = severity,
					ActivityId = activityId,
					RelatedActivityId = relatedActivityId,
					Title = Assembly.GetExecutingAssembly().GetName().Name,
					MachineName = Environment.MachineName,
					ManagedThreadName = Thread.CurrentThread.Name,
					Win32ThreadId = GetCurrentThreadId().ToString(),
					TimeStamp = DateTime.UtcNow,
				};

				logEntry.ExtendedProperties["ManagedThreadId"] = Thread.CurrentThread.ManagedThreadId;

				writer.Write(logEntry);
			}
		}
	*/
		private static ICollection<string> GetCategories(TraceCategory category)
		{
            //var categories = new List<string>();
            //if (category.HasFlag(TraceCategory.General))
            //    categories.Add(TraceCategoryName.General);
            //if (category.HasFlag(TraceCategory.Network))
            //    categories.Add(TraceCategoryName.Network);
            //if (category.HasFlag(TraceCategory.Reports))
            //    categories.Add(TraceCategoryName.Reports);
            //if (category.HasFlag(TraceCategory.Descent))
            //    categories.Add(TraceCategoryName.Descent);
            //if (category.HasFlag(TraceCategory.Licensing))
            //    categories.Add(TraceCategoryName.Licensing);
            //if (category.HasFlag(TraceCategory.Tasks))
            //    categories.Add(TraceCategoryName.Tasks);
            //if (category.HasFlag(TraceCategory.Storage))
            //    categories.Add(TraceCategoryName.Storage);
            //if (category.HasFlag(TraceCategory.Facade))
            //    categories.Add(TraceCategoryName.Facade);
            //if (category.HasFlag(TraceCategory.Policy))
            //    categories.Add(TraceCategoryName.Policy);
            //if (category.HasFlag(TraceCategory.Directory))
            //    categories.Add(TraceCategoryName.Directory);
            //if( category.HasFlag( TraceCategory.Push ) )
            //    categories.Add( TraceCategoryName.Push );
            //return categories;
            throw new NotImplementedException();
		}

		[DllImport("kernel32.dll")]
		private static extern uint GetCurrentThreadId();
	}

	[Flags]
	public enum TraceCategory
	{
		None      = 0,
		General	  = 0x0001,
		Network   = 0x0002,
		Reports   = 0x0004,
		Descent   = 0x0008,
		Licensing = 0x0010,
		Tasks     = 0x0020,
		Storage   = 0x0040,
		Facade    = 0x0080,
		Policy    = 0x0100,
		Directory = 0x0200,
		Push      = 0x0400,
	}
}
