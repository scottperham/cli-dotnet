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
}
