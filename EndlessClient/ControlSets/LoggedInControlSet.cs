// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EndlessClient.UIControls;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.ControlSets
{
	public class LoggedInControlSet : IntermediateControlSet
	{
		private readonly ICharacterInfoPanelFactory _characterInfoPanelFactory;
		private readonly List<CharacterInfoPanel> _characterInfoPanels;

		private XNAButton _changePasswordButton;

		public override GameStates GameState
		{
			get { return GameStates.LoggedIn; }
		}

		public LoggedInControlSet(KeyboardDispatcher dispatcher,
								  IMainButtonController mainButtonController,
								  ICharacterInfoPanelFactory characterInfoPanelFactory)
			: base(dispatcher, mainButtonController)
		{
			_characterInfoPanelFactory = characterInfoPanelFactory;
			_characterInfoPanels = new List<CharacterInfoPanel>();
		}

		protected override void InitializeControlsHelper(IControlSet currentControlSet)
		{
			base.InitializeControlsHelper(currentControlSet);

			_changePasswordButton = GetControl(currentControlSet, GameControlIdentifier.ChangePasswordButton, GetPasswordButton);
			_characterInfoPanels.AddRange(_characterInfoPanelFactory.CreatePanels());

			_allComponents.Add(_changePasswordButton);
			_allComponents.AddRange(_characterInfoPanels);
		}

		public override IGameComponent FindComponentByControlIdentifier(GameControlIdentifier control)
		{
			switch (control)
			{
				case GameControlIdentifier.Character1Panel: return _characterInfoPanels[0];
				case GameControlIdentifier.Character2Panel: return _characterInfoPanels[1];
				case GameControlIdentifier.Character3Panel: return _characterInfoPanels[2];
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
			//todo: button.OnClick += ...
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
			//todo:
		}
	}
}
