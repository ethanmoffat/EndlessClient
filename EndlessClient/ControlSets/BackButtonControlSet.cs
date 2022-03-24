using System;
using EndlessClient.Content;
using EndlessClient.Controllers;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.ControlSets
{
    public abstract class BackButtonControlSet : BaseControlSet
    {
        protected readonly IMainButtonController _mainButtonController;

        private Texture2D _backButtonTexture;

        private XNAButton _backButton;

        protected BackButtonControlSet(IMainButtonController mainButtonController)
        {
            _mainButtonController = mainButtonController;
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
                new Vector2(589, 0),
                new Rectangle(0, 0, _backButtonTexture.Width, _backButtonTexture.Height / 2),
                new Rectangle(0, _backButtonTexture.Height / 2, _backButtonTexture.Width, _backButtonTexture.Height / 2))
            {
                DrawOrder = 100,
                ClickArea = new Rectangle(4, 16, 16, 16)
            };
            button.OnClick += DoBackButtonClick;

            return button;
        }

        protected virtual void DoBackButtonClick(object sender, EventArgs e)
        {
            _mainButtonController.GoToInitialState();
        }
    }
}
