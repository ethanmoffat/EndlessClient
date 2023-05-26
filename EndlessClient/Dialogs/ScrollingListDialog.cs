using EndlessClient.Dialogs.Factories;
using EndlessClient.Dialogs.Services;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Graphics;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Input.InputListeners;
using System;
using System.Collections.Generic;
using System.Linq;
using XNAControls;

namespace EndlessClient.Dialogs
{
    [Flags]
    public enum ScrollingListDialogButtons
    {
        None         = 0x00,
        Add          = 0x01,
        Cancel       = 0x02,
        Back         = 0x04,
        Next         = 0x08,
        Ok           = 0x10,
        History      = 0x20,
        Progress     = 0x40,
        Delete       = 0x80,
        DualButtons  = 0x800,
        // indicates a configuration in which a pairing of DualButtons is already defined, but the order is reversed
        Alternate    = 0x1000,
                     
        AddCancel    = DualButtons | Add | Cancel,
        BackCancel   = DualButtons | Back | Cancel,
        BackOk       = DualButtons | Back | Ok,
        CancelOk     = DualButtons | Cancel | Ok,
        OkCancel     = DualButtons | Cancel | Ok | Alternate,
        BackNext     = DualButtons | Back | Next,
        CancelNext   = DualButtons | Cancel | Next,
        HistoryOk    = DualButtons | History | Ok,
        ProgressOk   = DualButtons | Progress | Ok,
        DeleteCancel = DualButtons | Delete | Cancel,

        // There is only one button, but we want it to show on the right side as if it were a dual button setting
        OffsetCancel = DualButtons | Alternate | Cancel,
    }

    public enum DialogType
    {
        // large
        Shop,
        FriendIgnore = Shop,
        Locker = Shop,
        Message = Shop,
        Guild = Shop,
        Inn = Shop,
        Law = Shop,

        // large no scroll
        Chest,

        // medium
        QuestProgressHistory,
        Board = QuestProgressHistory,

        // small
        NpcQuestDialog,

        // small no scroll
        BankAccountDialog,

        Jukebox,
        Barber,
    }

    public class ScrollingListDialog : BaseEODialog
    {
        private readonly List<ListDialogItem> _listItems;
        protected readonly ScrollBar _scrollBar;

        protected readonly IXNALabel _titleText;
        private ListDialogItem.ListItemStyle _listItemType;

        protected readonly XNAButton _add, _back, _cancel;
        protected readonly XNAButton _next, _ok, _delete;
        protected readonly XNAButton _history, _progress;

        protected readonly Vector2 _button1Position, _button2Position, _buttonCenterPosition;

        private ScrollingListDialogButtons _buttons;

        public IReadOnlyList<string> NamesList => _listItems.Select(item => item.PrimaryText).ToList();

        public string Title
        {
            get => _titleText.Text;
            set => _titleText.Text = value;
        }

        public int ItemsToShow
        {
            get
            {
                if (ListItemType == ListDialogItem.ListItemStyle.Large)
                    return 5;

                switch (DialogType)
                {

                    case DialogType.Shop:
                    case DialogType.QuestProgressHistory:
                        return 12;
                    case DialogType.NpcQuestDialog: return 6;
                    default: throw new NotImplementedException();
                }
            }
        }

        public ListDialogItem.ListItemStyle ListItemType
        {
            get => _listItemType;
            set
            {
                if (value == ListDialogItem.ListItemStyle.Large && DialogType == DialogType.NpcQuestDialog)
                    throw new InvalidOperationException("Can't use large ListDialogItem with small scrolling dialog");

                _listItemType = value;
                _scrollBar.LinesToRender = ItemsToShow;
            }
        }

        public ScrollingListDialogButtons Buttons
        {
            get => _buttons;
            set
            {
                _buttons = value;
                _add.Visible = Buttons.HasFlag(ScrollingListDialogButtons.Add);
                _back.Visible = Buttons.HasFlag(ScrollingListDialogButtons.Back);
                _next.Visible = Buttons.HasFlag(ScrollingListDialogButtons.Next);
                _ok.Visible = Buttons.HasFlag(ScrollingListDialogButtons.Ok);
                _cancel.Visible = Buttons.HasFlag(ScrollingListDialogButtons.Cancel);
                _history.Visible = Buttons.HasFlag(ScrollingListDialogButtons.History);
                _progress.Visible = Buttons.HasFlag(ScrollingListDialogButtons.Progress);
                _delete.Visible = Buttons.HasFlag(ScrollingListDialogButtons.Delete);

                if (Buttons.HasFlag(ScrollingListDialogButtons.DualButtons))
                {
                    if (Buttons == ScrollingListDialogButtons.BackCancel ||
                        Buttons == ScrollingListDialogButtons.AddCancel ||
                        Buttons == ScrollingListDialogButtons.DeleteCancel)
                    {
                        _add.DrawPosition = _button1Position;
                        _back.DrawPosition = _button1Position;
                        _delete.DrawPosition = _button1Position;
                        _cancel.DrawPosition = _button2Position;
                    }
                    else
                    {
                        var alternate = Buttons.HasFlag(ScrollingListDialogButtons.Alternate);

                        _back.DrawPosition = _button1Position;
                        _cancel.DrawPosition = alternate ? _button2Position : _button1Position;
                        _history.DrawPosition = _button1Position;
                        _progress.DrawPosition = _button1Position;

                        _next.DrawPosition = _button2Position;
                        _ok.DrawPosition = alternate ? _button1Position : _button2Position;
                    }
                }
                else
                {
                    _add.DrawPosition = _buttonCenterPosition;
                    _back.DrawPosition = _buttonCenterPosition;
                    _next.DrawPosition = _buttonCenterPosition;
                    _ok.DrawPosition = _buttonCenterPosition;
                    _cancel.DrawPosition = _buttonCenterPosition;
                }
            }
        }

