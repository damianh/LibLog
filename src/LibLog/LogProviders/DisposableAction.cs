namespace YourRootNamespace.Logging.LogProviders
{
    using System;
    using System.Diagnostics.CodeAnalysis;

#if LIBLOG_EXCLUDE_CODE_COVERAGE
    [ExcludeFromCodeCoverage]
#endif
    internal class DisposableAction : IDisposable
    {
        private readonly Action _onDispose;

        public DisposableAction(Action onDispose = null)
        {
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            if(_onDispose != null) _onDispose.Invoke();
        }
    }
}