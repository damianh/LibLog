namespace LibLog.Example.NLog
{
    using System;
    using global::NLog;
    using global::NLog.Config;
    using global::NLog.Targets;
    using LibLog.Example.Library;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = new LoggingConfiguration();
            var consoleTarget = new ColoredConsoleTarget();
            config.AddTarget("console", consoleTarget);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, consoleTarget));
            LogManager.Configuration = config;

            Foo.Bar();

            Console.ReadLine();
        }
    }
}