// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Account
{
    public class ChangePasswordParameters : IChangePasswordParameters
    {
        public string AccountName { get; }
        public string OldPassword { get; }
        public string NewPassword { get; }

        public ChangePasswordParameters(string accountName,
            string oldPassword,
            string newPassword)
        {
            AccountName = accountName;
            OldPassword = oldPassword;
            NewPassword = newPassword;
        }
    }

    public interface IChangePasswordParameters
    {
        string AccountName { get; }
        string OldPassword { get; }
        string NewPassword { get; }
    }
}