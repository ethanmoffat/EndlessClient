// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;

namespace EOLib.Data.Account
{
	public interface IAccountActions
	{
		CreateAccountParameterResult CheckAccountCreateParameters(ICreateAccountParameters createAccountCreateparameters);

		Task<AccountReply> CheckAccountNameWithServer(string accountName);

		Task<AccountReply> CreateAccount(ICreateAccountParameters parameters);
	}
}
