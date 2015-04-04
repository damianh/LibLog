namespace YourRootNamespace.Logging.LogProviders
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using FluentAssertions;
    using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
    using Microsoft.Practices.EnterpriseLibrary.Logging;
    using Microsoft.Practices.EnterpriseLibrary.Logging.Filters;
    using Microsoft.Practices.ServiceLocation;
    using Xunit;
    using Xunit.Extensions;
    using LogLevel = YourRootNamespace.Logging.LogLevel;

    public class EntLibLogProviderLoggingTests : IDisposable
    {
        private static readonly ILog Sut;
        private static readonly LogWriterStub Target =  new LogWriterStub();

        static EntLibLogProviderLoggingTests()
        {
            var container = new ServiceLocatorStub();
            container.Register<LogWriter>(Target);
            EnterpriseLibraryContainer.Current = container;
            Sut = new LoggerExecutionWrapper(new EntLibLogProvider().GetLogger("Test"));
        }

        public void Dispose()
        {
            Target.Logs.Clear();
        }

        [Theory]
        [InlineData(LogLevel.Debug, TraceEventType.Verbose)]
        [InlineData(LogLevel.Error, TraceEventType.Error)]
        [InlineData(LogLevel.Fatal, TraceEventType.Critical)]
        [InlineData(LogLevel.Info, TraceEventType.Information)]
        [InlineData(LogLevel.Trace, TraceEventType.Verbose)]
        [InlineData(LogLevel.Warn, TraceEventType.Warning)]
        public void Should_be_able_to_log_message(LogLevel logLevel, TraceEventType severity)
        {
            Sut.Log(logLevel, () => "m");

            Target.Logs[0].Message.Should().Be("m");
            Target.Logs[0].Severity.Should().Be(severity);
        }

        [Theory]
        [InlineData(LogLevel.Debug, TraceEventType.Verbose)]
        [InlineData(LogLevel.Error, TraceEventType.Error)]
        [InlineData(LogLevel.Fatal, TraceEventType.Critical)]
        [InlineData(LogLevel.Info, TraceEventType.Information)]
        [InlineData(LogLevel.Trace, TraceEventType.Verbose)]
        [InlineData(LogLevel.Warn, TraceEventType.Warning)]
        public void Should_be_able_to_log_message_and_exception(LogLevel logLevel, TraceEventType severity)
        {
            var exception = new Exception("e");

            Sut.Log(logLevel, () => "m", exception);

            Target.Logs[0].Message.Should().Be("m" + Environment.NewLine + exception);
            Target.Logs[0].Severity.Should().Be(severity);
        }

        [Theory]
        [InlineData(LogLevel.Debug, TraceEventType.Verbose)]
        [InlineData(LogLevel.Error, TraceEventType.Error)]
        [InlineData(LogLevel.Fatal, TraceEventType.Critical)]
        [InlineData(LogLevel.Info, TraceEventType.Information)]
        [InlineData(LogLevel.Trace, TraceEventType.Verbose)]
        [InlineData(LogLevel.Warn, TraceEventType.Warning)]
        public void Should_be_able_to_log_message_with_formatparams(LogLevel logLevel, TraceEventType severity)
        {
            Sut.Log(logLevel, () => "m {0}", null, "replaced");

            Target.Logs[0].Message.Should().Be("m replaced");
            Target.Logs[0].Severity.Should().Be(severity);
        }

        [Fact]
        public void Should_log_message_with_curly_brackets()
        {
            Sut.Log(LogLevel.Debug, () => "Query language substitutions: {'true'='1', 'false'='0', 'yes'=''Y'', 'no'=''N''}");

            Target.Logs[0].Message.Should().Contain("Query language substitutions: {'true'='1', 'false'='0', 'yes'=''Y'', 'no'=''N''}");
        }

        [Theory]
        [InlineData(LogLevel.Debug, TraceEventType.Verbose)]
        [InlineData(LogLevel.Error, TraceEventType.Error)]
        [InlineData(LogLevel.Fatal, TraceEventType.Critical)]
        [InlineData(LogLevel.Info, TraceEventType.Information)]
        [InlineData(LogLevel.Trace, TraceEventType.Verbose)]
        [InlineData(LogLevel.Warn, TraceEventType.Warning)]
        public void Should_be_able_to_log_message_and_exception_with_formatparams(LogLevel logLevel, TraceEventType severity)
        {
            var exception = new Exception("e");

            Sut.Log(logLevel, () => "m {abc}", exception, "replaced");

            Target.Logs[0].Message.Should().Be("m replaced" + Environment.NewLine + exception);
            Target.Logs[0].Severity.Should().Be(severity);
        }

        [Fact]
        public void Can_check_is_log_level_enabled()
        {
            Sut.AssertCanCheckLogLevelsEnabled();
        }

        private class ServiceLocatorStub : ServiceLocatorImplBase
        {
            private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();

            protected override object DoGetInstance(Type serviceType, string key)
            {
                return _instances[serviceType];
            }

            protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
            {
                return new[] {DoGetInstance(serviceType, null)};
            }

            public void Register<TService>(TService instance)
            {
                _instances[typeof (TService)] = instance;
            }
        }

        private class LogWriterStub : LogWriter
        {
            public readonly List<LogEntry> Logs = new List<LogEntry>();

            public override T GetFilter<T>()
            {
                return default(T);
            }

            public override T GetFilter<T>(string name)
            {
                return default(T);
            }

            public override ILogFilter GetFilter(string name)
            {
                return null;
            }

            public override IEnumerable<LogSource> GetMatchingTraceSources(LogEntry logEntry)
            {
                return Enumerable.Empty<LogSource>();
            }

            public override bool IsLoggingEnabled()
            {
                return true;
            }

            public override bool IsTracingEnabled()
            {
                return true;
            }

            public override bool ShouldLog(LogEntry log)
            {
                return true;
            }

            public override void Write(LogEntry log)
            {
                Logs.Add(log);
            }

            public override IDictionary<string, LogSource> TraceSources
            {
                get { return new Dictionary<string, LogSource>(); }
            }
        }
    }
}
