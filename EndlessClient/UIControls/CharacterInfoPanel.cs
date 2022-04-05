using System;
using System.Threading;
using System.Threading.Tasks;
using EndlessClient.Controllers;
using EndlessClient.Dialogs.Services;
using EndlessClient.Input;
using EndlessClient.Rendering;
using EndlessClient.Rendering.Factories;
using EndlessClient.Rendering.Sprites;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;

namespace EndlessClient.UIControls
{
    public class CharacterInfoPanel : XNAControl
    {
        private readonly INativeGraphicsManager _gfxManager;
        private readonly ICharacter _character;
        private readonly ILoginController _loginController;
        private readonly ICharacterManagementController _characterManagementController;
        private readonly IRendererRepositoryResetter _rendererRepositoryResetter;
        private readonly CharacterControl _characterControl;
        private readonly ISpriteSheet _adminGraphic;
        private readonly IUserInputProvider _userInputProvider;

        private readonly Texture2D _backgroundImage;

        private readonly IXNAButton _loginButton, _deleteButton;
        private readonly IXNALabel _nameLabel, _levelLabel;

        private int _clickRequests;
        private readonly int _characterIndex;

        //top left - 334, 36 + ndx*124
        protected CharacterInfoPanel(int characterIndex,
                                     INativeGraphicsManager gfxManager,
                                     IEODialogButtonService dialogButtonService)
        {
            _characterIndex = characterIndex;
            _gfxManager = gfxManager;
            DrawPosition = new Vector2(334, 36 + characterIndex*124);

            _loginButton = new XNAButton(dialogButtonService.SmallButtonSheet,
                new Vector2(161, 57),
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Login),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Login));
            _loginButton.OnClick += async (o, e) => await LoginButtonClick();
            _loginButton.SetParentControl(this);

