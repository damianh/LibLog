namespace LibLog.Logging.LogProviders
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using log4net;
    using log4net.Appender;
    using log4net.Config;
    using log4net.Core;
    using Xunit;
    using Xunit.Extensions;
    using ILog = LibLog.Logging.ILog;

    public class Log4NetLogProviderLoggingTests : IDisposable
    {
        private readonly MemoryAppender _memoryAppender;
        private readonly ILog _sut;
        private readonly ILogProvider _logProvider;

        public Log4NetLogProviderLoggingTests()
        {
            _memoryAppender = new MemoryAppender();
            BasicConfigurator.Configure(_memoryAppender);
            _logProvider = new Log4NetLogProvider();
            _sut = new LoggerExecutionWrapper(new Log4NetLogProvider().GetLogger("Test"));
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

        [Theory]
        [InlineData(LogLevel.Debug, "DEBUG")]
        [InlineData(LogLevel.Error, "ERROR")]
        [InlineData(LogLevel.Fatal, "FATAL")]
        [InlineData(LogLevel.Info, "INFO")]
        [InlineData(LogLevel.Trace, "DEBUG")] //Trace messages in log4net are rendered as Debug
        [InlineData(LogLevel.Warn, "WARN")]
        public void Should_be_able_to_log_message_with_formatParams(LogLevel logLevel, string messagePrefix)
        {
            _sut.Log(logLevel, () => "m {0}", null, "replaced");

            GetSingleMessage().Should().Be(messagePrefix + "|m replaced|");
        }

        [Theory]
        [InlineData(LogLevel.Debug, "DEBUG")]
        [InlineData(LogLevel.Error, "ERROR")]
        [InlineData(LogLevel.Fatal, "FATAL")]
        [InlineData(LogLevel.Info, "INFO")]
        [InlineData(LogLevel.Trace, "DEBUG")] //Trace messages in log4net are rendered as Debug
        [InlineData(LogLevel.Warn, "WARN")]
        public void Should_be_able_to_log_message_and_exception_with_formatParams(LogLevel logLevel, string messagePrefix)
        {
            _sut.Log(logLevel, () => "m {abc}", new Exception("e"), "replaced");

            GetSingleMessage().Should().Be(messagePrefix + "|m replaced|e");
        }

        [Fact]
        public void Can_check_is_log_level_enabled()
        {
            _sut.AssertCanCheckLogLevelsEnabled();
        }

        [Fact]
        public void Can_open_nested_diagnostics_context()
        {
            using (_logProvider.OpenNestedContext("context"))
            {
                _sut.Info("m");
                var loggingEvent = _memoryAppender.GetEvents().Single();

                loggingEvent.Properties.GetKeys().Should().Contain("NDC");
                loggingEvent.Properties["NDC"].Should().Be("context");
            }
        }

        [Fact]
        public void Can_open_mapped_diagnostics_context()
        {
            using (_logProvider.OpenMappedContext("key", "value"))
            {
                _sut.Info("m");
                var loggingEvent = _memoryAppender.GetEvents().Single();

                loggingEvent.Properties.GetKeys().Should().Contain("key");
                loggingEvent.Properties["key"].Should().Be("value");
            }
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