namespace LibLog.Logging.LogProviders
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using FluentAssertions;
    using Gibraltar.Agent;
    using Xunit;
    using Xunit.Extensions;
    using ILog = LibLog.Logging.ILog;

    public class LoupeProviderLoggingTests : IDisposable
    {
        private readonly ILog _logger;

        public LoupeProviderLoggingTests()
        {
            _logger = new LoupeLogProvider().GetLogger("Test");
        }

        public void Dispose()
        {
            //this is debatably sketchy since we're not likely to be really shutting down, but otherwise we can hang the unit tests.
            Log.EndSession();
        }

        [Theory]
        [InlineData(LogLevel.Debug, "DEBUG")]
        [InlineData(LogLevel.Error, "ERROR")]
        [InlineData(LogLevel.Fatal, "CRITICAL")] //Fatal messages in Loupe are rendered as Critical
        [InlineData(LogLevel.Info, "INFO")]
        [InlineData(LogLevel.Trace, "DEBUG")] //Trace messages in Loupe are rendered as Debug
        [InlineData(LogLevel.Warn, "WARN")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Should_be_able_to_log_message(LogLevel logLevel, string messagePrefix)
        {
            _logger.Log(logLevel, () => messagePrefix + " log message");
        }

        [Theory]
        [InlineData(LogLevel.Debug, "DEBUG")]
        [InlineData(LogLevel.Error, "ERROR")]
        [InlineData(LogLevel.Fatal, "CRITICAL")] //Fatal messages in Loupe are rendered as Critical
        [InlineData(LogLevel.Info, "INFO")]
        [InlineData(LogLevel.Trace, "DEBUG")] //Trace messages in Loupe are rendered as Debug
        [InlineData(LogLevel.Warn, "WARN")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Should_be_able_to_log_message_and_exception(LogLevel logLevel, string messagePrefix)
        {
            _logger.Log(logLevel, () => messagePrefix + " log message with exception", new Exception("e"));
        }

        [Fact]
        public void Can_check_is_log_level_enabled()
        {
            _logger.AssertCanCheckLogLevelsEnabled();
        }
    }
}