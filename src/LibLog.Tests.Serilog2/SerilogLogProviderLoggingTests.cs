namespace LibLog.Logging.LogProviders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Remoting.Messaging;
    using Serilog;
    using Serilog.Context;
    using Serilog.Events;
    using Shouldly;
    using Xunit;
    using YourRootNamespace.Logging;
    using YourRootNamespace.Logging.LogProviders;
    using LogLevel = YourRootNamespace.Logging.LogLevel;

    public class SerilogLogProviderLoggingTests : IDisposable
    {
        private readonly ILog _sut;
        private LogEvent _logEvent;
        private readonly SerilogLogProvider _logProvider;
        private readonly IEnumerable<LogLevel> _allLevels = Enum.GetValues(typeof(LogLevel)).Cast<LogLevel>().ToList();
        private readonly IDictionary<LogLevel, Predicate<ILog>> _checkIsEnabledFor = new Dictionary<LogLevel, Predicate<ILog>>
        {
            {LogLevel.Trace, log => log.IsTraceEnabled()},
            {LogLevel.Debug, log => log.IsDebugEnabled()},
            {LogLevel.Info, log => log.IsInfoEnabled()},
            {LogLevel.Warn, log => log.IsWarnEnabled()},
            {LogLevel.Error, log => log.IsErrorEnabled()},
            {LogLevel.Fatal, log => log.IsFatalEnabled()},
        };

        public SerilogLogProviderLoggingTests()
        {
            var logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Is(LogEventLevel.Verbose)
                .WriteTo.Observers(obs => obs.Subscribe(logEvent => _logEvent = logEvent))
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger = logger;
            _logProvider = new SerilogLogProvider();
            _sut = new LoggerExecutionWrapper(_logProvider.GetLogger("Test"));
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

            _logEvent.Level.ShouldBe(logEventLevel);
            _logEvent.RenderMessage().ShouldBe("m");
        }

        [Theory]
        [InlineData(LogLevel.Debug, LogEventLevel.Debug)]
        [InlineData(LogLevel.Error, LogEventLevel.Error)]
        [InlineData(LogLevel.Fatal, LogEventLevel.Fatal)]
        [InlineData(LogLevel.Info, LogEventLevel.Information)]
        [InlineData(LogLevel.Trace, LogEventLevel.Verbose)]
        [InlineData(LogLevel.Warn, LogEventLevel.Warning)]
        public void Should_be_able_to_log_message_with_param(LogLevel logLevel, LogEventLevel logEventLevel)
        {
            _sut.Log(logLevel, () => "m {0}", null, "param");

            _logEvent.Level.ShouldBe(logEventLevel);
            _logEvent.RenderMessage().ShouldBe("m \"param\"");
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

            _logEvent.Level.ShouldBe(logEventLevel);
            _logEvent.RenderMessage().ShouldBe("m");
            _logEvent.Exception.ShouldBe(exception);
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

                _logEvent.Properties.Keys.ShouldContain("NDC");
                _logEvent.Properties["NDC"].ToString().ShouldBe("\"context\"");
            }
        }

        [Fact]
        public void Can_open_mapped_diagnostics_context()
        {
            using (_logProvider.OpenMappedContext("key", "value"))
            {
                _sut.Info("m");

                _logEvent.Properties.Keys.ShouldContain("key");
                _logEvent.Properties["key"].ToString().ShouldBe("\"value\"");
            }
        }

        [Fact]
        public void Can_open_mapped_diagnostics_context_destructured()
        {
            var context = new MyMappedContext();

            using (_logProvider.OpenMappedContext("key", context, true))
            {
                _sut.Info("m");

                _logEvent.Properties.Keys.ShouldContain("key");
                _logEvent.Properties["key"].ToString().ShouldBe("MyMappedContext { ThirtySeven: 37, Name: \"World\", Level: Trace }");
            }
        }

        [Fact]
        public void Can_open_mapped_diagnostics_context_not_destructured()
        {
            var context = new MyMappedContext();

            using (_logProvider.OpenMappedContext("key", context, false))
            {
                _sut.Info("m");

                _logEvent.Properties.Keys.ShouldContain("key");
                _logEvent.Properties["key"].ToString().ShouldBe("\"World\"");
            }
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
                            .ShouldBeTrue();
                           // "loglevel: '{0}' should be enabled when minimum (serilog) level is '{1}'", expectedEnabled, minimum);
                    }

                    foreach (var expectedDisabled in _allLevels.Except(expectedEnabledLevels))
                    {
                        _checkIsEnabledFor[expectedDisabled](log)
                            .ShouldBeFalse();
                        //"loglevel '{0}' should be diabled when minimum (serilog) level is '{1}'", expectedDisabled, minimum);
                    }
                });
        }

        [Fact]
        public void Can_log_structured_message()
        {
            _sut.InfoFormat("Structured {data} message", "log");

            _logEvent.RenderMessage().ShouldBe("Structured \"log\" message");
            _logEvent.Properties.Keys.ShouldContain("data");
            _logEvent.Properties["data"].ToString().ShouldBe("\"log\"");
        }
        [Fact]
        public void Can_log_structured_serialized_message()
        {
            _sut.InfoFormat("Structured {@data} message", new{ Log="log",Count="1"});
            _logEvent.RenderMessage().ShouldBe("Structured { Log: \"log\", Count: \"1\" } message");
            _logEvent.Properties.Keys.ShouldContain("data");
        }

        public void Dispose()
        {
            // Workaround for SerializationException on LogContext when using xUnit.net
            // https://github.com/serilog/serilog/issues/109#issuecomment-40256706
            CallContext.FreeNamedDataSlot(typeof(LogContext).FullName);
        }

        private static void AutoRollbackLoggerSetup(LogEventLevel minimumLevel, Action<ILog> @do)
        {
            var originalLogger = Log.Logger;
            try
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Is(minimumLevel)
                    .CreateLogger();

                @do(new LoggerExecutionWrapper(new SerilogLogProvider().GetLogger("Test")));
            }
            finally
            {
                Log.Logger = originalLogger;
            }
        }
    }
}