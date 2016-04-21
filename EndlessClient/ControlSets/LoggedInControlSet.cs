// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using Microsoft.Xna.Framework;
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

		protected override void InitializeControlsHelper(IControlSet currentControlSet)
		{
			base.InitializeControlsHelper(currentControlSet);

			//login panels
			//password change button
		}

		public override IGameComponent FindComponentByControlIdentifier(GameControlIdentifier control)
		{
			switch (control)
			{
				case GameControlIdentifier.Character1Panel:
				case GameControlIdentifier.Character2Panel:
				case GameControlIdentifier.Character3Panel:
				case GameControlIdentifier.ChangePasswordButton: return null;
				default: return base.FindComponentByControlIdentifier(control);
			}
		}

		protected override XNAButton GetCreateButton(bool isCreateCharacterButton)
		{
			var button = base.GetCreateButton(isCreateCharacterButton);
			button.OnClick += DoCreateCharacter;
			return button;
		}

		private void DoCreateCharacter(object sender, EventArgs e)
		{
			
		}
	}
}
