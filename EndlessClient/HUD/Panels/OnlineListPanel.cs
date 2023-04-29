using EndlessClient.Audio;
using EndlessClient.ControlSets;
using EndlessClient.HUD.Controls;
using EndlessClient.Services;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Domain.Online;
using EOLib.Domain.Party;
using EOLib.Extensions;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Input;
using MonoGame.Extended.Input.InputListeners;
using Optional.Unsafe;
using System;
using System.Collections.Generic;
using System.Linq;
using XNAControls;

namespace EndlessClient.HUD.Panels
{
    public class OnlineListPanel : DraggableHudPanel
    {
        private enum Filter
        {
            All,
            Friends,
            Admins,
            Party,
            Max
        }

        private const int DRAW_NAME_X = 18,
                          DRAW_OFFSET_Y = 23;

        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly IOnlinePlayerProvider _onlinePlayerProvider;
        private readonly IPartyDataProvider _partyDataProvider;
        private readonly IFriendIgnoreListService _friendIgnoreListService;
        private readonly ISfxPlayer _sfxPlayer;

        private readonly BitmapFont _chatFont;

        private readonly List<OnlinePlayerInfo> _onlineList;
        private readonly IXNALabel _totalNumberOfPlayers;
        private readonly ScrollBar _scrollBar;
        private readonly ClickableArea _filterClickArea;

        private readonly Texture2D _weirdOffsetTextureSheet, _chatIconsTexture;
        private readonly Rectangle[] _filterTextureSources;

        private HashSet<OnlinePlayerInfo> _cachedList;

        private Filter _filter;
        private List<OnlinePlayerInfo> _filteredList;
        private IReadOnlyList<string> _friendList;

        public OnlineListPanel(INativeGraphicsManager nativeGraphicsManager,
                               IHudControlProvider hudControlProvider,
                               IOnlinePlayerProvider onlinePlayerProvider,
                               IPartyDataProvider partyDataProvider,
                               IFriendIgnoreListService friendIgnoreListService,
                               ISfxPlayer sfxPlayer,
                               BitmapFont chatFont)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _hudControlProvider = hudControlProvider;
            _onlinePlayerProvider = onlinePlayerProvider;
            _partyDataProvider = partyDataProvider;
            _friendIgnoreListService = friendIgnoreListService;
            _sfxPlayer = sfxPlayer;
            _chatFont = chatFont;
            _onlineList = new List<OnlinePlayerInfo>();

            BackgroundImage = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 36);
            DrawArea = new Rectangle(102, 330, BackgroundImage.Width, BackgroundImage.Height);

            _totalNumberOfPlayers = new XNALabel(Constants.FontSize09)
            {
                AutoSize = false,
                ForeColor = ColorConstants.LightGrayText,
                TextAlign = LabelAlignment.MiddleRight,
                DrawArea = new Rectangle(454, 3, 27, 14),
                BackColor = Color.Transparent,
            };
            _totalNumberOfPlayers.SetParentControl(this);

            _scrollBar = new ScrollBar(new Vector2(467, 20), new Vector2(16, 97), ScrollBarColors.LightOnMed, _nativeGraphicsManager)
            {
                LinesToRender = 7,
                Visible = true
            };
            _scrollBar.SetParentControl(this);
            SetScrollWheelHandler(_scrollBar);

            _filterClickArea = new ClickableArea(new Rectangle(2, 2, 14, 14));
            _filterClickArea.SetParentControl(this);
            _filterClickArea.OnClick += FilterClickArea_Click;

