//===============================================================================
// LibLog
//
// https://github.com/damianh/LibLog
//===============================================================================
// Copyright © 2011-2014 Damian Hickey.  All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//===============================================================================

namespace LibLog.Logging
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using LibLog.Logging.LogProviders;

    public interface ILog
    {
        void Log(LogLevel logLevel, Func<string> messageFunc);

        void Log<TException>(LogLevel logLevel, Func<string> messageFunc, TException exception) where TException : Exception;
    }

    public enum LogLevel
    {
        Trace,
        Debug,
        Info,
        Warn,
        Error,
        Fatal
    }

    public static class LogExtensions
    {
        public static void Debug(this ILog logger, Func<string> messageFunc)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Debug, messageFunc);
        }

        public static void Debug(this ILog logger, string message)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Debug, () => message);
        }

        public static void DebugFormat(this ILog logger, string message, params object[] args)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Debug, () => string.Format(CultureInfo.InvariantCulture, message, args));
        }

        public static void Error(this ILog logger, string message)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Error, () => message);
        }

        public static void ErrorFormat(this ILog logger, string message, params object[] args)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Error, () => string.Format(CultureInfo.InvariantCulture, message, args));
        }

        public static void ErrorException(this ILog logger, string message, Exception exception)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Error, () => message, exception);
        }

        public static void Info(this ILog logger, Func<string> messageFunc)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Info, messageFunc);
        }

        public static void Info(this ILog logger, string message)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Info, () => message);
        }

        public static void InfoFormat(this ILog logger, string message, params object[] args)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Info, () => string.Format(CultureInfo.InvariantCulture, message, args));
        }

        public static void Warn(this ILog logger, Func<string> messageFunc)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Warn, messageFunc);
        }

        public static void Warn(this ILog logger, string message)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Warn, () => message);
        }

        public static void WarnFormat(this ILog logger, string message, params object[] args)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Warn, () => string.Format(CultureInfo.InvariantCulture, message, args));
        }

        public static void WarnException(this ILog logger, string message, Exception ex)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Warn, () => string.Format(CultureInfo.InvariantCulture, message), ex);
        }

        private static void GuardAgainstNullLogger(ILog logger)
        {
            if (logger == null)
            {
                throw new ArgumentException("logger is null", "logger");
            }
        }
    }

    public interface ILogProvider
    {
        ILog GetLogger(string name);
    }

    public static class LogProvider
    {
        private static ILogProvider _currentLogProvider;

        public static ILog For<T>()
        {
            return GetLogger(typeof(T));
        }

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
            if (Log4NetLogProvider.IsLoggerAvailable())
            {
                return new Log4NetLogProvider();
            }
            return EntLibLogProvider.IsLoggerAvailable() ? new EntLibLogProvider() : null;
        }

        public class NoOpLogger : ILog
        {
            public void Log(LogLevel logLevel, Func<string> messageFunc)
            { }

            public void Log<TException>(LogLevel logLevel, Func<string> messageFunc, TException exception)
                where TException : Exception
            { }
        }
    }

    public class LoggerExecutionWrapper : ILog
    {
        private readonly ILog _logger;
        public const string FailedToGenerateLogMessage = "Failed to generate log message";

        public ILog WrappedLogger
        {
            get { return _logger; }
        }

        public LoggerExecutionWrapper(ILog logger)
        {
            _logger = logger;
        }

        public void Log(LogLevel logLevel, Func<string> messageFunc)
        {
            Func<string> wrappedMessageFunc = () =>
            {
                try
                {
                    return messageFunc();
                }
                catch (Exception ex)
                {
                    Log(LogLevel.Error, () => FailedToGenerateLogMessage, ex);
                }
                return null;
            };
            _logger.Log(logLevel, wrappedMessageFunc);
        }

        public void Log<TException>(LogLevel logLevel, Func<string> messageFunc, TException exception) where TException : Exception
        {
            Func<string> wrappedMessageFunc = () =>
            {
                try
                {
                    return messageFunc();
                }
                catch (Exception ex)
                {
                    Log(LogLevel.Error, () => FailedToGenerateLogMessage, ex);
                }
                return null;
            };
            _logger.Log(logLevel, wrappedMessageFunc, exception);
        }
    }
}

