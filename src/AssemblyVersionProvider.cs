using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace cli_dotnet
{
    public class AssemblyVersionProvider : IVersionProvider
    {
        public IEnumerable<string> GetVersions()
        {
            var asm = Assembly.GetEntryAssembly();

            yield return "Assembly Version: " + (asm.GetName().Version ?? new Version()).ToString();

            var fvi = FileVersionInfo.GetVersionInfo(asm.Location);

            yield return "Assembly File Version: " + fvi.FileVersion;
            yield return "Product Version: " + fvi.ProductVersion;
        }
    }
}
