namespace YourRootNamespace.Logging.LogProviders
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;

#if LIBLOG_EXCLUDE_CODE_COVERAGE
    [ExcludeFromCodeCoverage]
#endif
    internal class Log4NetLogProvider : LogProviderBase
    {
        private readonly Func<string, object> _getLoggerByNameDelegate;

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "LogManager")]
        public Log4NetLogProvider()
        {
            if (!IsLoggerAvailable()) throw new LibLogException("log4net.LogManager not found");
            _getLoggerByNameDelegate = GetGetLoggerMethodCall();
        }

        public static bool ProviderIsAvailableOverride { get; set; } = true;

        public override Logger GetLogger(string name)
        {
            return new Log4NetLogger(_getLoggerByNameDelegate(name)).Log;
        }

        internal static bool IsLoggerAvailable()
        {
            return ProviderIsAvailableOverride && GetLogManagerType() != null;
        }

        protected override OpenNdc GetOpenNdcMethod()
        {
            var logicalThreadContextType = Type.GetType("log4net.LogicalThreadContext, log4net");
            var stacksProperty = logicalThreadContextType.GetProperty("Stacks");
            var logicalThreadContextStacksType = stacksProperty.PropertyType;
            var stacksIndexerProperty = logicalThreadContextStacksType.GetProperty("Item");
            var stackType = stacksIndexerProperty.PropertyType;
            var pushMethod = stackType.GetMethod("Push");

            var messageParameter =
                Expression.Parameter(typeof(string), "message");

            // message => LogicalThreadContext.Stacks.Item["NDC"].Push(message);
            var callPushBody =
                Expression.Call(
                    Expression.Property(Expression.Property(null, stacksProperty),
                        stacksIndexerProperty,
                        Expression.Constant("NDC")),
                    pushMethod,
                    messageParameter);

            var result =
                Expression.Lambda<OpenNdc>(callPushBody, messageParameter)
                    .Compile();

            return result;
        }

        protected override OpenMdc GetOpenMdcMethod()
        {
            var logicalThreadContextType = Type.GetType("log4net.LogicalThreadContext, log4net");
            var propertiesProperty = logicalThreadContextType.GetProperty("Properties");
            var logicalThreadContextPropertiesType = propertiesProperty.PropertyType;
            var propertiesIndexerProperty = logicalThreadContextPropertiesType.GetProperty("Item");

            var removeMethod = logicalThreadContextPropertiesType.GetMethod("Remove");

            var keyParam = Expression.Parameter(typeof(string), "key");
            var valueParam = Expression.Parameter(typeof(string), "value");

            var propertiesExpression = Expression.Property(null, propertiesProperty);

            // (key, value) => LogicalThreadContext.Properties.Item[key] = value;
            var setProperties =
                Expression.Assign(Expression.Property(propertiesExpression, propertiesIndexerProperty, keyParam),
                    valueParam);

            // key => LogicalThreadContext.Properties.Remove(key);
            var removeMethodCall = Expression.Call(propertiesExpression, removeMethod, keyParam);

            var set = Expression
                .Lambda<Action<string, string>>(setProperties, keyParam, valueParam)
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
            return Type.GetType("log4net.LogManager, log4net");
        }

        private static Func<string, object> GetGetLoggerMethodCall()
        {
            var logManagerType = GetLogManagerType();
            var log4netAssembly = Assembly.GetAssembly(logManagerType);
            var method = logManagerType.GetMethod("GetLogger", typeof(Assembly), typeof(string));
            var repositoryAssemblyParam = Expression.Parameter(typeof(Assembly), "repositoryAssembly");
            var nameParam = Expression.Parameter(typeof(string), "name");
            var methodCall = Expression.Call(null, method, repositoryAssemblyParam, nameParam);
            var lambda = Expression
                .Lambda<Func<Assembly, string, object>>(methodCall, repositoryAssemblyParam, nameParam).Compile();
            return name => lambda(log4netAssembly, name);
        }

#if LIBLOG_EXCLUDE_CODE_COVERAGE
    [ExcludeFromCodeCoverage]
