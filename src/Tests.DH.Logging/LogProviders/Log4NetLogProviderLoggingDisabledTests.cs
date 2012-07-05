namespace Tests.DH.Logging.LogProviders
{
	using System;
	using Xunit;
	using global::DH.Logging.LogProviders;
	using log4net;
	using log4net.Appender;
	using log4net.Config;
	using log4net.Filter;
	using LogLevel = global::DH.Logging.LogLevel;

	public class Log4NetLogProviderLoggingDisabledTests : IDisposable
	{
		private readonly MemoryAppender _memoryAppender;
		private readonly global::DH.Logging.ILog _sut;

		public Log4NetLogProviderLoggingDisabledTests()
		{
			_memoryAppender = new MemoryAppender();
			_memoryAppender.AddFilter(new DenyAllFilter());
			BasicConfigurator.Configure(_memoryAppender);
			_sut = new Log4NetLogProvider().GetLogger("Test");
		}

		public void Dispose()
		{
			LogManager.Shutdown();
		}

		[Fact]
		public void For_Debug_Then_should_not_log()
		{
			AssertShouldNotLog(LogLevel.Debug);
		}

		[Fact]
		public void For_Error_Then_should_not_log()
		{
			AssertShouldNotLog(LogLevel.Error);
		}

		[Fact]
		public void For_Fatal_Then_should_not_log()
		{
			AssertShouldNotLog(LogLevel.Fatal);
		}

		[Fact]
		public void For_Info_Then_should_not_log()
		{
			AssertShouldNotLog(LogLevel.Info);
		}

		[Fact]
		public void For_Trace_Then_should_not_log()
		{
			AssertShouldNotLog(LogLevel.Trace);
		}

		[Fact]
		public void For_Warn_Then_should_not_log()
		{
			AssertShouldNotLog(LogLevel.Warn);
		}

		private void AssertShouldNotLog(LogLevel logLevel)
		{
			_sut.Log(logLevel, () => "m");
			_sut.Log(logLevel, () => "m", new Exception("e"));
			Assert.Empty(_memoryAppender.GetEvents());
		}
	}
}