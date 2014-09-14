namespace LibLog.Logging
{
    using System;
    using FluentAssertions;
    using LibLog.Logging.LogProviders;
    using Xunit;

    public class LogProviderTests : IDisposable
    {
        [Fact]
        public void When_NLog_is_available_Then_should_get_NLogLogger()
        {
            LogProvider.SetCurrentLogProvider(null);
            NLogLogProvider.ProviderIsAvailableOverride = true;
            Log4NetLogProvider.ProviderIsAvailableOverride = true;
            ILog logger = LogProvider.GetCurrentClassLogger();

            ((LoggerExecutionWrapper) logger).WrappedLogger.Should().BeOfType<NLogLogProvider.NLogLogger>();
        }

        [Fact]
        public void When_Log4Net_is_available_Then_should_get_Log4NetLogger()
        {
            LogProvider.SetCurrentLogProvider(null);
            NLogLogProvider.ProviderIsAvailableOverride = false;
            Log4NetLogProvider.ProviderIsAvailableOverride = true;
            ILog logger = LogProvider.GetLogger(GetType());

            ((LoggerExecutionWrapper)logger).WrappedLogger.Should().BeOfType<Log4NetLogProvider.Log4NetLogger>();
        }

        [Fact]
        public void When_neither_NLog_nor_Log4Net_is_available_Then_should_get_EntLibLogger()
        {
            LogProvider.SetCurrentLogProvider(null);
            NLogLogProvider.ProviderIsAvailableOverride = false;
            Log4NetLogProvider.ProviderIsAvailableOverride = false;
            EntLibLogProvider.ProviderIsAvailableOverride = true;
            ILog logger = LogProvider.GetLogger(GetType());

            ((LoggerExecutionWrapper)logger).WrappedLogger.Should().BeOfType<EntLibLogProvider.EntLibLogger>();
        }

        [Fact]
        public void When_neither_NLog_nor_Log4Net_nor_EntLib_is_available_Then_should_get_SeriLogLogger()
        {
            LogProvider.SetCurrentLogProvider(null);
            NLogLogProvider.ProviderIsAvailableOverride = false;
            Log4NetLogProvider.ProviderIsAvailableOverride = false;
            EntLibLogProvider.ProviderIsAvailableOverride = false;
            SerilogLogProvider.ProviderIsAvailableOverride = true;
            ILog logger = LogProvider.GetLogger(GetType());

            ((LoggerExecutionWrapper)logger).WrappedLogger.Should().BeOfType<SerilogLogProvider.SerilogLogger>();
        }

        [Fact]
        public void When_neither_NLog_nor_Log4Net_or_EntLib_nor_Serilog_is_available_Then_should_get_NoOpLogger()
        {
            LogProvider.SetCurrentLogProvider(null);
            NLogLogProvider.ProviderIsAvailableOverride = false;
            Log4NetLogProvider.ProviderIsAvailableOverride = false;
            EntLibLogProvider.ProviderIsAvailableOverride = false;
            SerilogLogProvider.ProviderIsAvailableOverride = false;
            ILog logger = LogProvider.GetLogger(GetType());

            logger.Should().BeOfType<LogProvider.NoOpLogger>();
        }

        public void Dispose()
        {
            NLogLogProvider.ProviderIsAvailableOverride = true;
            Log4NetLogProvider.ProviderIsAvailableOverride = true;
            EntLibLogProvider.ProviderIsAvailableOverride = true;
            SerilogLogProvider.ProviderIsAvailableOverride = true;
        }
    }
}