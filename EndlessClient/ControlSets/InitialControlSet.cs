using EndlessClient.Content;
using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EOLib;
using EOLib.Config;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading.Tasks;
using XNAControls;

namespace EndlessClient.ControlSets
{
    public class InitialControlSet : BaseControlSet
    {
        private readonly IConfigurationProvider _configProvider;
        private readonly IMainButtonController _mainButtonController;

        private IXNAButton _createAccount,
                          _login,
                          _viewCredits,
                          _exitGame;
        private IXNALabel _versionInfo;

        protected IXNAPictureBox _personPicture;

        private readonly Texture2D[] _personSet1;
        private readonly Random _randomGen;

        private Task _mainButtonClickTask;

        public override GameStates GameState => GameStates.Initial;

        public InitialControlSet(IConfigurationProvider configProvider,
                                 IMainButtonController mainButtonController)
        {
            _configProvider = configProvider;
            _mainButtonController = mainButtonController;
            _personSet1 = new Texture2D[4];
            _randomGen = new Random();
        }

        public override void InitializeResources(INativeGraphicsManager gfxManager, IContentProvider contentProvider)
        {
            base.InitializeResources(gfxManager, contentProvider);

            for (int i = 0; i < _personSet1.Length; ++i)
                _personSet1[i] = gfxManager.TextureFromResource(GFXTypes.PreLoginUI, 41 + i, true);
        }

        protected override void InitializeControlsHelper(IControlSet currentControlSet)
        {
            _createAccount = GetControl(currentControlSet, GameControlIdentifier.InitialCreateAccount, GetMainCreateAccountButton);
            _login = GetControl(currentControlSet, GameControlIdentifier.InitialLogin, GetMainLoginButton);
            _viewCredits = GetControl(currentControlSet, GameControlIdentifier.InitialViewCredits, GetViewCreditsButton);
            _exitGame = GetControl(currentControlSet, GameControlIdentifier.InitialExitGame, GetExitButton);
            _versionInfo = GetControl(currentControlSet, GameControlIdentifier.InitialVersionLabel, GetVersionInfoLabel);
            _personPicture = GetControl(currentControlSet, GameControlIdentifier.PersonDisplay1, GetPersonPicture1);

            _allComponents.Add(_createAccount);
            _allComponents.Add(_login);
            _allComponents.Add(_viewCredits);
            _allComponents.Add(_exitGame);
            _allComponents.Add(_versionInfo);
            _allComponents.Add(_personPicture);
        }

        public override IGameComponent FindComponentByControlIdentifier(GameControlIdentifier control)
        {
            switch (control)
            {
                case GameControlIdentifier.InitialCreateAccount: return _createAccount;
                case GameControlIdentifier.InitialLogin: return _login;
                case GameControlIdentifier.InitialViewCredits: return _viewCredits;
                case GameControlIdentifier.InitialExitGame: return _exitGame;
                case GameControlIdentifier.InitialVersionLabel: return _versionInfo;
                case GameControlIdentifier.PersonDisplay1: return _personPicture;
                default: return base.FindComponentByControlIdentifier(control);
            }
        }

        private IXNAButton GetMainCreateAccountButton()
        {
            var button = MainButtonCreationHelper(GameControlIdentifier.InitialCreateAccount);
            button.OnClick += (o, e) => AsyncMainButtonClick(_mainButtonController.ClickCreateAccount);
            return button;
        }

        private IXNAButton GetMainLoginButton()
        {
            var button = MainButtonCreationHelper(GameControlIdentifier.InitialLogin);
            button.OnClick += (o, e) => AsyncMainButtonClick(_mainButtonController.ClickLogin);
            return button;
        }

        private IXNAButton GetViewCreditsButton()
        {
            var button = MainButtonCreationHelper(GameControlIdentifier.InitialViewCredits);
            button.OnClick += (o, e) => _mainButtonController.ClickViewCredits();
            return button;
        }

        private IXNAButton GetExitButton()
        {
            var button = MainButtonCreationHelper(GameControlIdentifier.InitialExitGame);
            button.OnClick += (o, e) => _mainButtonController.ClickExit();
            return button;
        }

        private void AsyncMainButtonClick(Func<Task> clickHandler)
        {
            if (_mainButtonClickTask == null)
            {
                _mainButtonClickTask = clickHandler();
                _mainButtonClickTask.ContinueWith(_ => _mainButtonClickTask = null);
            }
        }

        private IXNAButton MainButtonCreationHelper(GameControlIdentifier whichControl)
        {
            int i;
            switch (whichControl)
            {
                case GameControlIdentifier.InitialCreateAccount: i = 0; break;
                case GameControlIdentifier.InitialLogin: i = 1; break;
                case GameControlIdentifier.InitialViewCredits: i = 2; break;
                case GameControlIdentifier.InitialExitGame: i = 3; break;
                default: throw new ArgumentException("Invalid control specified for helper", nameof(whichControl));
            }

            var widthFactor = _mainButtonTexture.Width / 2;
            var heightFactor = _mainButtonTexture.Height / 4;
            var outSource = new Rectangle(0, i * heightFactor, widthFactor, heightFactor);
            var overSource = new Rectangle(widthFactor, i * heightFactor, widthFactor, heightFactor);

            return new XNAButton(_mainButtonTexture, new Vector2(25, 280 + i * 40), outSource, overSource)
            {
                //DrawOrder = i + 1
            };
        }

        private IXNALabel GetVersionInfoLabel()
        {
            return new XNALabel(Constants.FontSize07)
            {
                AutoSize = true,
                Text = string.Format(Constants.VersionInfoFormat,
                                     _configProvider.VersionMajor,
                                     _configProvider.VersionMinor,
                                     _configProvider.VersionBuild,
                                     _configProvider.Host,
                                     _configProvider.Port),
                ForeColor = Color.FromNonPremultiplied(190, 170, 150, 255),
                DrawArea = new Rectangle(28, 457, 1, 1)
            };
        }

        private IXNAPictureBox GetPersonPicture1()
        {
            var texture = _personSet1[_randomGen.Next(4)];
         

            return new XNAPictureBox
            {
                Texture = texture,
                DrawArea = new Rectangle(229, 70, texture.Width, texture.Height)
            };
        }

        protected void ExcludePersonPicture1()
        {
            _personPicture.Dispose();
            _allComponents.Remove(_personPicture);
        }
    }
}
