using System.Collections.Generic;

namespace cli_dotnet
{
    public interface ICommandHelper
    {
        bool TryShowHelpOrVersion(CommandPart commandPart, CommandAttribute command, string key, ICommandExecutorOptions options, GlobalOptionsWrapper globalOptions);
        bool TryShowHelpOrVersion(CommandPart commandPart, VerbAttribute verb, string key, ICommandExecutorOptions options, GlobalOptionsWrapper globalOptions);
        void WriteVersion(ICommandExecutorOptions options);
        void WriteVerbHelp(VerbAttribute verb, ICommandExecutorOptions options, GlobalOptionsWrapper globalOptions);
        void WriteCommandHelp(CommandAttribute command, ICommandExecutorOptions options, GlobalOptionsWrapper globalOptions);
        void GetVerbHelp(VerbAttribute verb, string prefix, SortedDictionary<string, string> help);
    }
}
