// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Data.Account
{
	public class CreateAccountParameters : ICreateAccountParameters
	{
		public string AccountName { get; private set; }
		public string Password { get; private set; }
		public string ConfirmPassword { get; private set; }
		public string RealName { get; private set; }
		public string Location { get; private set; }
		public string Email { get; private set; }

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
}