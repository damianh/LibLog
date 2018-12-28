//===============================================================================
// LibLog
//
// https://github.com/damianh/LibLog
//===============================================================================
// Copyright © 2011-2017 Damian Hickey.  All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//===============================================================================

// @formatter:off — disable resharper formatter after this line
// ReSharper disable PossibleNullReferenceException

// Define LIBLOG_PUBLIC to enable ability to GET a logger (LogProvider.For<>() etc) from outside this library. NOTE:
// this can have unintended consequences of consumers of your library using your library to resolve a logger. If the
// reason is because you want to open this functionality to other projects within your solution,
// consider [InternalsVisibleTo] instead.
// 
// Define LIBLOG_PROVIDERS_ONLY if your library provides its own logging API and you just want to use the
// LibLog providers internally to provide built in support for popular logging frameworks.

#pragma warning disable 1591

using global::System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "YourRootNamespace.Logging")]
[assembly: SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Scope = "member", Target = "YourRootNamespace.Logging.Logger.#Invoke(YourRootNamespace.Logging.LogLevel,System.Func`1<System.String>,System.Exception,System.Object[])")]

// If you copied this file manually, you need to change all "YourRootNameSpace" so not to clash with other libraries
// that use LibLog
namespace YourRootNamespace.Logging
{
    using global::System.Collections.Generic;
    using global::System.Diagnostics.CodeAnalysis;
    using global::YourRootNamespace.Logging.LogProviders;
    using global::System;
#if !LIBLOG_PROVIDERS_ONLY
    using global::System.Diagnostics;
    using global::System.Runtime.CompilerServices;
#endif

    /// <summary>
    /// Provides a mechanism to create instances of <see cref="ILog" /> objects.
    /// </summary>
#if LIBLOG_EXCLUDE_CODE_COVERAGE
    [ExcludeFromCodeCoverage]
#endif
#if LIBLOG_PROVIDERS_ONLY
    internal
#else
    public
#endif
    static class LogProvider
    {
        private static readonly Lazy<ILogProvider> ResolvedLogProvider = new Lazy<ILogProvider>(ForceResolveLogProvider);
#if !LIBLOG_PROVIDERS_ONLY
        private static dynamic s_currentLogProvider;
        private static Action<ILogProvider> s_onCurrentLogProviderSet;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static LogProvider()
        {
            IsDisabled = false;
        }

