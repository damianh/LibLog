namespace LibLog.Logging.LogProviders
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using LibLog.Logging;
    using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
    using Microsoft.Practices.EnterpriseLibrary.Logging;
    using Microsoft.Practices.EnterpriseLibrary.Logging.Filters;
    using Microsoft.Practices.ServiceLocation;
    using Xunit;

    public class EntLibLogProviderLoggingTests : IDisposable
    {
        private class ServiceLocatorStub : ServiceLocatorImplBase
        {
            private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();

            protected override object DoGetInstance(Type serviceType, string key)
            {
                return _instances[serviceType];
            }

            protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
            {
                return new[] { DoGetInstance(serviceType, null) };
            }

            public void Register<TService>(TService instance)
            {
                _instances[typeof (TService)] = instance;
            }
        }

        private class LogWriterStub : LogWriter
        {
            public readonly List<LogEntry> Entries = new List<LogEntry>();

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
                return false;
            }

            public override bool ShouldLog(LogEntry log)
            {
                return true;
            }

            public override void Write(LogEntry log)
            {
                Entries.Add(log);
            }

            public override IDictionary<string, LogSource> TraceSources
            {
                get { return new Dictionary<string, LogSource>(); }
            }
        }

        public void Dispose()
        {
            EnterpriseLibraryContainer.Current = null;
        }

        [Fact]
        public void Provider_should_log_events()
        {
            var writer = new LogWriterStub();
            var container = new ServiceLocatorStub();
            container.Register<LogWriter>(writer);
            EnterpriseLibraryContainer.Current = container;

            LogProvider.SetCurrentLogProvider(new EntLibLogProvider());

            var log = LogProvider.GetLogger(GetType());
            log.Log(LogLevel.Error, () => "test");

            Assert.NotEmpty(writer.Entries);
            Assert.Equal(1, writer.Entries.Count);
            Assert.Equal("test", writer.Entries.Single().Message);
            Assert.Equal(1, writer.Entries.Single().Categories.Count);
            Assert.Equal(GetType().FullName, writer.Entries.Single().Categories.Single());
            Assert.Equal(TraceEventType.Error, writer.Entries.Single().Severity);
        }
    }
}