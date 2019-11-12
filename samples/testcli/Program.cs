using cli_dotnet;
using System;
using System.Collections.Generic;
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

                await ExecuteAsync(command, state);
            }
        }

        async static Task ExecuteAsync(string command, State state)
        {
            var commandExecutor = new CommandExecutor(command);

            await commandExecutor.ExecuteAsync(new CommandRoot(state));
        }
    }

    public class State
    {
        public class User
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public bool CanLogin { get; set; }
        }

        public User CurrentContext = null;
        public Dictionary<string, User> Users { get; } = new Dictionary<string, User>(StringComparer.OrdinalIgnoreCase);
    }

    public class CommandRoot
    {
        private readonly State _state;

        public CommandRoot(State state)
        {
            User = new UserActions(state);
            _state = state;
        }

        [Verb]
        public UserActions User { get; }

        [Command]
        public Task Login(
            [Value]string username, 
            [Value]string password)
        {
            if (!_state.Users.TryGetValue(username, out var user) || user.Password != password)
            {
                Console.WriteLine("Unable to login, bad username or password");
                return Task.CompletedTask;
            }

            if (!user.CanLogin)
            {
                Console.WriteLine("User not allowed to login");
                return Task.CompletedTask;
            }

            Console.WriteLine("Login successful");

            _state.CurrentContext = user;

            return Task.CompletedTask;
        }

        [Command]
        public Task Logout()
        {
            _state.CurrentContext = null;
            return Task.CompletedTask;
        }
    }

    public class UserActions
    {
        private readonly State _state;

        public UserActions(State state)
        {
            _state = state;
        }

        [Command]
        public Task Create(
            [Value] string username, 
            [Value] string password,
            [Option(longForm:"preventlogin")] bool preventLogin = false)
        {
            if (_state.Users.ContainsKey(username))
            {
                Console.WriteLine("User already exists with that username");
                return Task.CompletedTask;
            }

            _state.Users[username] = new State.User
            {
                Username = username,
                Password = password,
                CanLogin = !preventLogin
            };

            Console.WriteLine("User created");

            return Task.CompletedTask;
        }
    }
}
