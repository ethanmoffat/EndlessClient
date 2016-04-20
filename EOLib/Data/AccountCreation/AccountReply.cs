// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Data.AccountCreation
{
	public enum AccountReply : short
	{
		THIS_IS_WRONG = 0,
		Exists = 1,
		NotApproved = 2,
		Created = 3,
		ChangeFailed = 5,
		ChangeSuccess = 6,
		Continue = 1000
	}
}