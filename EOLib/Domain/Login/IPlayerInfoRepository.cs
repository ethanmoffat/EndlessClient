// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Login
{
	public interface IPlayerInfoRepository
	{
		string LoggedInAccountName { get; set; }

		int PlayerID { get; set; }

		bool IsFirstTimePlayer { get; set; }
	}

	public interface ILoggedInAccountNameProvider
	{
		string LoggedInAccountName { get; }

		int PlayerID { get; }

		bool IsFirstTimePlayer { get; }
	}

	public class PlayerInfoRepository : IPlayerInfoRepository, ILoggedInAccountNameProvider
	{
		public string LoggedInAccountName { get; set; }

		public int PlayerID { get; set; }

		public bool IsFirstTimePlayer { get; set; }
	}
}
