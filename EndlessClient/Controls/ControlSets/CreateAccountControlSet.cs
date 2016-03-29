// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.Controls.ControlSets
{
	public class CreateAccountControlSet : BaseControlSet
	{
		private readonly KeyboardDispatcher _dispatcher;

		private IGameComponent _tbAccountName,
							   _tbPassword,
							   _tbConfirm,
							   _tbRealName,
							   _tbLocation,
							   _tbEmail,
							   _btnCreate,
							   _btnCancel;
		//todo: add the back button

		private TextBoxClickEventHandler _clickHandler;
		private TextBoxTabEventHandler _tabHandler;

		public override GameStates GameState { get { return GameStates.CreateAccount; } }

		public CreateAccountControlSet(KeyboardDispatcher dispatcher)
		{
			_dispatcher = dispatcher;
		}

		protected override void InitializeControlsHelper(IControlSet currentControlSet)
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

			_clickHandler = new TextBoxClickEventHandler(_dispatcher, _allComponents.OfType<XNATextBox>().ToArray());
			_tabHandler = new TextBoxTabEventHandler(_dispatcher, _allComponents.OfType<XNATextBox>().ToArray());
		}

		public override IGameComponent FindComponentByControlIdentifier(GameControlIdentifier control)
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

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_clickHandler.Dispose();
				_tabHandler.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
