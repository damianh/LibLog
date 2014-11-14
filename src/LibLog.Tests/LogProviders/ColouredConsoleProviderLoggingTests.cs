namespace LibLog.Logging.LogProviders
{
    using System;
    using System.IO;

    using FluentAssertions;

    using Xunit;
    using Xunit.Extensions;

    public class ColouredConsoleProviderLoggingTests
    {
        private readonly ILog _sut;

        private readonly TextWriter _originalConsoleOut;

        private StringWriter _target;

        public ColouredConsoleProviderLoggingTests()
        {
            _originalConsoleOut = Console.Out;
            _target = new StringWriter();
            Console.SetOut(_target);

            ColouredConsoleLogProvider.MessageFormatter =
                (name, level, message, exception) => string.Format("{0}|{1}|{2}", level, message, exception != null ? exception.Message : null);
            
            _sut = new ColouredConsoleLogProvider().GetLogger("Test");
        }

        public void Dispose()
        {
            Console.SetOut(_originalConsoleOut);
        }

        [Theory]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Fatal)]
        [InlineData(LogLevel.Info)]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Warn)]
        public void Should_be_able_to_log_message(LogLevel logLevel)
        {
            _sut.Log(logLevel, () => "m");
            _target.ToString().Trim().Should().Be(logLevel.ToString() + "|m|");
        }

        [Theory]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Fatal)]
        [InlineData(LogLevel.Info)]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Warn)]
        public void Should_be_able_to_log_message_and_exception(LogLevel logLevel)
        {
            _sut.Log(logLevel, () => "m", new Exception("e"));

            _target.ToString().Trim().Should().Be(logLevel.ToString() + "|m|e");
        }

        [Fact]
        public void Can_check_is_log_level_enabled()
        {
            _sut.AssertCanCheckLogLevelsEnabled();
        }
    }
}