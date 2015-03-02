namespace LibLog.Logging.LogProviders
{
    using System;
    using System.IO;
    using FluentAssertions;
    using Xunit;
    using Xunit.Extensions;

    public class ColouredConsoleProviderLoggingTests : IDisposable
    {
        private readonly ILog _sut;
        private readonly TextWriter _originalConsoleOut;
        private readonly StringWriter _target;

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
            _target.ToString().Trim().Should().Be(logLevel + "|m|");
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

            _target.ToString().Trim().Should().Be(logLevel + "|m|e");
        }
        
        [Theory]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Fatal)]
        [InlineData(LogLevel.Info)]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Warn)]
        public void Should_be_able_to_log_message_with_formatParams(LogLevel logLevel)
        {
            _sut.Log(logLevel, () => "m {0}", null, "replaced");
            _target.ToString().Trim().Should().Be(logLevel.ToString() + "|m replaced|");
        }

        [Theory]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Fatal)]
        [InlineData(LogLevel.Info)]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Warn)]
        public void Should_be_able_to_log_message_and_exception_with_formatParams(LogLevel logLevel)
        {
            _sut.Log(logLevel, () => "m {abc}", new Exception("e"), "replaced");

            _target.ToString().Trim().Should().Be(logLevel.ToString() + "|m replaced|e");
        }

        [Theory]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Fatal)]
        [InlineData(LogLevel.Info)]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Warn)]
        public void Should_be_able_to_log_message_if_contains_brackets(LogLevel logLevel)
        {
            _sut.Log(logLevel, () => "m {ab c}", new Exception("e"));

            _target.ToString().Trim().Should().Be(logLevel.ToString() + "|m {ab c}|e");
        }

        [Fact]
        public void Should_log_message_with_curly_brackets()
        {
            _sut.Log(LogLevel.Debug, () => "Query language substitutions: {'true'='1', 'false'='0', 'yes'=''Y'', 'no'=''N''}");

            _target.ToString().Should().Be("Debug|Query language substitutions: {'true'='1', 'false'='0', 'yes'=''Y'', 'no'=''N''}|\r\n");
        }

        [Fact]
        public void Can_check_is_log_level_enabled()
        {
            _sut.AssertCanCheckLogLevelsEnabled();
        }
    }
}