// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EndlessClient.Dialogs.Services;
using EndlessClient.Old;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.Localization;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using XNAControls.Old;

namespace EndlessClient.Dialogs
{
    public class TradeDialog : EODialogBase
    {
        //dialog has:
        // - 2 lists of items on each side
        // - 2 scroll bars (1 each side)
        // - 2 name labels (1 each side)
        // - 2 agree/trading labels (1 each side)
        // - Ok/cancel buttons

        private short m_leftPlayerID, m_rightPlayerID;
        private string m_leftNameStr, m_rightNameStr;
        private readonly XNALabel m_leftPlayerName, m_rightPlayerName;
        private readonly XNALabel m_leftPlayerStatus, m_rightPlayerStatus;
        private readonly ScrollBar m_leftScroll, m_rightScroll;
        private bool m_leftAgrees, m_rightAgrees;
        private readonly List<ListDialogItem> m_leftItems, m_rightItems;

        private readonly OldCharacter m_main; //local reference

        private int m_recentPartnerRemoves;

        public static TradeDialog Instance { get; private set; }

        public bool MainPlayerAgrees
        {
            get
            {
                return (m_main.ID == m_leftPlayerID && m_leftAgrees) ||
                       (m_main.ID == m_rightPlayerID && m_rightAgrees);
            }
        }

        public TradeDialog(PacketAPI apiHandle)
            : base(apiHandle)
        {
            bgTexture = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 50);
            _setSize(bgTexture.Width, bgTexture.Height);

            Instance = this;
            DialogClosing += (sender, args) => Instance = null;
            m_main = OldWorld.Instance.MainPlayer.ActiveCharacter;

            m_leftItems = new List<ListDialogItem>();
            m_rightItems = new List<ListDialogItem>();

            m_leftPlayerID = 0;
            m_rightPlayerID = 0;

            m_leftPlayerName = new XNALabel(new Rectangle(20, 14, 166, 20), Constants.FontSize08pt5)
            {
                AutoSize = false,
                TextAlign = LabelAlignment.MiddleLeft,
                ForeColor = ColorConstants.LightGrayText
            };
            m_leftPlayerName.SetParent(this);
            m_rightPlayerName = new XNALabel(new Rectangle(285, 14, 166, 20), Constants.FontSize08pt5)
            {
                AutoSize = false,
                TextAlign = LabelAlignment.MiddleLeft,
                ForeColor = ColorConstants.LightGrayText
            };
            m_rightPlayerName.SetParent(this);
            m_leftPlayerStatus = new XNALabel(new Rectangle(195, 14, 79, 20), Constants.FontSize08pt5)
            {
                AutoSize = false,
                TextAlign = LabelAlignment.MiddleLeft,
                Text = OldWorld.GetString(EOResourceID.DIALOG_TRADE_WORD_TRADING),
                ForeColor = ColorConstants.LightGrayText
            };
            m_leftPlayerStatus.SetParent(this);
            m_rightPlayerStatus = new XNALabel(new Rectangle(462, 14, 79, 20), Constants.FontSize08pt5)
            {
                AutoSize = false,
                TextAlign = LabelAlignment.MiddleLeft,
                Text = OldWorld.GetString(EOResourceID.DIALOG_TRADE_WORD_TRADING),
                ForeColor = ColorConstants.LightGrayText
            };
            m_rightPlayerStatus.SetParent(this);

            m_leftScroll = new ScrollBar(this, new Vector2(252, 44), new Vector2(16, 199), ScrollBarColors.LightOnMed) { LinesToRender = 5 };
            m_rightScroll = new ScrollBar(this, new Vector2(518, 44), new Vector2(16, 199), ScrollBarColors.LightOnMed) { LinesToRender = 5 };

            //BUTTONSSSS
            XNAButton ok = new XNAButton(smallButtonSheet, new Vector2(356, 252), _getSmallButtonOut(SmallButton.Ok),
                _getSmallButtonOver(SmallButton.Ok));
            ok.OnClick += _buttonOkClicked;
            ok.SetParent(this);
            dlgButtons.Add(ok);
            XNAButton cancel = new XNAButton(smallButtonSheet, new Vector2(449, 252), _getSmallButtonOut(SmallButton.Cancel),
                _getSmallButtonOver(SmallButton.Cancel));
            cancel.OnClick += _buttonCancelClicked;
            cancel.SetParent(this);
            dlgButtons.Add(cancel);

            Timer localTimer = new Timer(state =>
            {
                if (m_recentPartnerRemoves > 0)
                    m_recentPartnerRemoves--;
            }, null, 0, 5000);

            DialogClosing += (o, e) =>
            {
                if (e.Result == XNADialogResult.Cancel)
                {
                    if (!m_api.TradeClose())
                        ((EOGame)Game).DoShowLostConnectionDialogAndReturnToMainMenu();
                    ((EOGame)Game).Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION, EOResourceID.STATUS_LABEL_TRADE_ABORTED);
                }

