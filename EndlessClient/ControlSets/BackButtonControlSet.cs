using EndlessClient.Content;
using EndlessClient.Controllers;
using EndlessClient.Rendering;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using XNAControls;

namespace EndlessClient.ControlSets
{
    public abstract class BackButtonControlSet : BaseControlSet
    {
        protected readonly IMainButtonController _mainButtonController;
        private readonly IClientWindowSizeRepository _clientWindowSizeRepository;
        private Texture2D _backButtonTexture;

        private XNAButton _backButton;

        protected BackButtonControlSet(IMainButtonController mainButtonController,
                                       IClientWindowSizeRepository clientWindowSizeRepository)
        {
            _mainButtonController = mainButtonController;
            _clientWindowSizeRepository = clientWindowSizeRepository;
        }

        public override void InitializeResources(INativeGraphicsManager gfxManager, IContentProvider contentProvider)
        {
            base.InitializeResources(gfxManager, contentProvider);

            _backButtonTexture = gfxManager.TextureFromResource(GFXTypes.PreLoginUI, 24, true);
        }

        protected override void InitializeControlsHelper(IControlSet currentControlSet)
        {
            _backButton = GetControl(currentControlSet, GameControlIdentifier.BackButton, GetBackButton);

            _allComponents.Add(_backButton);
        }

        private XNAButton GetBackButton()
        {
            var button = new XNAButton(
                _backButtonTexture,
                new Vector2(_clientWindowSizeRepository.Width - _backButtonTexture.Width, 0),
                new Rectangle(0, 0, _backButtonTexture.Width, _backButtonTexture.Height / 2),
                new Rectangle(0, _backButtonTexture.Height / 2, _backButtonTexture.Width, _backButtonTexture.Height / 2))
            {
                UpdateOrder = -1,
                DrawOrder = 100,
                ClickArea = new Rectangle(4, 16, 16, 16)
            };
            button.OnClick += DoBackButtonClick;

            _clientWindowSizeRepository.GameWindowSizeChanged += (o, e) =>
            {
                button.DrawPosition = new Vector2(_clientWindowSizeRepository.Width - _backButtonTexture.Width, 0);
            };

            return button;
        }

        protected virtual void DoBackButtonClick(object sender, EventArgs e)
        {
            _mainButtonController.GoToInitialState();
        }
    }
}