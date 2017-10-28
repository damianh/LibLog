namespace LibLog.Logging.LogProviders
{
    using LogLevel = YourRootNamespace.Logging.LogLevel;

    internal class MyMappedContext
    {
        public int ThirtySeven
        {
            get
            {
                return 37;
            }
        }

        public string Name { get => name; set => name = value; }
        public LogLevel Level { get => level; set => level = value; }

        string name = "World";
        LogLevel level = LogLevel.Trace;

        public override string ToString()
        {
            return name;
        }
    }
}