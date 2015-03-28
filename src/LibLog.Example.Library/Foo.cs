namespace LibLog.Example.Library
{
    using LibLog.Example.Library.Logging;

    public static class Foo
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        public static void Bar()
        {
            Logger.Info("Baz");
        }
    }
}