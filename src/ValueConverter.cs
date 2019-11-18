using System;
using System.Reflection;

namespace cli_dotnet
{
    public class ValueConverter : IValueConverter
    {
        object IValueConverter.GetValue(string value, ParameterInfo parameterInfo, string name)
        {
            var type = parameterInfo.ParameterType;

            if (type == typeof(bool))
            {
                return true;
            }

            return Convert.ChangeType(value, Type.GetTypeCode(type));
        }

        object IValueConverter.CreateDefaultValue(Type type)
        {
            if (type == typeof(string))
            {
                return "";
            }

            return Activator.CreateInstance(type);
        }
    }
}