            _weirdOffsetTextureSheet = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 27, true);
            _chatIconsTexture = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 32, true);

            _filterTextureSources = new Rectangle[4];
            for (int i = 0; i < _filterTextureSources.Length; ++i)
                _filterTextureSources[i] = new Rectangle(i % 2 == 0 ? 0 : 12, i >= 2 ? 246 : 233, 12, 13);

            _cachedList = new HashSet<OnlinePlayerInfo>();
            _filter = Filter.All;
            _filteredList = new List<OnlinePlayerInfo>();
        }

        public override void Initialize()
        {
            _totalNumberOfPlayers.Initialize();
            _scrollBar.Initialize();
            _filterClickArea.Initialize();

            base.Initialize();
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (!_cachedList.SetEquals(_onlinePlayerProvider.OnlinePlayers))
            {
                _cachedList = _onlinePlayerProvider.OnlinePlayers.ToHashSet();
                
                // keep the friends list data from overriding the displayed data in this panel
                // it will be friends list data if all titles (or any field other than name) are empty
                if (!_cachedList.All(x => x.Title == string.Empty))
                {
                    _onlineList.Clear();
                    _onlineList.AddRange(_cachedList);

                    _onlineList.Sort((a, b) => a.Name.CompareTo(b.Name));
                    _filteredList = new List<OnlinePlayerInfo>(_onlineList);

                    _totalNumberOfPlayers.Text = $"{_onlineList.Count}";
                    _scrollBar.UpdateDimensions(_onlineList.Count);
                    _scrollBar.ScrollToTop();

                    _friendList = _friendIgnoreListService.LoadList(Constants.FriendListFile);

                    ApplyFilter();
                }
            }

            base.OnUpdateControl(gameTime);
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            base.OnDrawControl(gameTime);

            const int DRAW_ICON_X = 4,
                      DRAW_TITLE_X = 133,
                      DRAW_GUILD_X = 245,
                      DRAW_CLASS_X = 359;


            _spriteBatch.Begin();

            _spriteBatch.Draw(_weirdOffsetTextureSheet, new Vector2(DrawAreaWithParentOffset.X + 4, DrawAreaWithParentOffset.Y + 2), _filterTextureSources[(int)_filter], Color.White);

            // todo: either a) use renderable approach like Chat/News or b) remove renderable approach in favor of this simpler method
            for (int i = _scrollBar.ScrollOffset; i < _scrollBar.ScrollOffset + _scrollBar.LinesToRender && i < _filteredList.Count; ++i)
            {
                int yCoord = DRAW_OFFSET_Y + DrawAreaWithParentOffset.Y + (i - _scrollBar.ScrollOffset) * 13;
                _spriteBatch.Draw(_chatIconsTexture, new Vector2(DrawAreaWithParentOffset.X + DRAW_ICON_X, yCoord), GetChatIconSourceRectangle(_filteredList[i].Icon), Color.White);
                _spriteBatch.DrawString(_chatFont, _filteredList[i].Name, new Vector2(DrawAreaWithParentOffset.X + DRAW_NAME_X, yCoord), Color.Black);
                _spriteBatch.DrawString(_chatFont, _filteredList[i].Title, new Vector2(DrawAreaWithParentOffset.X + DRAW_TITLE_X, yCoord), Color.Black);
                _spriteBatch.DrawString(_chatFont, _filteredList[i].Guild, new Vector2(DrawAreaWithParentOffset.X + DRAW_GUILD_X, yCoord), Color.Black);
                _spriteBatch.DrawString(_chatFont, _filteredList[i].Class, new Vector2(DrawAreaWithParentOffset.X + DRAW_CLASS_X, yCoord), Color.Black);
            }

            _spriteBatch.End();
        }

        protected override bool HandleClick(IXNAControl control, MouseEventArgs eventArgs)
        {
            if (eventArgs.Button != MouseButton.Right)
                return false;

            var mousePos = eventArgs.Position;
            if (mousePos.X >= DrawAreaWithParentOffset.X + DRAW_NAME_X && mousePos.X <= _scrollBar.DrawAreaWithParentOffset.X &&
                mousePos.Y >= DrawAreaWithParentOffset.Y + DRAW_OFFSET_Y && mousePos.Y <= DrawAreaWithParentOffset.Y + DrawAreaWithParentOffset.Height)
            {
                var index = (mousePos.Y - (DrawAreaWithParentOffset.Y + DRAW_OFFSET_Y)) / 13;
                if (index >= 0 && index <= _filteredList.Count)
                {
                    var name = _filteredList[_scrollBar.ScrollOffset + index].Name;
                    _hudControlProvider.GetComponent<ChatTextBox>(HudControlIdentifier.ChatTextBox).Text = $"!{name} ";
                }
            }

            return true;
        }

        private void FilterClickArea_Click(object sender, EventArgs e)
        {
            _sfxPlayer.PlaySfx(SoundEffectID.DialogButtonClick);
            _filter = (Filter)(((int)_filter + 1) % (int)Filter.Max);
            _scrollBar.ScrollToTop();

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            switch (_filter)
            {
                case Filter.Friends: _filteredList = _onlineList.Where(x => _friendList.Contains(x.Name, StringComparer.InvariantCultureIgnoreCase)).ToList(); break;
                case Filter.Admins: _filteredList = _onlineList.Where(IsAdminIcon).ToList(); break;
                case Filter.Party: _filteredList = _onlineList.Where(x => _partyDataProvider.Members.Any(y => string.Equals(y.Name, x.Name, StringComparison.InvariantCultureIgnoreCase))).ToList();  break;
                case Filter.All:
                default: _filteredList = new List<OnlinePlayerInfo>(_onlineList); break;
            }

            _scrollBar.UpdateDimensions(_filteredList.Count);
        }

        private static bool IsAdminIcon(OnlinePlayerInfo onlineInfo)
        {
            switch (onlineInfo.Icon)
            {
                case OnlineIcon.GM:
                case OnlineIcon.HGM:
                case OnlineIcon.GMParty:
                case OnlineIcon.HGMParty:
                    return true;
                default:
                    return false;
            }
        }

        private static Rectangle? GetChatIconSourceRectangle(OnlineIcon icon)
        {
            var (x, y, width, height) = icon.ToChatIcon().GetChatIconRectangleBounds().ValueOrDefault();
            return new Rectangle(x, y, width, height);
        }
    }
}