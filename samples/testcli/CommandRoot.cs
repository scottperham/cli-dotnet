using cli_dotnet;
using System;
using System.Threading.Tasks;

namespace testcli
{
    public class CommandRoot
    {
        private readonly State _state;

        public CommandRoot(State state)
        {
            UserAction = new UserActions(state);
            _state = state;
        }

        [Verb]
        public UserActions UserAction { get; }

        [Command(helpText:"Logs in a user")]
        public Task Login(
            [Value(helpText:"The username of the user to login")]string username, 
            [Value(helpText: "The password of the user to login")]string password)
        {
            if (!_state.Users.TryGetValue(username, out var user) || user.Password != password)
            {
                throw new Exception("Unable to login, bad username or password");
            }

            if (!user.CanLogin)
            {
                throw new Exception("User not allowed to login");
            }

            Console.WriteLine("Login successful");

            _state.CurrentContext = user;

            return Task.CompletedTask;
        }

        [Command(helpText:"Logs out the current user")]
        public Task Logout()
        {
            _state.CurrentContext = null;
            return Task.CompletedTask;
        }
    }
}
