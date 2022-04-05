using EndlessClient.Dialogs.Factories;
using EndlessClient.Dialogs.Services;
using EndlessClient.HUD.Inventory;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Shop;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EndlessClient.Dialogs
{
    public class ShopDialog : ScrollingListDialog
    {
        private enum ShopState
        {
            None,
            Initial,
            Buying,
            Selling,
            Crafting
        }

        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IItemTransferDialogFactory _itemTransferDialogFactory;
        private readonly IEODialogIconService _dialogIconService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IShopDataProvider _shopDataProvider;
        private readonly ICharacterInventoryProvider _characterInventoryProvider;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly IInventorySpaceValidator _inventorySpaceValidator;
        private IReadOnlyList<IShopItem> _buyItems, _sellItems;
        private IReadOnlyList<IShopCraftItem> _craftItems;

        private ShopState _state;

        private Option<int> _cachedShopId;

        public ShopDialog(INativeGraphicsManager nativeGraphicsManager,
                          IEOMessageBoxFactory messageBoxFactory,
                          IItemTransferDialogFactory itemTransferDialogFactory,
                          IEODialogButtonService dialogButtonService,
                          IEODialogIconService dialogIconService,
                          ILocalizedStringFinder localizedStringFinder,
                          IShopDataProvider shopDataProvider,
                          ICharacterInventoryProvider characterInventoryProvider,
                          IEIFFileProvider eifFileProvider,
                          IInventorySpaceValidator inventorySpaceValidator)
            : base(nativeGraphicsManager, dialogButtonService)
        {
            _messageBoxFactory = messageBoxFactory;
            _itemTransferDialogFactory = itemTransferDialogFactory;
            _dialogIconService = dialogIconService;
            _localizedStringFinder = localizedStringFinder;
            _shopDataProvider = shopDataProvider;
            _characterInventoryProvider = characterInventoryProvider;
            _eifFileProvider = eifFileProvider;
            _inventorySpaceValidator = inventorySpaceValidator;

            Buttons = ScrollingListDialogButtons.Cancel;
            ListItemType = ListDialogItem.ListItemStyle.Large;

            BackAction += (_, _) => SetState(ShopState.Initial);
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            _cachedShopId.MatchNone(() =>
            {
                _shopDataProvider.ShopID.SomeWhen(x => x > 0)
                    .MatchSome(x =>
                    {
                        _cachedShopId = Option.Some(_shopDataProvider.ShopID);

                        Title = _shopDataProvider.ShopName;

                        _buyItems = _shopDataProvider.TradeItems.Where(x => x.Buy > 0).ToList();
                        _sellItems = _shopDataProvider.TradeItems.Where(x => x.Sell > 0 && _characterInventoryProvider.ItemInventory.Any(inv => inv.ItemID == x.ID && inv.Amount > 0)).ToList();
                        _craftItems = _shopDataProvider.CraftItems;

                        SetState(ShopState.Initial);
                    });
            });

            base.OnUpdateControl(gameTime);
        }

        private void SetState(ShopState state)
        {
            if (_state == state || state == ShopState.None)
                return;

            if (state == ShopState.Buying && _buyItems.Count == 0)
            {
                var msg = _messageBoxFactory.CreateMessageBox(DialogResourceID.SHOP_NOTHING_IS_FOR_SALE);
                msg.ShowDialog();
                return;
            }

            if (state == ShopState.Selling && _sellItems.Count == 0)
            {
                var msg = _messageBoxFactory.CreateMessageBox(DialogResourceID.SHOP_NOT_BUYING_YOUR_ITEMS);
                msg.ShowDialog();
                return;
            }

            ClearItemList();

            switch (state)
            {
                case ShopState.Initial:
                    {
                        var buyItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large)
                        {
                            PrimaryText = _localizedStringFinder.GetString(EOResourceID.DIALOG_SHOP_BUY_ITEMS),
                            SubText = $"{_buyItems.Count} {_localizedStringFinder.GetString(EOResourceID.DIALOG_SHOP_ITEMS_IN_STORE)}",
                            IconGraphic = _dialogIconService.IconSheet,
                            IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.Buy),
                            ShowIconBackGround = false,
                            OffsetY = 45,
                        };
                        buyItem.LeftClick += (_, _) => SetState(ShopState.Buying);
                        buyItem.Initialize();

                        var sellItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large)
                        {
                            PrimaryText = _localizedStringFinder.GetString(EOResourceID.DIALOG_SHOP_SELL_ITEMS),
                            SubText = $"{_sellItems.Count} {_localizedStringFinder.GetString(EOResourceID.DIALOG_SHOP_ITEMS_ACCEPTED)}",
                            IconGraphic = _dialogIconService.IconSheet,
                            IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.Sell),
                            ShowIconBackGround = false,
                            OffsetY = 45,
                        };
                        sellItem.LeftClick += (_, _) => SetState(ShopState.Selling);
                        sellItem.Initialize();

                        var craftItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large)
                        {
                            PrimaryText = _localizedStringFinder.GetString(EOResourceID.DIALOG_SHOP_CRAFT_ITEMS),
                            SubText = $"{_craftItems.Count} {_localizedStringFinder.GetString(EOResourceID.DIALOG_SHOP_ITEMS_ACCEPTED)}",
                            IconGraphic = _dialogIconService.IconSheet,
                            IconGraphicSource = _dialogIconService.GetDialogIconSource(DialogIcon.Craft),
                            ShowIconBackGround = false,
                            OffsetY = 45,
                        };
                        craftItem.LeftClick += (_, _) => SetState(ShopState.Crafting);
                        craftItem.Initialize();

                        AddItemToList(buyItem, sortList: false);
                        AddItemToList(sellItem, sortList: false);
                        AddItemToList(craftItem, sortList: false);

                        Buttons = ScrollingListDialogButtons.Cancel;
                    }
                    break;

                case ShopState.Buying:
                case ShopState.Selling:
                    {
                        var buying = state == ShopState.Buying;
                        var items = new List<ListDialogItem>();
                        foreach (var item in buying ? _buyItems : _sellItems)
                        {
                            var data = _eifFileProvider.EIFFile[item.ID];
                            var genderExtra = data.Type == EOLib.IO.ItemType.Armor ? $"({_localizedStringFinder.GetString(EOResourceID.FEMALE - data.Gender)})" : string.Empty;
                            var subText = $"{_localizedStringFinder.GetString(EOResourceID.DIALOG_SHOP_PRICE)}: {(buying ? item.Buy : item.Sell)} {genderExtra}";

                            var listItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large)
                            {
                                PrimaryText = data.Name,
                                SubText = subText,
                                IconGraphic = GraphicsManager.TextureFromResource(GFXTypes.Items, 2 * data.Graphic - 1, transparent: true),
                                OffsetY = 45
                            };
                            listItem.LeftClick += TradeItem;
                            listItem.RightClick += TradeItem;
                            listItem.Initialize();

                            items.Add(listItem);
                        }

                        SetItemList(items);
                        Buttons = ScrollingListDialogButtons.BackCancel;
                    }
                    break;

                case ShopState.Crafting:
                    {
                        var items = new List<ListDialogItem>();
                        foreach (var item in _craftItems.Where(x => x.Ingredients.Count > 0))
                        {
                            var data = _eifFileProvider.EIFFile[item.ID];
                            var genderExtra = data.Type == EOLib.IO.ItemType.Armor ? $"({_localizedStringFinder.GetString(EOResourceID.FEMALE - data.Gender)})" : string.Empty;
                            var subText = $"{_localizedStringFinder.GetString(EOResourceID.DIALOG_SHOP_CRAFT_INGREDIENTS)}: {item.Ingredients.Count} {genderExtra}";

                            var listItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large)
                            {
                                PrimaryText = data.Name,
                                SubText = subText,
                                IconGraphic = GraphicsManager.TextureFromResource(GFXTypes.Items, 2 * data.Graphic - 1, transparent: true),
                                OffsetY = 45
                            };
                            listItem.LeftClick += CraftItem;
                            listItem.RightClick += CraftItem;
                            listItem.Initialize();

                            items.Add(listItem);
                        }

                        SetItemList(items);
                        Buttons = ScrollingListDialogButtons.BackCancel;
                    }
                    break;
            }

            _state = state;
        }

        private void TradeItem(object sender, EventArgs e)
        {
            var listItemIndex = ((ListDialogItem)sender).Index;
            var buying = _state == ShopState.Buying;

            var shopItem = (buying ? _buyItems : _sellItems)[listItemIndex];

            //if (m_state != ShopState.Buying && m_state != ShopState.Selling)
            //    return;
            //bool isBuying = m_state == ShopState.Buying;

            //InventoryItem ii = OldWorld.Instance.MainPlayer.ActiveCharacter.Inventory.Find(x => (isBuying ? x.ItemID == 1 : x.ItemID == item.ID));
            //var rec = OldWorld.Instance.EIF[item.ID];
            //if (isBuying)
            //{
            //    if (!EOGame.Instance.Hud.InventoryFits((short)item.ID))
            //    {
            //        EOMessageBox.Show(OldWorld.GetString(EOResourceID.DIALOG_TRANSFER_NOT_ENOUGH_SPACE),
            //            OldWorld.GetString(EOResourceID.STATUS_LABEL_TYPE_WARNING),
            //            EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
            //        return;
            //    }

            //    if (rec.Weight + OldWorld.Instance.MainPlayer.ActiveCharacter.Weight >
            //        OldWorld.Instance.MainPlayer.ActiveCharacter.MaxWeight)
            //    {
            //        EOMessageBox.Show(OldWorld.GetString(EOResourceID.DIALOG_TRANSFER_NOT_ENOUGH_WEIGHT),
            //            OldWorld.GetString(EOResourceID.STATUS_LABEL_TYPE_WARNING),
            //            EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
            //        return;
            //    }

            //    if (ii.Amount < item.Buy)
            //    {
            //        EOMessageBox.Show(DialogResourceID.WARNING_YOU_HAVE_NOT_ENOUGH, " gold.", EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
            //        return;
            //    }
            //}
            //else if (ii.Amount == 0)
            //    return; //can't sell if amount of item is 0

            ////special case: no need for prompting if selling an item with count == 1 in inventory
            //if (!isBuying && ii.Amount == 1)
            //{
            //    string _message =
            //        $"{OldWorld.GetString(EOResourceID.DIALOG_WORD_SELL)} 1 {rec.Name} {OldWorld.GetString(EOResourceID.DIALOG_WORD_FOR)} {item.Sell} gold?";
            //    EOMessageBox.Show(_message, OldWorld.GetString(EOResourceID.DIALOG_SHOP_SELL_ITEMS), EODialogButtons.OkCancel,
            //        EOMessageBoxStyle.SmallDialogSmallHeader, (oo, ee) =>
            //        {
            //            if (ee.Result == XNADialogResult.OK && !m_api.SellItem((short)item.ID, 1))
            //            {
            //                EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
            //            }
            //        });
            //}
            //else
            //{
            //    ItemTransferDialog dlg = new ItemTransferDialog(rec.Name, ItemTransferDialog.TransferType.ShopTransfer,
            //        isBuying ? item.MaxBuy : ii.Amount, isBuying ? EOResourceID.DIALOG_TRANSFER_BUY : EOResourceID.DIALOG_TRANSFER_SELL);
            //    dlg.DialogClosing += (o, e) =>
            //    {
            //        if (e.Result == XNADialogResult.OK)
            //        {
            //            string _message =
            //                $"{OldWorld.GetString(isBuying ? EOResourceID.DIALOG_WORD_BUY : EOResourceID.DIALOG_WORD_SELL)} {dlg.SelectedAmount} {rec.Name} {OldWorld.GetString(EOResourceID.DIALOG_WORD_FOR)} {(isBuying ? item.Buy : item.Sell)*dlg.SelectedAmount} gold?";

            //            EOMessageBox.Show(_message,
            //                OldWorld.GetString(isBuying ? EOResourceID.DIALOG_SHOP_BUY_ITEMS : EOResourceID.DIALOG_SHOP_SELL_ITEMS),
            //                EODialogButtons.OkCancel, EOMessageBoxStyle.SmallDialogSmallHeader, (oo, ee) =>
            //                {
            //                    if (ee.Result == XNADialogResult.OK)
            //                    {
            //                        //only actually do the buy/sell if the user then clicks "OK" in the second prompt
            //                        if (isBuying && !m_api.BuyItem((short)item.ID, dlg.SelectedAmount) ||
            //                            !isBuying && !m_api.SellItem((short)item.ID, dlg.SelectedAmount))
            //                        {
            //                            EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
            //                        }
            //                    }
            //                });
            //        }
            //    };
            //}
        }

        private void CraftItem(object sender, EventArgs e)
        {
            var listItemIndex = ((ListDialogItem)sender).Index;

            var craftItem = _craftItems[listItemIndex];

            //if (m_state != ShopState.Crafting)
            //    return;

            //var craftItemRec = OldWorld.Instance.EIF[item.ID];
            //// ReSharper disable once LoopCanBeConvertedToQuery
            //foreach (var ingredient in item.Ingredients)
            //{
            //    if (OldWorld.Instance.MainPlayer.ActiveCharacter.Inventory.FindIndex(_item => _item.ItemID == ingredient.Item1 && _item.Amount >= ingredient.Item2) < 0)
            //    {
            //        string _message = OldWorld.GetString(EOResourceID.DIALOG_SHOP_CRAFT_MISSING_INGREDIENTS) + "\n\n";
            //        foreach (var ingred in item.Ingredients)
            //        {
            //            var localRec = OldWorld.Instance.EIF[ingred.Item1];
            //            _message += $"+  {ingred.Item2}  {localRec.Name}\n";
            //        }
            //        string _caption =
            //            $"{OldWorld.GetString(EOResourceID.DIALOG_SHOP_CRAFT_INGREDIENTS)} {OldWorld.GetString(EOResourceID.DIALOG_WORD_FOR)} {craftItemRec.Name}";
            //        EOMessageBox.Show(_message, _caption, EODialogButtons.Cancel, EOMessageBoxStyle.LargeDialogSmallHeader);
            //        return;
            //    }
            //}

            //if (!EOGame.Instance.Hud.InventoryFits((short)item.ID))
            //{
            //    EOMessageBox.Show(OldWorld.GetString(EOResourceID.DIALOG_TRANSFER_NOT_ENOUGH_SPACE),
            //        OldWorld.GetString(EOResourceID.STATUS_LABEL_TYPE_WARNING),
            //        EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
            //    return;
            //}

            //string _message2 = OldWorld.GetString(EOResourceID.DIALOG_SHOP_CRAFT_PUT_INGREDIENTS_TOGETHER) + "\n\n";
            //foreach (var ingred in item.Ingredients)
            //{
            //    var localRec = OldWorld.Instance.EIF[ingred.Item1];
            //    _message2 += $"+  {ingred.Item2}  {localRec.Name}\n";
            //}
            //string _caption2 =
            //    $"{OldWorld.GetString(EOResourceID.DIALOG_SHOP_CRAFT_INGREDIENTS)} {OldWorld.GetString(EOResourceID.DIALOG_WORD_FOR)} {craftItemRec.Name}";
            //EOMessageBox.Show(_message2, _caption2, EODialogButtons.OkCancel, EOMessageBoxStyle.LargeDialogSmallHeader, (o, e) =>
            //{
            //    if (e.Result == XNADialogResult.OK && !m_api.CraftItem((short)item.ID))
            //    {
            //        EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
            //    }
            //});
        }
    }
}
