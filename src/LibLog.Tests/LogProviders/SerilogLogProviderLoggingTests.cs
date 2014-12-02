namespace LibLog.Logging.LogProviders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        [Theory]
        [InlineData(LogEventLevel.Verbose, new []{LogLevel.Trace, LogLevel.Debug, LogLevel.Info, LogLevel.Warn, LogLevel.Error, LogLevel.Fatal})]
        [InlineData(LogEventLevel.Debug, new []{LogLevel.Debug, LogLevel.Info, LogLevel.Warn, LogLevel.Error, LogLevel.Fatal})]
        [InlineData(LogEventLevel.Information, new[]{LogLevel.Info, LogLevel.Warn, LogLevel.Error, LogLevel.Fatal})]
        [InlineData(LogEventLevel.Warning, new[]{LogLevel.Warn, LogLevel.Error, LogLevel.Fatal})]
        [InlineData(LogEventLevel.Error, new[]{LogLevel.Error, LogLevel.Fatal})]
        [InlineData(LogEventLevel.Fatal, new []{LogLevel.Fatal})]
        public void Should_enable_self_and_above_when_setup_with(LogEventLevel minimum, LogLevel[] expectedEnabledLevels)
        {
            AutoRollbackLoggerSetup(minimum,
                log =>
                {
                    foreach (var expectedEnabled in expectedEnabledLevels)
                    {
                        _checkIsEnabledFor[expectedEnabled](log)
                            .Should()
                            .BeTrue("loglevel: '{0}' should be enabled when minimum (serilog) level is '{1}'", expectedEnabled, minimum);
                    }

                    foreach (var expectedDisabled in _allLevels.Except(expectedEnabledLevels))
                    {
                        _checkIsEnabledFor[expectedDisabled](log)
                            .Should()
                            .BeFalse("loglevel '{0}' should be diabled when minimum (serilog) level is '{1}'", expectedDisabled, minimum);
                    }
                });
        }

        private readonly IDictionary<LogLevel, Predicate<ILog>> _checkIsEnabledFor = new Dictionary<LogLevel, Predicate<ILog>>
        {
            {LogLevel.Trace, log => log.IsTraceEnabled()},
            {LogLevel.Debug, log => log.IsDebugEnabled()},
            {LogLevel.Info, log => log.IsInfoEnabled()},
            {LogLevel.Warn, log => log.IsWarnEnabled()},
            {LogLevel.Error, log => log.IsErrorEnabled()},
            {LogLevel.Fatal, log => log.IsFatalEnabled()},
        };

        private readonly IEnumerable<LogLevel> _allLevels = Enum.GetValues(typeof (LogLevel)).Cast<LogLevel>().ToList();

        private static void AutoRollbackLoggerSetup(LogEventLevel minimumLevel, Action<ILog> @do)
        {
            var originalLogger = Log.Logger;
            try
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Is(minimumLevel)
                    .CreateLogger();

                @do(new SerilogLogProvider().GetLogger("Test"));
            }
            finally
            {
                Log.Logger = originalLogger;
            }
        }
    }
}