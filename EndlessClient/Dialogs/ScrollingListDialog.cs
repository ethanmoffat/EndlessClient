using EndlessClient.Dialogs.Services;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using XNAControls;

namespace EndlessClient.Dialogs
{
    [Flags]
    public enum ScrollingListDialogButtons
    {
        Add = 1,
        Cancel = 2,
        Back = 4,
        Next = 8,
        Ok = 16,
        DualButtons = 32,
        // todo: if enum values are ever added to this, the logic in ScrollingListDialog.Buttons needs to be updated
        AddCancel = DualButtons | Add | Cancel,
        BackCancel = DualButtons | Back | Cancel,
        BackOk = DualButtons | Back | Ok,
        CancelOk = DualButtons | Cancel | Ok,
        BackNext = DualButtons | Back | Next,
        CancelNext = DualButtons | Cancel | Next,
    }

    public enum ScrollingListDialogSize
    {
        LargeDialog,
        SmallDialog,
    }

    public class ScrollingListDialog : BaseEODialog
    {
        private readonly List<ListDialogItem> _listItems;
        protected readonly ScrollBar _scrollBar;

        protected readonly IXNALabel _titleText;
        private ListDialogItem.ListItemStyle _listItemType;

        protected readonly XNAButton _add, _back, _cancel;
        protected readonly XNAButton _next, _ok;

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

        public int ItemsToShow { get; set; }

        public ListDialogItem.ListItemStyle ListItemType
        {
            get => _listItemType;
            set
            {
                if (value == ListDialogItem.ListItemStyle.Large && DialogSize == ScrollingListDialogSize.SmallDialog)
                    throw new InvalidOperationException("Can't use large ListDialogItem with small scrolling dialog");

                _listItemType = value;
                ItemsToShow = _listItemType == ListDialogItem.ListItemStyle.Large ? 5 : DialogSize == ScrollingListDialogSize.SmallDialog ? 6 : 12;
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

        public bool ChildControlClickHandled { get; set; }

        public ScrollingListDialog(INativeGraphicsManager nativeGraphicsManager,
                                   IEODialogButtonService dialogButtonService,
                                   ScrollingListDialogSize dialogSize = ScrollingListDialogSize.LargeDialog)
            : base(isInGame: true)
        {
            GraphicsManager = nativeGraphicsManager;
            DialogSize = dialogSize;

            var isLargeDialog = DialogSize == ScrollingListDialogSize.LargeDialog;

            _listItems = new List<ListDialogItem>();

            _titleText = new XNALabel(Constants.FontSize09)
            {
                DrawArea = isLargeDialog ? new Rectangle(16, 13, 253, 19) : new Rectangle(16, 16, 255, 18),
                AutoSize = false,
                TextAlign = LabelAlignment.MiddleLeft,
                ForeColor = ColorConstants.LightGrayText
            };
            _titleText.SetParentControl(this);

            _scrollBar = new ScrollBar(new Vector2(252, 44), new Vector2(16, isLargeDialog ? 199 : 99), ScrollBarColors.LightOnMed, GraphicsManager);
            _scrollBar.SetParentControl(this);

            BackgroundTexture = GraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, isLargeDialog ? 52 : 67);

            var yCoord = isLargeDialog ? 252 : 152;

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

            ItemsToShow = ListItemType == ListDialogItem.ListItemStyle.Large ? 5 : DialogSize == ScrollingListDialogSize.SmallDialog ? 6 : 12;

            _button1Position = new Vector2(isLargeDialog ? 48 : 89, yCoord);
            _button2Position = new Vector2(isLargeDialog ? 144 : 183, yCoord);
            _buttonCenterPosition = new Vector2(96, yCoord);

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
            }
        }

        public void AddItemToList(ListDialogItem item, bool sortList)
        {
            _listItems.Add(item);

            if (sortList)
                _listItems.Sort((item1, item2) => item1.PrimaryText.CompareTo(item2.PrimaryText));

            for (int i = 0; i < _listItems.Count; ++i)
                _listItems[i].Index = i;

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
    }
}
