// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework;

namespace EndlessClient.Controls.ControlSets
{
	public class CreateAccountControlSet : BaseGameStateControlSet, IGameStateControlSet
	{
		private IGameComponent _tbAccountName,
							   _tbPassword,
							   _tbConfirm,
							   _tbRealName,
							   _tbLocation,
							   _tbEmail,
							   _btnCreate,
							   _btnCancel;
		//todo: add the back button

		public GameStates GameState { get { return GameStates.CreateAccount; } }

		public override void InitializeControls(IGameStateControlSet currentControlSet)
		{
			_tbAccountName = GetControl(currentControlSet, GameControlIdentifier.CreateAccountName, GetCreateAccountNameTextBox);
			_tbPassword = GetControl(currentControlSet, GameControlIdentifier.CreateAccountPassword, GetCreateAccountPasswordTextBox);
			_tbConfirm = GetControl(currentControlSet, GameControlIdentifier.CreateAccountPasswordConfirm, GetCreateAccountConfirmTextBox);
			_tbRealName = GetControl(currentControlSet, GameControlIdentifier.CreateAccountRealName, GetCreateAccountRealNameTextBox);
			_tbLocation = GetControl(currentControlSet, GameControlIdentifier.CreateAccountLocation, GetCreateAccountLocationTextBox);
			_tbEmail = GetControl(currentControlSet, GameControlIdentifier.CreateAccountEmail, GetCreateAccountEmailTextBox);
			_btnCreate = GetControl(currentControlSet, GameControlIdentifier.CreateAccountButton, () => GetCreateButton(false));
			_btnCancel = GetControl(currentControlSet, GameControlIdentifier.CreateAccountCancelButton, GetCreateAccountCancelButton);

			_allComponents.Add(_tbAccountName);
			_allComponents.Add(_tbPassword);
			_allComponents.Add(_tbConfirm);
			_allComponents.Add(_tbRealName);
			_allComponents.Add(_tbLocation);
			_allComponents.Add(_tbEmail);
			_allComponents.Add(_btnCreate);
			_allComponents.Add(_btnCancel);
		}

		public IGameComponent FindComponentByControlIdentifier(GameControlIdentifier control)
		{
			switch (control)
			{
				//labels are a texture; will eventually be a control
				case GameControlIdentifier.CreateAccountLabels: return null;
				case GameControlIdentifier.CreateAccountName: return _tbAccountName;
				case GameControlIdentifier.CreateAccountPassword: return _tbPassword;
				case GameControlIdentifier.CreateAccountPasswordConfirm: return _tbConfirm;
				case GameControlIdentifier.CreateAccountRealName: return _tbRealName;
				case GameControlIdentifier.CreateAccountLocation: return _tbLocation;
				case GameControlIdentifier.CreateAccountEmail: return _tbEmail;
				case GameControlIdentifier.CreateAccountButton: return _btnCreate;
				case GameControlIdentifier.CreateAccountCancelButton: return _btnCancel;
				default: return null;
			}
		}
	}
}
