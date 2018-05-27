namespace $rootnamespace$.Logging
{
    using System;

#if !LIBLOG_PROVIDERS_ONLY || LIBLOG_PUBLIC
    public
#else
    internal
#endif
        delegate bool Logger(LogLevel logLevel, Func<string> messageFunc, Exception exception = null, params object[] formatParameters);
}
