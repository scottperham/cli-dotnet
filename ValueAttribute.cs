using System;
using System.Reflection;

namespace CLI
{
    public class ValueAttribute : ParameterAttribute
    {
        public ValueAttribute(string helpText = default)
            : base(null)
        {
            HelpText = helpText;
        }

        public string HelpText { get; }
    }
}