        public DialogType DialogType { get; }

        public event EventHandler AddAction;

        public event EventHandler BackAction;

        public event EventHandler NextAction;

        public event EventHandler HistoryAction;

        public event EventHandler ProgressAction;

        public ScrollingListDialog(INativeGraphicsManager nativeGraphicsManager,
                                   IEODialogButtonService dialogButtonService,
                                   DialogType dialogType = DialogType.Shop)
            : base(nativeGraphicsManager, isInGame: true)
        {
            DialogType = dialogType;

            _listItems = new List<ListDialogItem>();

            _titleText = new XNALabel(Constants.FontSize08pt5)
            {
                DrawArea = GetTitleDrawArea(DialogType),
                AutoSize = false,
                TextAlign = LabelAlignment.MiddleLeft,
                ForeColor = ColorConstants.LightGrayText,
                Visible = DialogType != DialogType.Chest,
            };
            _titleText.SetParentControl(this);

            _scrollBar = new ScrollBar(new Vector2(DialogType == DialogType.QuestProgressHistory ? 449 : 252, 44), new Vector2(16, GetScrollBarHeight(DialogType)), ScrollBarColors.LightOnMed, GraphicsManager)
            {
                Visible = DialogType != DialogType.Chest && DialogType != DialogType.BankAccountDialog && DialogType != DialogType.Jukebox,
            };
            _scrollBar.SetParentControl(this);
            SetScrollWheelHandler(_scrollBar);

            BackgroundTexture = GraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, GetBackgroundTexture(DialogType));
            BackgroundTextureSource = GetBackgroundSourceRectangle(BackgroundTexture, DialogType);