namespace LibLog.Logging.LogProviders
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;

    public class NLogLogProvider : ILogProvider
    {
        private readonly Func<string, object> _getLoggerByNameDelegate;
        private static bool _providerIsAvailableOverride = true;

        public NLogLogProvider()
        {
            if (!IsLoggerAvailable())
            {
                throw new InvalidOperationException("NLog.LogManager not found");
            }
            _getLoggerByNameDelegate = GetGetLoggerMethodCall();
        }

        public static bool ProviderIsAvailableOverride
        {
            get { return _providerIsAvailableOverride; }
            set { _providerIsAvailableOverride = value; }
        }

        public ILog GetLogger(string name)
        {
            return new NLogLogger(_getLoggerByNameDelegate(name));
        }

        public static bool IsLoggerAvailable()
        {
            return ProviderIsAvailableOverride && GetLogManagerType() != null;
        }

        private static Type GetLogManagerType()
        {
            return Type.GetType("NLog.LogManager, nlog");
        }

        private static Func<string, object> GetGetLoggerMethodCall()
        {
            Type logManagerType = GetLogManagerType();
            MethodInfo method = logManagerType.GetMethod("GetLogger", new[] { typeof(string) });
            ParameterExpression nameParam = Expression.Parameter(typeof(string), "name");
            MethodCallExpression methodCall = Expression.Call(null, method, new Expression[] { nameParam });
            return Expression.Lambda<Func<string, object>>(methodCall, new[] { nameParam }).Compile();
        }

        public class NLogLogger : ILog
        {
            private readonly dynamic _logger;

            internal NLogLogger(dynamic logger)
            {
                _logger = logger;
            }

            public void Log(LogLevel logLevel, Func<string> messageFunc)
            {
                switch (logLevel)
                {
                    case LogLevel.Debug:
                        if (_logger.IsDebugEnabled)
                        {
                            _logger.Debug(messageFunc());
                        }
                        break;
                    case LogLevel.Info:
                        if (_logger.IsInfoEnabled)
                        {
                            _logger.Info(messageFunc());
                        }
                        break;
                    case LogLevel.Warn:
                        if (_logger.IsWarnEnabled)
                        {
                            _logger.Warn(messageFunc());
                        }
                        break;
                    case LogLevel.Error:
                        if (_logger.IsErrorEnabled)
                        {
                            _logger.Error(messageFunc());
                        }
                        break;
                    case LogLevel.Fatal:
                        if (_logger.IsFatalEnabled)
                        {
                            _logger.Fatal(messageFunc());
                        }
                        break;
                    default:
                        if (_logger.IsTraceEnabled)
                        {
                            _logger.Trace(messageFunc());
                        }
                        break;
                }
            }

            public void Log<TException>(LogLevel logLevel, Func<string> messageFunc, TException exception)
                where TException : Exception
            {
                switch (logLevel)
                {
                    case LogLevel.Debug:
                        if (_logger.IsDebugEnabled)
                        {
                            _logger.DebugException(messageFunc(), exception);
                        }
                        break;
                    case LogLevel.Info:
                        if (_logger.IsInfoEnabled)
                        {
                            _logger.InfoException(messageFunc(), exception);
                        }
                        break;
                    case LogLevel.Warn:
                        if (_logger.IsWarnEnabled)
                        {
                            _logger.WarnException(messageFunc(), exception);
                        }
                        break;
                    case LogLevel.Error:
                        if (_logger.IsErrorEnabled)
                        {
                            _logger.ErrorException(messageFunc(), exception);
                        }
                        break;
                    case LogLevel.Fatal:
                        if (_logger.IsFatalEnabled)
                        {
                            _logger.FatalException(messageFunc(), exception);
                        }
                        break;
                    default:
                        if (_logger.IsTraceEnabled)
                        {
                            _logger.TraceException(messageFunc(), exception);
                        }
                        break;
                }
            }
        }
    }

    public class Log4NetLogProvider : ILogProvider
    {
        private readonly Func<string, object> _getLoggerByNameDelegate;
        private static bool _providerIsAvailableOverride = true;

        public Log4NetLogProvider()
        {
            if (!IsLoggerAvailable())
            {
                throw new InvalidOperationException("log4net.LogManager not found");
            }
            _getLoggerByNameDelegate = GetGetLoggerMethodCall();
        }

        public static bool ProviderIsAvailableOverride
        {
            get { return _providerIsAvailableOverride; }
            set { _providerIsAvailableOverride = value; }
        }

        public ILog GetLogger(string name)
        {
            return new Log4NetLogger(_getLoggerByNameDelegate(name));
        }

        public static bool IsLoggerAvailable()
        {
            return ProviderIsAvailableOverride && GetLogManagerType() != null;
        }

        private static Type GetLogManagerType()
        {
            return Type.GetType("log4net.LogManager, log4net");
        }

        private static Func<string, object> GetGetLoggerMethodCall()
        {
            Type logManagerType = GetLogManagerType();
            MethodInfo method = logManagerType.GetMethod("GetLogger", new[] { typeof(string) });
            ParameterExpression nameParam = Expression.Parameter(typeof(string), "name");
            MethodCallExpression methodCall = Expression.Call(null, method, new Expression[] { nameParam });
            return Expression.Lambda<Func<string, object>>(methodCall, new[] { nameParam }).Compile();
        }

        public class Log4NetLogger : ILog
        {
            private readonly dynamic _logger;

            internal Log4NetLogger(dynamic logger)
            {
                _logger = logger;
            }

            public void Log(LogLevel logLevel, Func<string> messageFunc)
            {
                switch (logLevel)
                {
                    case LogLevel.Info:
                        if (_logger.IsInfoEnabled)
                        {
                            _logger.Info(messageFunc());
                        }
                        break;
                    case LogLevel.Warn:
                        if (_logger.IsWarnEnabled)
                        {
                            _logger.Warn(messageFunc());
                        }
                        break;
                    case LogLevel.Error:
                        if (_logger.IsErrorEnabled)
                        {
                            _logger.Error(messageFunc());
                        }
                        break;
                    case LogLevel.Fatal:
                        if (_logger.IsFatalEnabled)
                        {
                            _logger.Fatal(messageFunc());
                        }
                        break;
                    default:
                        if (_logger.IsDebugEnabled)
                        {
                            _logger.Debug(messageFunc()); // Log4Net doesn't have a 'Trace' level, so all Trace messages are written as 'Debug'
                        }
                        break;
                }
            }

            public void Log<TException>(LogLevel logLevel, Func<string> messageFunc, TException exception)
                where TException : Exception
            {
                switch (logLevel)
                {
                    case LogLevel.Info:
                        if (_logger.IsDebugEnabled)
                        {
                            _logger.Info(messageFunc(), exception);
                        }
                        break;
                    case LogLevel.Warn:
                        if (_logger.IsWarnEnabled)
                        {
                            _logger.Warn(messageFunc(), exception);
                        }
                        break;
                    case LogLevel.Error:
                        if (_logger.IsErrorEnabled)
                        {
                            _logger.Error(messageFunc(), exception);
                        }
                        break;
                    case LogLevel.Fatal:
                        if (_logger.IsFatalEnabled)
                        {
                            _logger.Fatal(messageFunc(), exception);
                        }
                        break;
                    default:
                        if (_logger.IsDebugEnabled)
                        {
                            _logger.Debug(messageFunc(), exception);
                        }
                        break;
                }
            }
        }
    }

    public class EntLibLogProvider : ILogProvider
    {
        private static bool _providerIsAvailableOverride = true;
        private readonly MethodInfo _logEntryMethod;
        private readonly Func<string, string, TraceEventType, object> _createEntryFunc;

        public EntLibLogProvider()
        {
            if (!IsLoggerAvailable())
            {
                throw new InvalidOperationException("Microsoft.Practices.EnterpriseLibrary.Logging.Logger not found");
            }
            _logEntryMethod = GetLoggerMethod();
            _createEntryFunc = GetCreateEntryFunc();
        }

        public static bool ProviderIsAvailableOverride
        {
            get { return _providerIsAvailableOverride; }
            set { _providerIsAvailableOverride = value; }
        }

        public ILog GetLogger(string name)
        {
            return new EntLibLogger(name, _createEntryFunc, _logEntryMethod);
        }

        public static bool IsLoggerAvailable()
        {
            return ProviderIsAvailableOverride && GetEntryType() != null;
        }

        private static MethodInfo GetLoggerMethod()
        {
            var loggingType = GetLoggingType("Logger");
            return loggingType.GetMethod("Write", new[] { GetEntryType() });
        }

        private static Type GetEntryType()
        {
            return GetLoggingType("LogEntry");
        }

        private static Type GetLoggingType(string name)
        {
            return Type.GetType(string.Format("Microsoft.Practices.EnterpriseLibrary.Logging.{0}, Microsoft.Practices.EnterpriseLibrary.Logging", name));
        }

        private static Func<string, string, TraceEventType, object> GetCreateEntryFunc()
        {
            var entryType = GetEntryType();

            var logNameParameter = Expression.Parameter(typeof(string), "logName");
            var messageParameter = Expression.Parameter(typeof(string), "message");
            var severityParameter = Expression.Parameter(typeof(TraceEventType), "severity");

            var memberInit = Expression.MemberInit(Expression.New(entryType), new[]
            {
                Expression.Bind(entryType.GetProperty("Message"), messageParameter),
                Expression.Bind(entryType.GetProperty("Severity"), severityParameter),
                Expression.Bind(entryType.GetProperty("TimeStamp"),
                    Expression.Property(null, typeof (DateTime).GetProperty("UtcNow"))),
                Expression.Bind(entryType.GetProperty("Categories"),
                    Expression.ListInit(
                        Expression.New(typeof (List<string>)),
                        typeof (List<string>).GetMethod("Add", new[] {typeof (string)}),
                        logNameParameter))
            });
            return Expression.Lambda<Func<string, string, TraceEventType, object>>(
                memberInit,
                logNameParameter,
                messageParameter,
                severityParameter).Compile();
        }

        public class EntLibLogger : ILog
        {
            private readonly string _loggerName;
            private readonly Func<string, string, TraceEventType, object> _createLogEntryFunc;
            private readonly MethodInfo _writeMethod;

            internal EntLibLogger(string loggerName, Func<string, string, TraceEventType, object> createLogEntryFunc, MethodInfo writeMethod)
            {
                _loggerName = loggerName;
                _createLogEntryFunc = createLogEntryFunc;
                _writeMethod = writeMethod;
            }

            public void Log(LogLevel logLevel, Func<string> messageFunc)
            {
                var severity = MapSeverity(logLevel);
                object entry = _createLogEntryFunc(_loggerName, messageFunc(), severity);
                _writeMethod.Invoke(null, new[] { entry });
            }

            public void Log<TException>(LogLevel logLevel, Func<string> messageFunc, TException exception)
                where TException : Exception
            {
                var severity = MapSeverity(logLevel);
                var message = messageFunc() + Environment.NewLine + exception;
                object entry = _createLogEntryFunc(_loggerName, message, severity);
                _writeMethod.Invoke(null, new[] { entry });
            }

            private static TraceEventType MapSeverity(LogLevel logLevel)
            {
                switch (logLevel)
                {
                    case LogLevel.Fatal:
                        return TraceEventType.Critical;
                    case LogLevel.Error:
                        return TraceEventType.Error;
                    case LogLevel.Warn:
                        return TraceEventType.Warning;
                    case LogLevel.Info:
                        return TraceEventType.Information;
                    default:
                        return TraceEventType.Verbose;
                }
            }
        }
    }

    public class SerilogLogProvider : ILogProvider
    {
        private readonly Func<string, object> _getLoggerByNameDelegate;
        private static bool _providerIsAvailableOverride = true;

        public SerilogLogProvider()
        {
            if (!IsLoggerAvailable())
            {
                throw new InvalidOperationException("Serilog.Log not found");
            }
            _getLoggerByNameDelegate = GetForContextMethodCall();
        }

        public static bool ProviderIsAvailableOverride
        {
            get { return _providerIsAvailableOverride; }
            set { _providerIsAvailableOverride = value; }
        }

        public ILog GetLogger(string name)
        {
            return new SerilogLogger(_getLoggerByNameDelegate(name));
        }

        public static bool IsLoggerAvailable()
        {
            return ProviderIsAvailableOverride && GetLogManagerType() != null;
        }

        private static Type GetLogManagerType()
        {
            return Type.GetType("Serilog.Log, Serilog");
        }

        private static Func<string, object> GetForContextMethodCall()
        {
            Type logManagerType = GetLogManagerType();
            MethodInfo method = logManagerType.GetMethod("ForContext", new[] { typeof(string) , typeof(object), typeof(bool)});
            ParameterExpression propertyNameParam = Expression.Parameter(typeof(string), "propertyName");
            ParameterExpression valueParam = Expression.Parameter(typeof(object), "value");
            ParameterExpression destructureObjectsParam = Expression.Parameter(typeof(bool), "destructureObjects");
            MethodCallExpression methodCall = Expression.Call(null, method, new Expression[]
            {
                propertyNameParam, 
                valueParam,
                destructureObjectsParam
            });
            var func = Expression.Lambda<Func<string, object, bool, object>>(methodCall, new[]
            {
                propertyNameParam,
                valueParam,
                destructureObjectsParam
            }).Compile();
            return name => func("Name", name, false);
        }

        public class SerilogLogger : ILog
        {
            private readonly object _logger;
            private static readonly object DebugLevel;
            private static readonly object ErrorLevel;
            private static readonly object FatalLevel;
            private static readonly object InformationLevel;
            private static readonly object VerboseLevel;
            private static readonly object WarningLevel;
            private static readonly Func<object, object, bool> IsEnabled;
            private static readonly Action<object, object, string> Write;
            private static readonly Action<object, object, Exception, string> WriteException;

            static SerilogLogger()
            {
                var logEventTypeType = Type.GetType("Serilog.Events.LogEventLevel, Serilog");
                DebugLevel = Enum.Parse(logEventTypeType, "Debug");
                ErrorLevel = Enum.Parse(logEventTypeType, "Error");
                FatalLevel = Enum.Parse(logEventTypeType, "Fatal");
                InformationLevel = Enum.Parse(logEventTypeType, "Information");
                VerboseLevel = Enum.Parse(logEventTypeType, "Verbose");
                WarningLevel = Enum.Parse(logEventTypeType, "Warning");

                // Func<object, object, bool> isEnabled = (logger, level) => { return ((ILogger)logger).IsEnabled(level); }
                var loggerType = Type.GetType("Serilog.ILogger, Serilog");
                MethodInfo isEnabledMethodInfo = loggerType.GetMethod("IsEnabled");
                ParameterExpression instanceParam = Expression.Parameter(typeof(object));
                UnaryExpression instanceCast = Expression.Convert(instanceParam, loggerType);
                ParameterExpression levelParam = Expression.Parameter(typeof(object));
                UnaryExpression levelCast = Expression.Convert(levelParam, logEventTypeType);
                MethodCallExpression isEnabledMethodCall = Expression.Call(instanceCast, isEnabledMethodInfo, levelCast);
                IsEnabled = Expression.Lambda<Func<object, object, bool>>(isEnabledMethodCall, new[]
                {
                    instanceParam,
                    levelParam
                }).Compile();

                // Action<object, object, string> Write =
                // (logger, level, message) => { ((ILogger)logger).Write(level, message, new object[]); }
                MethodInfo writeMethodInfo = loggerType.GetMethod("Write", new[] { logEventTypeType, typeof(string), typeof(object[]) });
                ParameterExpression messageParam = Expression.Parameter(typeof(string));
                ConstantExpression propertyValuesParam = Expression.Constant(new object[0]);
                MethodCallExpression writeMethodExp = Expression.Call(instanceCast, writeMethodInfo, levelCast, messageParam, propertyValuesParam);
                Write = Expression.Lambda<Action<object, object, string>>(writeMethodExp, new[]
                {
                    instanceParam,
                    levelParam,
                    messageParam
                }).Compile();

                // Action<object, object, string, Exception> WriteException =
                // (logger, level, exception, message) => { ((ILogger)logger).Write(level, exception, message, new object[]); }
                MethodInfo writeExceptionMethodInfo = loggerType.GetMethod("Write", new[]
                {
                    logEventTypeType,
                    typeof(Exception), 
                    typeof(string),
                    typeof(object[])
                });
                ParameterExpression exceptionParam = Expression.Parameter(typeof(Exception));
                writeMethodExp = Expression.Call(
                    instanceCast,
                    writeExceptionMethodInfo,
                    levelCast,
                    exceptionParam,
                    messageParam,
                    propertyValuesParam);
                WriteException = Expression.Lambda<Action<object, object, Exception, string>>(writeMethodExp, new[]
                {
                    instanceParam,
                    levelParam,
                    exceptionParam,
                    messageParam,
                }).Compile();
            }

            internal SerilogLogger(object logger)
            {
                _logger = logger;
            }

            public void Log(LogLevel logLevel, Func<string> messageFunc)
            {
                switch (logLevel)
                {
                    case LogLevel.Debug:
                        if (IsEnabled(_logger, DebugLevel))
                        {
                            Write(_logger, DebugLevel, messageFunc());
                        }
                        break;
                    case LogLevel.Info:
                        if (IsEnabled(_logger, InformationLevel))
                        {
                            Write(_logger, InformationLevel, messageFunc());
                        }
                        break;
                    case LogLevel.Warn:
                        if (IsEnabled(_logger, WarningLevel))
                        {
                            Write(_logger, WarningLevel, messageFunc());
                        }
                        break;
                    case LogLevel.Error:
                        if (IsEnabled(_logger, ErrorLevel))
                        {
                            Write(_logger, ErrorLevel, messageFunc());
                        }
                        break;
                    case LogLevel.Fatal:
                        if (IsEnabled(_logger, FatalLevel))
                        {
                            Write(_logger, FatalLevel, messageFunc());
                        }
                        break;
                    default:
                        if (IsEnabled(_logger, VerboseLevel))
                        {
                            Write(_logger, VerboseLevel, messageFunc());
                        }
                        break;
                }
            }

            public void Log<TException>(LogLevel logLevel, Func<string> messageFunc, TException exception)
                where TException : Exception
            {
                switch (logLevel)
                {
                    case LogLevel.Debug:
                        if (IsEnabled(_logger, DebugLevel))
                        {
                            WriteException(_logger, DebugLevel, exception, messageFunc());
                        }
                        break;
                    case LogLevel.Info:
                        if (IsEnabled(_logger, InformationLevel))
                        {
                            WriteException(_logger, InformationLevel, exception, messageFunc());
                        }
                        break;
                    case LogLevel.Warn:
                        if (IsEnabled(_logger, WarningLevel))
                        {
                            WriteException(_logger, WarningLevel, exception, messageFunc());
                        }
                        break;
                    case LogLevel.Error:
                        if (IsEnabled(_logger, ErrorLevel))
                        {
                            WriteException(_logger, ErrorLevel, exception, messageFunc());
                        }
                        break;
                    case LogLevel.Fatal:
                        if (IsEnabled(_logger, FatalLevel))
                        {
                            WriteException(_logger, FatalLevel, exception, messageFunc());
                        }
                        break;
                    default:
                        if (IsEnabled(_logger, VerboseLevel))
                        {
                            WriteException(_logger, VerboseLevel, exception, messageFunc());
                        }
                        break;
                }
            }
        }
    }
}