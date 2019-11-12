namespace cli_dotnet
{

    public class OptionAttribute : ParameterAttribute
    { 
        public OptionAttribute(char shortForm = default, string longForm = default, string helpText = default)
            : base(longForm)
        {
            ShortForm = shortForm;
            LongForm = longForm;
            HelpText = helpText;
        }

        public char ShortForm { get; }
        public string LongForm { get; }
        public string HelpText { get; }
    }
}
