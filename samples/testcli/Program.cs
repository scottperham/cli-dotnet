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

            if (args.Length == 0)
            {
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

                    await ExecuteAsync(command, state);

                    Console.WriteLine();
                }
            }
            else
            {
                await ExecuteAsync(null, state);
            }
        }
        
        async static Task ExecuteAsync(string command, State state)
        {
            try
            {
                await Cli.ExecuteAsync(new CommandRoot(state), command);
            }
            catch(Exception ex)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.GetBaseException().Message);
                Console.ForegroundColor = oldColor;
            }
        }
    }
}
