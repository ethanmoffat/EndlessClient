// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Data.Login
{
	public enum LoginReply : short
	{
		WrongUser = 1,
		WrongUserPass = 2,
		Ok = 3,
		LoggedIn = 5,
		Busy = 6,
		THIS_IS_WRONG = 255
	}
}