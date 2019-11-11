using System;
using System.Reflection;

namespace CLI
{
    public static class MemberInfoExtensions
    {
        public static bool TryGetCustomAttribute<T>(this MemberInfo memberInfo, out T attribute)
            where T : Attribute
        {
            attribute = memberInfo.GetCustomAttribute<T>();
            return attribute != null;
        }

        public static bool TryGetCustomAttribute<T>(this ParameterInfo memberInfo, out T attribute)
            where T : Attribute
        {
            attribute = memberInfo.GetCustomAttribute<T>();
            return attribute != null;
        }
    }
}
