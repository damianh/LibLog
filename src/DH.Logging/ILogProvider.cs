namespace DH.Logging
{
    public interface ILogProvider
    {
        ILog GetLogger(string name);
    }
}