            _deleteButton = new XNAButton(dialogButtonService.SmallButtonSheet,
                new Vector2(161, 85),
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Delete),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Delete));
            _deleteButton.OnClick += async (o, e) => await DeleteButtonClick();
            _deleteButton.SetParentControl(this);

            _backgroundImage = _gfxManager.TextureFromResource(GFXTypes.PreLoginUI, 11);
        }

        public CharacterInfoPanel(int characterIndex,
                                  ICharacter character,
                                  INativeGraphicsManager gfxManager,
                                  IEODialogButtonService dialogButtonService,
                                  ILoginController loginController,
                                  ICharacterManagementController characterManagementController,
                                  ICharacterRendererFactory rendererFactory,
                                  IRendererRepositoryResetter rendererRepositoryResetter,
                                  IUserInputProvider userInputProvider)
            : this(characterIndex, gfxManager, dialogButtonService)
        {
            _character = character;
            _loginController = loginController;
            _characterManagementController = characterManagementController;
            _rendererRepositoryResetter = rendererRepositoryResetter;
            _userInputProvider = userInputProvider;

            _characterControl = new CharacterControl(character, rendererFactory)
            {
                DrawPosition = new Vector2(68, 28)
            };
            _characterControl.SetParentControl(this);

            _nameLabel = new XNALabel(Constants.FontSize08pt5)
            {
                DrawArea = GetNameLabelLocation(),
                ForeColor = ColorConstants.BeigeText,
                Text = CapitalizeName(character.Name),
                TextAlign = LabelAlignment.MiddleCenter,
                AutoSize = false
            };
            _nameLabel.SetParentControl(this);

            _levelLabel = new XNALabel(Constants.FontSize09)
            {
                DrawArea = GetLevelLabelLocation(),
                ForeColor = ColorConstants.BeigeText,
                Text = character.Stats.Stats[CharacterStat.Level].ToString()
            };
            _levelLabel.SetParentControl(this);

            _adminGraphic = CreateAdminGraphic(character.AdminLevel);
        }

        public override void Initialize()
        {
            _characterControl.Initialize();

            _loginButton.Initialize();
            _deleteButton.Initialize();
            _nameLabel.Initialize();
            _levelLabel.Initialize();

            base.Initialize();
        }

        protected override bool ShouldUpdate()
        {
            return Visible;
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            DoUpdateLogic(gameTime);
            base.OnUpdateControl(gameTime);
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            _spriteBatch.Begin();
            _spriteBatch.Draw(_backgroundImage, DrawPositionWithParentOffset, Color.White);
            _spriteBatch.End();

            DoDrawLogic(gameTime);

            base.OnDrawControl(gameTime);
        }

        protected virtual async Task LoginButtonClick()
        {
            if (Interlocked.Increment(ref _clickRequests) != 1)
                return;

            try
            {
                _rendererRepositoryResetter.ResetRenderers();

                await _loginController.LoginToCharacter(_character);
            }
            finally
            {
                Interlocked.Exchange(ref _clickRequests, 0);
            }
        }

        protected virtual async Task DeleteButtonClick()
        {
            if (Interlocked.Increment(ref _clickRequests) != 1)
                return;

            try
            {
                await _characterManagementController.DeleteCharacter(_character);
            }
            finally
            {
                Interlocked.Exchange(ref _clickRequests, 0);
            }
        }

        protected virtual void DoUpdateLogic(GameTime gameTime)
        {
            _characterControl.Update(gameTime);

            var previousKeyState = _userInputProvider.PreviousKeyState;
            var currentKeyState = _userInputProvider.CurrentKeyState;
            if (currentKeyState.IsKeyPressedOnce(previousKeyState, Keys.D1 + _characterIndex))
            {
                Task.Run(async () => await LoginButtonClick());
            }
        }

        protected virtual void DoDrawLogic(GameTime gameTime)
        {
            _characterControl.Draw(gameTime);

            if (_adminGraphic.HasTexture && !_spriteBatch.IsDisposed)
            {
                _spriteBatch.Begin();
                _spriteBatch.Draw(_adminGraphic.SheetTexture, GetAdminGraphicLocation(), _adminGraphic.SourceRectangle, Color.White);
                _spriteBatch.End();
            }
        }

        private static Rectangle GetNameLabelLocation()
        {
            return new Rectangle(165, 26, 89, 22);
        }

        private static Rectangle GetLevelLabelLocation()
        {
            return new Rectangle(29, 99, 1, 1);
        }

        private Vector2 GetAdminGraphicLocation()
        {
            return new Vector2(DrawAreaWithParentOffset.X + 109, DrawAreaWithParentOffset.Y + 97);
        }

        private static string CapitalizeName(string name)
        {
            return string.IsNullOrEmpty(name) ? string.Empty : (char)(name[0] - 32) + name.Substring(1);
        }

        private ISpriteSheet CreateAdminGraphic(AdminLevel adminLevel)
        {
            var adminGraphic = _gfxManager.TextureFromResource(GFXTypes.PreLoginUI, 22);
            
            switch (adminLevel)
            {
                case AdminLevel.Player:
                    return new EmptySpriteSheet();
                case AdminLevel.Guide:
                    return new SpriteSheet(adminGraphic, new Rectangle(252, 39, 17, 17));
                case AdminLevel.Guardian:
                case AdminLevel.GM:
                case AdminLevel.HGM:
                    return new SpriteSheet(adminGraphic, new Rectangle(233, 39, 17, 17));
                default:
                    throw new ArgumentOutOfRangeException(nameof(adminLevel), adminLevel, null);
            }
        }
    }

    /// <summary>
    /// This is a no-op class that represents an empty character slot. The buttons don't do anything, and nothing is drawn / updated
    /// </summary>
    public class EmptyCharacterInfoPanel : CharacterInfoPanel
    {
        public EmptyCharacterInfoPanel(int characterIndex,
            INativeGraphicsManager gfxManager,
            IEODialogButtonService dialogButtonService)
            : base(characterIndex, gfxManager, dialogButtonService)
        {
        }

        public override void Initialize()
        {
        }

        protected override void DoUpdateLogic(GameTime gameTime)
        {
        }

        protected override void DoDrawLogic(GameTime gameTime)
        {
        }

        protected override Task LoginButtonClick()
        {
            return Task.FromResult(false);
        }

        protected override Task DeleteButtonClick()
        {
            return Task.FromResult(false);
        }
    }
}
