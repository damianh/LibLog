namespace LibLog.Example.ColoredConsoleLogProvider
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using LibLog.Example.Library.Logging;
    using LibLog.Example.Library.Logging.LogProviders;

    public class ColoredConsoleLogProvider : LogProviderBase
    {
        private static readonly Dictionary<LogLevel, ConsoleColor> Colors = new Dictionary<LogLevel, ConsoleColor>
            {
                {LogLevel.Fatal, ConsoleColor.Red},
                {LogLevel.Error, ConsoleColor.Yellow},
                {LogLevel.Warn, ConsoleColor.Magenta},
                {LogLevel.Info, ConsoleColor.White},
                {LogLevel.Debug, ConsoleColor.Gray},
                {LogLevel.Trace, ConsoleColor.DarkGray},
            };

        public override Logger GetLogger(string name)
        {
            return (logLevel, messageFunc, exception, formatParameters) =>
            {
                if (messageFunc == null)
                {   
                    return true; // All log levels are enabled
                }

                if (Colors.TryGetValue(logLevel, out ConsoleColor consoleColor))
                {
                    var originalForeground = Console.ForegroundColor;
                    try
                    {
                        Console.ForegroundColor = consoleColor;
                        WriteMessage(logLevel, name, messageFunc, formatParameters, exception);
                    }
                    finally
                    {
                        Console.ForegroundColor = originalForeground;
                    }
                }
                else
                {
                    WriteMessage(logLevel, name, messageFunc, formatParameters, exception);
                }

                return true;
            };
        }

        private static void WriteMessage(
            LogLevel logLevel,
            string name,
            Func<string> messageFunc,
            object[] formatParameters,
            Exception exception)
        {
            var message = string.Format(CultureInfo.InvariantCulture, messageFunc(), formatParameters);
            if (exception != null)
            {
                message = message + "|" + exception;
            }
            Console.WriteLine("{0} | {1} | {2} | {3}", DateTime.UtcNow, logLevel, name, message);
        }
    }
}
