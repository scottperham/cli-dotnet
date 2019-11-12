using System;
using System.Collections.Generic;
using System.Reflection;

namespace cli_dotnet
{
    [AttributeUsage(AttributeTargets.Property)]
    public class VerbAttribute : Attribute
    { 
        public VerbAttribute(string name = default, string helpText = default)
        {
            Name = name;
            HelpText = helpText;
        }

        public string Name { get; }
        public string HelpText { get; }

        internal bool IsRoot { get; set; }
        internal PropertyInfo Property { get; set; }
        internal VerbAttribute ParentVerb { get; set; }
        internal object Instance { get; set; }
        internal Dictionary<string, VerbAttribute> Verbs { get; } = new Dictionary<string, VerbAttribute>(StringComparer.OrdinalIgnoreCase);
        internal Dictionary<string, CommandAttribute> Commands { get; } = new Dictionary<string, CommandAttribute>(StringComparer.OrdinalIgnoreCase);

        internal string GetName() => Name ?? Property?.Name ?? "{Root}";
    }
}
