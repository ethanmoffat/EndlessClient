// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Login
{
	public interface ILoggedInAccountNameRepository
	{
		string LoggedInAccountName { get; set; }
	}

	public interface ILoggedInAccountNameProvider
	{
		string LoggedInAccountName { get; }
	}

	public class LoggedInAccountNameRepository : ILoggedInAccountNameRepository, ILoggedInAccountNameProvider
	{
		public string LoggedInAccountName { get; set; }
	}
}
