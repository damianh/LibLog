namespace LibLog.Logging.LogProviders
{
    using System;
    using System.Runtime.CompilerServices;
    using Gibraltar.Agent;
    using Xunit;
    using Xunit.Extensions;
    using YourRootNamespace.Logging;
    using YourRootNamespace.Logging.LogProviders;
    using ILog = YourRootNamespace.Logging.ILog;

    public class LoupeProviderLoggingTests : IDisposable
    {
        private readonly ILog _sut;

        public LoupeProviderLoggingTests()
        {
            _sut = new LoggerExecutionWrapper(new LoupeLogProvider().GetLogger("Test"));
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
            _sut.Log(logLevel, () => messagePrefix + " log message");
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
            _sut.Log(logLevel, () => messagePrefix + " log message with exception", new Exception("e"));
        }

        [Theory]
        [InlineData(LogLevel.Debug, "DEBUG")]
        [InlineData(LogLevel.Error, "ERROR")]
        [InlineData(LogLevel.Fatal, "CRITICAL")] //Fatal messages in Loupe are rendered as Critical
        [InlineData(LogLevel.Info, "INFO")]
        [InlineData(LogLevel.Trace, "DEBUG")] //Trace messages in Loupe are rendered as Debug
        [InlineData(LogLevel.Warn, "WARN")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Should_be_able_to_log_message_with_params(LogLevel logLevel, string messagePrefix)
        {
            _sut.Log(logLevel, () => messagePrefix + " log message {0}", null, "replaced");
        }

        [Theory]
        [InlineData(LogLevel.Debug, "DEBUG")]
        [InlineData(LogLevel.Error, "ERROR")]
        [InlineData(LogLevel.Fatal, "CRITICAL")] //Fatal messages in Loupe are rendered as Critical
        [InlineData(LogLevel.Info, "INFO")]
        [InlineData(LogLevel.Trace, "DEBUG")] //Trace messages in Loupe are rendered as Debug
        [InlineData(LogLevel.Warn, "WARN")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Should_be_able_to_log_message_and_exception_with_formatparams(LogLevel logLevel, string messagePrefix)
        {
            _sut.Log(logLevel, () => messagePrefix + " log message {abc} with exception", new Exception("e"), "replaced");
        }

        [Fact]
        public void Can_check_is_log_level_enabled()
        {
            _sut.AssertCanCheckLogLevelsEnabled();
        }
    }
}