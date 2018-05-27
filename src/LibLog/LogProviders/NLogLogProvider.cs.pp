namespace $rootnamespace$.Logging.LogProviders
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;

#if LIBLOG_EXCLUDE_CODE_COVERAGE
    [ExcludeFromCodeCoverage]
#endif
    internal class NLogLogProvider : LogProviderBase
    {
        private readonly Func<string, object> _getLoggerByNameDelegate;

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "LogManager")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "NLog")]
        public NLogLogProvider()
        {
            if (!IsLoggerAvailable()) throw new LibLogException("NLog.LogManager not found");
            _getLoggerByNameDelegate = GetGetLoggerMethodCall();
        }

        public static bool ProviderIsAvailableOverride { get; set; } = true;

        public override Logger GetLogger(string name)
        {
            return new NLogLogger(_getLoggerByNameDelegate(name)).Log;
        }

        public static bool IsLoggerAvailable()
        {
            return ProviderIsAvailableOverride && GetLogManagerType() != null;
        }

        protected override OpenNdc GetOpenNdcMethod()
        {
            var ndcContextType = Type.GetType("NLog.NestedDiagnosticsContext, NLog");
            var pushMethod = ndcContextType.GetMethod("Push", typeof(string));
            var messageParam = Expression.Parameter(typeof(string), "message");
            var pushMethodCall = Expression.Call(null, pushMethod, messageParam);
            return Expression.Lambda<OpenNdc>(pushMethodCall, messageParam).Compile();
        }

        protected override OpenMdc GetOpenMdcMethod()
        {
            var mdcContextType = Type.GetType("NLog.MappedDiagnosticsContext, NLog");

            var setMethod = mdcContextType.GetMethod("Set", typeof(string), typeof(string));
            var removeMethod = mdcContextType.GetMethod("Remove", typeof(string));
            var keyParam = Expression.Parameter(typeof(string), "key");
            var valueParam = Expression.Parameter(typeof(string), "value");

            var setMethodCall = Expression.Call(null, setMethod, keyParam, valueParam);
            var removeMethodCall = Expression.Call(null, removeMethod, keyParam);

            var set = Expression
                .Lambda<Action<string, string>>(setMethodCall, keyParam, valueParam)
                .Compile();
            var remove = Expression
                .Lambda<Action<string>>(removeMethodCall, keyParam)
                .Compile();

            return (key, value, _) =>
            {
                set(key, value.ToString());
                return new DisposableAction(() => remove(key));
            };
        }

        private static Type GetLogManagerType()
        {
            return Type.GetType("NLog.LogManager, NLog");
        }

        private static Func<string, object> GetGetLoggerMethodCall()
        {
            var logManagerType = GetLogManagerType();
            var method = logManagerType.GetMethod("GetLogger", typeof(string));
            var nameParam = Expression.Parameter(typeof(string), "name");
            var methodCall = Expression.Call(null, method, nameParam);
            return Expression.Lambda<Func<string, object>>(methodCall, nameParam).Compile();
        }

#if LIBLOG_EXCLUDE_CODE_COVERAGE
    [ExcludeFromCodeCoverage]
