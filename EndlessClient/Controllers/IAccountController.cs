// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;
using EOLib.Domain.Account;

namespace EndlessClient.Controllers
{
	public interface IAccountController
	{
		Task CreateAccount(ICreateAccountParameters createAccountParameters);

		Task ChangePassword();
	}
}
