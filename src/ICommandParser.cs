using System.Collections.Generic;

namespace cli_dotnet
{
    public interface ICommandParser
    {
        string GetString(StringReference reference);
        IEnumerable<CommandPart> Parse();
    }
}
