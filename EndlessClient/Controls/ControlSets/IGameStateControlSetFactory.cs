// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EndlessClient.Controls.ControlSets
{
	public interface IGameStateControlSetFactory
	{
		IControlSet CreateForInitial(IControlSet currentControlSet);

		IControlSet CreateForCreateAccount(IControlSet currentControlSet);

		IControlSet CreateForEnterLoginInfo(IControlSet currentControlSet);

		IControlSet CreateForViewCredits(IControlSet currentControlSet);

		IControlSet CreateForSelectCharacter(IControlSet currentControlSet);

		IControlSet CreateForLoginAsCharacter(IControlSet currentControlSet);
	}
}
