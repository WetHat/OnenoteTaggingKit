using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;

namespace WetHatLab.OneNote.TaggingKit
{
    /// <summary>
    /// Tracing catergory for use with the <see cref="TraceLogger"/>
    /// </summary>
    internal struct TraceCategory
    {
        private static readonly string TRACE_INFO = "INFO";
        private static readonly string TRACE_WARNING = "WARNING";
        private static readonly string TRACE_ERROR = "ERROR";

        private string _category;
        private string _caller;
        private int    _line;

        /// <summary>
        /// Get the tracing category name
        /// </summary>
        internal string Category   { get { return _category; } }
        /// <summary>
        /// Get the name of the calling method which requested logging
        /// </summary>
        internal string CallerName { get { return _caller;  } }
        /// <summary>
        /// Get the line number of the caller from where the caller requested logging
        /// </summary>
        internal int Line          { get { return _line;  } }

        private TraceCategory(string category, string caller, int line)
        {
            _category = category;
            _caller = caller;
            _line = line;
        }

        /// <summary>
        /// Get the info tracing category
        /// </summary>
        /// <param name="callerName">name of the calling method. Provided by the compiler</param>
        /// <param name="line">caller line number from where logging is requested. Provided by the compiler</param>
        /// <returns>category instance</returns>
        internal static TraceCategory Info([CallerMemberName] string callerName = "", [CallerLineNumber] int line = -1)
        {
            return new TraceCategory(TRACE_INFO, callerName, line);
        }

        /// <summary>
        /// Get the warning tracing category
        /// </summary>
        /// <param name="callerName">name of the calling method. Provided by the compiler</param>
        /// <param name="line">caller line number from where logging is requested. Provided by the compiler</param>
        /// <returns>category instance</returns>
        internal static TraceCategory Warning([CallerMemberName] string callerName = "", [CallerLineNumber] int line = -1)
        {
            return new TraceCategory(TRACE_WARNING, callerName, line);
        }

        /// <summary>
        /// Get the error tracing category
        /// </summary>
        /// <param name="callerName">name of the calling method. Provided by the compiler</param>
        /// <param name="line">caller line number from where logging is requested. Provided by the compiler</param>
        /// <returns>category instance</returns>
        internal static TraceCategory Error([CallerMemberName] string callerName = "", [CallerLineNumber] int line = -1)
        {
            return new TraceCategory(TRACE_ERROR, callerName, line);
        }
    }

    /// <summary>
    /// Add-In specific logging utility
    /// </summary>
    internal class TraceLogger
    {
        static string _logfile = null;

        /// <summary>
        /// Show a message box for an exception.
        /// </summary>
        /// <param name="message">Message to describe the faling operation</param>
        /// <param name="ex">exception</param>
        internal static void ShowGenericMessageBox(string message, Exception ex)
        {
            Trace.Flush();
            MessageBox.Show(string.Format(Properties.Resources.TaggingKit_ErrorBox_GenericSevereError,
                                          message,
                                          ex.Message,
                                          Properties.Resources.TaggingKit_Support_Website,
                                          TraceLogger.LogFile),
                            string.Format(Properties.Resources.TaggingKit_ErrorBox_Title,
                                          Properties.Resources.TaggingKit_About_Appname),MessageBoxButton.OK,MessageBoxImage.Error);
        }

        /// <summary>
        /// get the path to the log file
        /// </summary>
        internal static string LogFile
        {
            get
            {
                if (_logfile == null)
                {
                    _logfile = Path.Combine(Path.GetTempPath(), "taggingkit_" + DateTime.Now.Ticks.ToString("X") + ".log");
                }
                return _logfile;
            }
        }

        /// <summary>
        /// register the logging utility with the tracing system.
        /// </summary>
        internal static void Register()
        {
            FileStream log = new FileStream(LogFile, FileMode.OpenOrCreate);
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

        /// <summary>
        /// log a message.
        /// </summary>
        /// <param name="category">logging category</param>
        /// <param name="message">logging message</param>
        /// <param name="args">parameters for the logging message</param>
        internal static void Log(TraceCategory category, string message, params object[] args)
        {
#if TRACE
            Trace.Write(category.CallerName,category.Category);
            Trace.Write(" (");
            Trace.Write(category.Line);
            Trace.Write(") | ");
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

        /// <summary>
        /// Flush all cached log messages.
        /// </summary>
        internal static void Flush()
        {
            Trace.Flush();
        }
    }
}
