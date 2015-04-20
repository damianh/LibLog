﻿namespace LibLog.Logging
{
    using FluentAssertions;
    using System;
    using NLog;
    using NLog.Config;
    using NLog.Targets;
    using Xunit;
    using YourRootNamespace.Logging;
    using YourRootNamespace.Logging.LogProviders;

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
        public void When_no_logger_is_available_Then_should_get_NoOpLogger()
        {
            LogProvider.SetCurrentLogProvider(null);
            NLogLogProvider.ProviderIsAvailableOverride = false;
            Log4NetLogProvider.ProviderIsAvailableOverride = false;
            EntLibLogProvider.ProviderIsAvailableOverride = false;
            SerilogLogProvider.ProviderIsAvailableOverride = false;
            LoupeLogProvider.ProviderIsAvailableOverride = false;
            CatelLogProvider.ProviderIsAvailableOverride = false;

            ILog logger = LogProvider.For<LogProviderTests>();

            logger.Should().BeOfType<LogProvider.NoOpLogger>();
        }

        [Fact]
        public void When_set_current_log_provider_then_should_raise_OnCurrentLogProviderSet()
        {
            ILogProvider provider = null;
            LogProvider.OnCurrentLogProviderSet = p => provider = p;

            LogProvider.SetCurrentLogProvider(new NLogLogProvider());

            provider.Should().NotBeNull();
        }

        [Fact]
        public void When_disable_logging_via_property_then_should_not_log()
        {
            var config = new LoggingConfiguration();
            var target = new MemoryTarget
            {
                Layout = "${level:uppercase=true}|${ndc}|${mdc:item=key}|${message}|${exception}"
            };
            config.AddTarget("memory", target);
            config.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Trace, target));
            LogManager.Configuration = config;
            LogProvider.SetCurrentLogProvider(new NLogLogProvider());

            LogProvider.IsDisabled = true;
            var logger = LogProvider.GetLogger("DisableLogging");
            logger.Info("test");

            target.Logs.Should().BeEmpty();
        }

        [Fact]
        public void When_enable_logging_via_property_then_should_log()
        {
            LogProvider.IsDisabled = true;
            var config = new LoggingConfiguration();
            var target = new MemoryTarget
            {
                Layout = "${level:uppercase=true}|${ndc}|${mdc:item=key}|${message}|${exception}"
            };
            config.AddTarget("memory", target);
            config.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Trace, target));
            LogManager.Configuration = config;
            LogProvider.SetCurrentLogProvider(new NLogLogProvider());

            LogProvider.IsDisabled = false;
            var logger = LogProvider.GetLogger("DisableLogging");
            logger.Info("test");

            target.Logs.Should().NotBeEmpty();
        }

#if  !LIBLOG_PORTABLE
        [Fact]
        public void When_disable_logging_via_env_var_then_should_not_log()
        {
            var config = new LoggingConfiguration();
            var target = new MemoryTarget
            {
                Layout = "${level:uppercase=true}|${ndc}|${mdc:item=key}|${message}|${exception}"
            };
            config.AddTarget("memory", target);
            config.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Trace, target));
            LogManager.Configuration = config;
            LogProvider.SetCurrentLogProvider(new NLogLogProvider());

            Environment.SetEnvironmentVariable(LogProvider.DisableLoggingEnvironmentVariable, "true");
            var logger = LogProvider.GetLogger("DisableLogging");
            logger.Info("test");

            target.Logs.Should().BeEmpty();
        }

        [Fact]
        public void When_enable_logging_via_env_var_then_should_log()
        {
            Environment.SetEnvironmentVariable(LogProvider.DisableLoggingEnvironmentVariable, "true");
            var config = new LoggingConfiguration();
            var target = new MemoryTarget
            {
                Layout = "${level:uppercase=true}|${ndc}|${mdc:item=key}|${message}|${exception}"
            };
            config.AddTarget("memory", target);
            config.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Trace, target));
            LogManager.Configuration = config;
            LogProvider.SetCurrentLogProvider(new NLogLogProvider());

            Environment.SetEnvironmentVariable(LogProvider.DisableLoggingEnvironmentVariable, "false");
            var logger = LogProvider.GetLogger("DisableLogging");
            logger.Info("test");

            target.Logs.Should().NotBeEmpty();
        }
#endif

        public void Dispose()
        {
            NLogLogProvider.ProviderIsAvailableOverride = true;
            Log4NetLogProvider.ProviderIsAvailableOverride = true;
            EntLibLogProvider.ProviderIsAvailableOverride = true;
            SerilogLogProvider.ProviderIsAvailableOverride = true;
            LoupeLogProvider.ProviderIsAvailableOverride = true;
#if !LIBLOG_PORTABLE
            Environment.SetEnvironmentVariable(LogProvider.DisableLoggingEnvironmentVariable, "false");
#endif
        }
    }
}
