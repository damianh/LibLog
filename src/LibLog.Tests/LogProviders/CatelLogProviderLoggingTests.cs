namespace LibLog.Logging.LogProviders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Catel.Logging;
    using FluentAssertions;
    using Xunit;
    using Xunit.Extensions;
    using YourRootNamespace.Logging;
    using YourRootNamespace.Logging.LogProviders;
    using ILog = YourRootNamespace.Logging.ILog;
    using LogManager = log4net.LogManager;

    public class CatelProviderLoggingTests : IDisposable
    {
        private class CatelLogListener : Catel.Logging.LogListenerBase
        {
            private readonly List<LogEntry> _logEntries = new List<LogEntry>();

            protected override void Write(Catel.Logging.ILog log, string message, LogEvent logEvent, object extraData, DateTime time)
            {
                if (log.TargetType != typeof (object))
                {
                    return;
                }

                _logEntries.Add(new LogEntry(log, message, logEvent, extraData, time));

                base.Write(log, message, logEvent, extraData, time);
            }

            public IEnumerable<LogEntry> GetEntries()
            {
                return _logEntries;
            }
        }

        private readonly CatelLogListener _logListener;
        private readonly ILog _sut;
        private readonly ILogProvider _logProvider;

        public CatelProviderLoggingTests()
        {
            _logListener = new CatelLogListener();
            Catel.Logging.LogManager.AddListener(_logListener);

            _sut = new LoggerExecutionWrapper(new CatelLogProvider().GetLogger("Test"));
        }

        public void Dispose()
        {
            LogManager.Shutdown();
        }

        [Theory]
        [InlineData(LogLevel.Debug, "Debug")]
        [InlineData(LogLevel.Error, "Error")]
        [InlineData(LogLevel.Fatal, "Error")] //Fatal messages in Catel are rendered as Error
        [InlineData(LogLevel.Info, "Info")]
        [InlineData(LogLevel.Trace, "Debug")] //Trace messages in Catel are rendered as Debug
        [InlineData(LogLevel.Warn, "Warning")]
        public void Should_be_able_to_log_message(LogLevel logLevel, string messagePrefix)
        {
            _sut.Log(logLevel, () => "m");

            GetSingleMessage().Should().Be(messagePrefix + "|m|");
        }

        [Theory]
        [InlineData(LogLevel.Debug, "Debug")]
        [InlineData(LogLevel.Error, "Error")]
        [InlineData(LogLevel.Fatal, "Error")] //Fatal messages in Catel are rendered as Error
        [InlineData(LogLevel.Info, "Info")]
        [InlineData(LogLevel.Trace, "Debug")] //Trace messages in Catel are rendered as Debug
        [InlineData(LogLevel.Warn, "Warning")]
        public void Should_be_able_to_log_message_and_exception(LogLevel logLevel, string messagePrefix)
        {
            _sut.Log(logLevel, () => "m", new Exception("e"));

            GetSingleMessage().Should().Be(messagePrefix + "|m | [Exception] System.Exception: e|");
        }

        [Theory]
        [InlineData(LogLevel.Debug, "Debug")]
        [InlineData(LogLevel.Error, "Error")]
        [InlineData(LogLevel.Fatal, "Error")] //Fatal messages in Catel are rendered as Error
        [InlineData(LogLevel.Info, "Info")]
        [InlineData(LogLevel.Trace, "Debug")] //Trace messages in Catel are rendered as Debug
        [InlineData(LogLevel.Warn, "Warning")]
        public void Should_be_able_to_log_message_with_formatParams(LogLevel logLevel, string messagePrefix)
        {
            _sut.Log(logLevel, () => "m {0}", null, "replaced");

            GetSingleMessage().Should().Be(messagePrefix + "|m replaced|");
        }

        [Theory]
        [InlineData(LogLevel.Debug, "Debug")]
        [InlineData(LogLevel.Error, "Error")]
        [InlineData(LogLevel.Fatal, "Error")] //Fatal messages in Catel are rendered as Error
        [InlineData(LogLevel.Info, "Info")]
        [InlineData(LogLevel.Trace, "Debug")] //Trace messages in Catel are rendered as Debug
        [InlineData(LogLevel.Warn, "Warning")]
        public void Should_be_able_to_log_message_and_exception_with_formatParams(LogLevel logLevel, string messagePrefix)
        {
            _sut.Log(logLevel, () => "m {abc}", new Exception("e"), "replaced");

            GetSingleMessage().Should().Be(messagePrefix + "|m replaced | [Exception] System.Exception: e|");
        }
        [Theory]
        [InlineData(LogLevel.Debug, "Debug")]
        [InlineData(LogLevel.Error, "Error")]
        [InlineData(LogLevel.Fatal, "Error")] //Fatal messages in Catel are rendered as Error
        [InlineData(LogLevel.Info, "Info")]
        [InlineData(LogLevel.Trace, "Debug")] //Trace messages in Catel are rendered as Debug
        [InlineData(LogLevel.Warn, "Warning")]
        public void Should_be_able_to_log_message_and_exception_with_formatParams_modifiers(LogLevel logLevel, string messagePrefix)
        {
            _sut.Log(logLevel, () => "m {@abc}", new Exception("e"), "replaced");

            GetSingleMessage().Should().Be(messagePrefix + "|m replaced | [Exception] System.Exception: e|");
        }

        [Fact]
        public void Can_check_is_log_level_enabled()
        {
            _sut.AssertCanCheckLogLevelsEnabled();
        }

        //[Fact]
        //public void Can_open_nested_diagnostics_context()
        //{
        //    using (_logProvider.OpenNestedContext("context"))
        //    {
        //        _sut.Info("m");
        //        var loggingEvent = _logListener.GetEntries().Single();

        //        loggingEvent.Properties.GetKeys().Should().Contain("NDC");
        //        loggingEvent.Properties["NDC"].Should().Be("context");
        //    }
        //}

        //[Fact]
        //public void Can_open_mapped_diagnostics_context()
        //{
        //    using (_logProvider.OpenMappedContext("key", "value"))
        //    {
        //        _sut.Info("m");
        //        var loggingEvent = _logListener.GetEntries().Single();

        //        loggingEvent.Properties.GetKeys().Should().Contain("key");
        //        loggingEvent.Properties["key"].Should().Be("value");
        //    }
        //}

        [Fact]
        public void Should_log_message_with_curly_brackets()
        {
            _sut.Log(LogLevel.Debug, () => "Query language substitutions: {'true'='1', 'false'='0', 'yes'=''Y'', 'no'=''N''}");

            GetSingleMessage().Should().Contain("Debug|Query language substitutions: {'true'='1', 'false'='0', 'yes'=''Y'', 'no'=''N''}");
        }

        private string GetSingleMessage()
        {
            var logEntry = _logListener.GetEntries().Single();
            return string.Format(
                "{0}|{1}|",
                logEntry.LogEvent,
                logEntry.Message);
        }
    }
}