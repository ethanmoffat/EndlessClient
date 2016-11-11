// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Threading;
using System.Threading.Tasks;
using EndlessClient.Controllers;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Factories;
using EndlessClient.Rendering.Sprites;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.UIControls
{
    public class CharacterInfoPanel : XNAControl
    {
        private readonly INativeGraphicsManager _gfxManager;
        private readonly ICharacter _character;
        private readonly ILoginController _loginController;
        private readonly ICharacterManagementController _characterManagementController;
        private readonly ICharacterRendererResetter _characterRendererResetter;
        private readonly ICharacterStateCache _characterStateCache;
        private readonly CharacterControl _characterControl;
        private readonly ISpriteSheet _adminGraphic;

        private readonly Texture2D _backgroundImage;

        private int _clickRequests;

        //top left - 334, 36 + ndx*124
        protected CharacterInfoPanel(int characterIndex, INativeGraphicsManager gfxManager)
        {
            _gfxManager = gfxManager;
            DrawLocation = new Vector2(334, 36 + characterIndex*124);

            var smallButtonTextures = _gfxManager.TextureFromResource(GFXTypes.PreLoginUI, 15, true);

            var loginButton = new XNAButton(smallButtonTextures,
                new Vector2(161, 57),
                new Rectangle(0, 58, 91, 29),
                new Rectangle(91, 58, 91, 29));
            loginButton.OnClick += async (o, e) => await LoginButtonClick();
            loginButton.SetParent(this);

            var deleteButton = new XNAButton(smallButtonTextures,
                new Vector2(161, 85),
                new Rectangle(0, 87, 91, 29),
                new Rectangle(91, 87, 91, 29));
            deleteButton.OnClick += async (o, e) => await DeleteButtonClick();
            deleteButton.SetParent(this);

            _backgroundImage = _gfxManager.TextureFromResource(GFXTypes.PreLoginUI, 11);
        }

        public CharacterInfoPanel(int characterIndex,
                                  ICharacter character,
                                  INativeGraphicsManager gfxManager,
                                  ILoginController loginController,
                                  ICharacterManagementController characterManagementController,
                                  ICharacterRendererFactory rendererFactory,
                                  ICharacterRendererResetter characterRendererResetter,
                                  ICharacterStateCache characterStateCache)
            : this(characterIndex, gfxManager)
        {
            _character = character;
            _loginController = loginController;
            _characterManagementController = characterManagementController;
            _characterRendererResetter = characterRendererResetter;
            _characterStateCache = characterStateCache;

            _characterControl = new CharacterControl(character.RenderProperties, rendererFactory)
            {
                DrawLocation = new Vector2(61, 24)
            };
            _characterControl.SetParent(this);

            var nameLabel = new XNALabel(GetNameLabelLocation(), Constants.FontSize08pt5)
            {
                ForeColor = ColorConstants.BeigeText,
                Text = CapitalizeName(character.Name),
                TextAlign = LabelAlignment.MiddleCenter,
                AutoSize = false
            };
            nameLabel.SetParent(this);

            var levelLabel = new XNALabel(GetLevelLabelLocation(), Constants.FontSize08pt75)
            {
                ForeColor = ColorConstants.BeigeText,
                Text = character.Stats.Stats[CharacterStat.Level].ToString()
            };
            levelLabel.SetParent(this);

            _adminGraphic = CreateAdminGraphic(character.AdminLevel);
        }

        public override void Initialize()
        {
            _characterControl.Initialize();

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            if (!ShouldUpdate())
                return;

            DoUpdateLogic(gameTime);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!Visible)
                return;

            SpriteBatch.Begin();
            SpriteBatch.Draw(_backgroundImage, DrawLocation, Color.White);
            SpriteBatch.End();

            DoDrawLogic(gameTime);

            base.Draw(gameTime);
        }

        protected virtual async Task LoginButtonClick()
        {
            if (Interlocked.Increment(ref _clickRequests) != 1)
                return;

            try
            {
                _characterRendererResetter.ResetRenderers();
                _characterStateCache.Reset();

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
        }

        protected virtual void DoDrawLogic(GameTime gameTime)
        {
            _characterControl.Draw(gameTime);

            if (_adminGraphic.HasTexture)
            {
                SpriteBatch.Begin();
                SpriteBatch.Draw(_adminGraphic.SheetTexture, GetAdminGraphicLocation(), _adminGraphic.SourceRectangle, Color.White);
                SpriteBatch.End();
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
            return new Vector2(DrawAreaWithOffset.X + 109, DrawAreaWithOffset.Y + 97);
        }

        private static string CapitalizeName(string name)
        {
            return (char)(name[0] - 32) + name.Substring(1);
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
                    throw new ArgumentOutOfRangeException("adminLevel", adminLevel, null);
            }
        }
    }

    /// <summary>
    /// This is a no-op class that represents an empty character slot. The buttons don't do anything, and nothing is drawn / updated
    /// </summary>
    public class EmptyCharacterInfoPanel : CharacterInfoPanel
    {
        public EmptyCharacterInfoPanel(int characterIndex, INativeGraphicsManager gfxManager)
            : base(characterIndex, gfxManager)
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