            _add = new XNAButton(dialogButtonService.SmallButtonSheet, Vector2.Zero,
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Add),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Add))
            { 
                Visible = false,
                UpdateOrder = 1,
            };
            _add.SetParentControl(this);
            _add.OnClick += (o, e) => AddAction?.Invoke(o, e);

            _back = new XNAButton(dialogButtonService.SmallButtonSheet, Vector2.Zero,
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Back),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Back))
            {
                Visible = false,
                UpdateOrder = 1,
            };
            _back.SetParentControl(this);
            _back.OnClick += (o, e) => BackAction?.Invoke(o, e);

            _next = new XNAButton(dialogButtonService.SmallButtonSheet, Vector2.Zero,
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Next),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Next))
            {
                Visible = false,
                UpdateOrder = 1,
            };
            _next.SetParentControl(this);
            _next.OnClick += (o, e) => NextAction?.Invoke(o, e);

            _history = new XNAButton(dialogButtonService.SmallButtonSheet, Vector2.Zero,
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.History),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.History))
            {
                Visible = false,
                UpdateOrder = 1,
            };
            _history.SetParentControl(this);
            _history.OnClick += (o, e) => HistoryAction?.Invoke(o, e);

            _progress = new XNAButton(dialogButtonService.SmallButtonSheet, Vector2.Zero,
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Progress),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Progress))
            {
                Visible = false,
                UpdateOrder = 1,
            };
            _progress.SetParentControl(this);
            _progress.OnClick += (o, e) => ProgressAction?.Invoke(o, e);

            _delete = new XNAButton(dialogButtonService.SmallButtonSheet, Vector2.Zero,
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Delete),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Delete))
            {
                Visible = false,
                UpdateOrder = 1,
            };
            _delete.SetParentControl(this);
            _delete.OnClick += (o, e) => ProgressAction?.Invoke(o, e);

            _ok = new XNAButton(dialogButtonService.SmallButtonSheet, Vector2.Zero,
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Ok),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Ok))
            {
                Visible = false,
                UpdateOrder = 2,
            };
            _ok.SetParentControl(this);
            _ok.OnClick += CloseButton_Click;

            _cancel = new XNAButton(dialogButtonService.SmallButtonSheet, Vector2.Zero,
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Cancel),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Cancel))
            {
                Visible = false,
                UpdateOrder = 2,
            };
            _cancel.SetParentControl(this);
            _cancel.OnClick += CloseButton_Click;

            _button1Position = GetButton1Position(DrawArea, _ok.DrawArea, DialogType);
            _button2Position = GetButton2Position(DrawArea, _ok.DrawArea, DialogType);
            _buttonCenterPosition = GetButtonCenterPosition(DrawArea, _ok.DrawArea, DialogType);

            Buttons = ScrollingListDialogButtons.AddCancel;

            CenterInGameView();

            if (!Game.Window.AllowUserResizing)
                DrawPosition = new Vector2(DrawPosition.X, 15);
        }

        protected virtual void CloseButton_Click(object sender, MouseEventArgs e)
        {
            if (sender == _ok)
                Close(XNADialogResult.OK);
            else if (sender == _cancel)
                Close(XNADialogResult.Cancel);
        }

        public void SetItemList(List<ListDialogItem> itemList)
        {
            if (!itemList.All(x => x.Style == ListItemType))
                throw new ArgumentException($"Expected items of type {ListItemType}", nameof(itemList));

            ClearItemList();

            _scrollBar.UpdateDimensions(itemList.Count);

            for (int i = 0; i < itemList.Count; ++i)
            {
                _listItems.Add(itemList[i]);
                _listItems[i].Index = i;
                if (i > _scrollBar.LinesToRender)
                    _listItems[i].Visible = false;

                _listItems[i].Initialize();
            }
        }

        public void AddItemToList(ListDialogItem item, bool sortList)
        {
            _listItems.Add(item);

            if (sortList)
                _listItems.Sort((item1, item2) => item1.PrimaryText.CompareTo(item2.PrimaryText));

            for (int i = 0; i < _listItems.Count; ++i)
                _listItems[i].Index = i;

            item.Initialize();

            _scrollBar.UpdateDimensions(_listItems.Count);
        }

        public void RemoveFromList(ListDialogItem item)
        {
            _listItems.Remove(item);

            _scrollBar.UpdateDimensions(_listItems.Count);
            if (_listItems.Count <= _scrollBar.LinesToRender)
                _scrollBar.ScrollToTop();

            for (int i = 0; i < _listItems.Count; ++i)
                _listItems[i].Index = i;

            item.Dispose();
        }

        public void HighlightTextByLabel(IReadOnlyList<string> activeLabels)
        {
            var matchingListItems = _listItems.Where(x => activeLabels.Any(y => y.Equals(x.PrimaryText, StringComparison.InvariantCultureIgnoreCase)));
            foreach (var item in matchingListItems)
            {
                item.Highlight();
            }
        }

        public void ClearHighlightedText()
        {
            foreach (var item in _listItems)
                item.ClearHighlight();
        }

        public void ClearItemList()
        {
            foreach (var item in _listItems)
                item.Dispose();

            _listItems.Clear();
            _scrollBar.UpdateDimensions(0);
            _scrollBar.ScrollToTop();
        }

        public void AddTextAsListItems(BitmapFont font, Action linkClickAction, params string[] messages)
        {
            ListItemType = ListDialogItem.ListItemStyle.Small;

            var drawStrings = new List<string>();
            var ts = new TextSplitter(string.Empty, font) { LineLength = 200 };
            foreach (string s in messages)
            {
                ts.Text = s;
                drawStrings.AddRange(ts.NeedsProcessing ? ts.SplitIntoLines() : new[] { s });
                drawStrings.Add(" ");
            }

            foreach (string s in drawStrings)
            {
                var link = s.Length > 0 && s[0] == '*';
                var nextItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Small)
                {
                    PrimaryText = link ? s.Remove(0, 1) : s
                };

                if (link)
                {
                    nextItem.SetPrimaryClickAction((_, _) => linkClickAction());
                }

                AddItemToList(nextItem, sortList: false);
            }

        }

        public override void Initialize()
        {
            _add.Initialize();
            _back.Initialize();
            _next.Initialize();
            _ok.Initialize();
            _cancel.Initialize();
            _scrollBar.Initialize();
            _titleText.Initialize();

            base.Initialize();
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (_listItems.Count > _scrollBar.LinesToRender)
            {
                for (int i = 0; i < _listItems.Count; ++i)
                {
                    var curr = _listItems[i];
                    if (i < _scrollBar.ScrollOffset)
                    {
                        curr.Visible = false;
                        continue;
                    }

                    if (i < _scrollBar.LinesToRender + _scrollBar.ScrollOffset)
                    {
                        curr.Visible = true;
                        curr.Index = i - _scrollBar.ScrollOffset;
                    }
                    else
                    {
                        curr.Visible = false;
                    }
                }
            }
            else if (_listItems.Any(_item => !_item.Visible))
            {
                _listItems.ForEach(_item => _item.Visible = true);
            }

            base.OnUpdateControl(gameTime);
        }

        protected static Rectangle GetTitleDrawArea(DialogType size)
        {
            switch(size)
            {
                case DialogType.Shop:
                case DialogType.Chest: return new Rectangle(16, 13, 253, 19);
                case DialogType.QuestProgressHistory:
                    return new Rectangle(18, 14, 452, 19);
                case DialogType.Jukebox: return new Rectangle(24, 20, 263, 19);
                case DialogType.NpcQuestDialog: return new Rectangle(16, 16, 255, 18);
                case DialogType.BankAccountDialog: return new Rectangle(129, 20, 121, 16);
                default: throw new NotImplementedException();
            }
        }

        protected static int GetScrollBarHeight(DialogType size)
        {
            switch (size)
            {
                case DialogType.Shop:
                case DialogType.Chest:
                case DialogType.QuestProgressHistory:
                    return 199;
                case DialogType.Jukebox:
                case DialogType.NpcQuestDialog:
                case DialogType.BankAccountDialog: return 99;
                default: throw new NotImplementedException();
            }
        }

        private static int GetBackgroundTexture(DialogType size)
        {
            switch (size)
            {
                case DialogType.Shop: return 52;
                case DialogType.Chest: return 51;
                case DialogType.QuestProgressHistory: return 59;
                case DialogType.Jukebox: return 60;
                case DialogType.NpcQuestDialog: return 67;
                case DialogType.BankAccountDialog: return 53;
                default: throw new NotImplementedException();
            }
        }

        protected static Rectangle? GetBackgroundSourceRectangle(Texture2D backgroundTexture, DialogType size)
        {
            switch (size)
            {
                case DialogType.Shop:
                case DialogType.Chest: return null;
                case DialogType.QuestProgressHistory:
                    return new Rectangle(0, 0, backgroundTexture.Width, backgroundTexture.Height / 2);
                case DialogType.Jukebox:
                case DialogType.NpcQuestDialog:
                case DialogType.BankAccountDialog:  return null;
                default: throw new NotImplementedException();
            }
        }

        private static Vector2 GetButton1Position(Rectangle dialogArea, Rectangle buttonArea, DialogType size)
        {
            var yCoord = GetButtonYCoordinate(dialogArea);
            switch (size)
            {
                // buttons are centered on these dialogs
                case DialogType.Shop: 
                case DialogType.Chest:
                case DialogType.BankAccountDialog:
                case DialogType.Jukebox: return new Vector2((int)Math.Floor((dialogArea.Width - buttonArea.Width) / 2.0) - 48, yCoord);
                // buttons are offset from center on these dialogs
                case DialogType.QuestProgressHistory:
                    return new Vector2(288, yCoord);
                case DialogType.NpcQuestDialog: return new Vector2(89, yCoord);
                default: throw new NotImplementedException();
            }
        }

        private static Vector2 GetButton2Position(Rectangle dialogArea, Rectangle buttonArea, DialogType size)
        {
            var yCoord = GetButtonYCoordinate(dialogArea);
            switch (size)
            {
                // buttons are centered on these dialogs
                case DialogType.Shop:
                case DialogType.Chest:
                case DialogType.BankAccountDialog:
                case DialogType.Jukebox: return new Vector2((int)Math.Floor((dialogArea.Width - buttonArea.Width) / 2.0) + 48, yCoord);
                // buttons are offset from center on these dialogs
                case DialogType.QuestProgressHistory:
                    return new Vector2(380, yCoord);
                case DialogType.NpcQuestDialog: return new Vector2(183, yCoord);
                default: throw new NotImplementedException();
            }
        }

        private static Vector2 GetButtonCenterPosition(Rectangle dialogArea, Rectangle buttonArea, DialogType dialogSize)
        {
            // chest dialog has a button built in to the graphic that needs to be covered up...
            if (dialogSize == DialogType.Chest)
                return new Vector2(92, 227);

            // bank dialog has a button built in to the graphic that needs to be covered up...
            if (dialogSize == DialogType.BankAccountDialog)
                return new Vector2(92, 191);

            // jukebox dialog has a button built in to the graphic that needs to be covered up...
            if (dialogSize == DialogType.Jukebox)
                return new Vector2(92, 158);

            var yCoord = GetButtonYCoordinate(dialogArea);
            return new Vector2((dialogArea.Width - buttonArea.Width) / 2, yCoord);
        }

        private static int GetButtonYCoordinate(Rectangle dialogArea)
        {
            // this should always be 38 from the bottom
            return dialogArea.Height - 38;
        }
    }
}
