// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Data.AccountCreation
{
	public interface ICreateAccountParameterValidator
	{
		bool AccountNameIsNotLongEnough(string account);

		bool AccountNameIsTooObvious(string account);

		bool PasswordMismatch(string input, string confirm);

		bool PasswordIsTooShort(string password);

		bool PasswordIsTooObvious(string password);

		bool EmailIsInvalid(string email);
	}
}
