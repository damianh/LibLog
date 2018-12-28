namespace YourRootNamespace.Logging
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Extension methods for the <see cref="ILog"/> interface.
    /// </summary>
#if LIBLOG_EXCLUDE_CODE_COVERAGE
    [ExcludeFromCodeCoverage]
#endif
#if LIBLOG_PUBLIC
    public
#else
    internal
#endif
    static class LogExtensions
    {
        internal static readonly object[] EmptyParams = new object[0];

        /// <summary>
        ///     Check if the <see cref="LogLevel.Debug"/> log level is enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to check with.</param>
        /// <returns>True if the log level is enabled; false otherwise.</returns>
        public static bool IsDebugEnabled(this ILog logger)
        {
            GuardAgainstNullLogger(logger);
            return logger.Log(LogLevel.Debug, null, null, EmptyParams);
        }

        /// <summary>
        ///     Check if the <see cref="LogLevel.Error"/> log level is enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to check with.</param>
        /// <returns>True if the log level is enabled; false otherwise.</returns>
        public static bool IsErrorEnabled(this ILog logger)
        {
            GuardAgainstNullLogger(logger);
            return logger.Log(LogLevel.Error, null, null, EmptyParams);
        }

        /// <summary>
        ///     Check if the <see cref="LogLevel.Fatal"/> log level is enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to check with.</param>
        /// <returns>True if the log level is enabled; false otherwise.</returns>
        public static bool IsFatalEnabled(this ILog logger)
        {
            GuardAgainstNullLogger(logger);
            return logger.Log(LogLevel.Fatal, null, null, EmptyParams);
        }

        /// <summary>
        ///     Check if the <see cref="LogLevel.Info"/> log level is enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to check with.</param>
        /// <returns>True if the log level is enabled; false otherwise.</returns>
        public static bool IsInfoEnabled(this ILog logger)
        {
            GuardAgainstNullLogger(logger);
            return logger.Log(LogLevel.Info, null, null, EmptyParams);
        }

        /// <summary>
        ///     Check if the <see cref="LogLevel.Trace"/> log level is enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to check with.</param>
        /// <returns>True if the log level is enabled; false otherwise.</returns>
        public static bool IsTraceEnabled(this ILog logger)
        {
            GuardAgainstNullLogger(logger);
            return logger.Log(LogLevel.Trace, null, null, EmptyParams);
        }

        /// <summary>
        ///     Check if the <see cref="LogLevel.Warn"/> log level is enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to check with.</param>
        /// <returns>True if the log level is enabled; false otherwise.</returns>
        public static bool IsWarnEnabled(this ILog logger)
        {
            GuardAgainstNullLogger(logger);
            return logger.Log(LogLevel.Warn, null, null, EmptyParams);
        }

        /// <summary>
        ///     Logs a message at the <see cref="LogLevel.Debug"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="messageFunc">The message function.</param>
        public static void Debug(this ILog logger, Func<string> messageFunc)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Debug, WrapLogInternal(messageFunc), null, EmptyParams);
        }

        /// <summary>
        ///     Logs a message at the <see cref="LogLevel.Debug"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="message">The message.</param>
        public static void Debug(this ILog logger, string message)
        {
            if (logger.IsDebugEnabled()) logger.Log(LogLevel.Debug, message.AsFunc(), null, EmptyParams);
        }

        /// <summary>
        ///     Logs a message at the <see cref="LogLevel.Debug"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">Optional format parameters for the message.</param>
        public static void Debug(this ILog logger, string message, params object[] args)
        {
            logger.DebugFormat(message, args);
        }

        /// <summary>
        ///     Logs an exception at the <see cref="LogLevel.Debug"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">Optional format parameters for the message.</param>
        public static void Debug(this ILog logger, Exception exception, string message, params object[] args)
        {
            logger.DebugException(message, exception, args);
        }


        /// <summary>
        ///     Logs a message at the <see cref="LogLevel.Debug"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">Optional format parameters for the message.</param>
        public static void DebugFormat(this ILog logger, string message, params object[] args)
        {
            if (logger.IsDebugEnabled()) logger.LogFormat(LogLevel.Debug, message, args);
        }

        /// <summary>
        ///     Logs an exception at the <see cref="LogLevel.Debug"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        public static void DebugException(this ILog logger, string message, Exception exception)
        {
            if (logger.IsDebugEnabled()) logger.Log(LogLevel.Debug, message.AsFunc(), exception, EmptyParams);
        }

        /// <summary>
        ///     Logs an exception at the <see cref="LogLevel.Debug"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">Optional format parameters for the message.</param>
        public static void DebugException(this ILog logger, string message, Exception exception,
            params object[] formatParams)
        {
            if (logger.IsDebugEnabled()) logger.Log(LogLevel.Debug, message.AsFunc(), exception, formatParams);
        }

        /// <summary>
        ///     Logs a message at the <see cref="LogLevel.Error"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="messageFunc">The message function.</param>
        public static void Error(this ILog logger, Func<string> messageFunc)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Error, WrapLogInternal(messageFunc), null, EmptyParams);
        }

        /// <summary>
        ///     Logs a message at the <see cref="LogLevel.Error"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="message">The message.</param>
        public static void Error(this ILog logger, string message)
        {
            if (logger.IsErrorEnabled()) logger.Log(LogLevel.Error, message.AsFunc(), null, EmptyParams);
        }

        /// <summary>
        ///     Logs a message at the <see cref="LogLevel.Error"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">Optional format parameters for the message.</param>
        public static void Error(this ILog logger, string message, params object[] args)
        {
            logger.ErrorFormat(message, args);
        }

        /// <summary>
        ///     Logs an exception at the <see cref="LogLevel.Error"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">Optional format parameters for the message.</param>
        public static void Error(this ILog logger, Exception exception, string message, params object[] args)
        {
            logger.ErrorException(message, exception, args);
        }

        /// <summary>
        ///     Logs a message at the <see cref="LogLevel.Error"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">Optional format parameters for the message.</param>
        public static void ErrorFormat(this ILog logger, string message, params object[] args)
        {
            if (logger.IsErrorEnabled()) logger.LogFormat(LogLevel.Error, message, args);
        }

        /// <summary>
        ///     Logs an exception at the <see cref="LogLevel.Error"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="formatParams">Optional format parameters for the message.</param>
        public static void ErrorException(this ILog logger, string message, Exception exception,
            params object[] formatParams)
        {
            if (logger.IsErrorEnabled()) logger.Log(LogLevel.Error, message.AsFunc(), exception, formatParams);
        }

        /// <summary>
        ///     Logs a message at the <see cref="LogLevel.Fatal"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="messageFunc">The message function.</param>
        public static void Fatal(this ILog logger, Func<string> messageFunc)
        {
            logger.Log(LogLevel.Fatal, WrapLogInternal(messageFunc), null, EmptyParams);
        }

        /// <summary>
        ///     Logs a message at the <see cref="LogLevel.Fatal"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="message">The message.</param>
        public static void Fatal(this ILog logger, string message)
        {
            if (logger.IsFatalEnabled()) logger.Log(LogLevel.Fatal, message.AsFunc(), null, EmptyParams);
        }

        /// <summary>
        ///     Logs a message at the <see cref="LogLevel.Fatal"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">Optional format parameters for the message.</param>
        public static void Fatal(this ILog logger, string message, params object[] args)
        {
            logger.FatalFormat(message, args);
        }

        /// <summary>
        ///     Logs an exception at the <see cref="LogLevel.Fatal"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">Optional format parameters for the message.</param>
        public static void Fatal(this ILog logger, Exception exception, string message, params object[] args)
        {
            logger.FatalException(message, exception, args);
        }

        /// <summary>
        ///     Logs a message at the <see cref="LogLevel.Fatal"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">Optional format parameters for the message.</param>
        public static void FatalFormat(this ILog logger, string message, params object[] args)
        {
            if (logger.IsFatalEnabled()) logger.LogFormat(LogLevel.Fatal, message, args);
        }

        /// <summary>
        ///     Logs an exception at the <see cref="LogLevel.Fatal"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="formatParams">Optional format parameters for the message.</param>
        public static void FatalException(this ILog logger, string message, Exception exception,
            params object[] formatParams)
        {
            if (logger.IsFatalEnabled()) logger.Log(LogLevel.Fatal, message.AsFunc(), exception, formatParams);
        }

        /// <summary>
        ///     Logs a message at the <see cref="LogLevel.Info"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="messageFunc">The message function.</param>
        public static void Info(this ILog logger, Func<string> messageFunc)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Info, WrapLogInternal(messageFunc), null, EmptyParams);
        }

        /// <summary>
        ///     Logs a message at the <see cref="LogLevel.Info"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="message">The message.</param>
        public static void Info(this ILog logger, string message)
        {
            if (logger.IsInfoEnabled()) logger.Log(LogLevel.Info, message.AsFunc(), null, EmptyParams);
        }

        /// <summary>
        ///     Logs a message at the <see cref="LogLevel.Info"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">Optional format parameters for the message.</param>
        public static void Info(this ILog logger, string message, params object[] args)
        {
            logger.InfoFormat(message, args);
        }

        /// <summary>
        ///     Logs an exception at the <see cref="LogLevel.Info"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">Optional format parameters for the message.</param>
        public static void Info(this ILog logger, Exception exception, string message, params object[] args)
        {
            logger.InfoException(message, exception, args);
        }

        /// <summary>
        ///     Logs a message at the <see cref="LogLevel.Info"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">Optional format parameters for the message.</param>
        public static void InfoFormat(this ILog logger, string message, params object[] args)
        {
            if (logger.IsInfoEnabled()) logger.LogFormat(LogLevel.Info, message, args);
        }


        /// <summary>
        ///     Logs an exception at the <see cref="LogLevel.Info"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="formatParams">Optional format parameters for the message.</param>
        public static void InfoException(this ILog logger, string message, Exception exception,
            params object[] formatParams)
        {
            if (logger.IsInfoEnabled()) logger.Log(LogLevel.Info, message.AsFunc(), exception, formatParams);
        }

        /// <summary>
        ///     Logs a message at the <see cref="LogLevel.Trace"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="messageFunc">The message function.</param>
        public static void Trace(this ILog logger, Func<string> messageFunc)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Trace, WrapLogInternal(messageFunc), null, EmptyParams);
        }

        /// <summary>
        ///     Logs a message at the <see cref="LogLevel.Trace"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="message">The message.</param>
        public static void Trace(this ILog logger, string message)
        {
            if (logger.IsTraceEnabled()) logger.Log(LogLevel.Trace, message.AsFunc(), null, EmptyParams);
        }

        /// <summary>
        ///     Logs a message at the <see cref="LogLevel.Trace"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">Optional format parameters for the message.</param>
        public static void Trace(this ILog logger, string message, params object[] args)
        {
            logger.TraceFormat(message, args);
        }

        /// <summary>
        ///     Logs an exception at the <see cref="LogLevel.Trace"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">Optional format parameters for the message.</param>
        public static void Trace(this ILog logger, Exception exception, string message, params object[] args)
        {
            logger.TraceException(message, exception, args);
        }

        /// <summary>
        ///     Logs a message at the <see cref="LogLevel.Trace"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">Optional format parameters for the message.</param>
        public static void TraceFormat(this ILog logger, string message, params object[] args)
        {
            if (logger.IsTraceEnabled()) logger.LogFormat(LogLevel.Trace, message, args);
        }

        /// <summary>
        ///     Logs an exception at the <see cref="LogLevel.Trace"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="formatParams">Optional format parameters for the message.</param>
        public static void TraceException(this ILog logger, string message, Exception exception,
            params object[] formatParams)
        {
            if (logger.IsTraceEnabled()) logger.Log(LogLevel.Trace, message.AsFunc(), exception, formatParams);
        }

        /// <summary>
        ///     Logs a message at the <see cref="LogLevel.Warn"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="messageFunc">The message function.</param>
        public static void Warn(this ILog logger, Func<string> messageFunc)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Warn, WrapLogInternal(messageFunc), null, EmptyParams);
        }

        /// <summary>
        ///     Logs a message at the <see cref="LogLevel.Warn"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="message">The message.</param>
        public static void Warn(this ILog logger, string message)
        {
            if (logger.IsWarnEnabled()) logger.Log(LogLevel.Warn, message.AsFunc(), null, EmptyParams);
        }

        /// <summary>
        ///     Logs a message at the <see cref="LogLevel.Warn"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">Optional format parameters for the message.</param>
        public static void Warn(this ILog logger, string message, params object[] args)
        {
            logger.WarnFormat(message, args);
        }

        /// <summary>
        ///     Logs an exception at the <see cref="LogLevel.Warn"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">Optional format parameters for the message.</param>
        public static void Warn(this ILog logger, Exception exception, string message, params object[] args)
        {
            logger.WarnException(message, exception, args);
        }

        /// <summary>
        ///     Logs a message at the <see cref="LogLevel.Warn"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">Optional format parameters for the message.</param>
        public static void WarnFormat(this ILog logger, string message, params object[] args)
        {
            if (logger.IsWarnEnabled()) logger.LogFormat(LogLevel.Warn, message, args);
        }

        /// <summary>
        ///     Logs an exception at the <see cref="LogLevel.Warn"/> log level, if enabled.
        /// </summary>
        /// <param name="logger">The <see cref="ILog"/> to use.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="formatParams">Optional format parameters for the message.</param>
        public static void WarnException(this ILog logger, string message, Exception exception,
            params object[] formatParams)
        {
            if (logger.IsWarnEnabled()) logger.Log(LogLevel.Warn, message.AsFunc(), exception, formatParams);
        }

        // ReSharper disable once UnusedParameter.Local
        private static void GuardAgainstNullLogger(ILog logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");
        }

        private static void LogFormat(this ILog logger, LogLevel logLevel, string message, params object[] args)
        {
            logger.Log(logLevel, message.AsFunc(), null, args);
        }

        // Avoid the closure allocation, see https://gist.github.com/AArnott/d285feef75c18f6ecd2b
        private static Func<T> AsFunc<T>(this T value) where T : class
        {
            return value.Return;
        }

        private static T Return<T>(this T value)
        {
            return value;
        }

        // Allow passing callsite-logger-type to LogProviderBase using messageFunc
        internal static Func<string> WrapLogSafeInternal(LoggerExecutionWrapper logger, Func<string> messageFunc)
        {
            var WrappedMessageFunc = new Func<string>(() => {
                try
                {
                    return messageFunc();
                }
                catch (Exception ex)
                {
                    logger.WrappedLogger(LogLevel.Error, () => LoggerExecutionWrapper.FailedToGenerateLogMessage, ex,
                        EmptyParams);
                }

                return null;
            });

            return WrappedMessageFunc;
        }

        // Allow passing callsite-logger-type to LogProviderBase using messageFunc
        private static Func<string> WrapLogInternal(Func<string> messageFunc)
        {
			var WrappedMessageFunc = new Func<string>(() => {
				return messageFunc();
			});

            return WrappedMessageFunc;
        }
    }
}
