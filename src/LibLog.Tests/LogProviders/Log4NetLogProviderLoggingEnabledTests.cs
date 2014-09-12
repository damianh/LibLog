using System;
using System.Linq;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using Xunit;

namespace LibLog.Logging.LogProviders
{
    public class Log4NetLogProviderLoggingEnabledTests : IDisposable
    {
        private readonly MemoryAppender _memoryAppender;
        private readonly ILog _sut;

        public Log4NetLogProviderLoggingEnabledTests()
        {
            _memoryAppender = new MemoryAppender();
            BasicConfigurator.Configure(_memoryAppender);
            _sut = new Log4NetLogProvider().GetLogger("Test");
        }

        public void Dispose()
        {
            LogManager.Shutdown();
        }

        [Fact]
        public void Should_be_able_to_log_debug_exception()
        {
            _sut.Log(LogLevel.Debug, () => "m", new Exception("e"));
            Assert.NotEmpty(_memoryAppender.GetEvents());
            Assert.Equal("DEBUG|m|e", GetSingleMessage());
        }

        [Fact]
        public void Should_be_able_to_log_debug_message()
        {
            _sut.Log(LogLevel.Debug, () => "m");
            Assert.NotEmpty(_memoryAppender.GetEvents());
            Assert.Equal("DEBUG|m|", GetSingleMessage());
        }

        [Fact]
        public void Should_be_able_to_log_error_exception()
        {
            _sut.Log(LogLevel.Error, () => "m", new Exception("e"));
            Assert.NotEmpty(_memoryAppender.GetEvents());
            Assert.Equal("ERROR|m|e", GetSingleMessage());
        }

        [Fact]
        public void Should_be_able_to_log_error_message()
        {
            _sut.Log(LogLevel.Error, () => "m");
            Assert.NotEmpty(_memoryAppender.GetEvents());
            Assert.Equal("ERROR|m|", GetSingleMessage());
        }

        [Fact]
        public void Should_be_able_to_log_fatal_exception()
        {
            _sut.Log(LogLevel.Fatal, () => "m", new Exception("e"));
            Assert.NotEmpty(_memoryAppender.GetEvents());
            Assert.Equal("FATAL|m|e", GetSingleMessage());
        }

        [Fact]
        public void Should_be_able_to_log_fatal_message()
        {
            _sut.Log(LogLevel.Fatal, () => "m");
            Assert.NotEmpty(_memoryAppender.GetEvents());
            Assert.Equal("FATAL|m|", GetSingleMessage());
        }

        [Fact]
        public void Should_be_able_to_log_info_exception()
        {
            _sut.Log(LogLevel.Info, () => "m", new Exception("e"));
            Assert.NotEmpty(_memoryAppender.GetEvents());
            Assert.Equal("INFO|m|e", GetSingleMessage());
        }

        [Fact]
        public void Should_be_able_to_log_info_message()
        {
            _sut.Log(LogLevel.Info, () => "m");
            Assert.NotEmpty(_memoryAppender.GetEvents());
            Assert.Equal("INFO|m|", GetSingleMessage());
        }

        [Fact]
        public void Should_be_able_to_log_trace_exception()
        {
            _sut.Log(LogLevel.Trace, () => "m", new Exception("e"));
            Assert.NotEmpty(_memoryAppender.GetEvents());
            Assert.Equal("DEBUG|m|e", GetSingleMessage()); //Trace messages in log4net are rendered as Debug
        }

        [Fact]
        public void Should_be_able_to_log_trace_message()
        {
            _sut.Log(LogLevel.Trace, () => "m");
            Assert.NotEmpty(_memoryAppender.GetEvents());
            Assert.Equal("DEBUG|m|", GetSingleMessage()); //Trace messages in log4net are rendered as Debug
        }

        [Fact]
        public void Should_be_able_to_log_warn_exception()
        {
            _sut.Log(LogLevel.Warn, () => "m", new Exception("e"));
            Assert.NotEmpty(_memoryAppender.GetEvents());
            Assert.Equal("WARN|m|e", GetSingleMessage());
        }

        [Fact]
        public void Should_be_able_to_log_warn_message()
        {
            _sut.Log(LogLevel.Warn, () => "m");
            Assert.NotEmpty(_memoryAppender.GetEvents());
            Assert.Equal("WARN|m|", GetSingleMessage());
        }

        private string GetSingleMessage()
        {
            LoggingEvent loggingEvent = _memoryAppender.GetEvents().Single();
            return string.Format(
                "{0}|{1}|{2}",
                loggingEvent.Level,
                loggingEvent.MessageObject,
                loggingEvent.ExceptionObject != null ? loggingEvent.ExceptionObject.Message : string.Empty);
        }
    }
}