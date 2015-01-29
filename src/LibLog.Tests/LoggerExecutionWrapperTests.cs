﻿namespace LibLog.Logging
{
    using System;
    using Xunit;

    public class LoggerExecutionWrapperTests
    {
        private readonly LoggerExecutionWrapper _sut;
        private readonly FakeLogger _fakeLogger;

        public LoggerExecutionWrapperTests()
        {
            _fakeLogger = new FakeLogger();
            _sut = new LoggerExecutionWrapper(_fakeLogger);
        }

        [Fact]
        public void When_logging_and_message_factory_throws_Then_should_log_exception()
        {
            var loggingException = new Exception("Message");
            _sut.Log(LogLevel.Info, () => { throw loggingException; });
            Assert.Same(loggingException, _fakeLogger.Exception);
            Assert.Equal(LoggerExecutionWrapper.FailedToGenerateLogMessage, _fakeLogger.Message);
        }

        [Fact]
        public void When_logging_with_exception_and_message_factory_throws_Then_should_log_exception()
        {
            var appException = new Exception("Message");
            var loggingException = new Exception("Message");
            _sut.Log(LogLevel.Info, () => { throw loggingException; }, appException);
            Assert.Same(loggingException, _fakeLogger.Exception);
            Assert.Equal(LoggerExecutionWrapper.FailedToGenerateLogMessage, _fakeLogger.Message);
        }

        [Fact]
        public void When_Asking_If_LogLevel_Is_Enabled_The_LoggerExecutionWrapper_Should_Not_Wrap_Message()
        {
            _sut.IsDebugEnabled();
            Assert.NotEqual(LoggerExecutionWrapper.FailedToGenerateLogMessage, _fakeLogger.Message);
        }

        public class FakeLogger : ILog
        {
            private LogLevel _logLevel;

            public LogLevel LogLevel
            {
                get { return _logLevel; }
            }

            public string Message
            {
                get { return _message; }
            }

            public Exception Exception
            {
                get { return _exception; }
            }

            private string _message;
            private Exception _exception;

            public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception, params object[] formatParameters)
            {
                if (messageFunc != null)
                {
                    string message = messageFunc();
                    if (message != null)
                    {
                        _logLevel = logLevel;
                        _message = messageFunc() ?? _message;
                        _exception = exception;
                    }
                }
                return true;
            }
        }
    }
}