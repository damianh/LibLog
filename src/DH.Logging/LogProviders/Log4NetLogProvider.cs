namespace DH.Logging.LogProviders
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using DH.Logging;

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
            MethodInfo method = logManagerType.GetMethod("GetLogger", new[] {typeof(string)});
            ParameterExpression resultValue;
            ParameterExpression keyParam = Expression.Parameter(typeof(string), "key");
            MethodCallExpression methodCall = Expression.Call(null, method, new Expression[] {resultValue = keyParam});
            return Expression.Lambda<Func<string, object>>(methodCall, new[] {resultValue}).Compile();
        }

        public class Log4NetLogger : ILog
        {
            private readonly dynamic _logger;

            internal Log4NetLogger(object logger)
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
}