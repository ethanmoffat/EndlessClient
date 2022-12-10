﻿using EndlessClient.Content;
using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EndlessClient.Input;
using EOLib;
using EOLib.Domain.Account;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Threading.Tasks;
using XNAControls;

namespace EndlessClient.ControlSets
{
    public class CreateAccountControlSet : IntermediateControlSet
    {
        private readonly IAccountController _accountController;

        private Texture2D _labelsTexture;

        private IXNATextBox _tbAccountName,
                            _tbPassword,
                            _tbConfirm,
                            _tbRealName,
                            _tbLocation,
                            _tbEmail;
        private IXNAButton _btnCancel;
        private IXNAPanel _labels;

        private TextBoxClickEventHandler _clickHandler;
        private TextBoxTabEventHandler _tabHandler;

        private Task _createAccountTask;

        public override GameStates GameState => GameStates.CreateAccount;

        public CreateAccountControlSet(KeyboardDispatcher dispatcher,
                                       IMainButtonController mainButtonController,
                                       IAccountController accountController)
            : base(dispatcher, mainButtonController)
        {
            _accountController = accountController;
        }

        public override void InitializeResources(INativeGraphicsManager gfxManager, IContentProvider contentProvider)
        {
            base.InitializeResources(gfxManager, contentProvider);

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

            var textBoxes = _allComponents.OfType<IXNATextBox>().ToArray();
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

        private IXNATextBox GetCreateAccountNameTextBox()
        {
            var tb = AccountInputTextBoxCreationHelper(GameControlIdentifier.CreateAccountName);
            tb.MaxChars = 16;
            return tb;
        }

        private IXNATextBox GetCreateAccountPasswordTextBox()
        {
            var tb = AccountInputTextBoxCreationHelper(GameControlIdentifier.CreateAccountPassword);
            tb.PasswordBox = true;
            tb.MaxChars = 12;
            return tb;
        }

        private IXNATextBox GetCreateAccountConfirmTextBox()
        {
            var tb = AccountInputTextBoxCreationHelper(GameControlIdentifier.CreateAccountPasswordConfirm);
            tb.PasswordBox = true;
            tb.MaxChars = 12;
            return tb;
        }

        private IXNATextBox GetCreateAccountRealNameTextBox()
        {
            return AccountInputTextBoxCreationHelper(GameControlIdentifier.CreateAccountRealName);
        }

        private IXNATextBox GetCreateAccountLocationTextBox()
        {
            return AccountInputTextBoxCreationHelper(GameControlIdentifier.CreateAccountLocation);
        }

        private IXNATextBox GetCreateAccountEmailTextBox()
        {
            return AccountInputTextBoxCreationHelper(GameControlIdentifier.CreateAccountEmail);
        }

        private IXNATextBox AccountInputTextBoxCreationHelper(GameControlIdentifier whichControl)
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
                default: throw new ArgumentException("Invalid control specified for helper", nameof(whichControl));
            }

            //set the first  3 Y coord to start at 69  and move up by 51 each time
            //set the second 3 Y coord to start at 260 and move up by 51 each time
            var txtYCoord = (i < 3 ? 69 : 260) + i % 3 * 51;
            var drawArea = new Rectangle(358, txtYCoord, 240, _textBoxBackground.Height);
            return new XNATextBox(drawArea,
                Constants.FontSize08,
                _textBoxBackground,
                _textBoxLeft,
                _textBoxRight,
                _textBoxCursor)
            {
                LeftPadding = 4,
                MaxChars = 35,
                Text = "",
                DefaultText = " "
            };
        }

        private IXNAButton GetCreateAccountCancelButton()
        {
            var button = new XNAButton(_secondaryButtonTexture,
                                       new Vector2(481, 417),
                                       new Rectangle(0, 40, 120, 40),
                                       new Rectangle(120, 40, 120, 40));
            button.OnClick += (o, e) => _mainButtonController.GoToInitialState();
            return button;
        }

        private IXNAPanel GetCreateAccountLabels()
        {
            var labelsPanel = new XNAPanel();
            for (int srcYIndex = 0; srcYIndex < 6; ++srcYIndex)
            {
                var texturePictureBox = new XNAPictureBox
                {
                    Texture = _labelsTexture,
                    SourceRectangle = new Rectangle(0, srcYIndex * (srcYIndex < 2 ? 14 : 15), 149, 15),
                    DrawPosition = new Vector2(430, (srcYIndex < 3 ? 50 : 241) + 10 + srcYIndex % 3 * 51)
                };
                texturePictureBox.SetParentControl(labelsPanel);
            }
            return labelsPanel;
        }

        protected override IXNAButton GetCreateButton()
        {
            var button = base.GetCreateButton();
            button.OnClick += DoCreateAccount;
            return button;
        }

        private void DoCreateAccount(object sender, EventArgs e)
        {
            if (_createAccountTask == null)
            {
                _createAccountTask = _accountController.CreateAccount(
                    new CreateAccountParameters(_tbAccountName.Text,
                                                _tbPassword.Text,
                                                _tbConfirm.Text,
                                                _tbRealName.Text,
                                                _tbLocation.Text,
                                                _tbEmail.Text));
                _createAccountTask.ContinueWith(_ => _createAccountTask = null);
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
