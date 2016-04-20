// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;
using EOLib.Data.Account;

namespace EndlessClient.Controllers
{
	public interface ICreateAccountController
	{
		Task CreateAccount(ICreateAccountParameters createAccountParameters);
	}
}
