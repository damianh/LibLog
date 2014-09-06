namespace DH.Logging.LogProviders
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using DH.Logging;

    public class NLogLogProvider : ILogProvider
    {
        private readonly Func<string, object> _getLoggerByNameDelegate;
        private static bool _providerIsAvailableOverride = true;
        
        public NLogLogProvider()
        {
            if(!IsLoggerAvailable())
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
            MethodInfo method = logManagerType.GetMethod("GetLogger", new[] {typeof(string)});
            ParameterExpression resultValue;
            ParameterExpression keyParam = Expression.Parameter(typeof(string), "key");
            MethodCallExpression methodCall = Expression.Call(null, method, new Expression[] {resultValue = keyParam});
            return Expression.Lambda<Func<string, object>>(methodCall, new[] {resultValue}).Compile();
        }

        public class NLogLogger : ILog
        {
            private readonly dynamic _logger;

            internal NLogLogger(object logger)
            {
                _logger = logger;
            }

            public void Log(LogLevel logLevel, Func<string> messageFunc)
            {
                switch(logLevel)
                {
                    case LogLevel.Debug:
                        if(_logger.IsDebugEnabled)
                        {
                            _logger.Debug(messageFunc());
                        }
                        break;
                    case LogLevel.Info:
                        if(_logger.IsInfoEnabled)
                        {
                            _logger.Info(messageFunc());
                        }
                        break;
                    case LogLevel.Warn:
                        if(_logger.IsWarnEnabled)
                        {
                            _logger.Warn(messageFunc());
                        }
                        break;
                    case LogLevel.Error:
                        if(_logger.IsErrorEnabled)
                        {
                            _logger.Error(messageFunc());
                        }
                        break;
                    case LogLevel.Fatal:
                        if(_logger.IsFatalEnabled)
                        {
                            _logger.Fatal(messageFunc());
                        }
                        break;
                    default:
                        if(_logger.IsTraceEnabled)
                        {
                            _logger.Trace(messageFunc());
                        }
                        break;
                }
            }

            public void Log<TException>(LogLevel logLevel, Func<string> messageFunc, TException exception)
                where TException : Exception
            {
                switch(logLevel)
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
}