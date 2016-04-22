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
		private XNAButton _changePasswordButton;

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

			_changePasswordButton = GetControl(currentControlSet, GameControlIdentifier.ChangePasswordButton, GetPasswordButton);
			//login panels

			_allComponents.Add(_changePasswordButton);
		}

		public override IGameComponent FindComponentByControlIdentifier(GameControlIdentifier control)
		{
			switch (control)
			{
				case GameControlIdentifier.Character1Panel:
				case GameControlIdentifier.Character2Panel:
				case GameControlIdentifier.Character3Panel: return null;
				case GameControlIdentifier.ChangePasswordButton: return _changePasswordButton;
				default: return base.FindComponentByControlIdentifier(control);
			}
		}

		private XNAButton GetPasswordButton()
		{
			var button = new XNAButton(_secondaryButtonTexture,
				new Vector2(454, 417),
				new Rectangle(0, 120, 120, 40),
				new Rectangle(120, 120, 120, 40));
			//button.OnClick += ...
			return button;
		}

		protected override XNAButton GetCreateButton()
		{
			var button = base.GetCreateButton();
			button.OnClick += DoCreateCharacter;
			return button;
		}

		private void DoCreateCharacter(object sender, EventArgs e)
		{
			
		}
	}
}
