using System.Collections.Generic;
using System.Linq;
using EndlessClient.Dialogs.Services;
using EndlessClient.Old;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Graphics;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls.Old;

namespace EndlessClient.Dialogs.Old
{
    public enum ScrollingListDialogButtons
    {
        AddCancel,
        Cancel,
        BackCancel,
    }

    public class OldScrollingListDialog : EODialogBase
    {
        //needs: - title label
        //         - scroll bar
        //         - lower buttons (configurable)
        //         - list of items (names, like friend list; items, like shop)
        //OldListDialogItem needs way to set width and offsets

        private readonly List<OldListDialogItem> m_listItems = new List<OldListDialogItem>();
        private readonly object m_listItemLock = new object();
        protected OldScrollBar m_scrollBar;

        /// <summary>
        /// List of strings containing the primary text field of each child item
        /// </summary>
        public List<string> NamesList
        {
            get
            {
                lock (m_listItemLock)
                    return m_listItems.Select(item => item.Text).ToList();
            }
        }

        protected XNALabel m_titleText;
        private OldListDialogItem.ListItemStyle _listItemType;
        private ScrollingListDialogButtons _buttons;

        public string Title
        {
            get { return m_titleText.Text; }
            set { m_titleText.Text = value; }
        }

        /// <summary>
        /// The number of items to display in the scrolling view at one time when ListItemType is Large
        /// </summary>
        public int LargeItemStyleMaxItemDisplay { get; set; }

        /// <summary>
        /// The number of items to display in the scrolling view at one time when ListItemType is Small
        /// </summary>
        public int SmallItemStyleMaxItemDisplay { get; set; }

        public OldListDialogItem.ListItemStyle ListItemType
        {
            get { return _listItemType; }
            set
            {
                _listItemType = value;
                m_scrollBar.LinesToRender = ListItemType == OldListDialogItem.ListItemStyle.Small
                    ? SmallItemStyleMaxItemDisplay
                    : LargeItemStyleMaxItemDisplay;
            }
        }

        public ScrollingListDialogButtons Buttons
        {
            get { return _buttons; }
            set
            {
                _buttons = value;
                _setButtons(_buttons);
            }
        }

