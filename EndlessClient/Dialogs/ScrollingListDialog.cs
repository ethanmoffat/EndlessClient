using EndlessClient.Dialogs.Services;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using XNAControls;

namespace EndlessClient.Dialogs
{
    [Flags]
    public enum ScrollingListDialogButtons
    {
        None        = 0x00,
        Add         = 0x01,
        Cancel      = 0x02,
        Back        = 0x04,
        Next        = 0x08,
        Ok          = 0x10,
        History     = 0x20,
        Progress    = 0x40,
        DualButtons = 0x80,

        AddCancel   = DualButtons | Add | Cancel,
        BackCancel  = DualButtons | Back | Cancel,
        BackOk      = DualButtons | Back | Ok,
        CancelOk    = DualButtons | Cancel | Ok,
        BackNext    = DualButtons | Back | Next,
        CancelNext  = DualButtons | Cancel | Next,
        HistoryOk   = DualButtons | History | Ok,
        ProgressOk  = DualButtons | Progress | Ok,
    }

    public enum ScrollingListDialogSize
    {
        Large,            // standard dialog with large list items (locker, shop, friend/ignore list)
        LargeNoScroll,    // standard dialog with large list items / no scrollbar (chest)
        Medium,           // quest progress/history dialog
        MediumWithHeader, // todo: implement boards
        Small,            // npc quest dialog
        SmallNoScroll,    // bank account dialog
    }

    public class ScrollingListDialog : BaseEODialog
    {
        private readonly List<ListDialogItem> _listItems;
        protected readonly ScrollBar _scrollBar;

        protected readonly IXNALabel _titleText;
        private ListDialogItem.ListItemStyle _listItemType;

        protected readonly XNAButton _add, _back, _cancel;
        protected readonly XNAButton _next, _ok;
        protected readonly XNAButton _history, _progress;

        protected readonly Vector2 _button1Position, _button2Position, _buttonCenterPosition;

        private ScrollingListDialogButtons _buttons;

        // cancel button debounce
        private bool _otherClicked;

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

