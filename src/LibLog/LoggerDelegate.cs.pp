namespace $rootnamespace$.Logging
{
    using System;

#if LIBLOG_PROVIDERS_ONLY
    internal
#else
    public
#endif
        delegate bool Logger(LogLevel logLevel, Func<string> messageFunc, Exception exception = null, params object[] formatParameters);
}
