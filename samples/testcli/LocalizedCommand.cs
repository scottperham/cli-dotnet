using cli_dotnet;

namespace testcli
{
    public class LocalizedCommandAttribute : CommandAttribute
    {
        private readonly string _helpTextKey;

        public LocalizedCommandAttribute(string name = null, string helpTextKey = null) 
            : base(name, default)
        {
            _helpTextKey = helpTextKey;
        }

        public override string HelpText => "Localized - " + _helpTextKey;
    }
}
