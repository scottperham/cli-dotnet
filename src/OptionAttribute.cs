using System;
using System.Reflection;

namespace cli_dotnet
{
    public class OptionAttribute : ParameterAttribute
    {
        readonly string _longForm;
        private readonly bool _noLongForm;

        public OptionAttribute(char shortForm = default, string longForm = default, string helpText = default, bool noLongForm = default)
            : base(longForm)
        {
            ShortForm = shortForm;
            _longForm = longForm;
            HelpText = helpText;
            _noLongForm = noLongForm;
        }

        public char ShortForm { get; }
        public string LongForm { get => _noLongForm ? null : (_longForm ?? Parameter.Name);  }
        public string HelpText { get; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class GlobalOptionAttribute : Attribute
    {
        readonly string _longForm;
        private readonly bool _noLongForm;

        public GlobalOptionAttribute(char shortForm = default, string longForm = default, string helpText = default, bool noLongForm = default)
        {
            ShortForm = shortForm;
            _longForm = longForm;
            HelpText = helpText;
            _noLongForm = noLongForm;
        }

        internal PropertyInfo Property { get; set; }
        public char ShortForm { get; }
        public string LongForm { get => _noLongForm ? null : (_longForm ?? Property.Name); }
        public string HelpText { get; }

        internal string GetName() => _longForm ?? Property.Name;
    }
}
