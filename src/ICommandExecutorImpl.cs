﻿using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace cli_dotnet
{
    public interface ICommandExecutorImpl
    {
        Task ExecuteInternalAsync(VerbAttribute verb, IEnumerator<CommandPart> commandParts, GlobalOptionsWrapper globalOptions);
        Task ExecuteCommandAsync(CommandAttribute command, IEnumerator<CommandPart> commandParts, GlobalOptionsWrapper globalOptions);
        bool TryAddToLastParameter(ParameterInfo lastParameter, string value, SortedList<int, object> parameters);
        void AddDefaultValues(CommandAttribute command, SortedList<int, object> parameters);
        Task ExecuteActualCommandAsync(CommandAttribute command, SortedList<int, object> parameters);
        ParameterInfo SetOptionParameter(string key, CommandPart commandPart, CommandAttribute command, SortedList<int, object> parameters);
        ParameterInfo SetValueParameter(string key, CommandAttribute command, SortedList<int, object> parameters, ParameterInfo lastParameter);

        GlobalOptionsWrapper GetGlobalOptions<TGlobalOptions>(TGlobalOptions globalOptions);
        bool TrySetGlobalOption(string key, CommandPart commandPart, GlobalOptionsWrapper globalOptions);
    }
}
