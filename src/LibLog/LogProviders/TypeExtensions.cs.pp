namespace $rootnamespace$.Logging.LogProviders
{
    using System;
    using System.Reflection;

#if LIBLOG_EXCLUDE_CODE_COVERAGE
    [ExcludeFromCodeCoverage]
#endif
    internal static class TypeExtensions
    {
        internal static ConstructorInfo GetConstructorPortable(this Type type, params Type[] types)
        {
            return type.GetConstructor(types);
        }

        internal static MethodInfo GetMethod(this Type type, string name, params Type[] types)
        {
            return type.GetMethod(name, types);
        }
    }
}
