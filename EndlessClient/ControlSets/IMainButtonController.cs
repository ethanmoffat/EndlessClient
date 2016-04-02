// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;

namespace EndlessClient.ControlSets
{
	public interface IMainButtonController
	{
		void GoToInitialState();

		Task ClickCreateAccount();

		Task ClickLogin();

		void ClickViewCredits();

		void ClickExit();
	}
}
