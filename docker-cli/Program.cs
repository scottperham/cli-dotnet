using cli_dotnet;
using System;
using System.Threading.Tasks;

namespace docker_cli
{
    class Program
    {
        async static Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                while (true)
                {
                    Console.Write("> ");

                    var command = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(command))
                    {
                        continue;
                    }

                    if (command.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }

                    await ExecuteAsync(command);

                    Console.WriteLine();
                }
            }
            else
            {
                await ExecuteAsync(null);
            }
        }

        async static Task ExecuteAsync(string command)
        {
            var options = CommandExecutorOptions.Default;
            options.ValuesFirst = false;

            try
            {
                await Cli.ExecuteAsync(new CommandRoot(), command, new GlobalOptions(), options);
            }
            catch (Exception ex)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.GetBaseException().Message);
                Console.ForegroundColor = oldColor;
            }
        }
    }
}
