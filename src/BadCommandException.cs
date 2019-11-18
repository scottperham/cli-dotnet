using System;

namespace cli_dotnet
{
    public class BadCommandException : Exception
    { 
        public BadCommandException(string exception)
            : base(exception)
        { }

        public BadCommandException(VerbAttribute verb, string badVerb)
            : base($"Unknown verb {badVerb}")
        {
            Verb = verb;
            BadCommand = badVerb;
        }

        public BadCommandException(CommandAttribute command, string badCommand)
            : base($"Malformed or missing command {badCommand}")
        {
            Command = command;
            BadCommand = badCommand;
        }

        public VerbAttribute Verb { get; }
        public CommandAttribute Command { get; }
        public string BadCommand { get; }
    }
}
