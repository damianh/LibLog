namespace Raven.Tests.Abstractions.Logging.LogProviders
{
	using System;
	using DH.Logging;
	using DH.Logging.LogProviders;
	using NLog;
	using NLog.Config;
	using NLog.Targets;
	using Xunit;
	using LogLevel = NLog.LogLevel;

	public class NLogLogProviderLoggingDisabedTests : IDisposable
	{
		private ILog sut;
		private MemoryTarget target;

		private void ConfigureLogger(NLog.LogLevel nlogLogLevel)
		{
			var config = new LoggingConfiguration();
			target = new MemoryTarget();
			target.Layout = "${level:uppercase=true}|${message}|${exception}";
			config.AddTarget("memory", target);
			var loggingRule = new LoggingRule("*", LogLevel.Trace, target);
			loggingRule.DisableLoggingForLevel(nlogLogLevel);
			config.LoggingRules.Add(loggingRule);
			LogManager.Configuration = config;
			sut = new NLogLogProvider().GetLogger("Test");
		}

		[Fact]
		public void For_Trace_Then_should_not_log()
		{
			ConfigureLogger(LogLevel.Trace);
			AssertShouldNotLog(DH.Logging.LogLevel.Trace);
		}

		[Fact]
		public void For_Debug_Then_should_not_log()
		{
			ConfigureLogger(LogLevel.Debug);
			AssertShouldNotLog(DH.Logging.LogLevel.Debug);
		}

		[Fact]
		public void For_Info_Then_should_not_log()
		{
			ConfigureLogger(LogLevel.Info);
			AssertShouldNotLog(DH.Logging.LogLevel.Info);
		}

		[Fact]
		public void For_Warn_Then_should_not_log()
		{
			ConfigureLogger(LogLevel.Warn);
			AssertShouldNotLog(DH.Logging.LogLevel.Warn);
		}

		[Fact]
		public void For_Error_Then_should_not_log()
		{
			ConfigureLogger(LogLevel.Error);
			AssertShouldNotLog(DH.Logging.LogLevel.Error);
		}

		[Fact]
		public void For_Fatal_Then_should_not_log()
		{
			ConfigureLogger(LogLevel.Fatal);
			AssertShouldNotLog(DH.Logging.LogLevel.Fatal);
		}

		private void AssertShouldNotLog(DH.Logging.LogLevel logLevel)
		{
			sut.Log(logLevel, () => "m");
			sut.Log(logLevel, () => "m", new Exception("e"));
			Assert.Empty(target.Logs);
		}

		public void Dispose()
		{
			LogManager.Configuration = null;
		}
	}
}