using System.Reflection;

namespace cli_dotnet
{
    public class TypeHelper : ITypeHelper
    {
        public bool TryGetOptionAttribute(ParameterInfo parameterInfo, out OptionAttribute optionAttribute)
        {
            optionAttribute = parameterInfo.GetCustomAttribute<OptionAttribute>();
            return optionAttribute != null;
        }

        public bool TryGetValueAttribute(ParameterInfo parameterInfo, out ValueAttribute valueAttribute)
        {
            valueAttribute = parameterInfo.GetCustomAttribute<ValueAttribute>();
            return valueAttribute != null;
        }

        public bool TryGetVerbAttribute(PropertyInfo propertyInfo, out VerbAttribute verbAttribute)
        {
            verbAttribute = propertyInfo.GetCustomAttribute<VerbAttribute>();
            return verbAttribute != null;
        }

        public bool TryGetCommandAttribute(MethodInfo methodInfo, out CommandAttribute commandAttribute)
        {
            commandAttribute = methodInfo.GetCustomAttribute<CommandAttribute>();
            return commandAttribute != null;
        }

        public MemberInfo[] GetPropertiesAndMethods(object instance)
        {
            return instance.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.GetProperty | BindingFlags.InvokeMethod);
        }
    }
}
