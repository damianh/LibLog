namespace LibLog.Logging.LogProviders
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using log4net;
    using log4net.Appender;
    using log4net.Config;
    using log4net.Core;
    using Xunit.Extensions;
    using ILog = LibLog.Logging.ILog;

    public class Log4NetLogProviderLoggingTests : IDisposable
    {
        private readonly MemoryAppender _memoryAppender;
        private readonly ILog _sut;

        public Log4NetLogProviderLoggingTests()
        {
            _memoryAppender = new MemoryAppender();
            BasicConfigurator.Configure(_memoryAppender);
            _sut = new Log4NetLogProvider().GetLogger("Test");
        }

        public void Dispose()
        {
            LogManager.Shutdown();
        }

        [Theory]
        [InlineData(LogLevel.Debug, "DEBUG")]
        [InlineData(LogLevel.Error, "ERROR")]
        [InlineData(LogLevel.Fatal, "FATAL")]
        [InlineData(LogLevel.Info, "INFO")]
        [InlineData(LogLevel.Trace, "DEBUG")] //Trace messages in log4net are rendered as Debug
        [InlineData(LogLevel.Warn, "WARN")]
        public void Should_be_able_to_log_message(LogLevel logLevel, string messagePrefix)
        {
            _sut.Log(logLevel, () => "m");

            GetSingleMessage().Should().Be(messagePrefix + "|m|");
        }

        [Theory]
        [InlineData(LogLevel.Debug, "DEBUG")]
        [InlineData(LogLevel.Error, "ERROR")]
        [InlineData(LogLevel.Fatal, "FATAL")]
        [InlineData(LogLevel.Info, "INFO")]
        [InlineData(LogLevel.Trace, "DEBUG")] //Trace messages in log4net are rendered as Debug
        [InlineData(LogLevel.Warn, "WARN")]
        public void Should_be_able_to_log_message_and_exception(LogLevel logLevel, string messagePrefix)
        {
            _sut.Log(logLevel, () => "m", new Exception("e"));

            GetSingleMessage().Should().Be(messagePrefix + "|m|e");
        }

        private string GetSingleMessage()
        {
            LoggingEvent loggingEvent = _memoryAppender.GetEvents().Single();
            return string.Format(
                "{0}|{1}|{2}",
                loggingEvent.Level,
                loggingEvent.MessageObject,
                loggingEvent.ExceptionObject != null ? loggingEvent.ExceptionObject.Message : string.Empty);
        }
    }
}