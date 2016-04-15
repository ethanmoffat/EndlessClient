// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using EndlessClient.GameExecution;
using EndlessClient.Input;
using EOLib;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.ControlSets
{
	public class CreateAccountControlSet : BaseControlSet
	{
		private readonly KeyboardDispatcher _dispatcher;

		private XNATextBox _tbAccountName,
						   _tbPassword,
						   _tbConfirm,
						   _tbRealName,
						   _tbLocation,
						   _tbEmail;
		private XNAButton _btnCreate,
						  _btnCancel;

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
				case GameControlIdentifier.CreateAccountLabels: return null; //todo
				case GameControlIdentifier.CreateAccountName: return _tbAccountName;
				case GameControlIdentifier.CreateAccountPassword: return _tbPassword;
				case GameControlIdentifier.CreateAccountPasswordConfirm: return _tbConfirm;
				case GameControlIdentifier.CreateAccountRealName: return _tbRealName;
				case GameControlIdentifier.CreateAccountLocation: return _tbLocation;
				case GameControlIdentifier.CreateAccountEmail: return _tbEmail;
				case GameControlIdentifier.CreateAccountButton: return _btnCreate;
				case GameControlIdentifier.CreateAccountCancelButton: return _btnCancel;
				case GameControlIdentifier.BackButton: return null; //todo
				default: return base.FindComponentByControlIdentifier(control);
			}
		}

		private XNATextBox GetCreateAccountNameTextBox()
		{
			var tb = AccountInputTextBoxCreationHelper(GameControlIdentifier.CreateAccountName);
			tb.MaxChars = 16;
			return tb;
		}

		private XNATextBox GetCreateAccountPasswordTextBox()
		{
			var tb = AccountInputTextBoxCreationHelper(GameControlIdentifier.CreateAccountPassword);
			tb.PasswordBox = true;
			tb.MaxChars = 12;
			return tb;
		}

		private XNATextBox GetCreateAccountConfirmTextBox()
		{
			var tb = AccountInputTextBoxCreationHelper(GameControlIdentifier.CreateAccountPasswordConfirm);
			tb.PasswordBox = true;
			tb.MaxChars = 12;
			return tb;
		}

		private XNATextBox GetCreateAccountRealNameTextBox()
		{
			return AccountInputTextBoxCreationHelper(GameControlIdentifier.CreateAccountRealName);
		}

		private XNATextBox GetCreateAccountLocationTextBox()
		{
			return AccountInputTextBoxCreationHelper(GameControlIdentifier.CreateAccountLocation);
		}

		private XNATextBox GetCreateAccountEmailTextBox()
		{
			return AccountInputTextBoxCreationHelper(GameControlIdentifier.CreateAccountEmail);
		}

		private XNATextBox AccountInputTextBoxCreationHelper(GameControlIdentifier whichControl)
		{
			int i;
			switch (whichControl)
			{
				case GameControlIdentifier.CreateAccountName: i = 0; break;
				case GameControlIdentifier.CreateAccountPassword: i = 1; break;
				case GameControlIdentifier.CreateAccountPasswordConfirm: i = 2; break;
				case GameControlIdentifier.CreateAccountRealName: i = 3; break;
				case GameControlIdentifier.CreateAccountLocation: i = 4; break;
				case GameControlIdentifier.CreateAccountEmail: i = 5; break;
				default: throw new ArgumentException("Invalid control specified for helper", "whichControl");
			}

			//set the first  3 Y coord to start at 69  and move up by 51 each time
			//set the second 3 Y coord to start at 260 and move up by 51 each time
			var txtYCoord = (i < 3 ? 69 : 260) + i % 3 * 51;
			var drawArea = new Rectangle(358, txtYCoord, 240, _textBoxTextures[0].Height);
			return new XNATextBox(drawArea, _textBoxTextures, Constants.FontSize08)
			{
				LeftPadding = 4,
				MaxChars = 35,
				DefaultText = " "
			};
		}

		private XNAButton GetCreateButton(bool isCreateCharacterButton)
		{
			return new XNAButton(_secondaryButtonTexture,
								 new Vector2(isCreateCharacterButton ? 334 : 359, 417),
								 new Rectangle(0, 0, 120, 40),
								 new Rectangle(120, 0, 120, 40));
		}

		private XNAButton GetCreateAccountCancelButton()
		{
			return new XNAButton(_secondaryButtonTexture,
								 new Vector2(481, 417),
								 new Rectangle(0, 40, 120, 40),
								 new Rectangle(120, 40, 120, 40));
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
