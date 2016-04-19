// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;

namespace EOLib.Data.AccountCreation
{
	public class CreateAccountActions : ICreateAccountActions
	{
		private readonly ICreateAccountParameterValidator _createAccountParameterValidator;

		public CreateAccountActions(ICreateAccountParameterValidator createAccountParameterValidator)
		{
			_createAccountParameterValidator = createAccountParameterValidator;
		}

		public void CheckAccountCreateParameters(ICreateAccountParameters parameters)
		{
			if (AnyFieldsStillEmpty(parameters))
				throw new CreateAccountParameterException(DATCONST1.ACCOUNT_CREATE_FIELDS_STILL_EMPTY);

			if (_createAccountParameterValidator.AccountNameIsNotLongEnough(parameters.AccountName))
				throw new CreateAccountParameterException(DATCONST1.ACCOUNT_CREATE_NAME_TOO_SHORT);

			if (_createAccountParameterValidator.AccountNameIsTooObvious(parameters.AccountName))
				throw new CreateAccountParameterException(DATCONST1.ACCOUNT_CREATE_NAME_TOO_OBVIOUS);

			if (_createAccountParameterValidator.PasswordMismatch(parameters.Password, parameters.ConfirmPassword))
				throw new CreateAccountParameterException(DATCONST1.ACCOUNT_CREATE_PASSWORD_MISMATCH);

			if (_createAccountParameterValidator.PasswordIsTooShort(parameters.Password))
				throw new CreateAccountParameterException(DATCONST1.ACCOUNT_CREATE_PASSWORD_TOO_SHORT);

			if (_createAccountParameterValidator.PasswordIsTooObvious(parameters.Password))
				throw new CreateAccountParameterException(DATCONST1.ACCOUNT_CREATE_PASSWORD_TOO_OBVIOUS);

			if (_createAccountParameterValidator.EmailIsInvalid(parameters.Email))
				throw new CreateAccountParameterException(DATCONST1.ACCOUNT_CREATE_EMAIL_INVALID);
		}

		private bool AnyFieldsStillEmpty(ICreateAccountParameters parameters)
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