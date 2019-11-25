using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace cli_dotnet
{
    public interface ICommandExecutorImpl
    {
        Task ExecuteInternalAsync(VerbAttribute verb, IEnumerator<CommandPart> commandParts);
        Task ExecuteCommandAsync(CommandAttribute command, IEnumerator<CommandPart> commandParts);
        bool TryAddToLastParameter(ParameterInfo lastParameter, ParameterInfo thisParameter, string value, SortedList<int, object> parameters);
        void AddDefaultValues(CommandAttribute command, SortedList<int, object> parameters);
        Task ExecuteCommand(CommandAttribute command, SortedList<int, object> parameters);
    }
}
