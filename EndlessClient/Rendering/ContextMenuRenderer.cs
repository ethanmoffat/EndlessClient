using EndlessClient.Audio;
using EndlessClient.ControlSets;
using EndlessClient.Dialogs.Actions;
using EndlessClient.Dialogs.Factories;
using EndlessClient.HUD;
using EndlessClient.HUD.Controls;
using EndlessClient.Rendering.Character;
using EndlessClient.Services;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Domain.Interact;
using EOLib.Domain.Map;
using EOLib.Domain.Party;
using EOLib.Domain.Trade;
using EOLib.Graphics;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using MonoGame.Extended.Input;
using MonoGame.Extended.Input.InputListeners;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using XNAControls;

namespace EndlessClient.Rendering
{
    public class ContextMenuRenderer : XNAControl, IContextMenuRenderer
    {
        private enum MenuAction
        {
            Paperdoll,
            Book,
            Join,
            Invite,
            Trade,
            Whisper,
            Friend,
            Ignore,
            NUM_MENU_ACTIONS
        }

        private readonly Texture2D _backgroundTexture, _bgfill;
        private readonly Rectangle _outSource, _overSource;
        private readonly Dictionary<Rectangle, Action> _menuActions;
        private Option<Rectangle> _overRect;

        private readonly IInGameDialogActions _inGameDialogActions;
        private readonly IPaperdollActions _paperdollActions;
        private readonly IBookActions _bookActions;
        private readonly IPartyActions _partyActions;
        private readonly ITradeActions _tradeActions;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IFriendIgnoreListService _friendIgnoreListService;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly IContextMenuRepository _contextMenuRepository;
        private readonly IPartyDataProvider _partyDataProvider;
        private readonly ICharacterRenderer _characterRenderer;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IClientWindowSizeProvider _clientWindowSizeProvider;
        private readonly ISfxPlayer _sfxPlayer;

        private static DateTime? _lastTradeRequestedTime;
        private static DateTime? _lastPartyRequestTime;

        public ContextMenuRenderer(INativeGraphicsManager nativeGraphicsManager,
                                   IInGameDialogActions inGameDialogActions,
                                   IPaperdollActions paperdollActions,
                                   IBookActions bookActions,
                                   IPartyActions partyActions,
                                   ITradeActions tradeActions,
                                   IStatusLabelSetter statusLabelSetter,
                                   IFriendIgnoreListService friendIgnoreListService,
                                   IHudControlProvider hudControlProvider,
                                   IContextMenuRepository contextMenuRepository,
                                   IPartyDataProvider partyDataProvider,
                                   ICharacterRenderer characterRenderer,
                                   ICurrentMapStateProvider currentMapStateProvider, 
                                   IEOMessageBoxFactory messageBoxFactory,
                                   IClientWindowSizeProvider clientWindowSizeProvider,
                                   ISfxPlayer sfxPlayer)
        {
            _menuActions = new Dictionary<Rectangle, Action>();
            _inGameDialogActions = inGameDialogActions;
            _paperdollActions = paperdollActions;
            _bookActions = bookActions;
            _partyActions = partyActions;
            _tradeActions = tradeActions;
            _statusLabelSetter = statusLabelSetter;
            _friendIgnoreListService = friendIgnoreListService;
            _hudControlProvider = hudControlProvider;
            _contextMenuRepository = contextMenuRepository;
            _partyDataProvider = partyDataProvider;
            _characterRenderer = characterRenderer;
            _currentMapStateProvider = currentMapStateProvider;
            _messageBoxFactory = messageBoxFactory;
            _clientWindowSizeProvider = clientWindowSizeProvider;
            _sfxPlayer = sfxPlayer;

            //first, load up the images. split in half: the right half is the 'over' text
            _backgroundTexture = nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 41, true);

            const int W = 96, H = 137;
            _outSource = new Rectangle(0, 0, W, H);
            _overSource = new Rectangle(W, 0, W, H);

            //this GFX is stupid. a bunch of white space throws off coordinates so I have to use hard-coded values
            //define regions for clicking and their associated actions
            //6,11,86,14
            for (int i = 0; i < (int)MenuAction.NUM_MENU_ACTIONS; ++i)
            {
                Rectangle region = new Rectangle(6, (i < 5 ? 11 : 13) + 14 * i, 86, 14);
                _menuActions.Add(region, GetActionFromMenuAction((MenuAction)i));
            }

