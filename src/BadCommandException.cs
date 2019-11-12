using System;

namespace cli_dotnet
{
    public class BadCommandException : Exception
    { 
        public BadCommandException(VerbAttribute verb, string badCommand)
        {
            Verb = verb;
            BadCommand = badCommand;
        }

        public BadCommandException(CommandAttribute command, string badCommand)
        {
            Command = command;
            BadCommand = badCommand;
        }

        public VerbAttribute Verb { get; }
        public CommandAttribute Command { get; }
        public string BadCommand { get; }
    }
}
