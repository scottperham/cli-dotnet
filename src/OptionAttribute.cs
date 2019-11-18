namespace cli_dotnet
{
    public class OptionAttribute : ParameterAttribute
    {
        readonly string _longForm;

        public OptionAttribute(char shortForm = default, string longForm = default, string helpText = default)
            : base(longForm)
        {
            ShortForm = shortForm;
            _longForm = longForm;
            HelpText = helpText;
        }

        public char ShortForm { get; }
        public string LongForm { get => _longForm ?? Parameter.Name;  }
        public string HelpText { get; }
    }
}
