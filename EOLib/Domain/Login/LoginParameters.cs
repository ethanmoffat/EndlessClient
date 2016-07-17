// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Login
{
    public class LoginParameters : ILoginParameters
    {
        public string Username { get; private set; }
        public string Password { get; private set; }

        public LoginParameters(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}