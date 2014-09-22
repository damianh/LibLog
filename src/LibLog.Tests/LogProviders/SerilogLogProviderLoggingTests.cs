namespace LibLog.Logging.LogProviders
{
    using System;
    using FluentAssertions;
    using Serilog;
    using Serilog.Events;
    using Xunit;
    using Xunit.Extensions;
    using LogLevel = LibLog.Logging.LogLevel;

    public class SerilogLogProviderLoggingTests
    {
        private readonly ILog _sut;
        private LogEvent _logEvent;

        public SerilogLogProviderLoggingTests()
        {
            var logger = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Verbose)
                .WriteTo.Observers(obs => obs.Subscribe(logEvent => _logEvent = logEvent))
                .WriteTo.Console()
                .CreateLogger();


            Log.Logger = logger;
            _sut = new SerilogLogProvider().GetLogger("Test");
        }

        [Theory]
        [InlineData(LogLevel.Debug, LogEventLevel.Debug)]
        [InlineData(LogLevel.Error, LogEventLevel.Error)]
        [InlineData(LogLevel.Fatal, LogEventLevel.Fatal)]
        [InlineData(LogLevel.Info, LogEventLevel.Information)]
        [InlineData(LogLevel.Trace, LogEventLevel.Verbose)]
        [InlineData(LogLevel.Warn, LogEventLevel.Warning)]
        public void Should_be_able_to_log_message(LogLevel logLevel, LogEventLevel logEventLevel)
        {
            _sut.Log(logLevel, () => "m");

            _logEvent.Level.Should().Be(logEventLevel);
            _logEvent.RenderMessage().Should().Be("m");
        }

        [Theory]
        [InlineData(LogLevel.Debug, LogEventLevel.Debug)]
        [InlineData(LogLevel.Error, LogEventLevel.Error)]
        [InlineData(LogLevel.Fatal, LogEventLevel.Fatal)]
        [InlineData(LogLevel.Info, LogEventLevel.Information)]
        [InlineData(LogLevel.Trace, LogEventLevel.Verbose)]
        [InlineData(LogLevel.Warn, LogEventLevel.Warning)]
        public void Should_be_able_to_log_message_and_exception(LogLevel logLevel, LogEventLevel logEventLevel)
        {
            var exception = new Exception("e");
            _sut.Log(logLevel, () => "m", exception);

            _logEvent.Level.Should().Be(logEventLevel);
            _logEvent.RenderMessage().Should().Be("m");
            _logEvent.Exception.Should().Be(exception);
        }

        [Fact]
        public void Can_check_is_log_level_enabled()
        {
            _sut.AssertCanCheckLogLevelsEnabled();
        }
    }
}