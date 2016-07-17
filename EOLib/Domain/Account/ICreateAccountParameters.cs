// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Account
{
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
