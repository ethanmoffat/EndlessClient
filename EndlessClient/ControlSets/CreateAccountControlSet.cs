// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EndlessClient.Input;
using EndlessClient.Rendering.Sprites;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Domain.Account;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.ControlSets
{
	public class CreateAccountControlSet : IntermediateControlSet
	{
		private readonly ICreateAccountController _createAccountController;

		private Texture2D _labelsTexture;

		private XNATextBox _tbAccountName,
						   _tbPassword,
						   _tbConfirm,
						   _tbRealName,
						   _tbLocation,
						   _tbEmail;

		private XNAButton _btnCancel;

		private XNAPanel _labels;

		private TextBoxClickEventHandler _clickHandler;

		private TextBoxTabEventHandler _tabHandler;

		public override GameStates GameState { get { return GameStates.CreateAccount; } }

		public CreateAccountControlSet(KeyboardDispatcher dispatcher,
									   IMainButtonController mainButtonController,
									   ICreateAccountController createAccountController)
			: base(dispatcher, mainButtonController)
		{
			_createAccountController = createAccountController;
		}

		public override void InitializeResources(INativeGraphicsManager gfxManager, ContentManager xnaContentManager)
		{
			base.InitializeResources(gfxManager, xnaContentManager);

			_labelsTexture = gfxManager.TextureFromResource(GFXTypes.PreLoginUI, 12, true);
		}

		protected override void InitializeControlsHelper(IControlSet currentControlSet)
		{
			_tbAccountName = GetControl(currentControlSet, GameControlIdentifier.CreateAccountName, GetCreateAccountNameTextBox);
			_tbPassword = GetControl(currentControlSet, GameControlIdentifier.CreateAccountPassword, GetCreateAccountPasswordTextBox);
			_tbConfirm = GetControl(currentControlSet, GameControlIdentifier.CreateAccountPasswordConfirm, GetCreateAccountConfirmTextBox);
			_tbRealName = GetControl(currentControlSet, GameControlIdentifier.CreateAccountRealName, GetCreateAccountRealNameTextBox);
			_tbLocation = GetControl(currentControlSet, GameControlIdentifier.CreateAccountLocation, GetCreateAccountLocationTextBox);
			_tbEmail = GetControl(currentControlSet, GameControlIdentifier.CreateAccountEmail, GetCreateAccountEmailTextBox);
			_btnCancel = GetControl(currentControlSet, GameControlIdentifier.CreateAccountCancelButton, GetCreateAccountCancelButton);
			_labels = GetControl(currentControlSet, GameControlIdentifier.CreateAccountLabels, GetCreateAccountLabels);

			_allComponents.Add(_tbAccountName);
			_allComponents.Add(_tbPassword);
			_allComponents.Add(_tbConfirm);
			_allComponents.Add(_tbRealName);
			_allComponents.Add(_tbLocation);
			_allComponents.Add(_tbEmail);
			_allComponents.Add(_btnCancel);
			_allComponents.Add(_labels);

			var textBoxes = _allComponents.OfType<XNATextBox>().ToArray();
			_clickHandler = new TextBoxClickEventHandler(_dispatcher, textBoxes);
			_tabHandler = new TextBoxTabEventHandler(_dispatcher, textBoxes);

			if (_dispatcher.Subscriber != null)
				_dispatcher.Subscriber.Selected = false;
			_dispatcher.Subscriber = _tbAccountName;
			_dispatcher.Subscriber.Selected = true;

			base.InitializeControlsHelper(currentControlSet);
		}

		public override IGameComponent FindComponentByControlIdentifier(GameControlIdentifier control)
		{
			switch (control)
			{
				case GameControlIdentifier.CreateAccountLabels: return _labels;
				case GameControlIdentifier.CreateAccountName: return _tbAccountName;
				case GameControlIdentifier.CreateAccountPassword: return _tbPassword;
				case GameControlIdentifier.CreateAccountPasswordConfirm: return _tbConfirm;
				case GameControlIdentifier.CreateAccountRealName: return _tbRealName;
				case GameControlIdentifier.CreateAccountLocation: return _tbLocation;
				case GameControlIdentifier.CreateAccountEmail: return _tbEmail;
				case GameControlIdentifier.CreateAccountCancelButton: return _btnCancel;
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
				Text = "",
				DefaultText = " "
			};
		}

		private XNAButton GetCreateAccountCancelButton()
		{
			var button = new XNAButton(_secondaryButtonTexture,
									   new Vector2(481, 417),
									   new Rectangle(0, 40, 120, 40),
									   new Rectangle(120, 40, 120, 40));
			button.OnClick += (o, e) => _mainButtonController.GoToInitialState();
			return button;
		}

		private XNAPanel GetCreateAccountLabels()
		{
			var labelsPanel = new XNAPanel();
			for (int srcYIndex = 0; srcYIndex < 6; ++srcYIndex)
			{
				var lblpos = new Vector2(358, (srcYIndex < 3 ? 50 : 241) + srcYIndex % 3 * 51);
				var labelTexture = new SpriteSheet(_labelsTexture, new Rectangle(0, srcYIndex * (srcYIndex < 2 ? 14 : 15), 149, 15));
				var texturePictureBox = new DisposablePictureBox(labelTexture.GetSourceTexture()) { DrawLocation = lblpos };
				labelsPanel.AddControl(texturePictureBox);
			}
			return labelsPanel;
		}

		protected override XNAButton GetCreateButton()
		{
			var button = base.GetCreateButton();
			button.OnClick += DoCreateAccount;
			return button;
		}

		private void DoCreateAccount(object sender, EventArgs e)
		{
			_createAccountController.CreateAccount(new CreateAccountParameters(
				_tbAccountName.Text,
				_tbPassword.Text,
				_tbConfirm.Text,
				_tbRealName.Text,
				_tbLocation.Text,
				_tbEmail.Text));
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
