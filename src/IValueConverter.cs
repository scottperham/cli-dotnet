using System;
using System.Reflection;

namespace cli_dotnet
{
    public interface IValueConverter
    {
        object GetValue(string value, ParameterInfo parameterInfo, string name);
        object CreateDefaultValue(Type type);
    }
}