            //set the fill color
            _bgfill = new Texture2D(GraphicsDevice, 1, 1);
            _bgfill.SetData(new[] { Color.White });

            SetPositionBasedOnCharacterRenderer(_characterRenderer);
            SetSize(W, H);

            OnMouseOver += ContextMenuRenderer_OnMouseOver;

            // Update this before map renderer so that clicks are handled first
            UpdateOrder = -20;
        }

        public override void Initialize()
        {
            base.Initialize();

            if (!Game.Components.Contains(this))
                Game.Components.Add(this);
        }

        private void SetPositionBasedOnCharacterRenderer(ICharacterRenderer renderer)
        {
            //rules for draw location:
            // 1. try to the right first - if it doesn't fit (width-wise), go to the left
            // 2. if it will be out of the game area, move it up/down so that it is not clipped by the screen bounds

            // hide and disable as soon as mouse click either on or out of bounds -- obviously handle event for mouse click on particular text

            Rectangle rendRect = renderer.DrawArea;

            DrawPosition = new Vector2(rendRect.Right + 20, rendRect.Y);

            if (DrawArea.Right > _clientWindowSizeProvider.Width - 15)
            {
                // case: goes off the right side of the screen, show on the left
                DrawPosition = new Vector2(rendRect.X - DrawArea.Width - 20, DrawPosition.Y);
            }

            // 308px is the bottom of the display area for map stuff
            if (DrawArea.Bottom > (_clientWindowSizeProvider.Resizable ? _clientWindowSizeProvider.Height : 308))
            {
                //case: goes off bottom of the screen, adjust new rectangle so it is above 308
                DrawPosition = new Vector2(DrawPosition.X, 298 - DrawArea.Height);
            }
            else if (DrawArea.Y < 25)
            {
                //case: goes off top of screen, adjust new rectangle so it aligns with top of character head
                DrawPosition = new Vector2(DrawPosition.X, 35);
            }

            _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION, EOResourceID.STATUS_LABEL_MENU_BELONGS_TO_PLAYER, $" {renderer.Character.Name}");
        }

        protected override bool ShouldUpdate()
        {
            return base.ShouldUpdate();
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            _spriteBatch.Begin();

            _spriteBatch.Draw(_bgfill, DrawAreaWithParentOffset, Color.FromNonPremultiplied(0xff, 0xff, 0xff, 192));

            _spriteBatch.Draw(_backgroundTexture, DrawAreaWithParentOffset, _outSource, Color.White);
            _overRect.MatchSome(r =>
            {
                _spriteBatch.Draw(_backgroundTexture,
                    new Vector2(DrawAreaWithParentOffset.X, DrawAreaWithParentOffset.Y) + new Vector2(r.X, r.Y),
                    r.WithPosition(new Vector2(r.X + _overSource.X, r.Y + _overSource.Y)),
                    Color.White);
            });

            _spriteBatch.End();

            base.OnDrawControl(gameTime);
        }

        private void ContextMenuRenderer_OnMouseOver(object sender, MouseStateExtended e)
        {
            bool found = false;
            foreach (var (sourceRect, menuAction) in _menuActions)
            {
                if (sourceRect.Contains(e.Position - DrawAreaWithParentOffset.Location))
                {
                    _overRect = Option.Some(sourceRect);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                _overRect = Option.None<Rectangle>();
            }
        }

        protected override bool HandleClick(IXNAControl control, MouseEventArgs eventArgs)
        {
            if (eventArgs.Button == MouseButton.Left)
            {
                _overRect.MatchSome(sourceRect =>
                {
                    if (!_menuActions.ContainsKey(sourceRect)) return;

                    var menuAction = _menuActions[sourceRect];
                    menuAction();
                });
            }

            _sfxPlayer.PlaySfx(SoundEffectID.DialogButtonClick);

            Game.Components.Remove(this);
            Dispose();

            _contextMenuRepository.ContextMenu = Option.None<IContextMenuRenderer>();

            return true;
        }

        /* Helper maps MenuAction enum value to a member method for easy initialization */
        private Action GetActionFromMenuAction(MenuAction menuAction)
        {
            switch (menuAction)
            {
                case MenuAction.Paperdoll: return ShowPaperdollAction;
                case MenuAction.Book: return ShowBook;
                case MenuAction.Join: return JoinParty;
                case MenuAction.Invite: return InviteToParty;
                case MenuAction.Trade: return Trade;
                case MenuAction.Whisper: return PrivateMessage;
                case MenuAction.Friend: return AddFriend;
                case MenuAction.Ignore: return AddIgnore;
                default: throw new ArgumentOutOfRangeException(nameof(menuAction));
            }
        }

        private void ShowPaperdollAction()
        {
            _paperdollActions.RequestPaperdoll(_characterRenderer.Character.ID);
            _inGameDialogActions.ShowPaperdollDialog(_characterRenderer.Character, isMainCharacter: false);
        }

        private void ShowBook()
        {
            _bookActions.RequestBook(_characterRenderer.Character.ID);
            _inGameDialogActions.ShowBookDialog(_characterRenderer.Character, isMainCharacter: false);
        }

        private void JoinParty()
        {
            if (_partyDataProvider.Members.Any(x => x.CharacterID == _characterRenderer.Character.ID))
            {
                _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, _characterRenderer.Character.Name + " ", EOResourceID.STATUS_LABEL_PARTY_IS_ALREADY_MEMBER, showChatError: true);
                return;
            }

            if (_lastPartyRequestTime != null && (DateTime.Now - _lastPartyRequestTime.Value).TotalSeconds < Constants.PartyRequestTimeoutSeconds)
            {
                _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.STATUS_LABEL_PARTY_RECENTLY_REQUESTED, showChatError: true);
                return;
            }

            _lastPartyRequestTime = DateTime.Now;
            _partyActions.RequestParty(PartyRequestType.Join, _characterRenderer.Character.ID);
            _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION, EOResourceID.STATUS_LABEL_PARTY_REQUESTED_TO_JOIN);
        }

        private void InviteToParty()
        {
            if (_partyDataProvider.Members.Any(x => x.CharacterID == _characterRenderer.Character.ID))
            {
                _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, _characterRenderer.Character.Name + " ", EOResourceID.STATUS_LABEL_PARTY_IS_ALREADY_MEMBER, showChatError: true);
                return;
            }

            if (_lastPartyRequestTime != null && (DateTime.Now - _lastPartyRequestTime.Value).TotalSeconds < Constants.PartyRequestTimeoutSeconds)
            {
                _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.STATUS_LABEL_PARTY_RECENTLY_REQUESTED, showChatError: true);
                return;
            }

            _lastPartyRequestTime = DateTime.Now;
            _partyActions.RequestParty(PartyRequestType.Invite, _characterRenderer.Character.ID);
            _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION, _characterRenderer.Character.Name, EOResourceID.STATUS_LABEL_PARTY_IS_INVITED);
        }

        private void Trade()
        {
            if (_currentMapStateProvider.IsJail)
            {
                _messageBoxFactory.CreateMessageBox(EOResourceID.JAIL_WARNING_CANNOT_TRADE, EOResourceID.STATUS_LABEL_TYPE_WARNING).ShowDialog();
                _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.JAIL_WARNING_CANNOT_TRADE);
                return;
            }

            if (_lastTradeRequestedTime != null && (DateTime.Now - _lastTradeRequestedTime.Value).TotalSeconds < Constants.TradeRequestTimeoutSeconds)
            {
                _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.STATUS_LABEL_TRADE_RECENTLY_REQUESTED);
                return;
            }

            _lastTradeRequestedTime = DateTime.Now;

            _tradeActions.RequestTrade(_characterRenderer.Character.ID);

            _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION, EOResourceID.STATUS_LABEL_TRADE_REQUESTED_TO_TRADE);
        }

        private void PrivateMessage()
        {
            _hudControlProvider.GetComponent<ChatTextBox>(HudControlIdentifier.ChatTextBox).Text = $"!{_characterRenderer.Character.Name} ";
        }

        private void AddFriend()
        {
            _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION, $"{_characterRenderer.Character.Name} ", EOResourceID.STATUS_LABEL_WILL_BE_YOUR_FRIEND);
            _friendIgnoreListService.SaveNewFriend(Constants.FriendListFile, _characterRenderer.Character.Name);
        }

        private void AddIgnore()
        {
            _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION, $"{_characterRenderer.Character.Name} ", EOResourceID.STATUS_LABEL_WILL_BE_IGNORED);
            _friendIgnoreListService.SaveNewIgnore(Constants.IgnoreListFile, _characterRenderer.Character.Name);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _bgfill.Dispose();

            base.Dispose(disposing);
        }
    }

    public interface IContextMenuRenderer : IXNAControl
    {
    }
}
