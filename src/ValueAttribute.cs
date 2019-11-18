namespace cli_dotnet
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
