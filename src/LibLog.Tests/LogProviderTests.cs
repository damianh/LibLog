namespace LibLog.Logging
{
    using FluentAssertions;
    using LibLog.Logging.LogProviders;
    using System;
    using Xunit;

    public class LogProviderTests : IDisposable
    {
        [Fact]
        public void When_NLog_is_available_Then_should_get_NLogLogger()
        {
            LogProvider.SetCurrentLogProvider(null);
            NLogLogProvider.ProviderIsAvailableOverride = true;
            Log4NetLogProvider.ProviderIsAvailableOverride = false;
            EntLibLogProvider.ProviderIsAvailableOverride = false;
            SerilogLogProvider.ProviderIsAvailableOverride = false;
            LoupeLogProvider.ProviderIsAvailableOverride = false;

            var logProvider = LogProvider.ResolveLogProvider();
            logProvider.Should().BeOfType<NLogLogProvider>();
        }

        [Fact]
        public void When_Log4Net_is_available_Then_should_get_Log4NetLogger()
        {
            LogProvider.SetCurrentLogProvider(null);
            NLogLogProvider.ProviderIsAvailableOverride = false;
            Log4NetLogProvider.ProviderIsAvailableOverride = true;
            EntLibLogProvider.ProviderIsAvailableOverride = false;
            SerilogLogProvider.ProviderIsAvailableOverride = false;
            LoupeLogProvider.ProviderIsAvailableOverride = false;

            var logProvider = LogProvider.ResolveLogProvider();
            logProvider.Should().BeOfType<Log4NetLogProvider>();
        }

        [Fact]
        public void When_EntLibLog_is_available_Then_should_get_EntLibLogger()
        {
            LogProvider.SetCurrentLogProvider(null);
            SerilogLogProvider.ProviderIsAvailableOverride = false;
            NLogLogProvider.ProviderIsAvailableOverride = false;
            Log4NetLogProvider.ProviderIsAvailableOverride = false;
            EntLibLogProvider.ProviderIsAvailableOverride = true;
            LoupeLogProvider.ProviderIsAvailableOverride = false;

            var logProvider = LogProvider.ResolveLogProvider();
            logProvider.Should().BeOfType<EntLibLogProvider>();
        }

        [Fact]
        public void When_Serilog_is_available_Then_should_get_SeriLogLogger()
        {
            LogProvider.SetCurrentLogProvider(null);
            NLogLogProvider.ProviderIsAvailableOverride = false;
            Log4NetLogProvider.ProviderIsAvailableOverride = false;
            EntLibLogProvider.ProviderIsAvailableOverride = false;
            SerilogLogProvider.ProviderIsAvailableOverride = true;
            LoupeLogProvider.ProviderIsAvailableOverride = false;

            var logProvider = LogProvider.ResolveLogProvider();
            logProvider.Should().BeOfType<SerilogLogProvider>();
        }

        [Fact]
        public void When_LoupLog_is_available_Then_should_get_LoupeLogger()
        {
            LogProvider.SetCurrentLogProvider(null);
            NLogLogProvider.ProviderIsAvailableOverride = false;
            Log4NetLogProvider.ProviderIsAvailableOverride = false;
            EntLibLogProvider.ProviderIsAvailableOverride = false;
            SerilogLogProvider.ProviderIsAvailableOverride = false;
            LoupeLogProvider.ProviderIsAvailableOverride = true;

            var logProvider = LogProvider.ResolveLogProvider();
            logProvider.Should().BeOfType<LoupeLogProvider>();
        }

        [Fact]
        public void When_ColourConsoleLoger_is_available_Then_should_get_ColourConsoleLoger()
        {
            LogProvider.SetCurrentLogProvider(null);
            NLogLogProvider.ProviderIsAvailableOverride = false;
            Log4NetLogProvider.ProviderIsAvailableOverride = false;
            EntLibLogProvider.ProviderIsAvailableOverride = false;
            SerilogLogProvider.ProviderIsAvailableOverride = false;
            LoupeLogProvider.ProviderIsAvailableOverride = false;
            ColouredConsoleLogProvider.ProviderIsAvailableOverride = true;

            var logProvider = LogProvider.ResolveLogProvider();
            logProvider.Should().BeOfType<ColouredConsoleLogProvider>();
        }

        [Fact]
        public void When_no_logger_is_available_Then_should_get_NoOpLogger()
        {
            LogProvider.SetCurrentLogProvider(null);
            NLogLogProvider.ProviderIsAvailableOverride = false;
            Log4NetLogProvider.ProviderIsAvailableOverride = false;
            EntLibLogProvider.ProviderIsAvailableOverride = false;
            SerilogLogProvider.ProviderIsAvailableOverride = false;
            LoupeLogProvider.ProviderIsAvailableOverride = false;
            ColouredConsoleLogProvider.ProviderIsAvailableOverride = false;

            ILog logger = LogProvider.For<LogProviderTests>();

            logger.Should().BeOfType<LogProvider.NoOpLogger>();
        }

        public void Dispose()
        {
            NLogLogProvider.ProviderIsAvailableOverride = true;
            Log4NetLogProvider.ProviderIsAvailableOverride = true;
            EntLibLogProvider.ProviderIsAvailableOverride = true;
            SerilogLogProvider.ProviderIsAvailableOverride = true;
            LoupeLogProvider.ProviderIsAvailableOverride = true;
            ColouredConsoleLogProvider.ProviderIsAvailableOverride = true;
        }
    }
}
