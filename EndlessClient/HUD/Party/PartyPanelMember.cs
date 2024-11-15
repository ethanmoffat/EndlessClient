using System;
using EndlessClient.Content;
using EOLib.Domain.Chat;
using EOLib.Domain.Party;
using EOLib.Extensions;
using EOLib.Graphics;
using EOLib.Shared;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.HUD.Party
{
    public class PartyPanelMember : XNAControl
    {
        private readonly IContentProvider _contentProvider;
        private readonly Texture2D _chatIconsTexture;

        private readonly IXNAButton _removeButton;
        private readonly IXNALabel _nameLabel, _levelLabel, _hpLabel;

        private ChatIcon _playerIcon;
        private Rectangle _iconSource;

        private int _displayIndex;
        public int DisplayIndex
        {
            get => _displayIndex;
            set
            {
                _displayIndex = value;

                DrawArea = new Rectangle(0, 20 + _displayIndex * 13, ImmediateParent?.DrawArea.Width ?? 0, 13);
            }
        }

        private PartyMember _partyMember;
        public PartyMember PartyMember
        {
            get => _partyMember;
            set
            {
                _partyMember = value;
                _nameLabel.Text = _partyMember.Name;
                _levelLabel.Text = $"{_partyMember.Level}";

                _playerIcon = _partyMember.IsLeader ? ChatIcon.Star : ChatIcon.Player;
                var (X, Y, Width, Height) = _playerIcon.GetChatIconRectangleBounds().ValueOr((0, 0, 0, 0));
                _iconSource = new Rectangle(X, Y, Width, Height);
            }
        }

        public event EventHandler RemoveAction;

        public PartyPanelMember(INativeGraphicsManager nativeGraphicsManager,
                                IContentProvider contentProvider,
                                bool isRemovable)
        {
            _contentProvider = contentProvider;
            _chatIconsTexture = nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 32, true);

            var removeTexture = nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 43);
            var delta = removeTexture.Height / 3;
            _removeButton = new XNAButton(removeTexture,
                new Vector2(337, 0),
                isRemovable
                    ? new Rectangle(0, 0, removeTexture.Width, delta)
                    : new Rectangle(0, delta, removeTexture.Width, delta),
                isRemovable
                    ? new Rectangle(0, delta * 2, removeTexture.Width, delta)
                    : new Rectangle(0, delta, removeTexture.Width, delta));

            if (isRemovable)
                _removeButton.OnMouseDown += (o, e) => RemoveAction?.Invoke(o, e);
            _removeButton.SetParentControl(this);

            _nameLabel = new XNALabel(Constants.FontSize08)
            {
                DrawPosition = new Vector2(23, 0),
                AutoSize = true,
                ForeColor = Color.Black
            };
            _nameLabel.SetParentControl(this);

            _levelLabel = new XNALabel(Constants.FontSize08)
            {
                DrawPosition = new Vector2(138, 0),
                AutoSize = true,
                ForeColor = Color.Black
            };
            _levelLabel.SetParentControl(this);

            _hpLabel = new XNALabel(Constants.FontSize08)
            {
                DrawPosition = new Vector2(205, 0),
                AutoSize = true,
                ForeColor = Color.Black,
                Text = "HP"
            };
            _hpLabel.SetParentControl(this);
        }

        public override void Initialize()
        {
            _removeButton.Initialize();
            _nameLabel.Initialize();
            _levelLabel.Initialize();
            _hpLabel.Initialize();

            base.Initialize();
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            var iconDrawPosition = DrawPositionWithParentOffset + new Vector2(5, 1);
            var healthBarDrawPosition = DrawPositionWithParentOffset + new Vector2(228, 1);

            var barTexture = _partyMember.PercentHealth > 50
                ? _contentProvider.Textures[ContentProvider.HPGreen]
                : _partyMember.PercentHealth > 25
                    ? _contentProvider.Textures[ContentProvider.HPYellow]
                    : _contentProvider.Textures[ContentProvider.HPRed];

            var barSource = new Rectangle(0, 0, (int)Math.Round(barTexture.Width * (_partyMember.PercentHealth / 100.0)), barTexture.Height);

            _spriteBatch.Begin();
            _spriteBatch.Draw(_chatIconsTexture, iconDrawPosition, _iconSource, Color.White);
            _spriteBatch.Draw(_contentProvider.Textures[ContentProvider.HPOutline], healthBarDrawPosition, Color.White);
            _spriteBatch.Draw(barTexture, healthBarDrawPosition, barSource, Color.White);
            _spriteBatch.End();

            base.OnDrawControl(gameTime);
        }
    }
}
