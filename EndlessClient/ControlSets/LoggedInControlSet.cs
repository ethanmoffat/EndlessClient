// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using XNAControls;

namespace EndlessClient.ControlSets
{
	public class LoggedInControlSet : IntermediateControlSet
	{
		public override GameStates GameState
		{
			get { return GameStates.LoggedIn; }
		}

		public LoggedInControlSet(KeyboardDispatcher dispatcher,
								  IMainButtonController mainButtonController)
			: base(dispatcher, mainButtonController)
		{
		}
	}
}
