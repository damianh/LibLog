namespace LibLog.Example.Serilog
{
    using System;
    using global::Serilog;
    using LibLog.Example.Library;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var log = new LoggerConfiguration()
                .WriteTo.ColoredConsole(outputTemplate: "{Timestamp:HH:mm} [{Level}] ({Name:l}) {Message}{NewLine}{Exception}")
                .CreateLogger();
            Log.Logger = log;

            Foo.Bar();

            Console.ReadLine();
        }
    }
}