                localTimer.Dispose();
            };

            Center(Game.GraphicsDevice);
            DrawLocation = new Vector2(DrawLocation.X, 30);
            endConstructor(false);
        }

        public void InitPlayerInfo(short player1, string player1Name, short player2, string player2Name)
        {
            m_leftPlayerID = player1;
            m_rightPlayerID = player2;
            m_leftNameStr = m_leftPlayerName.Text = char.ToUpper(player1Name[0]) + player1Name.Substring(1);
            m_rightNameStr = m_rightPlayerName.Text = char.ToUpper(player2Name[0]) + player2Name.Substring(1);
        }

        public void SetPlayerItems(short playerID, List<InventoryItem> items)
        {
            int xOffset;
            List<ListDialogItem> collectionRef;
            ScrollBar scrollRef;

            if (playerID == m_leftPlayerID)
            {
                collectionRef = m_leftItems;
                scrollRef = m_leftScroll;
                xOffset = -3;
                m_leftPlayerName.Text = string.Format("{0} {1}", m_leftNameStr, items.Count > 0 ? "[" + items.Count + "]" : "");

                if (m_leftAgrees)
                {
                    m_leftAgrees = false;
                    m_leftPlayerStatus.Text = OldWorld.GetString(EOResourceID.DIALOG_TRADE_WORD_TRADING);
                }

                //left player is NOT main, and right player (ie main) agrees, and the item count is different for left player
                //cancel the offer for the main player since the other player changed the offer
                if (m_main.ID != playerID && m_rightAgrees && collectionRef.Count != items.Count)
                {
                    m_rightAgrees = false;
                    m_rightPlayerStatus.Text = OldWorld.GetString(EOResourceID.DIALOG_TRADE_WORD_TRADING);
                    EOMessageBox.Show(DialogResourceID.TRADE_ABORTED_OFFER_CHANGED, EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                    ((EOGame)Game).Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.STATUS_LABEL_TRADE_OTHER_PLAYER_CHANGED_OFFER);
                }
            }
            else if (playerID == m_rightPlayerID)
            {
                collectionRef = m_rightItems;
                scrollRef = m_rightScroll;
                xOffset = 263;
                m_rightPlayerName.Text = string.Format("{0} {1}", m_rightNameStr, items.Count > 0 ? "[" + items.Count + "]" : "");

                if (m_rightAgrees)
                {
                    m_rightAgrees = false;
                    m_rightPlayerStatus.Text = OldWorld.GetString(EOResourceID.DIALOG_TRADE_WORD_TRADING);
                }

                //right player is NOT main, and left player (ie main) agrees, and the item count is different for right player
                //cancel the offer for the main player since the other player changed the offer
                if (m_main.ID != playerID && m_leftAgrees && collectionRef.Count != items.Count)
                {
                    m_leftAgrees = false;
                    m_leftPlayerStatus.Text = OldWorld.GetString(EOResourceID.DIALOG_TRADE_WORD_TRADING);
                    EOMessageBox.Show(DialogResourceID.TRADE_ABORTED_OFFER_CHANGED, EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                    ((EOGame)Game).Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.STATUS_LABEL_TRADE_OTHER_PLAYER_CHANGED_OFFER);
                }
            }
            else
                throw new ArgumentException("Invalid Player ID for trade session!", "playerID");

            if (m_main.ID != playerID && collectionRef.Count > items.Count)
                m_recentPartnerRemoves++;
            if (m_recentPartnerRemoves == 3)
            {
                EOMessageBox.Show(DialogResourceID.TRADE_OTHER_PLAYER_TRICK_YOU, EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                m_recentPartnerRemoves = -1000; //this will prevent the message from showing more than once (I'm too lazy to find something more elegant)
            }

            foreach (var oldItem in collectionRef) oldItem.Close();
            collectionRef.Clear();

            int index = 0;
            foreach (InventoryItem item in items)
            {
                int localID = item.ItemID;

                var rec = OldWorld.Instance.EIF[item.ItemID];
                string secondary = string.Format("x {0}  {1}", item.Amount, rec.Type == ItemType.Armor
                    ? "(" + (rec.Gender == 0 ? OldWorld.GetString(EOResourceID.FEMALE) : OldWorld.GetString(EOResourceID.MALE)) + ")"
                    : "");

                int gfxNum = item.ItemID == 1
                    ? 269 + 2 * (item.Amount >= 100000 ? 4 : (item.Amount >= 10000 ? 3 : (item.Amount >= 100 ? 2 : (item.Amount >= 2 ? 1 : 0))))
                    : 2 * rec.Graphic - 1;

                var nextItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, index++)
                {
                    Text = rec.Name,
                    SubText = secondary,
                    IconGraphic = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.Items, gfxNum, true),
                    ID = item.ItemID,
                    Amount = item.Amount,
                    OffsetX = xOffset,
                    OffsetY = 46
                };
                if (playerID == m_main.ID)
                    nextItem.OnRightClick += (sender, args) => _removeItem(localID);
                collectionRef.Add(nextItem);
            }

            scrollRef.UpdateDimensions(collectionRef.Count);
        }

        public void SetPlayerAgree(bool isMain, bool agrees)
        {
            short playerID = isMain ? (short)m_main.ID : (m_leftPlayerID == m_main.ID ? m_rightPlayerID : m_leftPlayerID);
            if (playerID == m_leftPlayerID)
            {
                if (agrees && !m_leftAgrees)
                    ((EOGame)Game).Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION,
                        isMain ? EOResourceID.STATUS_LABEL_TRADE_YOU_ACCEPT : EOResourceID.STATUS_LABEL_TRADE_OTHER_ACCEPT);
                else if (!agrees && m_leftAgrees)
                    ((EOGame)Game).Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION,
                        isMain ? EOResourceID.STATUS_LABEL_TRADE_YOU_CANCEL : EOResourceID.STATUS_LABEL_TRADE_OTHER_CANCEL);

                m_leftAgrees = agrees;
                m_leftPlayerStatus.Text =
                    OldWorld.GetString(agrees ? EOResourceID.DIALOG_TRADE_WORD_AGREE : EOResourceID.DIALOG_TRADE_WORD_TRADING);
            }
            else if (playerID == m_rightPlayerID)
            {
                if (agrees && !m_rightAgrees)
                    ((EOGame)Game).Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION,
                        isMain ? EOResourceID.STATUS_LABEL_TRADE_YOU_ACCEPT : EOResourceID.STATUS_LABEL_TRADE_OTHER_ACCEPT);
                else if (!agrees && m_rightAgrees)
                    ((EOGame)Game).Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION,
                        isMain ? EOResourceID.STATUS_LABEL_TRADE_YOU_CANCEL : EOResourceID.STATUS_LABEL_TRADE_OTHER_CANCEL);

                m_rightAgrees = agrees;
                m_rightPlayerStatus.Text =
                    OldWorld.GetString(agrees ? EOResourceID.DIALOG_TRADE_WORD_AGREE : EOResourceID.DIALOG_TRADE_WORD_TRADING);
            }
            else
                throw new ArgumentException("Invalid Player ID for trade session!");
        }

        public void CompleteTrade(short p1, List<InventoryItem> p1items, short p2, List<InventoryItem> p2items)
        {
            List<InventoryItem> mainCollection, otherCollection;
            if (p1 == m_main.ID)
            {
                mainCollection = p1items;
                otherCollection = p2items;
            }
            else if (p2 == m_main.ID)
            {
                mainCollection = p2items;
                otherCollection = p1items;
            }
            else
                throw new ArgumentException("Invalid player ID for trade session!");

            int weightDelta = 0;
            foreach (var item in mainCollection)
            {
                m_main.UpdateInventoryItem(item.ItemID, -item.Amount, true);
                weightDelta -= OldWorld.Instance.EIF[item.ItemID].Weight * item.Amount;
            }
            foreach (var item in otherCollection)
            {
                m_main.UpdateInventoryItem(item.ItemID, item.Amount, true);
                weightDelta += OldWorld.Instance.EIF[item.ItemID].Weight * item.Amount;
            }
            m_main.Weight += (byte)weightDelta;
            ((EOGame)Game).Hud.RefreshStats();

            Close(null, XNADialogResult.NO_BUTTON_PRESSED);
            EOMessageBox.Show(DialogResourceID.TRADE_SUCCESS, EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
        }

        private void _buttonOkClicked(object sender, EventArgs e)
        {
            if (m_leftPlayerID == m_main.ID)
            {
                if (m_leftAgrees) return; //main already agrees
            }
            else if (m_rightPlayerID == m_main.ID)
            {
                if (m_rightAgrees) return; //main already agrees
            }
            else
                throw new InvalidOperationException("Invalid Player ID for trade session!");

            if (m_leftItems.Count == 0 || m_rightItems.Count == 0)
            {
                EOMessageBox.Show(OldWorld.GetString(EOResourceID.DIALOG_TRADE_BOTH_PLAYERS_OFFER_ONE_ITEM),
                    OldWorld.GetString(EOResourceID.STATUS_LABEL_TYPE_WARNING), EODialogButtons.Ok,
                    EOMessageBoxStyle.SmallDialogSmallHeader);
                ((EOGame)Game).Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.DIALOG_TRADE_BOTH_PLAYERS_OFFER_ONE_ITEM);
                return;
            }

            List<ListDialogItem> mainCollection = m_main.ID == m_leftPlayerID ? m_leftItems : m_rightItems;
            List<ListDialogItem> otherCollection = m_main.ID == m_leftPlayerID ? m_rightItems : m_leftItems;

            //make sure that the items will fit!
            if (!((EOGame)Game).Hud.ItemsFit(
                    otherCollection.Select(_item => new InventoryItem(_item.ID, _item.Amount)).ToList(),
                    mainCollection.Select(_item => new InventoryItem(_item.ID, _item.Amount)).ToList()))
            {
                EOMessageBox.Show(OldWorld.GetString(EOResourceID.DIALOG_TRANSFER_NOT_ENOUGH_SPACE),
                    OldWorld.GetString(EOResourceID.STATUS_LABEL_TYPE_WARNING), EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                return;
            }

            //make sure the change in weight + existing weight is not greater than the max weight!
            int weightDelta = otherCollection.Sum(itemRef => OldWorld.Instance.EIF[itemRef.ID].Weight * itemRef.Amount);
            weightDelta = mainCollection.Aggregate(weightDelta, (current, itemRef) => current - OldWorld.Instance.EIF[itemRef.ID].Weight * itemRef.Amount);
            if (weightDelta + m_main.Weight > m_main.MaxWeight)
            {
                EOMessageBox.Show(OldWorld.GetString(EOResourceID.DIALOG_TRANSFER_NOT_ENOUGH_WEIGHT),
                    OldWorld.GetString(EOResourceID.STATUS_LABEL_TYPE_WARNING), EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                return;
            }

            EOMessageBox.Show(DialogResourceID.TRADE_DO_YOU_AGREE, EODialogButtons.OkCancel, EOMessageBoxStyle.SmallDialogSmallHeader,
                (o, dlgArgs) =>
                {
                    if (dlgArgs.Result == XNADialogResult.OK && !m_api.TradeAgree(true))
                    {
                        Close(null, XNADialogResult.NO_BUTTON_PRESSED);
                        ((EOGame)Game).DoShowLostConnectionDialogAndReturnToMainMenu();
                    }
                });
        }

        private void _buttonCancelClicked(object sender, EventArgs e)
        {
            if (m_main.ID == m_leftPlayerID)
            {
                if (!m_leftAgrees) //just quit
                    Close(dlgButtons[1], XNADialogResult.Cancel);
                else if (!m_api.TradeAgree(false)) //cancel agreement
                    ((EOGame)Game).DoShowLostConnectionDialogAndReturnToMainMenu();
            }
            else if (m_main.ID == m_rightPlayerID)
            {
                if (!m_rightAgrees) //just quit
                    Close(dlgButtons[1], XNADialogResult.Cancel);
                else if (!m_api.TradeAgree(false))
                    ((EOGame)Game).DoShowLostConnectionDialogAndReturnToMainMenu();
            }
            else
                throw new InvalidOperationException("Invalid player ID for trade session!");
        }

        //item right-click event handler
        private void _removeItem(int id)
        {
            if (!m_api.TradeRemoveItem((short)id))
            {
                Close(null, XNADialogResult.NO_BUTTON_PRESSED);
                ((EOGame)Game).DoShowLostConnectionDialogAndReturnToMainMenu();
            }
        }

        public override void Update(GameTime gt)
        {
            if (EOGame.Instance.Hud.IsInventoryDragging())
            {
                shouldClickDrag = false;
                SuppressParentClickDrag(true);
            }
            else
            {
                shouldClickDrag = true;
                SuppressParentClickDrag(false);
            }

            //do the hiding logic for both sides
            List<ScrollBar> scrollBars = new List<ScrollBar> { m_leftScroll, m_rightScroll };
            List<List<ListDialogItem>> lists = new List<List<ListDialogItem>> { m_leftItems, m_rightItems };
            for (int ndx = 0; ndx < 2; ++ndx)
            {
                var list = lists[ndx];
                var scroll = scrollBars[ndx];

                //which items should we render?
                if (list.Count > scroll.LinesToRender)
                {
                    for (int i = 0; i < list.Count; ++i)
                    {
                        ListDialogItem curr = list[i];
                        if (i < scroll.ScrollOffset)
                        {
                            curr.Visible = false;
                            continue;
                        }

                        if (i < scroll.LinesToRender + scroll.ScrollOffset)
                        {
                            curr.Visible = true;
                            curr.Index = i - scroll.ScrollOffset;
                        }
                        else
                        {
                            curr.Visible = false;
                        }
                    }
                }
                else if (list.Any(_item => !_item.Visible))
                    list.ForEach(_item => _item.Visible = true); //all items visible if less than # lines to render
            }

            base.Update(gt);
        }

        public void Close(XNADialogResult result)
        {
            Close(null, result);
            Close();
        }
    }
}
