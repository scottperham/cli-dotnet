using cli_dotnet;
using System;
using System.Threading.Tasks;

namespace testcli
{
    class Program
    {
        async static Task Main(string[] args)
        {
            var state = new State();

            while (true)
            {
                Console.Write($"{state.CurrentContext?.Username ?? "root"}> ");

                var command = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(command))
                {
                    continue;
                }

                if (command.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                try
                {
                    await ExecuteAsync(command, state);
                }
                catch(Exception ex)
                {
                    var oldColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.GetBaseException().Message);
                    Console.ForegroundColor = oldColor;
                }

                Console.WriteLine();
            }
        }

        async static Task ExecuteAsync(string command, State state)
        {
            var commandExecutor = new CommandExecutor(command);

            await commandExecutor.ExecuteAsync(new CommandRoot(state));
        }
    }
}
