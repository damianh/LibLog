namespace LibLog.Logging.LogProviders
{
    using System;
    using FluentAssertions;
    using LibLog.Logging.NLog4;
    using NLog;
    using NLog.Config;
    using NLog.Targets;
    using Xunit;
    using Xunit.Extensions;
    using YourRootNamespace.Logging;
    using YourRootNamespace.Logging.LogProviders;
    using LogLevel = YourRootNamespace.Logging.LogLevel;

    public class NLog4LogProviderLoggingTests : IDisposable
    {
        private readonly ILog _sut;
        private readonly MemoryTarget _target;
        private readonly ILogProvider _logProvider;

        public NLog4LogProviderLoggingTests()
        {
            var config = new LoggingConfiguration();
            _target = new MemoryTarget
            {
                Layout = "${level:uppercase=true}|${ndc}|${mdc:item=key}|${message}|${exception}"
            };
            config.AddTarget("memory", _target);
            config.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Trace, _target));
            LogManager.Configuration = config;
            _logProvider = new NLogLogProvider();
            _sut = new LoggerExecutionWrapper(new NLogLogProvider().GetLogger("Test"));
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

            _target.Logs[0].Should().Be(messagePrefix + "|||m|");
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

            _target.Logs[0].Should().Be(messagePrefix + "|||m formatParam|");
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

            _target.Logs[0].Should().Be(messagePrefix + "|||m|e");
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

            _target.Logs[0].Should().Be(messagePrefix + "|||m replaced|e");
        }
        [Theory]
        [InlineData(LogLevel.Debug, "DEBUG")]
        [InlineData(LogLevel.Error, "ERROR")]
        [InlineData(LogLevel.Fatal, "FATAL")]
        [InlineData(LogLevel.Info, "INFO")]
        [InlineData(LogLevel.Trace, "TRACE")]
        [InlineData(LogLevel.Warn, "WARN")]
        public void Should_be_able_to_log_message_and_exception_with_format_parameters_modifiers(LogLevel logLevel, string messagePrefix)
        {
            _sut.Log(logLevel, () => "m {@abc}", new Exception("e"), new[] { "replaced" });

            _target.Logs[0].Should().Be(messagePrefix + "|||m replaced|e");
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
                _target.Logs[0].Should().Be("INFO|context||m|");
            }
        }

        [Fact]
        public void Should_log_message_with_curly_brackets()
        {
            _sut.Log(LogLevel.Debug, () => "Query language substitutions: {'true'='1', 'false'='0', 'yes'=''Y'', 'no'=''N''}");

            _target.Logs[0].Should().Contain("DEBUG|||Query language substitutions: {'true'='1', 'false'='0', 'yes'=''Y'', 'no'=''N''}");
        }

        [Fact]
        public void Can_open_mapped_diagnostics_context()
        {
            using (_logProvider.OpenMappedContext("key", "value"))
            {
                _sut.Info("m");
                _target.Logs[0].Should().Be("INFO||value|m|");
            }
        }
    }
}