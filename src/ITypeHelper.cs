using System.Reflection;

namespace cli_dotnet
{
    public interface ITypeHelper
    {
        bool TryGetValueAttribute(ParameterInfo parameterInfo, out ValueAttribute valueAttribute);
        bool TryGetOptionAttribute(ParameterInfo parameterInfo, out OptionAttribute optionAttribute);
        bool TryGetVerbAttribute(PropertyInfo propertyInfo, out VerbAttribute verbAttribute);
        bool TryGetCommandAttribute(MethodInfo methodInfo, out CommandAttribute commandAttribute);
        MemberInfo[] GetPropertiesAndMethods(object instance);
    }
}
