using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.Content;
using EndlessClient.ControlSets;
using EndlessClient.Dialogs.Factories;
using EndlessClient.Dialogs.Services;
using EndlessClient.HUD.Controls;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Board;
using EOLib.Domain.Login;
using EOLib.Graphics;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Input.InputListeners;
using Optional;
using Optional.Collections;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class BoardDialog : ScrollingListDialog
    {
        private enum BoardDialogState
        {
            ViewList,
            ViewPost,
            CreatePost,
        }

        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IEOMessageBoxFactory _eoMessageBoxFactory;
        private readonly IBoardActions _boardActions;
        private readonly IBoardRepository _boardRepository;
        private readonly IPlayerInfoProvider _playerInfoProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly IHudControlProvider _hudControlProvider;

        private readonly XNATextBox _subject, _message;

        private BoardDialogState _state;
        private HashSet<BoardPostInfo> _cachedPostInfo;

        public BoardDialog(INativeGraphicsManager nativeGraphicsManager,
                           IEODialogButtonService dialogButtonService,
                           ILocalizedStringFinder localizedStringFinder,
                           IEOMessageBoxFactory eoMessageBoxFactory,
                           IBoardActions boardActions,
                           IBoardRepository boardRepository,
                           IPlayerInfoProvider playerInfoProvider,
                           ICharacterProvider characterProvider,
                           IContentProvider contentProvider,
                           IHudControlProvider hudControlProvider)
            : base(nativeGraphicsManager, dialogButtonService, DialogType.Board)
        {
            _localizedStringFinder = localizedStringFinder;
            _eoMessageBoxFactory = eoMessageBoxFactory;
            _boardActions = boardActions;
            _boardRepository = boardRepository;
            _playerInfoProvider = playerInfoProvider;
            _characterProvider = characterProvider;
            _hudControlProvider = hudControlProvider;
            ListItemType = ListDialogItem.ListItemStyle.Small;
            Title = _localizedStringFinder.GetString(EOResourceID.BOARD_TOWN_BOARD);
            _state = BoardDialogState.ViewList;
            _cachedPostInfo = new HashSet<BoardPostInfo>();

            _subject = new XNATextBox(new Rectangle(150, 44, 315, 19), Constants.FontSize08, caretTexture: contentProvider.Textures[ContentProvider.Cursor])
            {
                TextAlignment = LabelAlignment.MiddleLeft,
                TextColor = ColorConstants.LightGrayText,
                Visible = false,
                MaxWidth = 310
            };
            _subject.SetScrollWheelHandler(_scrollBar);
            _subject.SetParentControl(this);

            _message = new XNATextBox(new Rectangle(18, 80, 430, 168), Constants.FontSize08, caretTexture: contentProvider.Textures[ContentProvider.Cursor])
            {
                TextAlignment = LabelAlignment.TopLeft,
                TextColor = ColorConstants.LightGrayText,
                Visible = false,
                MaxWidth = 412,

                Multiline = true,
                ScrollHandler = _scrollBar,
                RowSpacing = 16,
            };
            _message.SetScrollWheelHandler(_scrollBar);
            _message.SetParentControl(this);

            _add.OnClick += AddButton_Click;
            _delete.OnClick += DeleteButton_Click;
        }

        public override void Initialize()
        {
            _subject.Initialize();
            _message.Initialize();

            base.Initialize();
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            switch (_state)
            {
                case BoardDialogState.ViewList:
                    if (!_cachedPostInfo.SetEquals(_boardRepository.Posts))
                    {
                        ClearItemList();

                        _cachedPostInfo = new HashSet<BoardPostInfo>(_boardRepository.Posts);

                        var index = 0;
                        foreach (var post in _cachedPostInfo)
                        {
                            var childItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Small, index++)
                            {
                                PrimaryText = char.ToUpper(post.Author[0]) + post.Author[1..],
                                SubText = post.Subject,
                                Data = post.PostId,
                                Visible = true,
                                UnderlineLinks = false,
                                ShowSubtext = true,
                                OffsetX = 2,
                                OffsetY = 48,
                            };

                            childItem.DrawArea = new Rectangle(childItem.DrawArea.Location, new Point(427, 16));
                            childItem.SetPrimaryClickAction(ChildItem_Click);
                            childItem.SetScrollWheelHandler(this);

                            AddItemToList(childItem, sortList: false);
                        }

                        _scrollBar.ScrollToTop();
                    }

                    break;
                case BoardDialogState.ViewPost:
                    if (_boardRepository.ActivePostMessage.Map(msg => msg != _message.Text).ValueOr(false))
                    {
                        _boardRepository.ActivePostMessage.MatchSome(msg => _message.Text = msg);
                        _scrollBar.ScrollToTop();
                    }
                    break;
            }

            base.OnUpdateControl(gameTime);
        }

        private void SetState(BoardDialogState state, int postId = -1)
        {
            // todo:
            // 1. backspace on scrolled newline is broken
            // 2. need 10 rows per message for parity (increase space between rows)

            if (state == _state)
                return;

            _state = state;

            _titleText.DrawArea = _state == BoardDialogState.ViewList
                ? GetTitleDrawArea(DialogType)
                : GetTitleDrawArea(DialogType).WithPosition(new Vector2(150, _titleText.DrawArea.Y));

            BackgroundTextureSource = _state == BoardDialogState.ViewList
                ? GetBackgroundSourceRectangle(BackgroundTexture, DialogType)
                : GetBackgroundSourceRectangle(BackgroundTexture, DialogType).Value.WithPosition(new Vector2(0, BackgroundTexture.Height / 2));

            _scrollBar.LinesToRender = _state == BoardDialogState.ViewList
                ? 12
                : 10;

            switch (_state)
            {
                case BoardDialogState.ViewList:
                    _hudControlProvider.GetComponent<ChatTextBox>(HudControlIdentifier.ChatTextBox).Selected = true;

                    Buttons = ScrollingListDialogButtons.AddCancel;
                    Title = _localizedStringFinder.GetString(EOResourceID.BOARD_TOWN_BOARD);
                    _subject.Visible = _message.Visible = false;
                    _subject.Selected = _message.Selected = false;

                    _scrollBar.DrawArea = new Rectangle(
                        _scrollBar.DrawArea.X, 44,
                        _scrollBar.DrawArea.Width, GetScrollBarHeight(DialogType));

                    break;

                case BoardDialogState.CreatePost:
                    Buttons = ScrollingListDialogButtons.OkCancel;
                    Title = _localizedStringFinder.GetString(EOResourceID.BOARD_POSTING_NEW_MESSAGE);

                    _subject.Text = _message.Text = string.Empty;
                    _subject.Visible = _message.Visible = true;
                    _subject.Enabled = _message.Enabled = true;

                    _subject.TabOrder = 0;
                    _message.TabOrder = 1;

                    _subject.Selected = true;

                    _scrollBar.DrawArea = new Rectangle(
                        _scrollBar.DrawArea.X, 74,
                        _scrollBar.DrawArea.Width, GetScrollBarHeight(DialogType) - 30);

                    ClearItemList();
                    _cachedPostInfo.Clear();
                    break;

                case BoardDialogState.ViewPost:
                    var author = _boardRepository.ActivePost.Map(x => x.Author).ValueOr(string.Empty);
                    var matchesAuthor = author.IndexOf(_characterProvider.MainCharacter.Name, StringComparison.OrdinalIgnoreCase) >= 0;
                    if (_playerInfoProvider.PlayerHasAdminCharacter || matchesAuthor)
                        Buttons = ScrollingListDialogButtons.DeleteCancel;
                    else
                        Buttons = ScrollingListDialogButtons.OffsetCancel;

                    _boardRepository.Posts.SingleOrNone(x => x.PostId == postId)
                        .MatchSome(post =>
                        {
                            Title = post.Author;
                            _subject.Text = post.Subject;
                        });

                    _subject.Visible = true;
                    _message.Text = _localizedStringFinder.GetString(EOResourceID.BOARD_LOADING_MESSAGE);
                    _message.Visible = true;

                    _subject.Enabled = _message.Enabled = false;

                    _scrollBar.DrawArea = new Rectangle(
                        _scrollBar.DrawArea.X, 74,
                        _scrollBar.DrawArea.Width, GetScrollBarHeight(DialogType) - 30);

                    ClearItemList();
                    _cachedPostInfo.Clear();
                    break;
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            var numPostsByThisPlayer = _boardRepository.Posts.Count(x => string.Equals(x.Author, _characterProvider.MainCharacter.Name, StringComparison.OrdinalIgnoreCase));
            if (numPostsByThisPlayer > 2)
            {
                var dlg = _eoMessageBoxFactory.CreateMessageBox(DialogResourceID.BOARD_ERROR_TOO_MANY_MESSAGES);
                dlg.ShowDialog();
            }
            else
            {
                SetState(BoardDialogState.CreatePost);
            }
        }

        private void DeleteButton_Click(object sender, MouseEventArgs e)
        {
            _boardRepository.ActivePost.MatchSome(x => _boardActions.DeletePost(x.PostId));
            SetState(BoardDialogState.ViewList);
        }

        protected override void CloseButton_Click(object sender, MouseEventArgs e)
        {
            if (sender == _cancel && _state == BoardDialogState.ViewList)
            {
                Close(XNADialogResult.Cancel);
            }
            else
            {
                if (sender == _ok && _state == BoardDialogState.CreatePost)
                {
                    if (string.IsNullOrEmpty(_message.Text))
                    {
                        var dlg = _eoMessageBoxFactory.CreateMessageBox(DialogResourceID.BOARD_ERROR_NO_MESSAGE);
                        dlg.ShowDialog();
                        return;
                    }
                    else if (string.IsNullOrEmpty(_subject.Text))
                    {
                        var dlg = _eoMessageBoxFactory.CreateMessageBox(DialogResourceID.BOARD_ERROR_NO_SUBJECT);
                        dlg.ShowDialog();
                        return;
                    }

                    _boardActions.AddPost(_subject.Text, _message.Text);
                }

                _boardRepository.ActivePost = Option.None<BoardPostInfo>();

                SetState(BoardDialogState.ViewList);
            }
        }

        private void ChildItem_Click(object sender, MouseEventArgs e)
        {
            var postId = sender is ListDialogItem itemSender
                ? (int)itemSender.Data
                : sender is IXNAHyperLink linkSender
                    ? (int)((ListDialogItem)linkSender.ImmediateParent).Data
                    : -1;

            if (postId >= 0)
            {
                _boardRepository.ActivePost = _boardRepository.Posts.SingleOrNone(x => x.PostId == postId);

                SetState(BoardDialogState.ViewPost, postId);
                _boardActions.ViewPost(postId);
            }
        }
    }
}
