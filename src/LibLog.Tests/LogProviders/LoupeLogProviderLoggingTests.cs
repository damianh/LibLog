#if !NET45

namespace LibLog.Logging.LogProviders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Gibraltar.Agent;
    using Gibraltar.Agent.Data;
    using Shouldly;
    using Xunit;
    using YourRootNamespace.Logging;
    using YourRootNamespace.Logging.LogProviders;
    using ILog = YourRootNamespace.Logging.ILog;

    public class LoupeProviderLoggingTests : IDisposable
    {
        private readonly ILog _sut;
        private readonly MessageTester _messageTester;

        public LoupeProviderLoggingTests()
        {
            _sut = new LoggerExecutionWrapper(new LoupeLogProvider().GetLogger("Test"));
            _messageTester = new MessageTester();
        }

        public void Dispose()
        {
            //this is debatably sketchy since we're not likely to be really shutting down, but otherwise we can hang the unit tests.
            Log.EndSession();

            _messageTester.Dispose();
        }

        [Theory]
        [InlineData(LogLevel.Debug, "DEBUG")]
        [InlineData(LogLevel.Error, "ERROR")]
        [InlineData(LogLevel.Fatal, "CRITICAL")] //Fatal messages in Loupe are rendered as Critical
        [InlineData(LogLevel.Info, "INFO")]
        [InlineData(LogLevel.Trace, "DEBUG")] //Trace messages in Loupe are rendered as Debug
        [InlineData(LogLevel.Warn, "WARN")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Should_be_able_to_log_message(LogLevel logLevel, string messagePrefix)
        {
            _messageTester.Reset();

            _sut.Log(logLevel, () => messagePrefix + " log message");

            _messageTester.WaitForMessages();
            _messageTester.Message.Count.ShouldBe(1);
            var firstMessage = _messageTester.Message.First();
            firstMessage.Caption.ShouldBe(messagePrefix + " log message");
        }

        [Theory]
        [InlineData(LogLevel.Debug, "DEBUG")]
        [InlineData(LogLevel.Error, "ERROR")]
        [InlineData(LogLevel.Fatal, "CRITICAL")] //Fatal messages in Loupe are rendered as Critical
        [InlineData(LogLevel.Info, "INFO")]
        [InlineData(LogLevel.Trace, "DEBUG")] //Trace messages in Loupe are rendered as Debug
        [InlineData(LogLevel.Warn, "WARN")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Should_be_able_to_log_message_and_exception(LogLevel logLevel, string messagePrefix)
        {
            _messageTester.Reset();

            _sut.Log(logLevel, () => messagePrefix + " log message with exception", new Exception("e"));

            _messageTester.WaitForMessages();
            _messageTester.Message.Count.ShouldBe(1);
            var firstMessage = _messageTester.Message.First();
            firstMessage.Caption.ShouldBe(messagePrefix + " log message with exception");
            firstMessage.HasException.ShouldBeTrue();
            firstMessage.Exception.ShouldNotBeNull();
            firstMessage.Exception.Message.ShouldBe("e");
        }

        [Theory]
        [InlineData(LogLevel.Debug, "DEBUG")]
        [InlineData(LogLevel.Error, "ERROR")]
        [InlineData(LogLevel.Fatal, "CRITICAL")] //Fatal messages in Loupe are rendered as Critical
        [InlineData(LogLevel.Info, "INFO")]
        [InlineData(LogLevel.Trace, "DEBUG")] //Trace messages in Loupe are rendered as Debug
        [InlineData(LogLevel.Warn, "WARN")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Should_be_able_to_log_message_with_params(LogLevel logLevel, string messagePrefix)
        {
            _messageTester.Reset();

            _sut.Log(logLevel, () => messagePrefix + " log message {0}", null, "replaced");

            _messageTester.WaitForMessages();
            _messageTester.Message.Count.ShouldBe(1);
            var firstMessage = _messageTester.Message.First();
            firstMessage.Caption.ShouldBe(messagePrefix + " log message replaced");
        }

        [Theory]
        [InlineData(LogLevel.Debug, "DEBUG")]
        [InlineData(LogLevel.Error, "ERROR")]
        [InlineData(LogLevel.Fatal, "CRITICAL")] //Fatal messages in Loupe are rendered as Critical
        [InlineData(LogLevel.Info, "INFO")]
        [InlineData(LogLevel.Trace, "DEBUG")] //Trace messages in Loupe are rendered as Debug
        [InlineData(LogLevel.Warn, "WARN")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Should_be_able_to_log_message_and_exception_with_formatparams(LogLevel logLevel, string messagePrefix)
        {
            _messageTester.Reset();

            _sut.Log(logLevel, () => messagePrefix + " log message {abc} with exception", new Exception("e"), "replaced");

            _messageTester.WaitForMessages();
            _messageTester.Message.Count.ShouldBe(1);
            var firstMessage = _messageTester.Message.First();
            firstMessage.Caption.ShouldBe(messagePrefix + " log message replaced with exception");
            firstMessage.HasException.ShouldBeTrue();
            firstMessage.Exception.ShouldNotBeNull();
            firstMessage.Exception.Message.ShouldBe("e");
        }

        [Theory]
        [InlineData(LogLevel.Debug, "DEBUG")]
        [InlineData(LogLevel.Error, "ERROR")]
        [InlineData(LogLevel.Fatal, "CRITICAL")] //Fatal messages in Loupe are rendered as Critical
        [InlineData(LogLevel.Info, "INFO")]
        [InlineData(LogLevel.Trace, "DEBUG")] //Trace messages in Loupe are rendered as Debug
        [InlineData(LogLevel.Warn, "WARN")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Should_be_able_to_log_message_and_exception_with_formatparams_modifiers(LogLevel logLevel, string messagePrefix)
        {
            _messageTester.Reset();

            _sut.Log(logLevel, () => messagePrefix + " log message {@abc} with exception", new Exception("e"), "replaced");

            _messageTester.WaitForMessages();
            _messageTester.Message.Count.ShouldBe(1);
            var firstMessage = _messageTester.Message.First();
            firstMessage.Caption.ShouldBe(messagePrefix + " log message replaced with exception");
            firstMessage.HasException.ShouldBeTrue();
            firstMessage.Exception.ShouldNotBeNull();
            firstMessage.Exception.Message.ShouldBe("e");
        }

        [Fact]
        public void Can_check_is_log_level_enabled()
        {
            _sut.AssertCanCheckLogLevelsEnabled();
        }


        /// <summary>
        /// Aggregates messages as they are generated so we can verify the logging results
        /// </summary>
        private class MessageTester : IDisposable
        {
            private List<ILogMessage> _message;

            public MessageTester()
            {
                Log.MessagePublished += LogOnMessage;
            }


            public List<ILogMessage> Message
            {
                get
                {
                    lock (this)
                    {
                        return _message;
                    }
                }
            }

            /// <summary>
            /// Wait for any messages in the queue to commit.
            /// </summary>
            public void WaitForMessages()
            {
                Gibraltar.Monitor.Log.Flush();
                Task.Delay(250).GetAwaiter().GetResult(); // TODO: do better signalling with a task.
            }

            /// <summary>
            /// Reset the buffer and counts
            /// </summary>
            public void Reset()
            {
                WaitForMessages();

                lock (this)
                {
                    _message = new List<ILogMessage>();
                }
            }

            public void Dispose()
            {
                Log.MessagePublished -= LogOnMessage;
            }

            private void LogOnMessage(object sender, LogMessageEventArgs e)
            {
                lock (this)
                {
                    _message.AddRange(e.Messages);
                }
            }
        }
    }
}

#endif