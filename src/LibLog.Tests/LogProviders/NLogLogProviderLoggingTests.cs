namespace LibLog.Logging.LogProviders
{
    using System;
    using FluentAssertions;
    using NLog;
    using NLog.Config;
    using NLog.Targets;
    using Xunit;
    using Xunit.Extensions;
    using LogLevel = LibLog.Logging.LogLevel;

    public class NLogLogProviderLoggingTests : IDisposable
    {
        private readonly ILog _sut;
        private readonly MemoryTarget _target;

        public NLogLogProviderLoggingTests()
        {
            var config = new LoggingConfiguration();
            _target = new MemoryTarget
            {
                Layout = "${level:uppercase=true}|${message}|${exception}"
            };
            config.AddTarget("memory", _target);
            config.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Trace, _target));
            LogManager.Configuration = config;
            _sut = new NLogLogProvider().GetLogger("Test");
        }

        public void Dispose()
        {
            LogManager.Configuration = null;
        }

        [Theory]
        [InlineData(LogLevel.Debug, "DEBUG")]
        [InlineData(LogLevel.Error, "ERROR")]
        [InlineData(LogLevel.Fatal, "FATAL")]
        [InlineData(LogLevel.Info, "INFO")]
        [InlineData(LogLevel.Trace, "TRACE")]
        [InlineData(LogLevel.Warn, "WARN")]
        public void Should_be_able_to_log_message(LogLevel logLevel, string messagePrefix)
        {
            _sut.Log(logLevel, () => "m");

            _target.Logs[0].Should().Be(messagePrefix + "|m|");
        }

        [Theory]
        [InlineData(LogLevel.Debug, "DEBUG")]
        [InlineData(LogLevel.Error, "ERROR")]
        [InlineData(LogLevel.Fatal, "FATAL")]
        [InlineData(LogLevel.Info, "INFO")]
        [InlineData(LogLevel.Trace, "TRACE")]
        [InlineData(LogLevel.Warn, "WARN")]
        public void Should_be_able_to_log_message_with_format_parameters(LogLevel logLevel, string messagePrefix)
        {
            _sut.Log(logLevel, () => "m {0}", null, "formatParam");

            _target.Logs[0].Should().Be(messagePrefix + "|m formatParam|");
        }

        [Theory]
        [InlineData(LogLevel.Debug, "DEBUG")]
        [InlineData(LogLevel.Error, "ERROR")]
        [InlineData(LogLevel.Fatal, "FATAL")]
        [InlineData(LogLevel.Info, "INFO")]
        [InlineData(LogLevel.Trace, "TRACE")]
        [InlineData(LogLevel.Warn, "WARN")]
        public void Should_be_able_to_log_message_and_exception(LogLevel logLevel, string messagePrefix)
        {
            _sut.Log(logLevel, () => "m", new Exception("e"));

            _target.Logs[0].Should().Be(messagePrefix + "|m|e");
        }

        [Theory]
        [InlineData(LogLevel.Debug, "DEBUG")]
        [InlineData(LogLevel.Error, "ERROR")]
        [InlineData(LogLevel.Fatal, "FATAL")]
        [InlineData(LogLevel.Info, "INFO")]
        [InlineData(LogLevel.Trace, "TRACE")]
        [InlineData(LogLevel.Warn, "WARN")]
        public void Should_be_able_to_log_message_and_exception_with_format_parameters(LogLevel logLevel, string messagePrefix)
        {
            _sut.Log(logLevel, () => "m {abc}", new Exception("e"), new []{"replaced"});

            _target.Logs[0].Should().Be(messagePrefix + "|m replaced|e");
        }

        [Fact]
        public void Can_check_is_log_level_enabled()
        {
           _sut.AssertCanCheckLogLevelsEnabled();
        }
    }
}