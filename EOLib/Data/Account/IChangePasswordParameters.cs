// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Data.Account
{
	public interface IChangePasswordParameters
	{
		string AccountName { get; }
		string OldPassword { get; }
		string NewPassword { get; }
		string ConfirmNewPassword { get; }
	}
}