// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;

namespace EOLib.Data.Account
{
	public class CreateAccountParameterValidator : ICreateAccountParameterValidator
	{
		private const string ValidEmailRegex = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+(?:[A-Z]{2}|com|org|net|edu|gov|mil|biz|info|mobi|name|aero|asia|jobs|museum)\b";

		public bool AccountNameIsNotLongEnough(string account)
		{
			return account.Length < 4;
		}

		public bool AccountNameIsTooObvious(string account)
		{
			return account.Distinct().Count() < 3;
		}

		public bool PasswordMismatch(string input, string confirm)
		{
			return input != confirm;
		}

		public bool PasswordIsTooShort(string password)
		{
			return password.Length < 6;
		}

		public bool PasswordIsTooObvious(string password)
		{
			return password.Distinct().Count() < 3;
		}

		public bool EmailIsInvalid(string email)
		{
			return !System.Text.RegularExpressions.Regex.IsMatch(email, ValidEmailRegex);
		}
	}
}