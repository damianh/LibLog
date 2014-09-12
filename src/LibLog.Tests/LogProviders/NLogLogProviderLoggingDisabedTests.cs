namespace LibLog.Logging.LogProviders
{
    using System;
    using NLog;
    using NLog.Config;
    using NLog.Targets;
    using Xunit;
    using LogLevel = LibLog.Logging.LogLevel;

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
            AssertShouldNotLog(LogLevel.Debug);
        }

        [Fact]
        public void For_Error_Then_should_not_log()
        {
            ConfigureLogger(NLog.LogLevel.Error);
            AssertShouldNotLog(LogLevel.Error);
        }

        [Fact]
        public void For_Fatal_Then_should_not_log()
        {
            ConfigureLogger(NLog.LogLevel.Fatal);
            AssertShouldNotLog(LogLevel.Fatal);
        }

        [Fact]
        public void For_Info_Then_should_not_log()
        {
            ConfigureLogger(NLog.LogLevel.Info);
            AssertShouldNotLog(LogLevel.Info);
        }

        [Fact]
        public void For_Trace_Then_should_not_log()
        {
            ConfigureLogger(NLog.LogLevel.Trace);
            AssertShouldNotLog(LogLevel.Trace);
        }

        [Fact]
        public void For_Warn_Then_should_not_log()
        {
            ConfigureLogger(NLog.LogLevel.Warn);
            AssertShouldNotLog(LogLevel.Warn);
        }

        private void AssertShouldNotLog(LogLevel logLevel)
        {
            _sut.Log(logLevel, () => "m");
            _sut.Log(logLevel, () => "m", new Exception("e"));
            Assert.Empty(_target.Logs);
        }

        private void ConfigureLogger(NLog.LogLevel nlogLogLevel)
        {
            var config = new LoggingConfiguration();
            _target = new MemoryTarget {Layout = "${level:uppercase=true}|${message}|${exception}"};
            config.AddTarget("memory", _target);
            var loggingRule = new LoggingRule("*", NLog.LogLevel.Trace, _target);
            loggingRule.DisableLoggingForLevel(nlogLogLevel);
            config.LoggingRules.Add(loggingRule);
            LogManager.Configuration = config;
            _sut = new NLogLogProvider().GetLogger("Test");
        }
    }
}