        public OldScrollingListDialog(PacketAPI api = null)
            : base(api)
        {
            _setBackgroundTexture(((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 52));

            //defaults
            LargeItemStyleMaxItemDisplay = 5;
            SmallItemStyleMaxItemDisplay = 12;

            m_titleText = new XNALabel(new Rectangle(16, 13, 253, 19), Constants.FontSize08pt75)
            {
                AutoSize = false,
                TextAlign = LabelAlignment.MiddleLeft,
                ForeColor = ColorConstants.LightGrayText
            };
            m_titleText.SetParent(this);

            m_scrollBar = new OldScrollBar(this, new Vector2(252, 44), new Vector2(16, 199), ScrollBarColors.LightOnMed);

            Center(Game.GraphicsDevice);
            DrawLocation = new Vector2(DrawLocation.X, 15);
            endConstructor(false);
        }

        public void SetItemList(List<OldListDialogItem> itemList)
        {
            if (itemList.Count == 0) return;

            if (m_listItems.Count == 0)
                m_scrollBar.LinesToRender = itemList[0].Style == OldListDialogItem.ListItemStyle.Large ? 5 : 12;

            m_scrollBar.UpdateDimensions(itemList.Count);

            OldListDialogItem.ListItemStyle firstStyle = itemList[0].Style;
            lock (m_listItemLock)
                for (int i = 0; i < itemList.Count; ++i)
                {
                    m_listItems.Add(itemList[i]);
                    m_listItems[i].Style = firstStyle;
                    m_listItems[i].Index = i;
                    if (i > m_scrollBar.LinesToRender)
                        m_listItems[i].Visible = false;
                }
        }

        public void AddItemToList(OldListDialogItem item, bool sortList)
        {
            if (m_listItems.Count == 0)
                m_scrollBar.LinesToRender = item.Style == OldListDialogItem.ListItemStyle.Large ? LargeItemStyleMaxItemDisplay : SmallItemStyleMaxItemDisplay;
            lock (m_listItemLock)
            {
                m_listItems.Add(item);
                if (sortList)
                    m_listItems.Sort((item1, item2) => item1.Text.CompareTo(item2.Text));
                for (int i = 0; i < m_listItems.Count; ++i)
                    m_listItems[i].Index = i;
            }
            m_scrollBar.UpdateDimensions(m_listItems.Count);
        }

        public void RemoveFromList(OldListDialogItem item)
        {
            int ndx;
            lock (m_listItemLock)
                ndx = m_listItems.FindIndex(_item => _item == item);
            if (ndx < 0) return;

            item.Close();

            lock (m_listItemLock)
            {
                m_listItems.RemoveAt(ndx);

                m_scrollBar.UpdateDimensions(m_listItems.Count);
                if (m_listItems.Count <= m_scrollBar.LinesToRender)
                    m_scrollBar.ScrollToTop();

                for (int i = 0; i < m_listItems.Count; ++i)
                {
                    //adjust indices (determines drawing position)
                    m_listItems[i].Index = i;
                }
            }
        }

        public void SetActiveItemList(List<string> activeLabels)
        {
            lock (m_listItemLock)
                foreach (OldListDialogItem item in m_listItems)
                {
                    if (activeLabels.Select(x => x.ToLower()).Contains(item.Text.ToLower()))
                    {
                        item.SetActive();
                    }
                }
        }

        protected void ClearItemList()
        {
            lock (m_listItemLock)
            {
                foreach (OldListDialogItem item in m_listItems)
                {
                    item.SetParent(null);
                    item.Close();
                }
                m_listItems.Clear();
            }
            m_scrollBar.UpdateDimensions(0);
            m_scrollBar.ScrollToTop();
        }

        protected void _setBackgroundTexture(Texture2D text)
        {
            bgTexture = text;
            _setSize(bgTexture.Width, bgTexture.Height);
        }

        protected void _setButtons(ScrollingListDialogButtons setButtons)
        {
            if (dlgButtons.Count > 0)
            {
                dlgButtons.ForEach(_btn =>
                {
                    _btn.SetParent(null);
                    _btn.Close();
                });

                dlgButtons.Clear();
            }

            _buttons = setButtons;
            switch (setButtons)
            {
                case ScrollingListDialogButtons.BackCancel:
                case ScrollingListDialogButtons.AddCancel:
                    {
                        SmallButton which = setButtons == ScrollingListDialogButtons.BackCancel ? SmallButton.Back : SmallButton.Add;
                        XNAButton add = new XNAButton(smallButtonSheet, new Vector2(48, 252), _getSmallButtonOut(which), _getSmallButtonOver(which));
                        add.SetParent(this);
                        add.OnClick += (o, e) => Close(add, setButtons == ScrollingListDialogButtons.BackCancel ? XNADialogResult.Back : XNADialogResult.Add);
                        XNAButton cancel = new XNAButton(smallButtonSheet, new Vector2(144, 252), _getSmallButtonOut(SmallButton.Cancel), _getSmallButtonOver(SmallButton.Cancel));
                        cancel.SetParent(this);
                        cancel.OnClick += (o, e) => Close(cancel, XNADialogResult.Cancel);

                        dlgButtons.Add(add);
                        dlgButtons.Add(cancel);
                    }
                    break;
                case ScrollingListDialogButtons.Cancel:
                    {
                        XNAButton cancel = new XNAButton(smallButtonSheet, new Vector2(96, 252), _getSmallButtonOut(SmallButton.Cancel), _getSmallButtonOver(SmallButton.Cancel));
                        cancel.SetParent(this);
                        cancel.OnClick += (o, e) => Close(cancel, XNADialogResult.Cancel);

                        dlgButtons.Add(cancel);
                    }
                    break;
            }
        }

        public override void Update(GameTime gt)
        {
            //which items should we render?
            lock (m_listItemLock)
            {
                if (m_listItems.Count > m_scrollBar.LinesToRender)
                {
                    for (int i = 0; i < m_listItems.Count; ++i)
                    {
                        OldListDialogItem curr = m_listItems[i];
                        if (i < m_scrollBar.ScrollOffset)
                        {
                            curr.Visible = false;
                            continue;
                        }

                        if (i < m_scrollBar.LinesToRender + m_scrollBar.ScrollOffset)
                        {
                            curr.Visible = true;
                            curr.Index = i - m_scrollBar.ScrollOffset;
                        }
                        else
                        {
                            curr.Visible = false;
                        }
                    }
                }
                else if (m_listItems.Any(_item => !_item.Visible))
                    m_listItems.ForEach(_item => _item.Visible = true); //all items visible if less than # lines to render
            }

            base.Update(gt);
        }
    }
}
