using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace WetHatLab.OneNote.TaggingKit
{
    internal struct TraceCategory
    {
        private static readonly string TRACE_INFO = "INFO";
        private static readonly string TRACE_WARNING = "WARNING";
        private static readonly string TRACE_ERROR = "ERROR";

        private string _category;
        private string _caller;
        private int    _line;
        internal string Category   { get { return _category; } }
        internal string CallerName { get { return _caller;  } }
        internal int Line          { get { return _line;  } }

        private TraceCategory(string category, string caller, int line)
        {
            _category = category;
            _caller = caller;
            _line = line;
        }

        internal static TraceCategory Info([CallerMemberName] string callerName = "", [CallerLineNumber] int line = -1)
        {
            return new TraceCategory(TRACE_INFO, callerName, line);
        }

        internal static TraceCategory Warning([CallerMemberName] string callerName = "", [CallerLineNumber] int line = -1)
        {
            return new TraceCategory(TRACE_WARNING, callerName, line);
        }

        internal static TraceCategory Error([CallerMemberName] string callerName = "", [CallerLineNumber] int line = -1)
        {
            return new TraceCategory(TRACE_ERROR, callerName, line);
        }
    }

    internal class TraceLogger
    {
        internal static void Register()
        {
            string logfile = Path.Combine(Path.GetTempPath(), "taggingkit_" + DateTime.Now.Ticks.ToString("X") +".log");
            FileStream log = new FileStream(logfile, FileMode.OpenOrCreate);
            // Creates the new trace listener.
            TextWriterTraceListener listener = new TextWriterTraceListener(log);
            Trace.Listeners.Add(listener);

            Log(TraceCategory.Info(),
                "{0} logging activated.\r\n\tAddin-Version: {1}\r\n\t.net Framework Version: {2}",
                Properties.Resources.TaggingKit_About_Appname,
                Assembly.GetExecutingAssembly().GetName().Version,
                Environment.Version
                );
            Flush();
        }

        internal static void Log(TraceCategory category, string message, params object[] args)
        {
#if TRACE
            Trace.Write(category.CallerName,category.Category);
            Trace.Write(" line ");
            Trace.Write("| ");
            try
            {
                Trace.WriteLine(string.Format(message, args));
            }
            catch (Exception ex)
            {
                Log(TraceCategory.Error(), "Logging failed {0}", ex);
            }
            
#endif //TRACE
        }

        internal static void Flush()
        {
            Trace.Flush();
        }
    }
}
