using System;
using NLog;
using NLog.Config;
using NLog.Targets;
using Xunit;

namespace DH.Logging.LogProviders
{
    public class NLogLogProviderLoggingDisabedTests : IDisposable
    {
        private ILog _sut;
        private MemoryTarget _target;

        public void Dispose()
        {
            LogManager.Configuration = null;
        }

        [Fact]
        public void For_Debug_Then_should_not_log()
        {
            ConfigureLogger(NLog.LogLevel.Debug);
            AssertShouldNotLog(global::DH.Logging.LogLevel.Debug);
        }

        [Fact]
        public void For_Error_Then_should_not_log()
        {
            ConfigureLogger(NLog.LogLevel.Error);
            AssertShouldNotLog(global::DH.Logging.LogLevel.Error);
        }

        [Fact]
        public void For_Fatal_Then_should_not_log()
        {
            ConfigureLogger(NLog.LogLevel.Fatal);
            AssertShouldNotLog(global::DH.Logging.LogLevel.Fatal);
        }

        [Fact]
        public void For_Info_Then_should_not_log()
        {
            ConfigureLogger(NLog.LogLevel.Info);
            AssertShouldNotLog(global::DH.Logging.LogLevel.Info);
        }

        [Fact]
        public void For_Trace_Then_should_not_log()
        {
            ConfigureLogger(NLog.LogLevel.Trace);
            AssertShouldNotLog(global::DH.Logging.LogLevel.Trace);
        }

        [Fact]
        public void For_Warn_Then_should_not_log()
        {
            ConfigureLogger(NLog.LogLevel.Warn);
            AssertShouldNotLog(global::DH.Logging.LogLevel.Warn);
        }

        private void AssertShouldNotLog(global::DH.Logging.LogLevel logLevel)
        {
            _sut.Log(logLevel, () => "m");
            _sut.Log(logLevel, () => "m", new Exception("e"));
            Assert.Empty(_target.Logs);
        }

        private void ConfigureLogger(NLog.LogLevel nlogLogLevel)
        {
            var config = new LoggingConfiguration();
            _target = new MemoryTarget();
            _target.Layout = "${level:uppercase=true}|${message}|${exception}";
            config.AddTarget("memory", _target);
            var loggingRule = new LoggingRule("*", NLog.LogLevel.Trace, _target);
            loggingRule.DisableLoggingForLevel(nlogLogLevel);
            config.LoggingRules.Add(loggingRule);
            LogManager.Configuration = config;
            _sut = new NLogLogProvider().GetLogger("Test");
        }
    }
}