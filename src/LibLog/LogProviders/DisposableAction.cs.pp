namespace $rootnamespace$.Logging.LogProviders
{
    using System;

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
            _onDispose?.Invoke();
        }
    }
}
