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

		public CreateAccountParameterResult CheckAccountCreateParameters(ICreateAccountParameters parameters)
		{
			if (AnyFieldsStillEmpty(parameters))
				return new CreateAccountParameterResult(WhichParameter.All, DATCONST1.ACCOUNT_CREATE_FIELDS_STILL_EMPTY);

			if (_createAccountParameterValidator.AccountNameIsNotLongEnough(parameters.AccountName))
				return new CreateAccountParameterResult(WhichParameter.AccountName, DATCONST1.ACCOUNT_CREATE_NAME_TOO_SHORT);

			if (_createAccountParameterValidator.AccountNameIsTooObvious(parameters.AccountName))
				return new CreateAccountParameterResult(WhichParameter.AccountName, DATCONST1.ACCOUNT_CREATE_NAME_TOO_OBVIOUS);

			if (_createAccountParameterValidator.PasswordMismatch(parameters.Password, parameters.ConfirmPassword))
				return new CreateAccountParameterResult(WhichParameter.Confirm, DATCONST1.ACCOUNT_CREATE_PASSWORD_MISMATCH);

			if (_createAccountParameterValidator.PasswordIsTooShort(parameters.Password))
				return new CreateAccountParameterResult(WhichParameter.Password, DATCONST1.ACCOUNT_CREATE_PASSWORD_TOO_SHORT);

			if (_createAccountParameterValidator.PasswordIsTooObvious(parameters.Password))
				return new CreateAccountParameterResult(WhichParameter.Password, DATCONST1.ACCOUNT_CREATE_PASSWORD_TOO_OBVIOUS);

			if (_createAccountParameterValidator.EmailIsInvalid(parameters.Email))
				return new CreateAccountParameterResult(WhichParameter.Email, DATCONST1.ACCOUNT_CREATE_EMAIL_INVALID);

			return new CreateAccountParameterResult(WhichParameter.None);
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