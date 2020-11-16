using System;
using System.Collections.Generic;

namespace cli_dotnet
{
    public interface IVersionProvider
    {
        IEnumerable<string> GetVersions();
    }
}
