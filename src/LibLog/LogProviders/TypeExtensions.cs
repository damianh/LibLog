namespace YourRootNamespace.Logging.LogProviders
{
    using System;
    using System.Reflection;

#if LIBLOG_EXCLUDE_CODE_COVERAGE
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
#endif
    internal static class TypeExtensions
    {
        internal static ConstructorInfo GetConstructorPortable(this Type type, params Type[] types)
        {
            return type.GetTypeInfo().GetConstructor(types);
        }

        internal static MethodInfo GetMethod(this Type type, string name, params Type[] types)
        {
            return type.GetMethod(name, types);
        }
    }
}