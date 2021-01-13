using System;
using System.Collections.Generic;
using System.Reflection;

namespace cli_dotnet
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    { 
        public CommandAttribute(string name = default, string helpText = default, string category = default)
        {
            Name = name;
            HelpText = helpText;
            Category = category;
        }

        public string Name { get; }
        public virtual string HelpText { get; }
        public string Category { get; }

        internal MethodInfo Method { get; set; }
        internal VerbAttribute ParentVerb { get; set; }
        internal Dictionary<string, OptionAttribute> Options { get; } = new Dictionary<string, OptionAttribute>(StringComparer.OrdinalIgnoreCase);
        internal List<ValueAttribute> Values { get; } = new List<ValueAttribute>();
        internal ParameterInfo GlobalOptionsParameter { get; set; }

        internal string GetName() => Name ?? Method.Name;
    }
}
