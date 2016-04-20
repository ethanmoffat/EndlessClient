// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EndlessClient.Controllers
{
	public interface ICreateAccountControllerRepository
	{
		ICreateAccountController CreateAccountController { get; set; }
	}

	public interface ICreateAccountControllerProvider
	{
		ICreateAccountController CreateAccountController { get; }
	}

	public class CreateAccountControllerRepository : ICreateAccountControllerRepository, ICreateAccountControllerProvider
	{
		public ICreateAccountController CreateAccountController { get; set; }
	}
}
