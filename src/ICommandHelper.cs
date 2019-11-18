using System.Collections.Generic;

namespace cli_dotnet
{
    public interface ICommandHelper
    {
        bool TryShowHelp(CommandPart commandPart, CommandAttribute command, string key, ICommandExecutorOptions options);
        bool TryShowHelp(CommandPart commandPart, VerbAttribute verb, string key, ICommandExecutorOptions options);
        void WriteVerbHelp(VerbAttribute verb, ICommandExecutorOptions options);
        void WriteCommandHelp(CommandAttribute command, ICommandExecutorOptions options);
        void GetVerbHelp(VerbAttribute verb, string prefix, SortedDictionary<string, string> help);
    }
}
