// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;

namespace EOLib.Data.AccountCreation
{
	public interface ICreateAccountActions
	{
		CreateAccountParameterResult CheckAccountCreateParameters(ICreateAccountParameters createAccountCreateparameters);

		Task<AccountReply> CheckAccountNameWithServer(string accountName);

		Task ShowAccountCreatePendingDialog();

		Task<AccountReply> CreateAccount(ICreateAccountParameters parameters);
	}
}
