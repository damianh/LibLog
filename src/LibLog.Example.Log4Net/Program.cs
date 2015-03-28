namespace LibLog.Example.Log4Net
{
    using System;
    using System.IO;
    using log4net.Config;
    using LibLog.Example.Library;

    internal class Program
    {
        private static void Main(string[] args)
        {
            XmlConfigurator.Configure(new FileInfo("log4net.config"));

            Foo.Bar();

            Console.ReadLine();
        }
    }
}