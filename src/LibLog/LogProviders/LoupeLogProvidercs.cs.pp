namespace $rootnamespace$.Logging.LogProviders
{
    using System;

#if LIBLOG_EXCLUDE_CODE_COVERAGE
    [ExcludeFromCodeCoverage]
#endif
    internal class LoupeLogProvider : LogProviderBase
    {
        /// <summary>
        ///     The form of the Loupe Log.Write method we're using
        /// </summary>
        internal delegate void WriteDelegate(
            int severity,
            string logSystem,
            int skipFrames,
            Exception exception,
            bool attributeToException,
            int writeMode,
            string detailsXml,
            string category,
            string caption,
            string description,
            params object[] args
        );

        private readonly WriteDelegate _logWriteDelegate;
        private const string LoupeAgentNetCoreDll = "Loupe.Agent.NETCore";
        private const string LoupeAgentNetFrameworkDll = "Gibraltar.Agent";

        public LoupeLogProvider()
        {
            if (!IsLoggerAvailable()) throw new LibLogException("Gibraltar.Agent.Log (Loupe) not found");

            _logWriteDelegate = GetLogWriteDelegate();
        }

        /// <summary>
        ///     Gets or sets a value indicating whether [provider is available override]. Used in tests.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [provider is available override]; otherwise, <c>false</c>.
        /// </value>
        public static bool ProviderIsAvailableOverride { get; set; } = true;

        public override Logger GetLogger(string name)
        {
            return new LoupeLogger(name, _logWriteDelegate).Log;
        }

        public static bool IsLoggerAvailable()
        {
            return ProviderIsAvailableOverride && GetLogManagerType() != null;
        }

        private static Type GetTypeFromCoreOrFrameworkDll(string typeName)
        {
            return Type.GetType($"{typeName}, {LoupeAgentNetCoreDll}") ?? Type.GetType($"{typeName}, {LoupeAgentNetFrameworkDll}");
        }

        private static Type GetLogManagerType()
        {
            return GetTypeFromCoreOrFrameworkDll("Gibraltar.Agent.Log");
        }

        private static WriteDelegate GetLogWriteDelegate()
        {
            var logManagerType = GetLogManagerType();
            var logMessageSeverityType = GetTypeFromCoreOrFrameworkDll("Gibraltar.Agent.LogMessageSeverity");
            var logWriteModeType = GetTypeFromCoreOrFrameworkDll("Gibraltar.Agent.LogWriteMode");

            var method = logManagerType.GetMethod(
                "Write",
                logMessageSeverityType, typeof(string), typeof(int), typeof(Exception), typeof(bool),
                logWriteModeType, typeof(string), typeof(string), typeof(string), typeof(string), typeof(object[]));

            var callDelegate = (WriteDelegate) method.CreateDelegate(typeof(WriteDelegate));
            return callDelegate;
        }

#if LIBLOG_EXCLUDE_CODE_COVERAGE
    [ExcludeFromCodeCoverage]
#endif
        internal class LoupeLogger
        {
            private const string LogSystem = "LibLog";

            private readonly string _category;
            private readonly WriteDelegate _logWriteDelegate;
            private readonly int _skipLevel;

            internal LoupeLogger(string category, WriteDelegate logWriteDelegate)
            {
                _category = category;
                _logWriteDelegate = logWriteDelegate;
#if DEBUG
                _skipLevel = 2;
#else
                _skipLevel = 1;
#endif
            }

            public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception,
                params object[] formatParameters)
            {
                if (messageFunc == null) return true;

                messageFunc = LogMessageFormatter.SimulateStructuredLogging(messageFunc, formatParameters);

                _logWriteDelegate(ToLogMessageSeverity(logLevel), LogSystem, _skipLevel, exception, true, 0, null,
                    _category, null, messageFunc.Invoke());

                return true;
            }

            private static int ToLogMessageSeverity(LogLevel logLevel)
            {
                switch (logLevel)
                {
                    case LogLevel.Trace:
                        return TraceEventTypeValues.Verbose;
                    case LogLevel.Debug:
                        return TraceEventTypeValues.Verbose;
                    case LogLevel.Info:
                        return TraceEventTypeValues.Information;
                    case LogLevel.Warn:
                        return TraceEventTypeValues.Warning;
                    case LogLevel.Error:
                        return TraceEventTypeValues.Error;
                    case LogLevel.Fatal:
                        return TraceEventTypeValues.Critical;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(logLevel));
                }
            }
        }
    }
}
