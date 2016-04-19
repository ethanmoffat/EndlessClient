// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;

namespace EOLib.Data.AccountCreation
{
	public class AccountCreateActions : IAccountCreateActions
	{
		private readonly IAccountCreateParameterProvider _accountCreateParameterProvider;
		private readonly IAccountCreateParameterValidator _accountCreateParameterValidator;

		public AccountCreateActions(IAccountCreateParameterProvider accountCreateParameterProvider,
			IAccountCreateParameterValidator accountCreateParameterValidator)
		{
			_accountCreateParameterProvider = accountCreateParameterProvider;
			_accountCreateParameterValidator = accountCreateParameterValidator;
		}

		public void CheckAccountCreateParameters()
		{
			if (AnyFieldsStillEmpty())
				throw new AccountCreateParameterException(DATCONST1.ACCOUNT_CREATE_FIELDS_STILL_EMPTY);

			if (_accountCreateParameterValidator.AccountNameIsNotLongEnough(Parameters.AccountName))
				throw new AccountCreateParameterException(DATCONST1.ACCOUNT_CREATE_NAME_TOO_SHORT);

			if (_accountCreateParameterValidator.AccountNameIsTooObvious(Parameters.AccountName))
				throw new AccountCreateParameterException(DATCONST1.ACCOUNT_CREATE_NAME_TOO_OBVIOUS);

			if (_accountCreateParameterValidator.PasswordMismatch(Parameters.Password, Parameters.ConfirmPassword))
				throw new AccountCreateParameterException(DATCONST1.ACCOUNT_CREATE_PASSWORD_MISMATCH);

			if (_accountCreateParameterValidator.PasswordIsTooShort(Parameters.Password))
				throw new AccountCreateParameterException(DATCONST1.ACCOUNT_CREATE_PASSWORD_TOO_SHORT);

			if (_accountCreateParameterValidator.PasswordIsTooObvious(Parameters.Password))
				throw new AccountCreateParameterException(DATCONST1.ACCOUNT_CREATE_PASSWORD_TOO_OBVIOUS);

			if (_accountCreateParameterValidator.EmailIsInvalid(Parameters.Email))
				throw new AccountCreateParameterException(DATCONST1.ACCOUNT_CREATE_EMAIL_INVALID);
		}

		private bool AnyFieldsStillEmpty()
		{
			return new[]
			{
				Parameters.AccountName,
				Parameters.Password,
				Parameters.ConfirmPassword,
				Parameters.RealName,
				Parameters.Location,
				Parameters.Email
			}.Any(x => x.Length == 0);
		}

		private IAccountCreateParameters Parameters
		{
			get { return _accountCreateParameterProvider.AccountCreateParameters; }
		}
	}
}