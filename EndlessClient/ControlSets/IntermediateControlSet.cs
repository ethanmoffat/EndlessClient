using System;
using EndlessClient.Content;
using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EndlessClient.Rendering;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.ControlSets
{
    public abstract class IntermediateControlSet : BackButtonControlSet
    {
        private readonly Texture2D[] _personSet2;
        private readonly Random _randomGen;

        private IXNAButton _btnCreate;
        private IXNAPictureBox _person2Picture;

        protected IntermediateControlSet(IMainButtonController mainButtonController,
                                         IClientWindowSizeRepository clientWindowSizeRepository)
            : base(mainButtonController, clientWindowSizeRepository)
        {
            _personSet2 = new Texture2D[8];
            _randomGen = new Random();
        }

        public override void InitializeResources(INativeGraphicsManager gfxManager, IContentProvider contentProvider)
        {
            base.InitializeResources(gfxManager, contentProvider);

            for (int i = 0; i < _personSet2.Length; ++i)
                _personSet2[i] = gfxManager.TextureFromResource(GFXTypes.PreLoginUI, 61 + i, true);
        }

        protected override void InitializeControlsHelper(IControlSet currentControlSet)
        {
            _btnCreate = GetControl(currentControlSet,
                GameState == GameStates.LoggedIn ? GameControlIdentifier.CreateCharacterButton : GameControlIdentifier.CreateAccountButton,
                GetCreateButton);
            _person2Picture = GetControl(currentControlSet, GameControlIdentifier.PersonDisplay2, GetPerson2Picture);

            _allComponents.Add(_btnCreate);
            _allComponents.Add(_person2Picture);

            base.InitializeControlsHelper(currentControlSet);
        }

        public override IGameComponent FindComponentByControlIdentifier(GameControlIdentifier control)
        {
            switch (control)
            {
                case GameControlIdentifier.CreateAccountButton:
                    return GameState == GameStates.CreateAccount ? _btnCreate : null;
                case GameControlIdentifier.CreateCharacterButton:
                    return GameState == GameStates.LoggedIn ? _btnCreate : null;
                case GameControlIdentifier.PersonDisplay2: return _person2Picture;
                default: return base.FindComponentByControlIdentifier(control);
            }
        }

        protected virtual IXNAButton GetCreateButton()
        {
            var isCreateCharacterButton = GameState == GameStates.LoggedIn;
            var button = new XNAButton(_secondaryButtonTexture,
                                       new Vector2(isCreateCharacterButton ? 334 : 359, 417),
                                       new Rectangle(0, 0, 120, 40),
                                       new Rectangle(120, 0, 120, 40));
            return button;
        }

        private IXNAPictureBox GetPerson2Picture()
        {
            var texture = _personSet2[_randomGen.Next(8)];
            return new XNAPictureBox
            {
                Texture = texture,
                DrawArea = new Rectangle(43, 140, texture.Width, texture.Height)
            };
        }
    }
}