                switch (DialogSize)
                {
                    case ScrollingListDialogSize.Large: return 12;
                    case ScrollingListDialogSize.Medium: return 10;
                    case ScrollingListDialogSize.Small: return 6;
                    default: throw new NotImplementedException();
                }
            }
        }

        public ListDialogItem.ListItemStyle ListItemType
        {
            get => _listItemType;
            set
            {
                if (value == ListDialogItem.ListItemStyle.Large && DialogSize == ScrollingListDialogSize.Small)
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

                if (Buttons.HasFlag(ScrollingListDialogButtons.DualButtons))
                {
                    if (Buttons == ScrollingListDialogButtons.BackCancel ||
                        Buttons == ScrollingListDialogButtons.AddCancel)
                    {
                        _add.DrawPosition = _button1Position;
                        _back.DrawPosition = _button1Position;
                        _cancel.DrawPosition = _button2Position;
                    }
                    else
                    {
                        _back.DrawPosition = _button1Position;
                        _cancel.DrawPosition = _button1Position;
                        _history.DrawPosition = _button1Position;
                        _progress.DrawPosition = _button1Position;

                        _next.DrawPosition = _button2Position;
                        _ok.DrawPosition = _button2Position;
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

        public INativeGraphicsManager GraphicsManager { get; }

        public ScrollingListDialogSize DialogSize { get; }

        public event EventHandler AddAction;

        public event EventHandler BackAction;

        public event EventHandler NextAction;

        public event EventHandler HistoryAction;

        public event EventHandler ProgressAction;

        public bool ChildControlClickHandled { get; set; }

        public ScrollingListDialog(INativeGraphicsManager nativeGraphicsManager,
                                   IEODialogButtonService dialogButtonService,
                                   ScrollingListDialogSize dialogSize = ScrollingListDialogSize.Large)
            : base(isInGame: true)
        {
            // todo: implement boards
            if (dialogSize == ScrollingListDialogSize.MediumWithHeader)
                throw new NotImplementedException();

            GraphicsManager = nativeGraphicsManager;
            DialogSize = dialogSize;

            _listItems = new List<ListDialogItem>();

            _titleText = new XNALabel(Constants.FontSize09)
            {
                DrawArea = GetTitleDrawArea(DialogSize),
                AutoSize = false,
                TextAlign = LabelAlignment.MiddleLeft,
                ForeColor = ColorConstants.LightGrayText,
                Visible = DialogSize != ScrollingListDialogSize.LargeNoScroll,
            };
            _titleText.SetParentControl(this);

            _scrollBar = new ScrollBar(new Vector2(DialogSize == ScrollingListDialogSize.Medium ? 449 : 252, 44), new Vector2(16, GetScrollBarHeight(DialogSize)), ScrollBarColors.LightOnMed, GraphicsManager)
            {
                Visible = DialogSize != ScrollingListDialogSize.LargeNoScroll && DialogSize != ScrollingListDialogSize.SmallNoScroll,
            };
            _scrollBar.SetParentControl(this);

            BackgroundTexture = GraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, GetBackgroundTexture(DialogSize));
            BackgroundTextureSource = GetBackgroundSourceRectangle(BackgroundTexture, DialogSize);

            _add = new XNAButton(dialogButtonService.SmallButtonSheet, Vector2.Zero,
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Add),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Add))
            { 
                Visible = false,
                UpdateOrder = 1,
            };
            _add.SetParentControl(this);
            _add.OnClick += (o, e) => AddAction?.Invoke(o, e);
            AddAction += (_, _) => _otherClicked = true;

            _back = new XNAButton(dialogButtonService.SmallButtonSheet, Vector2.Zero,
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Back),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Back))
            {
                Visible = false,
                UpdateOrder = 1,
            };
            _back.SetParentControl(this);
            _back.OnClick += (o, e) => BackAction?.Invoke(o, e);
            BackAction += (_, _) => _otherClicked = true;

            _next = new XNAButton(dialogButtonService.SmallButtonSheet, Vector2.Zero,
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Next),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Next))
            {
                Visible = false,
                UpdateOrder = 1,
            };
            _next.SetParentControl(this);
            _next.OnClick += (o, e) => NextAction?.Invoke(o, e);
            NextAction += (_, _) => _otherClicked = true;

            _history = new XNAButton(dialogButtonService.SmallButtonSheet, Vector2.Zero,
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.History),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.History))
            {
                Visible = false,
                UpdateOrder = 1,
            };
            _history.SetParentControl(this);
            _history.OnClick += (o, e) => HistoryAction?.Invoke(o, e);
            HistoryAction += (_, _) => _otherClicked = true;

            _progress = new XNAButton(dialogButtonService.SmallButtonSheet, Vector2.Zero,
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Progress),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Progress))
            {
                Visible = false,
                UpdateOrder = 1,
            };
            _progress.SetParentControl(this);
            _progress.OnClick += (o, e) => ProgressAction?.Invoke(o, e);
            ProgressAction += (_, _) => _otherClicked = true;

            _ok = new XNAButton(dialogButtonService.SmallButtonSheet, Vector2.Zero,
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Ok),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Ok))
            {
                Visible = false,
                UpdateOrder = 2,
            };
            _ok.SetParentControl(this);
            _ok.OnClick += (_, _) => { if (!_otherClicked) { Close(XNADialogResult.OK); } };

            _cancel = new XNAButton(dialogButtonService.SmallButtonSheet, Vector2.Zero,
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Cancel),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Cancel))
            {
                Visible = false,
                UpdateOrder = 2,
            };
            _cancel.SetParentControl(this);
            _cancel.OnClick += (_, _) => { if (!_otherClicked) { Close(XNADialogResult.Cancel); } };

            _button1Position = GetButton1Position(DrawArea, _ok.DrawArea, DialogSize);
            _button2Position = GetButton2Position(DrawArea, _ok.DrawArea, DialogSize);
            _buttonCenterPosition = GetButtonCenterPosition(DrawArea, _ok.DrawArea, DialogSize);

            Buttons = ScrollingListDialogButtons.AddCancel;

            CenterInGameView();
            DrawPosition = new Vector2(DrawPosition.X, 15);
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

        public void ClearItemList()
        {
            foreach (var item in _listItems)
                item.Dispose();

            _listItems.Clear();
            _scrollBar.UpdateDimensions(0);
            _scrollBar.ScrollToTop();
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
            ChildControlClickHandled = false;

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

            _otherClicked = false;
        }

        private static Rectangle GetTitleDrawArea(ScrollingListDialogSize size)
        {
            switch(size)
            {
                case ScrollingListDialogSize.Large:
                case ScrollingListDialogSize.LargeNoScroll: return new Rectangle(16, 13, 253, 19);
                case ScrollingListDialogSize.Medium: return new Rectangle(18, 14, 452, 19);
                case ScrollingListDialogSize.Small: return new Rectangle(16, 16, 255, 18);
                case ScrollingListDialogSize.SmallNoScroll: return new Rectangle(129, 20, 121, 16);
                default: throw new NotImplementedException();
            }
        }

        private static int GetScrollBarHeight(ScrollingListDialogSize size)
        {
            switch (size)
            {
                case ScrollingListDialogSize.Large:
                case ScrollingListDialogSize.LargeNoScroll:
                case ScrollingListDialogSize.Medium: return 199;
                case ScrollingListDialogSize.Small:
                case ScrollingListDialogSize.SmallNoScroll: return 99;
                default: throw new NotImplementedException();
            }
        }

        private static int GetBackgroundTexture(ScrollingListDialogSize size)
        {
            switch (size)
            {
                case ScrollingListDialogSize.Large: return 52;
                case ScrollingListDialogSize.LargeNoScroll: return 51;
                case ScrollingListDialogSize.Medium: return 59;
                case ScrollingListDialogSize.Small: return 67;
                case ScrollingListDialogSize.SmallNoScroll: return 53;
                default: throw new NotImplementedException();
            }
        }

        private static Rectangle? GetBackgroundSourceRectangle(Texture2D backgroundTexture, ScrollingListDialogSize size)
        {
            switch (size)
            {
                case ScrollingListDialogSize.Large:
                case ScrollingListDialogSize.LargeNoScroll: return null;
                case ScrollingListDialogSize.Medium: return new Rectangle(0, 0, backgroundTexture.Width, backgroundTexture.Height / 2);
                case ScrollingListDialogSize.Small:
                case ScrollingListDialogSize.SmallNoScroll:  return null;
                default: throw new NotImplementedException();
            }
        }

        private static Vector2 GetButton1Position(Rectangle dialogArea, Rectangle buttonArea, ScrollingListDialogSize size)
        {
            var yCoord = GetButtonYCoordinate(dialogArea);
            switch (size)
            {
                // buttons are centered on these dialogs
                case ScrollingListDialogSize.Large: 
                case ScrollingListDialogSize.LargeNoScroll:
                case ScrollingListDialogSize.SmallNoScroll: return new Vector2((int)Math.Floor((dialogArea.Width - buttonArea.Width) / 2.0) - 48, yCoord);
                // buttons are offset from center on these dialogs
                case ScrollingListDialogSize.Medium: return new Vector2(288, yCoord);
                case ScrollingListDialogSize.Small: return new Vector2(89, yCoord);
                default: throw new NotImplementedException();
            }
        }

        private static Vector2 GetButton2Position(Rectangle dialogArea, Rectangle buttonArea, ScrollingListDialogSize size)
        {
            var yCoord = GetButtonYCoordinate(dialogArea);
            switch (size)
            {
                // buttons are centered on these dialogs
                case ScrollingListDialogSize.Large:
                case ScrollingListDialogSize.LargeNoScroll:
                case ScrollingListDialogSize.SmallNoScroll: return new Vector2((int)Math.Floor((dialogArea.Width - buttonArea.Width) / 2.0) + 48, yCoord);
                // buttons are offset from center on these dialogs
                case ScrollingListDialogSize.Medium: return new Vector2(380, yCoord);
                case ScrollingListDialogSize.Small: return new Vector2(183, yCoord);
                default: throw new NotImplementedException();
            }
        }

        private static Vector2 GetButtonCenterPosition(Rectangle dialogArea, Rectangle buttonArea, ScrollingListDialogSize dialogSize)
        {
            // chest dialog has a button built in to the graphic that needs to be covered up...
            if (dialogSize == ScrollingListDialogSize.LargeNoScroll)
                return new Vector2(92, 227);

            // bank dialog has a button built in to the graphic that needs to be covered up...
            if (dialogSize == ScrollingListDialogSize.SmallNoScroll)
                return new Vector2(92, 191);

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
