namespace LibLog.Logging
{
    using System;
    using Shouldly;
    using Xunit;
    using YourRootNamespace.Logging;

    public class LoggerExecutionWrapperTests
    {
        private readonly LoggerExecutionWrapper _sut;
        private readonly FakeLogger _fakeLogger;

        public LoggerExecutionWrapperTests()
        {
            _fakeLogger = new FakeLogger();
            _sut = new LoggerExecutionWrapper(_fakeLogger.Log);
        }

        [Fact]
        public void When_logging_and_message_factory_throws_Then_should_log_exception()
        {
            var loggingException = new Exception("Message");
            _sut.Log(LogLevel.Info, () => { throw loggingException; });

            _fakeLogger.Exception.ShouldBe(loggingException);
            _fakeLogger.Message.ShouldBe(LoggerExecutionWrapper.FailedToGenerateLogMessage);
        }

        [Fact]
        public void When_logging_with_exception_and_message_factory_throws_Then_should_log_exception()
        {
            var appException = new Exception("Message");
            var loggingException = new Exception("Message");
            _sut.Log(LogLevel.Info, () => { throw loggingException; }, appException);

            _fakeLogger.Exception.ShouldBe(loggingException);
            _fakeLogger.Message.ShouldBe(LoggerExecutionWrapper.FailedToGenerateLogMessage);
        }

        [Fact]
        public void When_Asking_If_LogLevel_Is_Enabled_The_LoggerExecutionWrapper_Should_Not_Wrap_Message()
        {
            _sut.IsDebugEnabled();

            _fakeLogger.Message.ShouldNotBe(LoggerExecutionWrapper.FailedToGenerateLogMessage);
        }

        public class FakeLogger : ILog
        {
            private LogLevel _logLevel;

            public LogLevel LogLevel => _logLevel;

            public string Message => _message;

            public Exception Exception => _exception;

            private string _message;
            private Exception _exception;

            public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception, params object[] formatParameters)
            {
                string message = messageFunc?.Invoke();
                if (message != null)
                {
                    _logLevel = logLevel;
                    _message = messageFunc() ?? _message;
                    _exception = exception;
                }
                return true;
            }
        }
    }
}