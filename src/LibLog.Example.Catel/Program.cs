namespace LibLog.Example.Catel
{
    using System;
    using global::Catel;
    using global::Catel.Logging;
    using LibLog.Example.Library;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var consoleLogListener = new ConsoleLogListener();

            LogManager.AddListener(consoleLogListener);

            Foo.Bar();

            Console.ReadLine();
        }
    }
}