        /// <summary>
        /// Sets the current log provider.
        /// </summary>
        /// <param name="logProvider">The log provider.</param>
        public static void SetCurrentLogProvider(ILogProvider logProvider)
        {
            s_currentLogProvider = logProvider;

            RaiseOnCurrentLogProviderSet();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this is logging is disabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if logging is disabled; otherwise, <c>false</c>.
        /// </value>
        public static bool IsDisabled { get; set; }

        /// <summary>
        /// Sets an action that is invoked when a consumer of your library has called SetCurrentLogProvider. It is 
        /// important that hook into this if you are using child libraries (especially ilmerged ones) that are using
        /// LibLog (or other logging abstraction) so you adapt and delegate to them.
        /// <see cref="SetCurrentLogProvider"/> 
        /// </summary>
        internal static Action<ILogProvider> OnCurrentLogProviderSet
        {
            set
            {
                s_onCurrentLogProviderSet = value;
                RaiseOnCurrentLogProviderSet();
            }
        }

        internal static ILogProvider CurrentLogProvider { get { return s_currentLogProvider; } }

		/// <summary>
		/// Gets a logger for the specified type.
		/// </summary>
		/// <typeparam name="T">The type whose name will be used for the logger.</typeparam>
		/// <returns>An instance of <see cref="ILog"/></returns>
#if LIBLOG_PUBLIC
        public
#else
		internal
#endif
		static ILog For<T>() { return GetLogger(typeof(T)); }

        /// <summary>
        /// Gets a logger for the current class.
        /// </summary>
        /// <returns>An instance of <see cref="ILog"/></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
#if LIBLOG_PUBLIC
        public
#else
        internal
#endif
        static ILog GetCurrentClassLogger()
        {
            var stackFrame = new StackFrame(1, false);
            return GetLogger(stackFrame.GetMethod().DeclaringType);
        }

        /// <summary>
        /// Gets a logger for the specified type.
        /// </summary>
        /// <param name="type">The type whose name will be used for the logger.</param>
        /// <param name="fallbackTypeName">If the type is null then this name will be used as the log name instead</param>
        /// <returns>An instance of <see cref="ILog"/></returns>
#if LIBLOG_PUBLIC
        public
#else
        internal
#endif
        static ILog GetLogger(Type type, string fallbackTypeName = "System.Object")
        {
            // If the type passed in is null then fallback to the type name specified
            return GetLogger(type != null ? type.ToString() : fallbackTypeName);
        }

        /// <summary>
        /// Gets a logger with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>An instance of <see cref="ILog"/></returns>
#if LIBLOG_PUBLIC
        public
#else
        internal
#endif
        static ILog GetLogger(string name)
        {
            var logProvider = CurrentLogProvider ?? ResolveLogProvider();
            return logProvider == null
                ? NoOpLogger.Instance
                : (ILog)new LoggerExecutionWrapper(logProvider.GetLogger(name), () => IsDisabled);
        }

        /// <summary>
        /// Opens a nested diagnostics context.
        /// </summary>
        /// <param name="message">A message.</param>
        /// <returns>An <see cref="IDisposable"/> that closes context when disposed.</returns>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SetCurrentLogProvider")]
#if LIBLOG_PUBLIC
        public
#else
        internal
#endif
        static IDisposable OpenNestedContext(string message)
        {
            var logProvider = CurrentLogProvider ?? ResolveLogProvider();

            return logProvider == null
                ? new DisposableAction(() => { })
                : logProvider.OpenNestedContext(message);
        }

        /// <summary>
        /// Opens a mapped diagnostics context.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="destructure">A optional paramater to indicate message should be destructured.</param>
        /// <returns>An <see cref="IDisposable"/> that closes context when disposed.</returns>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SetCurrentLogProvider")]
#if LIBLOG_PUBLIC
        public
#else
        internal
#endif
        static IDisposable OpenMappedContext(string key, object value, bool destructure = false)
        {
            var logProvider = CurrentLogProvider ?? ResolveLogProvider();

            return logProvider == null
                ? new DisposableAction(() => { })
                : logProvider.OpenMappedContext(key, value, destructure);
        }
#endif

#if LIBLOG_PROVIDERS_ONLY
    private
#else
        internal
#endif
        delegate bool IsLoggerAvailable();

#if LIBLOG_PROVIDERS_ONLY
        private
#else
        internal
#endif
        delegate ILogProvider CreateLogProvider();

#if LIBLOG_PROVIDERS_ONLY
        private
#else
        internal
#endif
        static readonly List<Tuple<IsLoggerAvailable, CreateLogProvider>> LogProviderResolvers =
                new List<Tuple<IsLoggerAvailable, CreateLogProvider>>
        {
            new Tuple<IsLoggerAvailable, CreateLogProvider>(SerilogLogProvider.IsLoggerAvailable, () => new SerilogLogProvider()),
            new Tuple<IsLoggerAvailable, CreateLogProvider>(NLogLogProvider.IsLoggerAvailable, () => new NLogLogProvider()),
            new Tuple<IsLoggerAvailable, CreateLogProvider>(Log4NetLogProvider.IsLoggerAvailable, () => new Log4NetLogProvider()),
            new Tuple<IsLoggerAvailable, CreateLogProvider>(LoupeLogProvider.IsLoggerAvailable, () => new LoupeLogProvider()),
        };

#if !LIBLOG_PROVIDERS_ONLY
        private static void RaiseOnCurrentLogProviderSet()
        {
            if (s_onCurrentLogProviderSet != null)
            {
                s_onCurrentLogProviderSet(s_currentLogProvider);
            }
        }
#endif

        internal static ILogProvider ResolveLogProvider()
        {
            return ResolvedLogProvider.Value;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String,System.Object,System.Object)")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal static ILogProvider ForceResolveLogProvider()
        {
            try
            {
                foreach (var providerResolver in LogProviderResolvers)
                {
                    if (providerResolver.Item1())
                    {
                        return providerResolver.Item2();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "Exception occurred resolving a log provider. Logging for this assembly {0} is disabled. {1}",
                    typeof(LogProvider).Assembly.FullName,
                    ex);
            }
            return null;
        }

#if !LIBLOG_PROVIDERS_ONLY
#if LIBLOG_EXCLUDE_CODE_COVERAGE
    [ExcludeFromCodeCoverage]
#endif
        internal class NoOpLogger : ILog
        {
            internal static readonly NoOpLogger Instance = new NoOpLogger();

			public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception, params object[] formatParameters)
			{ return false; }
        }
#endif
    }
}
