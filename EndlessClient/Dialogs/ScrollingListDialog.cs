using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public enum ScrollingListDialogButtons
    {
        AddCancel,
        Cancel,
        BackCancel,
    }

    public class ScrollingListDialog : BaseEODialog
    {
        private static readonly Vector2 _cancelButtonRightPosition, _cancelButtonCenteredPosition;

        private readonly List<ListDialogItem> _listItems;
        protected readonly ScrollBar _scrollBar;

        protected readonly IXNALabel _titleText;
        private ListDialogItem.ListItemStyle _listItemType;

        protected readonly XNAButton _add, _back, _cancel;

        private ScrollingListDialogButtons _buttons;

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
                _add.Visible = Buttons == ScrollingListDialogButtons.AddCancel;
                _back.Visible = Buttons == ScrollingListDialogButtons.BackCancel;
                _cancel.DrawPosition = Buttons == ScrollingListDialogButtons.Cancel
                    ? _cancelButtonCenteredPosition
                    : _cancelButtonRightPosition;
            }
        }

        public INativeGraphicsManager GraphicsManager { get; }

        public event EventHandler AddAction;

        public event EventHandler BackAction;

        public bool ChildControlClickHandled { get; set; }

        static ScrollingListDialog()
        {
            _cancelButtonRightPosition = new Vector2(144, 252);
            _cancelButtonCenteredPosition = new Vector2(96, 252);
        }

        public ScrollingListDialog(IGameStateProvider gameStateProvider,
                                   INativeGraphicsManager nativeGraphicsManager,
                                   IEODialogButtonService dialogButtonService)
            : base(gameStateProvider)
        {
            _listItems = new List<ListDialogItem>();

            _titleText = new XNALabel(Constants.FontSize08pt75)
            {
                DrawArea = new Rectangle(16, 13, 253, 19),
                AutoSize = false,
                TextAlign = LabelAlignment.MiddleLeft,
                ForeColor = ColorConstants.LightGrayText
            };
            _titleText.SetParentControl(this);

            _scrollBar = new ScrollBar(new Vector2(252, 44), new Vector2(16, 199), ScrollBarColors.LightOnMed, nativeGraphicsManager);
            _scrollBar.SetParentControl(this);

            _add = new XNAButton(dialogButtonService.SmallButtonSheet,
                new Vector2(48, 252),
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Add),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Add))
            { 
                Visible = false
            };
            _add.SetParentControl(this);
            _add.OnClick += (o, e) => AddAction?.Invoke(o, e);

            _back = new XNAButton(dialogButtonService.SmallButtonSheet,
                new Vector2(48, 252),
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Back),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Back))
            {
                Visible = false
            };
            _back.SetParentControl(this);
            _back.OnClick += (o, e) => BackAction?.Invoke(o, e);

            _cancel = new XNAButton(dialogButtonService.SmallButtonSheet,
                _cancelButtonRightPosition,
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Cancel),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Cancel))
            {
                Visible = true
            };
            _cancel.SetParentControl(this);
            _cancel.OnClick += (_, _) => Close(XNADialogResult.Cancel);

            ItemsToShow = ListItemType == ListDialogItem.ListItemStyle.Large ? 5 : 12;

            BackgroundTexture = nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 52);

            CenterInGameView();
            DrawPosition = new Vector2(DrawPosition.X, 15);
            GraphicsManager = nativeGraphicsManager;
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
            _cancel.Initialize();
            _scrollBar.Initialize();
            _titleText.Initialize();

            base.Initialize();
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            base.OnUpdateControl(gameTime);

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
        }
    }
}
