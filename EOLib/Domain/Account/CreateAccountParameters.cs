// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Account
{
    public class CreateAccountParameters : ICreateAccountParameters
    {
        public string AccountName { get; }
        public string Password { get; }
        public string ConfirmPassword { get; }
        public string RealName { get; }
        public string Location { get; }
        public string Email { get; }

        public CreateAccountParameters(
            string accountName,
            string password,
            string confirmPassword,
            string realName,
            string location,
            string email)
        {
            AccountName = accountName;
            Password = password;
            ConfirmPassword = confirmPassword;
            RealName = realName;
            Location = location;
            Email = email;
        }
    }

    public interface ICreateAccountParameters
    {
        string AccountName { get; }
        string Password { get; }
        string ConfirmPassword { get; }
        string RealName { get; }
        string Location { get; }
        string Email { get; }
    }
}