#endif
        internal class NLogLogger
        {
            private static Func<string, object, string, object[], Exception, object> s_logEventInfoFact;

            private static object s_levelTrace;
            private static object s_levelDebug;
            private static object s_levelInfo;
            private static object s_levelWarn;
            private static object s_levelError;
            private static object s_levelFatal;

            private static bool s_structuredLoggingEnabled;
            private static readonly Lazy<bool> Initialized = new Lazy<bool>(Initialize);
            private static Exception s_initializeException;
            private readonly dynamic _logger;

            internal NLogLogger(dynamic logger)
            {
                _logger = logger;
            }

            private static bool Initialize()
            {
                try
                {
                    var logEventLevelType = Type.GetType("NLog.LogLevel, NLog");
                    if (logEventLevelType == null) throw new LibLogException("Type NLog.LogLevel was not found.");

                    var levelFields = logEventLevelType.GetFields().ToList();
                    s_levelTrace = levelFields.First(x => x.Name == "Trace").GetValue(null);
                    s_levelDebug = levelFields.First(x => x.Name == "Debug").GetValue(null);
                    s_levelInfo = levelFields.First(x => x.Name == "Info").GetValue(null);
                    s_levelWarn = levelFields.First(x => x.Name == "Warn").GetValue(null);
                    s_levelError = levelFields.First(x => x.Name == "Error").GetValue(null);
                    s_levelFatal = levelFields.First(x => x.Name == "Fatal").GetValue(null);

                    var logEventInfoType = Type.GetType("NLog.LogEventInfo, NLog");
                    if (logEventInfoType == null) throw new LibLogException("Type NLog.LogEventInfo was not found.");

                    var loggingEventConstructor =
                        logEventInfoType.GetConstructorPortable(logEventLevelType, typeof(string),
                            typeof(IFormatProvider), typeof(string), typeof(object[]), typeof(Exception));

                    var loggerNameParam = Expression.Parameter(typeof(string));
                    var levelParam = Expression.Parameter(typeof(object));
                    var messageParam = Expression.Parameter(typeof(string));
                    var messageArgsParam = Expression.Parameter(typeof(object[]));
                    var exceptionParam = Expression.Parameter(typeof(Exception));
                    var levelCast = Expression.Convert(levelParam, logEventLevelType);

                    var newLoggingEventExpression =
                        Expression.New(loggingEventConstructor,
                            levelCast,
                            loggerNameParam,
                            Expression.Constant(null, typeof(IFormatProvider)),
                            messageParam,
                            messageArgsParam,
                            exceptionParam
                        );

                    s_logEventInfoFact = Expression.Lambda<Func<string, object, string, object[], Exception, object>>(
                        newLoggingEventExpression,
                        loggerNameParam, levelParam, messageParam, messageArgsParam, exceptionParam).Compile();

                    s_structuredLoggingEnabled = IsStructuredLoggingEnabled();
                }
                catch (Exception ex)
                {
                    s_initializeException = ex;
                    return false;
                }

                return true;
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
            public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception,
                params object[] formatParameters)
            {
                if (!Initialized.Value)
                    throw new LibLogException(ErrorInitializingProvider, s_initializeException);

                if (messageFunc == null) return IsLogLevelEnable(logLevel);

                if (s_logEventInfoFact != null)
                {
                    if (IsLogLevelEnable(logLevel))
                    {
                        var formatMessage = messageFunc();
                        if (!s_structuredLoggingEnabled)
                        {
                            formatMessage =
                                LogMessageFormatter.FormatStructuredMessage(formatMessage,
                                    formatParameters,
                                    out _);
                            formatParameters = null; // Has been formatted, no need for parameters
                        }

                        var callsiteLoggerType = typeof(NLogLogger);
                        // Callsite HACK - Extract the callsite-logger-type from the messageFunc
                        var methodType = messageFunc.Method.DeclaringType;
                        if (methodType == typeof(LogExtensions) ||
                            methodType != null && methodType.DeclaringType == typeof(LogExtensions))
                            callsiteLoggerType = typeof(LogExtensions);
                        else if (methodType == typeof(LoggerExecutionWrapper) || methodType != null &&
                                 methodType.DeclaringType == typeof(LoggerExecutionWrapper))
                            callsiteLoggerType = typeof(LoggerExecutionWrapper);
                        var nlogLevel = TranslateLevel(logLevel);
                        var nlogEvent = s_logEventInfoFact(_logger.Name, nlogLevel, formatMessage, formatParameters,
                            exception);
                        _logger.Log(callsiteLoggerType, nlogEvent);
                        return true;
                    }

                    return false;
                }

                messageFunc = LogMessageFormatter.SimulateStructuredLogging(messageFunc, formatParameters);
                if (exception != null) return LogException(logLevel, messageFunc, exception);

                switch (logLevel)
                {
                    case LogLevel.Debug:
                        if (_logger.IsDebugEnabled)
                        {
                            _logger.Debug(messageFunc());
                            return true;
                        }

                        break;
                    case LogLevel.Info:
                        if (_logger.IsInfoEnabled)
                        {
                            _logger.Info(messageFunc());
                            return true;
                        }

                        break;
                    case LogLevel.Warn:
                        if (_logger.IsWarnEnabled)
                        {
                            _logger.Warn(messageFunc());
                            return true;
                        }

                        break;
                    case LogLevel.Error:
                        if (_logger.IsErrorEnabled)
                        {
                            _logger.Error(messageFunc());
                            return true;
                        }

                        break;
                    case LogLevel.Fatal:
                        if (_logger.IsFatalEnabled)
                        {
                            _logger.Fatal(messageFunc());
                            return true;
                        }

                        break;
                    default:
                        if (_logger.IsTraceEnabled)
                        {
                            _logger.Trace(messageFunc());
                            return true;
                        }

                        break;
                }

                return false;
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
            private bool LogException(LogLevel logLevel, Func<string> messageFunc, Exception exception)
            {
                switch (logLevel)
                {
                    case LogLevel.Debug:
                        if (_logger.IsDebugEnabled)
                        {
                            _logger.DebugException(messageFunc(), exception);
                            return true;
                        }

                        break;
                    case LogLevel.Info:
                        if (_logger.IsInfoEnabled)
                        {
                            _logger.InfoException(messageFunc(), exception);
                            return true;
                        }

                        break;
                    case LogLevel.Warn:
                        if (_logger.IsWarnEnabled)
                        {
                            _logger.WarnException(messageFunc(), exception);
                            return true;
                        }

                        break;
                    case LogLevel.Error:
                        if (_logger.IsErrorEnabled)
                        {
                            _logger.ErrorException(messageFunc(), exception);
                            return true;
                        }

                        break;
                    case LogLevel.Fatal:
                        if (_logger.IsFatalEnabled)
                        {
                            _logger.FatalException(messageFunc(), exception);
                            return true;
                        }

                        break;
                    default:
                        if (_logger.IsTraceEnabled)
                        {
                            _logger.TraceException(messageFunc(), exception);
                            return true;
                        }

                        break;
                }

                return false;
            }

            private bool IsLogLevelEnable(LogLevel logLevel)
            {
                switch (logLevel)
                {
                    case LogLevel.Debug:
                        return _logger.IsDebugEnabled;
                    case LogLevel.Info:
                        return _logger.IsInfoEnabled;
                    case LogLevel.Warn:
                        return _logger.IsWarnEnabled;
                    case LogLevel.Error:
                        return _logger.IsErrorEnabled;
                    case LogLevel.Fatal:
                        return _logger.IsFatalEnabled;
                    default:
                        return _logger.IsTraceEnabled;
                }
            }

            private object TranslateLevel(LogLevel logLevel)
            {
                switch (logLevel)
                {
                    case LogLevel.Trace:
                        return s_levelTrace;
                    case LogLevel.Debug:
                        return s_levelDebug;
                    case LogLevel.Info:
                        return s_levelInfo;
                    case LogLevel.Warn:
                        return s_levelWarn;
                    case LogLevel.Error:
                        return s_levelError;
                    case LogLevel.Fatal:
                        return s_levelFatal;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
                }
            }

            private static bool IsStructuredLoggingEnabled()
            {
                var configFactoryType = Type.GetType("NLog.Config.ConfigurationItemFactory, NLog");
                if (configFactoryType != null)
                {
                    var parseMessagesProperty = configFactoryType.GetProperty("ParseMessageTemplates");
                    if (parseMessagesProperty != null)
                    {
                        var defaultProperty = configFactoryType.GetProperty("Default");
                        if (defaultProperty != null)
                        {
                            var configFactoryDefault = defaultProperty.GetValue(null, null);
                            if (configFactoryDefault != null)
                            {
                                var parseMessageTemplates =
                                    parseMessagesProperty.GetValue(configFactoryDefault, null) as bool?;
                                if (parseMessageTemplates != false) return true;
                            }
                        }
                    }
                }

                return false;
            }
        }
    }
}
