// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;

namespace EOLib.Data.AccountCreation
{
	public class AccountCreateActions : IAccountCreateActions
	{
		private readonly IAccountCreateParameterValidator _accountCreateParameterValidator;

		public AccountCreateActions(IAccountCreateParameterValidator accountCreateParameterValidator)
		{
			_accountCreateParameterValidator = accountCreateParameterValidator;
		}

		public void CheckAccountCreateParameters(IAccountCreateParameters parameters)
		{
			if (AnyFieldsStillEmpty(parameters))
				throw new AccountCreateParameterException(DATCONST1.ACCOUNT_CREATE_FIELDS_STILL_EMPTY);

			if (_accountCreateParameterValidator.AccountNameIsNotLongEnough(parameters.AccountName))
				throw new AccountCreateParameterException(DATCONST1.ACCOUNT_CREATE_NAME_TOO_SHORT);

			if (_accountCreateParameterValidator.AccountNameIsTooObvious(parameters.AccountName))
				throw new AccountCreateParameterException(DATCONST1.ACCOUNT_CREATE_NAME_TOO_OBVIOUS);

			if (_accountCreateParameterValidator.PasswordMismatch(parameters.Password, parameters.ConfirmPassword))
				throw new AccountCreateParameterException(DATCONST1.ACCOUNT_CREATE_PASSWORD_MISMATCH);

			if (_accountCreateParameterValidator.PasswordIsTooShort(parameters.Password))
				throw new AccountCreateParameterException(DATCONST1.ACCOUNT_CREATE_PASSWORD_TOO_SHORT);

			if (_accountCreateParameterValidator.PasswordIsTooObvious(parameters.Password))
				throw new AccountCreateParameterException(DATCONST1.ACCOUNT_CREATE_PASSWORD_TOO_OBVIOUS);

			if (_accountCreateParameterValidator.EmailIsInvalid(parameters.Email))
				throw new AccountCreateParameterException(DATCONST1.ACCOUNT_CREATE_EMAIL_INVALID);
		}

		private bool AnyFieldsStillEmpty(IAccountCreateParameters parameters)
		{
			return new[]
			{
				parameters.AccountName,
				parameters.Password,
				parameters.ConfirmPassword,
				parameters.RealName,
				parameters.Location,
				parameters.Email
			}.Any(x => x.Length == 0);
		}
	}
}