namespace LibLog.Logging
{
    using System;
    using Shouldly;
    using Xunit;
    using YourRootNamespace.Logging;
    using YourRootNamespace.Logging.LogProviders;

    public class LogMessageFormatterTests
    {
        [Fact]
        public void When_arguments_are_unique_and_not_escaped_Then_should_replace_them()
        {
            Func<string> messageBuilder = () => "This is an {argument1} and this an {argument2}.";

            var formattedMessage = LogMessageFormatter.SimulateStructuredLogging(messageBuilder, new object[] { "arg0", "arg1" })();

            formattedMessage.ShouldBe("This is an arg0 and this an arg1.");
        }

        [Fact]
        public void When_arguments_are_escaped_Then_should_not_replace_them()
        {
            Func<string> messageBuilder = () => "This is an {argument} and this an {{escaped_argument}}.";

            var formattedMessage = LogMessageFormatter.SimulateStructuredLogging(messageBuilder, new object[] { "arg0", "arg1" })();

            formattedMessage.ShouldBe("This is an arg0 and this an {escaped_argument}.");
        }
    }
}
