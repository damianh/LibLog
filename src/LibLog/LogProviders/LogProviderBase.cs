namespace YourRootNamespace.Logging.LogProviders
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Base class for specific log providers.
    /// </summary>
#if LIBLOG_EXCLUDE_CODE_COVERAGE
    [ExcludeFromCodeCoverage]
#endif
#if LIBLOG_PUBLIC
    public
#else
    internal
#endif
    abstract class LogProviderBase : ILogProvider
    {
        private static readonly IDisposable NoopDisposableInstance = new DisposableAction();
        private readonly Lazy<OpenMdc> _lazyOpenMdcMethod;

        /// <summary>
        ///     Error message should initializing the log provider fail.
        /// </summary>
        protected const string ErrorInitializingProvider = "Unable to log due to problem initializing the log provider. See inner exception for details.";

        private readonly Lazy<OpenNdc> _lazyOpenNdcMethod;

        /// <summary>
        ///     Initialize an instance of the <see cref="LogProviderBase"/> class by initializing the references
        ///     to the nested and mapped diagnostics context-obtaining functions.
        /// </summary>
        protected LogProviderBase()
        {
            _lazyOpenNdcMethod
                = new Lazy<OpenNdc>(GetOpenNdcMethod);
            _lazyOpenMdcMethod
                = new Lazy<OpenMdc>(GetOpenMdcMethod);
        }

        /// <summary>
        /// Gets the specified named logger.
        /// </summary>
        /// <param name="name">Name of the logger.</param>
        /// <returns>The logger reference.</returns>
        public abstract Logger GetLogger(string name);

        /// <summary>
        /// Opens a nested diagnostics context. Not supported in EntLib logging.
        /// </summary>
        /// <param name="message">The message to add to the diagnostics context.</param>
        /// <returns>A disposable that when disposed removes the message from the context.</returns>
        public IDisposable OpenNestedContext(string message)
        {
            return _lazyOpenNdcMethod.Value(message);
        }

        /// <summary>
        /// Opens a mapped diagnostics context. Not supported in EntLib logging.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="destructure">Determines whether to call the destructor or not.</param>
        /// <returns>A disposable that when disposed removes the map from the context.</returns>
        public IDisposable OpenMappedContext(string key, object value, bool destructure = false)
        {
            return _lazyOpenMdcMethod.Value(key, value, destructure);
        }

        /// <summary>
        ///     Returns the provider-specific method to open a nested diagnostics context.
        /// </summary>
        /// <returns>A provider-specific method to open a nested diagnostics context.</returns>
        protected virtual OpenNdc GetOpenNdcMethod()
        {
            return (_) => NoopDisposableInstance;
        }

        /// <summary>
        ///     Returns the provider-specific method to open a mapped diagnostics context.
        /// </summary>
        /// <returns>A provider-specific method to open a mapped diagnostics context.</returns>
        protected virtual OpenMdc GetOpenMdcMethod()
        {
            return (_, __, ___) => NoopDisposableInstance;
        }

        /// <summary>
        ///     Delegate defining the signature of the method opening a nested diagnostics context.
        /// </summary>
        /// <param name="message">The message to add to the diagnostics context.</param>
        /// <returns>A disposable that when disposed removes the message from the context.</returns>
        protected delegate IDisposable OpenNdc(string message);

        /// <summary>
        ///     Delegate defining the signature of the method opening a mapped diagnostics context.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="destructure">Determines whether to call the destructor or not.</param>
        /// <returns>A disposable that when disposed removes the map from the context.</returns>
        protected delegate IDisposable OpenMdc(string key, object value, bool destructure);
    }
}