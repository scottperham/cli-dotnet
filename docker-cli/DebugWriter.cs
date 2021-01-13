using cli_dotnet;
using System;
using System.Collections;
using System.Reflection;
using System.Text;

namespace docker_cli
{
    public static class DebugWriter
    {
        public static void WriteMethodCall(MethodBase methodInfo, params object[] parameterValues)
        {
            Console.WriteLine("Calling: " + methodInfo.Name);

            foreach(var parameter in methodInfo.GetParameters())
            {
                if (parameter.ParameterType == typeof(GlobalOptions))
                {
                    Write((GlobalOptions)parameterValues[parameter.Position]);
                }
                else
                {
                    Console.WriteLine(parameter.Name + ": " + GetValueString(parameterValues[parameter.Position]));
                }
            }
        }

        public static string GetValueString(object value)
        {
            if (value == null)
            {
                return "";
            }

            if (value is IEnumerable && !(value is string))
            {
                var sb = new StringBuilder();

                foreach(var val in (IEnumerable)value)
                {
                    sb.Append((val ?? "").ToString() + ",");
                }

                return sb.ToString().TrimEnd(',');
            }

            return value.ToString();
        }

        public static void Write(GlobalOptions globalOptions)
        {
            foreach(var prop in typeof(GlobalOptions).GetProperties())
            {
                var value = prop.GetValue(globalOptions);

                if (prop.PropertyType.IsClass)
                {
                    if (value == null)
                    {
                        continue;
                    }
                }
                else if (value.Equals(Activator.CreateInstance(prop.PropertyType)))
                {
                    continue;
                }

                var name = prop.GetCustomAttribute<GlobalOptionAttribute>().LongForm;

                Console.WriteLine(name + " = " + value);
            }
        }

        public static void Write(string name, object value)
        {
            Console.WriteLine(name + " = " + value);
        }
    }
}
