using EndlessClient.Content;
using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EndlessClient.Input;
using EOLib;
using EOLib.Config;
using EOLib.Domain.Login;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Threading.Tasks;
using XNAControls;

namespace EndlessClient.ControlSets
{
    public class LoginPromptControlSet : InitialControlSet
    {
        private readonly KeyboardDispatcher _dispatcher;
        private readonly IMainButtonController _mainButtonController;
        private readonly ILoginController _loginController;

        private IXNATextBox _tbUsername, _tbPassword;
        private XNAButton _btnLogin, _btnCancel;
        private IXNAPictureBox _loginPanelBackground;

        private TextBoxClickEventHandler _clickHandler;
        private TextBoxTabEventHandler _tabHandler;
        private Texture2D _loginBackgroundTexture;

        private Task _loginTask;

        public override GameStates GameState => GameStates.Login;

        public LoginPromptControlSet(KeyboardDispatcher dispatcher,
                                     IConfigurationProvider configProvider,
                                     IMainButtonController mainButtonController,
                                     ILoginController loginController)
            : base(configProvider, mainButtonController)
        {
            _dispatcher = dispatcher;
            _mainButtonController = mainButtonController;
            _loginController = loginController;
        }

        public override void InitializeResources(INativeGraphicsManager gfxManager, IContentProvider contentProvider)
        {
            base.InitializeResources(gfxManager, contentProvider);

            _loginBackgroundTexture = gfxManager.TextureFromResource(GFXTypes.PreLoginUI, 2);
        }

        protected override void InitializeControlsHelper(IControlSet currentControlSet)
        {
            base.InitializeControlsHelper(currentControlSet);

            _loginPanelBackground = GetControl(currentControlSet, GameControlIdentifier.LoginPanelBackground, GetLoginPanelBackground);
            _tbUsername = GetControl(currentControlSet, GameControlIdentifier.LoginAccountName, GetLoginUserNameTextBox);
            _tbPassword = GetControl(currentControlSet, GameControlIdentifier.LoginPassword, GetLoginPasswordTextBox);
            _btnLogin = GetControl(currentControlSet, GameControlIdentifier.LoginButton, GetLoginAccountButton);
            _btnCancel = GetControl(currentControlSet, GameControlIdentifier.LoginCancel, GetLoginCancelButton);

            _allComponents.Add(_loginPanelBackground);
            _allComponents.Add(_tbUsername);
            _allComponents.Add(_tbPassword);
            _allComponents.Add(_btnLogin);
            _allComponents.Add(_btnCancel);

            _clickHandler = new TextBoxClickEventHandler(_dispatcher, _allComponents.OfType<IXNATextBox>().ToArray());
            _tabHandler = new TextBoxTabEventHandler(_dispatcher, _allComponents.OfType<IXNATextBox>().ToArray());

            if (_dispatcher.Subscriber != null)
                _dispatcher.Subscriber.Selected = false;
            _dispatcher.Subscriber = _tbUsername;
            _dispatcher.Subscriber.Selected = true;
        }

        public override IGameComponent FindComponentByControlIdentifier(GameControlIdentifier control)
        {
            switch (control)
            {
                case GameControlIdentifier.LoginPanelBackground: return _loginPanelBackground;
                case GameControlIdentifier.LoginAccountName: return _tbUsername;
                case GameControlIdentifier.LoginPassword: return _tbPassword;
                case GameControlIdentifier.LoginButton: return _btnLogin;
                case GameControlIdentifier.LoginCancel: return _btnCancel;
                default: return base.FindComponentByControlIdentifier(control);
            }
        }

        private IXNAPictureBox GetLoginPanelBackground()
        {
            return new XNAPictureBox
            {
                Texture = _loginBackgroundTexture,
                DrawArea = new Rectangle(266, 291, _loginBackgroundTexture.Width, _loginBackgroundTexture.Height),
                DrawOrder = _personPicture.DrawOrder + 1
            };
        }

        private IXNATextBox GetLoginUserNameTextBox()
        {
            var textBox = new XNATextBox(
                new Rectangle(402, 322, 140, _textBoxBackground.Height),
                Constants.FontSize08,
                _textBoxBackground,
                _textBoxLeft,
                _textBoxRight,
                _textBoxCursor)
            {
                MaxChars = 16,
                DefaultText = "Username",
                LeftPadding = 4,
                DrawOrder = _personPicture.DrawOrder + 2,
                DefaultTextColor = Color.FromNonPremultiplied(80, 80, 80, 0xff),
                TextColor = Color.Black
            };
            textBox.OnEnterPressed += DoLogin;
            return textBox;
        }

        private IXNATextBox GetLoginPasswordTextBox()
        {
            var textBox = new XNATextBox(
                new Rectangle(402, 358, 140, _textBoxBackground.Height),
                Constants.FontSize08,
                _textBoxBackground,
                _textBoxLeft,
                _textBoxRight,
                _textBoxCursor)
            {
                MaxChars = 12,
                PasswordBox = true,
                LeftPadding = 4,
                DefaultText = "Password",
                DrawOrder = _personPicture.DrawOrder + 2,
                DefaultTextColor = Color.FromNonPremultiplied(80, 80, 80, 0xff),
                TextColor = Color.Black
            };
            textBox.OnEnterPressed += DoLogin;
            return textBox;
        }

        private XNAButton GetLoginAccountButton()
        {
            var button = new XNAButton(_smallButtonSheet, new Vector2(361, 389), new Rectangle(0, 0, 91, 29), new Rectangle(91, 0, 91, 29))
            {
                DrawOrder = _personPicture.DrawOrder + 2
            };
            button.OnClick += DoLogin;
            return button;
        }

        private void DoLogin(object sender, EventArgs e)
        {
            if (_loginTask == null)
            {
                var loginParameters = new LoginParameters(_tbUsername.Text, _tbPassword.Text);
                _loginTask = _loginController.LoginToAccount(loginParameters);
                _loginTask.ContinueWith(_ => _loginTask = null);
            }
        }

        private XNAButton GetLoginCancelButton()
        {
            var button = new XNAButton(_smallButtonSheet, new Vector2(453, 389), new Rectangle(0, 29, 91, 29), new Rectangle(91, 29, 91, 29))
            {
                DrawOrder = _personPicture.DrawOrder + 2
            };
            button.OnClick += (o, e) => _mainButtonController.GoToInitialState();
            return button;
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
