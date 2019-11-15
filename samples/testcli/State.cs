using System;
using System.Collections.Generic;

namespace testcli
{
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
}
