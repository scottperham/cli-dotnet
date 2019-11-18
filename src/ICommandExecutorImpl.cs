using System.Collections.Generic;
using System.Threading.Tasks;

namespace cli_dotnet
{
    public interface ICommandExecutorImpl
    {
        Task ExecuteInternalAsync(VerbAttribute verb, IEnumerator<CommandPart> commandParts);
        Task ExecuteCommandAsync(CommandAttribute command, IEnumerator<CommandPart> commandParts);
    }
}
