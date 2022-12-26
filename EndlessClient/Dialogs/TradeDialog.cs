using EndlessClient.Dialogs.Factories;
using EndlessClient.Dialogs.Services;
using EndlessClient.HUD;
using EndlessClient.HUD.Inventory;
using EndlessClient.HUD.Panels;
using EndlessClient.Rendering.Map;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Trade;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Repositories;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using Optional.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class TradeDialog : BaseEODialog
    {
        private readonly ITradeActions _tradeActions;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IInventorySpaceValidator _inventorySpaceValidator;
        private readonly ITradeProvider _tradeProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly IMapItemGraphicProvider _mapItemGraphicProvider;
        private readonly InventoryPanel _inventoryPanel;

        private readonly IXNAPanel _leftPanel, _rightPanel;
        private readonly IXNALabel _leftPlayerName, _rightPlayerName;
        private readonly IXNALabel _leftPlayerStatus, _rightPlayerStatus;
        private readonly ScrollBar _leftScroll, _rightScroll;

        private readonly IXNAButton _ok, _cancel;

        private readonly List<ListDialogItem> _leftItems, _rightItems;

        // cached values: updated/compared from domain state
        private TradeOffer _leftOffer, _rightOffer;
        private int _leftScrollOffset, _rightScrollOffset;

        // tracks state around the trade partner quickly changing an offer
        // pops up warning for "other player trying to trick you"
        private int _recentPartnerItemChanges;
        private Stopwatch _partnerItemChangeTick;

        public TradeDialog(INativeGraphicsManager nativeGraphicsManager,
                           ITradeActions tradeActions,
                           ILocalizedStringFinder localizedStringFinder,
                           IEODialogButtonService dialogButtonService,
                           IEOMessageBoxFactory messageBoxFactory,
                           IStatusLabelSetter statusLabelSetter,
                           IInventorySpaceValidator inventorySpaceValidator,
                           ITradeProvider tradeProvider,
                           ICharacterProvider characterProvider,
                           IEIFFileProvider eifFileProvider,
                           IMapItemGraphicProvider mapItemGraphicProvider,
                           InventoryPanel inventoryPanel)
            : base(nativeGraphicsManager, isInGame: true)
        {
            _tradeActions = tradeActions;
            _localizedStringFinder = localizedStringFinder;
            _messageBoxFactory = messageBoxFactory;
            _statusLabelSetter = statusLabelSetter;
            _inventorySpaceValidator = inventorySpaceValidator;
            _tradeProvider = tradeProvider;
            _characterProvider = characterProvider;
            _eifFileProvider = eifFileProvider;
            _mapItemGraphicProvider = mapItemGraphicProvider;
            _inventoryPanel = inventoryPanel;
            BackgroundTexture = GraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 50);

            _leftPanel = new XNAPanel
            {
                DrawArea = new Rectangle(0, 0, BackgroundTexture.Width / 2, BackgroundTexture.Height),
            };
            _leftPanel.SetParentControl(this);

            _rightPanel = new XNAPanel
            {
                DrawArea = new Rectangle(0, BackgroundTexture.Width / 2, BackgroundTexture.Width / 2, BackgroundTexture.Height)
            };
            _rightPanel.SetParentControl(this);

            _leftPlayerName = new XNALabel(Constants.FontSize08pt5)
            {
                DrawArea = new Rectangle(20, 14, 166, 20),
                AutoSize = false,
                TextAlign = LabelAlignment.MiddleLeft,
                ForeColor = ColorConstants.LightGrayText
            };
            _leftPlayerName.SetParentControl(_leftPanel);

            _rightPlayerName = new XNALabel(Constants.FontSize08pt5)
            {
                DrawArea = new Rectangle(285, 14, 166, 20), // todo
                AutoSize = false,
                TextAlign = LabelAlignment.MiddleLeft,
                ForeColor = ColorConstants.LightGrayText
            };
            _rightPlayerName.SetParentControl(_rightPanel);

            _leftPlayerStatus = new XNALabel(Constants.FontSize08pt5)
            {
                DrawArea = new Rectangle(195, 14, 79, 20),
                AutoSize = false,
                TextAlign = LabelAlignment.MiddleLeft,
                ForeColor = ColorConstants.LightGrayText,
                Text = _localizedStringFinder.GetString(EOResourceID.DIALOG_TRADE_WORD_TRADING)
            };
            _leftPlayerStatus.SetParentControl(_leftPanel);

            _rightPlayerStatus = new XNALabel(Constants.FontSize08pt5)
            {
                DrawArea = new Rectangle(462, 14, 79, 20), // todo
                AutoSize = false,
                TextAlign = LabelAlignment.MiddleLeft,
                ForeColor = ColorConstants.LightGrayText,
                Text = _localizedStringFinder.GetString(EOResourceID.DIALOG_TRADE_WORD_TRADING)
            };
            _rightPlayerStatus.SetParentControl(_rightPanel);

            _leftScroll = new ScrollBar(new Vector2(252, 44), new Vector2(16, 199), ScrollBarColors.LightOnMed, GraphicsManager) { LinesToRender = 5 };
            _leftScroll.SetParentControl(_leftPanel);
            _leftPanel.SetScrollWheelHandler(_leftScroll);

            // todo: position
            _rightScroll = new ScrollBar(new Vector2(518, 44), new Vector2(16, 199), ScrollBarColors.LightOnMed, GraphicsManager) { LinesToRender = 5 };
            _rightScroll.SetParentControl(_rightPanel);
            _rightPanel.SetScrollWheelHandler(_rightScroll);

            _ok = new XNAButton(dialogButtonService.SmallButtonSheet, new Vector2(356, 252),
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Ok),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Ok));
            _ok.OnClick += OkButtonClicked;
            _ok.SetParentControl(_leftPanel);

            _cancel = new XNAButton(dialogButtonService.SmallButtonSheet, new Vector2(449, 252),
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Cancel),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Cancel));
            _cancel.OnClick += CancelButtonClicked;
            _cancel.SetParentControl(_leftPanel);

            _leftItems = new List<ListDialogItem>();
            _rightItems = new List<ListDialogItem>();
            _leftOffer = new TradeOffer.Builder().ToImmutable();
            _rightOffer = new TradeOffer.Builder().ToImmutable();
        }

        public override void Initialize()
        {
            base.Initialize();

            _leftPanel.Initialize();
            _rightPanel.Initialize();

            _leftPlayerName.Initialize();
            _rightPlayerName.Initialize();

            _leftPlayerStatus.Initialize();
            _rightPlayerStatus.Initialize();

            _leftScroll.Initialize();
            _rightScroll.Initialize();

            _ok.Initialize();
            _cancel.Initialize();
        }

        public void Close() => Close(XNADialogResult.NO_BUTTON_PRESSED);

        protected override void OnUnconditionalUpdateControl(GameTime gameTime)
        {
            var updateItemVisibility = false;

            // player one offer will always be on the left; player two always on the right
            if (_tradeProvider.PlayerOneOffer != null && !_tradeProvider.PlayerOneOffer.Equals(_leftOffer))
            {
                UpdateOffer(_tradeProvider.PlayerOneOffer, _leftOffer, _leftPlayerName, _leftPlayerStatus, _leftItems, -3);
                _leftOffer = _tradeProvider.PlayerOneOffer;
                _leftScroll.UpdateDimensions(_leftOffer.Items.Count);
                updateItemVisibility = true;
            }

            if (_tradeProvider.PlayerTwoOffer != null && !_tradeProvider.PlayerTwoOffer.Equals(_rightOffer))
            {
                UpdateOffer(_tradeProvider.PlayerTwoOffer, _rightOffer, _rightPlayerName, _rightPlayerStatus, _rightItems, 263);
                _rightOffer = _tradeProvider.PlayerTwoOffer;
                _rightScroll.UpdateDimensions(_rightOffer.Items.Count);
                updateItemVisibility = true;
            }

            if (updateItemVisibility || _leftScrollOffset != _leftScroll.ScrollOffset)
            {
                UpdateItemScrollIndexes(_leftScroll, _leftItems);
                _leftScrollOffset = _leftScroll.ScrollOffset;
            }

            if (updateItemVisibility || _rightScrollOffset != _rightScroll.ScrollOffset)
            {
                UpdateItemScrollIndexes(_rightScroll, _rightItems);
                _rightScrollOffset = _rightScroll.ScrollOffset;
            }

            if (_partnerItemChangeTick?.ElapsedMilliseconds > 1000 && _recentPartnerItemChanges > 0)
            {
                _recentPartnerItemChanges--;
                _partnerItemChangeTick = Stopwatch.StartNew();
            }

            base.OnUnconditionalUpdateControl(gameTime);
        }

        private void UpdateOffer(TradeOffer actualOffer, TradeOffer cachedOffer,
            IXNALabel playerNameLabel, IXNALabel playerStatusLabel,
            List<ListDialogItem> listItems,
            int listitemOffset)
        {
            if (actualOffer.PlayerName != cachedOffer.PlayerName || actualOffer.Items.Count != cachedOffer.Items.Count)
            {
                playerNameLabel.Text = $"{char.ToUpper(actualOffer.PlayerName[0]) + actualOffer.PlayerName[1..]}{(actualOffer.Items.Any() ? $"[{actualOffer.Items.Count}]" : "")}";
            }

            // todo: check if packets properly reset agrees to false when items change
            if (actualOffer.Agrees != cachedOffer.Agrees)
            {
                playerStatusLabel.Text = actualOffer.Agrees
                    ? _localizedStringFinder.GetString(EOResourceID.DIALOG_TRADE_WORD_AGREE)
                    : _localizedStringFinder.GetString(EOResourceID.DIALOG_TRADE_WORD_TRADING);

                if (actualOffer.Agrees)
                {
                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION,
                        actualOffer.PlayerID == _characterProvider.MainCharacter.ID
                            ? EOResourceID.STATUS_LABEL_TRADE_YOU_ACCEPT
                            : EOResourceID.STATUS_LABEL_TRADE_OTHER_ACCEPT);
                }
                else
                {
                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION,
                        actualOffer.PlayerID == _characterProvider.MainCharacter.ID
                            ? EOResourceID.STATUS_LABEL_TRADE_YOU_CANCEL
                            : EOResourceID.STATUS_LABEL_TRADE_OTHER_CANCEL);
                }
            }

            if (cachedOffer.Items == null || !actualOffer.Items.ToHashSet().SetEquals(cachedOffer.Items))
            {
                var added = actualOffer.Items.Except(cachedOffer.Items ?? Enumerable.Empty<InventoryItem>());
                var removed = (cachedOffer.Items ?? Enumerable.Empty<InventoryItem>()).Where(i => !actualOffer.Items.Contains(i));

                foreach (var addedItem in added)
                {
                    var itemRec = _eifFileProvider.EIFFile[addedItem.ItemID];
                    var subText = $"x {addedItem.Amount}  {(itemRec.Type == ItemType.Armor ? $"({_localizedStringFinder.GetString(itemRec.Gender == 0 ? EOResourceID.FEMALE : EOResourceID.MALE)})" : string.Empty)}";

                    var newListItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large)
                    {
                        Data = addedItem,
                        PrimaryText = itemRec.Name,
                        SubText = subText,
                        IconGraphic = _mapItemGraphicProvider.GetItemGraphic(addedItem.ItemID, addedItem.Amount),
                        OffsetX = listitemOffset,
                        OffsetY = 46
                    };

                    if (actualOffer.PlayerID == _characterProvider.MainCharacter.ID)
                        newListItem.RightClick += (_, _) => _tradeActions.RemoveItemFromOffer((short)itemRec.ID);

                    newListItem.SetParentControl(this);
                    listItems.Add(newListItem);
                }

                foreach (var removedItem in removed)
                {
                    listItems.SingleOrNone(y => ((InventoryItem)y.Data).Equals(removedItem))
                        .MatchSome(listItem =>
                        {
                            listItems.Remove(listItem);
                            listItem.Dispose();
                        });
                }

                if (cachedOffer.Items != null && actualOffer.PlayerID != 0 && actualOffer.PlayerID != _characterProvider.MainCharacter.ID)
                {
                    _partnerItemChangeTick = Stopwatch.StartNew();
                    _recentPartnerItemChanges++;

                    if (_recentPartnerItemChanges == 3)
                    {
                        var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.TRADE_OTHER_PLAYER_TRICK_YOU);
                        dlg.ShowDialog();
                        
                        // this will prevent the message from showing more than once per trade (I'm too lazy to find something more elegant)
                        _recentPartnerItemChanges = -1000;
                    }
                    else if ((_leftOffer == cachedOffer ? _rightOffer : _leftOffer).Agrees)
                    {
                        var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.TRADE_ABORTED_OFFER_CHANGED);
                        dlg.ShowDialog();

                        _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.STATUS_LABEL_TRADE_OTHER_PLAYER_CHANGED_OFFER);
                    }
                }
            }
        }

        private void UpdateItemScrollIndexes(ScrollBar scrollBar, List<ListDialogItem> items)
        {
            var scrollOffset = items.Count > scrollBar.LinesToRender ? scrollBar.ScrollOffset : 0;

            for (int i = 0; i < items.Count; i++)
            {
                items[i].Visible = i >= scrollOffset && i <= scrollBar.LinesToRender + scrollOffset;
                items[i].Index = i - scrollOffset;
            }
        }

        private void OkButtonClicked(object sender, EventArgs e)
        {
            var (offer, partnerOffer) = _leftOffer.PlayerID == _characterProvider.MainCharacter.ID
                ? (_leftOffer, _rightOffer)
                : (_rightOffer, _leftOffer);

            if (offer.Agrees)
                return;

            if (_leftOffer.Items.Count == 0 || _rightOffer.Items.Count == 0)
            {
                var dlg = _messageBoxFactory.CreateMessageBox(EOResourceID.DIALOG_TRADE_BOTH_PLAYERS_OFFER_ONE_ITEM, EOResourceID.STATUS_LABEL_TYPE_WARNING);
                dlg.ShowDialog();
                return;
            }

            if (!_inventorySpaceValidator.ItemsFit(offer.Items, partnerOffer.Items))
            {
                var dlg = _messageBoxFactory.CreateMessageBox(EOResourceID.DIALOG_TRANSFER_NOT_ENOUGH_SPACE, EOResourceID.STATUS_LABEL_TYPE_WARNING);
                dlg.ShowDialog();
                return;
            }

            var partnerItemWeight = partnerOffer.Items
                .Select(x => _eifFileProvider.EIFFile[x.ItemID].Weight * x.Amount)
                .Aggregate((a, b) => a + b);
            var offerItemWeight = offer.Items
                .Select(x => _eifFileProvider.EIFFile[x.ItemID].Weight * x.Amount)
                .Aggregate((a, b) => a + b);

            var stats = _characterProvider.MainCharacter.Stats;
            if (stats[CharacterStat.Weight] - offerItemWeight + partnerItemWeight > stats[CharacterStat.MaxWeight])
            {
                var dlg = _messageBoxFactory.CreateMessageBox(EOResourceID.DIALOG_TRANSFER_NOT_ENOUGH_WEIGHT, EOResourceID.STATUS_LABEL_TYPE_WARNING);
                dlg.ShowDialog();
                return;
            }

            var finalCheckDlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.TRADE_DO_YOU_AGREE, EODialogButtons.OkCancel);
            finalCheckDlg.DialogClosing += (o, e) =>
            {
                if (e.Result == XNADialogResult.OK)
                {
                    _tradeActions.AgreeToTrade(true);
                }
            };
            finalCheckDlg.ShowDialog();
        }

        private void CancelButtonClicked(object sender, EventArgs e)
        {
            var offer = _leftOffer.PlayerID == _characterProvider.MainCharacter.ID
                ? _leftOffer
                : _rightOffer;

            if (!offer.Agrees)
            {
                _tradeActions.CancelTrade();
                Close(XNADialogResult.Cancel);
            }
            else
            {
                _tradeActions.AgreeToTrade(false);
            }
        }
    }
}
