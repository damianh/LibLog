namespace DH.Logging
{
    using System;
    using System.Diagnostics;
    using DH.Logging.LogProviders;

    public static class LogProvider
    {
        private static ILogProvider _currentLogProvider;

        public static ILog GetCurrentClassLogger()
        {
            var stackFrame = new StackFrame(1, false);
            return GetLogger(stackFrame.GetMethod().DeclaringType);
        }

        public static ILog GetLogger(Type type)
        {
            return GetLogger(type.FullName);
        }

        public static ILog GetLogger(string name)
        {
            ILogProvider temp = _currentLogProvider ?? ResolveLogProvider();
            return temp == null ? new NoOpLogger() : (ILog)new LoggerExecutionWrapper(temp.GetLogger(name));
        }

        public static void SetCurrentLogProvider(ILogProvider logProvider)
        {
            _currentLogProvider = logProvider;
        }

        private static ILogProvider ResolveLogProvider()
        {
            if (NLogLogProvider.IsLoggerAvailable())
            {
                return new NLogLogProvider();
            }
            return Log4NetLogProvider.IsLoggerAvailable() ? new Log4NetLogProvider() : null;
        }

        public class NoOpLogger : ILog
        {
            public void Log(LogLevel logLevel, Func<string> messageFunc)
            {}

            public void Log<TException>(LogLevel logLevel, Func<string> messageFunc, TException exception)
                where TException : Exception
            {}
        }
    }
}