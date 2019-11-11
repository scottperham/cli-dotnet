using System;
using System.Collections.Generic;
using System.Reflection;

namespace CLI
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    { 
        public CommandAttribute(string name = default, string helpText = default)
        {
            Name = name;
            HelpText = helpText;
        }

        public string Name { get; }
        public string HelpText { get; }

        internal MethodInfo Method { get; set; }
        internal VerbAttribute ParentVerb { get; set; }
        internal Dictionary<string, OptionAttribute> Options { get; } = new Dictionary<string, OptionAttribute>(StringComparer.OrdinalIgnoreCase);
        internal List<ValueAttribute> Values { get; } = new List<ValueAttribute>();

        internal string GetName() => Name ?? Method.Name;
    }
}