#endif
        internal class Log4NetLogger
        {
            private static object s_levelAll;
            private static object s_levelDebug;
            private static object s_levelInfo;
            private static object s_levelWarn;
            private static object s_levelError;
            private static object s_levelFatal;
            private static Func<object, object, bool> s_isEnabledForDelegate;
            private static Action<object, object> s_logDelegate;
            private static Func<object, Type, object, string, Exception, object> s_createLoggingEvent;
            private static Action<object, string, object> s_loggingEventPropertySetter;

            private static readonly Lazy<bool> Initialized =
                new Lazy<bool>(Initialize, LazyThreadSafetyMode.ExecutionAndPublication);

            private static Exception s_initializeException;
            private readonly dynamic _logger;

            [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ILogger")]
            internal Log4NetLogger(dynamic logger)
            {
                _logger = logger.Logger;
            }

            private static bool Initialize()
            {
                try
                {
                    var logEventLevelType = Type.GetType("log4net.Core.Level, log4net");
                    if (logEventLevelType == null) throw new LibLogException("Type log4net.Core.Level was not found.");

                    var levelFields = logEventLevelType.GetFields().ToList();
                    s_levelAll = levelFields.First(x => x.Name == "All").GetValue(null);
                    s_levelDebug = levelFields.First(x => x.Name == "Debug").GetValue(null);
                    s_levelInfo = levelFields.First(x => x.Name == "Info").GetValue(null);
                    s_levelWarn = levelFields.First(x => x.Name == "Warn").GetValue(null);
                    s_levelError = levelFields.First(x => x.Name == "Error").GetValue(null);
                    s_levelFatal = levelFields.First(x => x.Name == "Fatal").GetValue(null);

                    // Func<object, object, bool> isEnabledFor = (logger, level) => { return ((log4net.Core.ILogger)logger).IsEnabled(level); }
                    var loggerType = Type.GetType("log4net.Core.ILogger, log4net");
                    if (loggerType == null) throw new LibLogException("Type log4net.Core.ILogger, was not found.");
                    var instanceParam = Expression.Parameter(typeof(object));
                    var instanceCast = Expression.Convert(instanceParam, loggerType);
                    var levelParam = Expression.Parameter(typeof(object));
                    var levelCast = Expression.Convert(levelParam, logEventLevelType);
                    s_isEnabledForDelegate = GetIsEnabledFor(loggerType, logEventLevelType, instanceCast, levelCast,
                        instanceParam, levelParam);

                    var loggingEventType = Type.GetType("log4net.Core.LoggingEvent, log4net");

                    s_createLoggingEvent = GetCreateLoggingEvent(instanceParam, instanceCast, levelParam, levelCast,
                        loggingEventType);

                    s_logDelegate = GetLogDelegate(loggerType, loggingEventType, instanceCast, instanceParam);

                    s_loggingEventPropertySetter = GetLoggingEventPropertySetter(loggingEventType);
                }
                catch (Exception ex)
                {
                    s_initializeException = ex;
                    return false;
                }

                return true;
            }

            private static Action<object, object> GetLogDelegate(Type loggerType, Type loggingEventType,
                UnaryExpression instanceCast,
                ParameterExpression instanceParam)
            {
                //Action<object, object, string, Exception> Log =
                //(logger, callerStackBoundaryDeclaringType, level, message, exception) => { ((ILogger)logger).Log(new LoggingEvent(callerStackBoundaryDeclaringType, logger.Repository, logger.Name, level, message, exception)); }
                var writeExceptionMethodInfo = loggerType.GetMethod("Log",
                    loggingEventType);

                var loggingEventParameter =
                    Expression.Parameter(typeof(object), "loggingEvent");

                var loggingEventCasted =
                    Expression.Convert(loggingEventParameter, loggingEventType);

                var writeMethodExp = Expression.Call(
                    instanceCast,
                    writeExceptionMethodInfo,
                    loggingEventCasted);

                var logDelegate = Expression.Lambda<Action<object, object>>(
                    writeMethodExp,
                    instanceParam,
                    loggingEventParameter).Compile();

                return logDelegate;
            }

            private static Func<object, Type, object, string, Exception, object> GetCreateLoggingEvent(
                ParameterExpression instanceParam, UnaryExpression instanceCast, ParameterExpression levelParam,
                UnaryExpression levelCast, Type loggingEventType)
            {
                var callerStackBoundaryDeclaringTypeParam = Expression.Parameter(typeof(Type));
                var messageParam = Expression.Parameter(typeof(string));
                var exceptionParam = Expression.Parameter(typeof(Exception));

                var repositoryProperty = loggingEventType.GetProperty("Repository");
                var levelProperty = loggingEventType.GetProperty("Level");

                var loggingEventConstructor =
                    loggingEventType.GetConstructorPortable(typeof(Type), repositoryProperty.PropertyType,
                        typeof(string), levelProperty.PropertyType, typeof(object), typeof(Exception));

                //Func<object, object, string, Exception, object> Log =
                //(logger, callerStackBoundaryDeclaringType, level, message, exception) => new LoggingEvent(callerStackBoundaryDeclaringType, ((ILogger)logger).Repository, ((ILogger)logger).Name, (Level)level, message, exception); }
                var newLoggingEventExpression =
                    Expression.New(loggingEventConstructor,
                        callerStackBoundaryDeclaringTypeParam,
                        Expression.Property(instanceCast, "Repository"),
                        Expression.Property(instanceCast, "Name"),
                        levelCast,
                        messageParam,
                        exceptionParam);

                var createLoggingEvent =
                    Expression.Lambda<Func<object, Type, object, string, Exception, object>>(
                            newLoggingEventExpression,
                            instanceParam,
                            callerStackBoundaryDeclaringTypeParam,
                            levelParam,
                            messageParam,
                            exceptionParam)
                        .Compile();

                return createLoggingEvent;
            }

            private static Func<object, object, bool> GetIsEnabledFor(Type loggerType, Type logEventLevelType,
                UnaryExpression instanceCast,
                UnaryExpression levelCast,
                ParameterExpression instanceParam,
                ParameterExpression levelParam)
            {
                var isEnabledMethodInfo = loggerType.GetMethod("IsEnabledFor", logEventLevelType);
                var isEnabledMethodCall = Expression.Call(instanceCast, isEnabledMethodInfo, levelCast);

                var result =
                    Expression.Lambda<Func<object, object, bool>>(isEnabledMethodCall, instanceParam, levelParam)
                        .Compile();

                return result;
            }

            private static Action<object, string, object> GetLoggingEventPropertySetter(Type loggingEventType)
            {
                var loggingEventParameter = Expression.Parameter(typeof(object), "loggingEvent");
                var keyParameter = Expression.Parameter(typeof(string), "key");
                var valueParameter = Expression.Parameter(typeof(object), "value");

                var propertiesProperty = loggingEventType.GetProperty("Properties");
                var item = propertiesProperty.PropertyType.GetProperty("Item");

                // ((LoggingEvent)loggingEvent).Properties[key] = value;
                var body =
                    Expression.Assign(
                        Expression.Property(
                            Expression.Property(Expression.Convert(loggingEventParameter, loggingEventType),
                                propertiesProperty), item, keyParameter), valueParameter);

                var result =
                    Expression.Lambda<Action<object, string, object>>
                        (body, loggingEventParameter, keyParameter,
                            valueParameter)
                        .Compile();

                return result;
            }

            public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception,
                params object[] formatParameters)
            {
                if (!Initialized.Value)
                    throw new LibLogException(ErrorInitializingProvider, s_initializeException);

                if (messageFunc == null) return IsLogLevelEnable(logLevel);

                if (!IsLogLevelEnable(logLevel)) return false;

                var formattedMessage =
                    LogMessageFormatter.FormatStructuredMessage(messageFunc(),
                        formatParameters,
                        out var patternMatches);

                var callerStackBoundaryType = typeof(Log4NetLogger);
                // Callsite HACK - Extract the callsite-logger-type from the messageFunc
                var methodType = messageFunc.Method.DeclaringType;
                if (methodType == typeof(LogExtensions) ||
                    methodType != null && methodType.DeclaringType == typeof(LogExtensions))
                    callerStackBoundaryType = typeof(LogExtensions);
                else if (methodType == typeof(LoggerExecutionWrapper) ||
                         methodType != null && methodType.DeclaringType == typeof(LoggerExecutionWrapper))
                    callerStackBoundaryType = typeof(LoggerExecutionWrapper);

                var translatedLevel = TranslateLevel(logLevel);

                object loggingEvent = s_createLoggingEvent(_logger, callerStackBoundaryType, translatedLevel,
                    formattedMessage, exception);

                PopulateProperties(loggingEvent, patternMatches, formatParameters);

                s_logDelegate(_logger, loggingEvent);

                return true;
            }

            private void PopulateProperties(object loggingEvent, IEnumerable<string> patternMatches,
                IEnumerable<object> formatParameters)
            {
                var enumerable = patternMatches as string[] ?? patternMatches.ToArray();
                if (enumerable.Any())
                {
                    var keyToValue =
                        enumerable.Zip(formatParameters,
                            (key, value) => new KeyValuePair<string, object>(key, value));

                    foreach (var keyValuePair in keyToValue)
                        s_loggingEventPropertySetter(loggingEvent, keyValuePair.Key, keyValuePair.Value);
                }
            }

            private bool IsLogLevelEnable(LogLevel logLevel)
            {
                var level = TranslateLevel(logLevel);
                return s_isEnabledForDelegate(_logger, level);
            }

            private object TranslateLevel(LogLevel logLevel)
            {
                switch (logLevel)
                {
                    case LogLevel.Trace:
                        return s_levelAll;
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
                        throw new ArgumentOutOfRangeException("logLevel", logLevel, null);
                }
            }
        }
    }
}