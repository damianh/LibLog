namespace Tests.DH.Logging
{
	using Xunit;
	using global::DH.Logging;
	using global::DH.Logging.LogProviders;

	public class LogProviderTests
	{
		[Fact]
		public void When_NLog_is_available_Then_should_get_NLogLogger()
		{
			LogProvider.SetCurrentLogProvider(null);
			NLogLogProvider.ProviderAvailabilityOverride = true;
			Log4NetLogProvider.ProviderAvailabilityOverride = true;
			ILog logger = LogProvider.GetCurrentClassLogger();
			Assert.IsType<NLogLogProvider.NLogLogger>(((LoggerExecutionWrapper)logger).WrappedLogger);
		}

		[Fact]
		public void When_Log4Net_is_available_Then_should_get_Log4NetLogger()
		{
			LogProvider.SetCurrentLogProvider(null);
			NLogLogProvider.ProviderAvailabilityOverride = false;
			Log4NetLogProvider.ProviderAvailabilityOverride = true;
			ILog logger = LogProvider.GetLogger(GetType());
			Assert.IsType<Log4NetLogProvider.Log4NetLogger>(((LoggerExecutionWrapper)logger).WrappedLogger);
		}

		[Fact]
		public void When_neither_NLog_or_Log4Net_is_available_Then_should_get_NoOpLogger()
		{
			LogProvider.SetCurrentLogProvider(null);
			NLogLogProvider.ProviderAvailabilityOverride = false;
			Log4NetLogProvider.ProviderAvailabilityOverride = false;
			ILog logger = LogProvider.GetLogger(GetType());
			Assert.IsType<LogProvider.NoOpLogger>(logger);
		}
	}
}