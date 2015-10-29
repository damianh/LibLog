namespace LibLog
{
    using System;
    using Xunit;
    using YourRootNamespace.Logging;

    public class LibLogTests
    {
        [Fact]
        public void Can_open_nested_diagnostic_context()
        {
            using (LogProvider.OpenNestedContext("a"))
            { }
        }

        [Fact]
        public void Can_open_mapped_diagnostic_context()
        {
            using (LogProvider.OpenMappedContext("a", "b"))
            { }
        }
    }
}
