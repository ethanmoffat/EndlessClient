// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EndlessClient.Controls.ControlSets
{
	public interface IGameStateControlSetFactory
	{
		IGameStateControlSet CreateForInitial(IGameStateControlSet currentControlSet);

		IGameStateControlSet CreateForCreateAccount(IGameStateControlSet currentControlSet);

		IGameStateControlSet CreateForEnterLoginInfo(IGameStateControlSet currentControlSet);

		IGameStateControlSet CreateForViewCredits(IGameStateControlSet currentControlSet);

		IGameStateControlSet CreateForSelectCharacter(IGameStateControlSet currentControlSet);

		IGameStateControlSet CreateForLoginAsCharacter(IGameStateControlSet currentControlSet);
	}
}
