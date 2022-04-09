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
using Optional.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using XNAControls;

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

        private readonly IShopActions _shopActions;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IItemTransferDialogFactory _itemTransferDialogFactory;
        private readonly IEODialogIconService _dialogIconService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IShopDataProvider _shopDataProvider;
        private readonly ICharacterInventoryProvider _characterInventoryProvider;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly IInventorySpaceValidator _inventorySpaceValidator;
        private IReadOnlyList<IShopItem> _buyItems, _sellItems;
        private IReadOnlyList<IShopCraftItem> _craftItems;

        private ShopState _state;

        private Option<int> _cachedShopId;
        private HashSet<IInventoryItem> _cachedInventory;
        private ulong _tick;

        public ShopDialog(INativeGraphicsManager nativeGraphicsManager,
                          IShopActions shopActions,
                          IEOMessageBoxFactory messageBoxFactory,
                          IItemTransferDialogFactory itemTransferDialogFactory,
                          IEODialogButtonService dialogButtonService,
                          IEODialogIconService dialogIconService,
                          ILocalizedStringFinder localizedStringFinder,
                          IShopDataProvider shopDataProvider,
                          ICharacterInventoryProvider characterInventoryProvider,
                          IEIFFileProvider eifFileProvider,
                          ICharacterProvider characterProvider,
                          IInventorySpaceValidator inventorySpaceValidator)
            : base(nativeGraphicsManager, dialogButtonService)
        {
            _shopActions = shopActions;
            _messageBoxFactory = messageBoxFactory;
            _itemTransferDialogFactory = itemTransferDialogFactory;
            _dialogIconService = dialogIconService;
            _localizedStringFinder = localizedStringFinder;
            _shopDataProvider = shopDataProvider;
            _characterInventoryProvider = characterInventoryProvider;
            _eifFileProvider = eifFileProvider;
            _characterProvider = characterProvider;
            _inventorySpaceValidator = inventorySpaceValidator;

            Buttons = ScrollingListDialogButtons.Cancel;
            ListItemType = ListDialogItem.ListItemStyle.Large;

            BackAction += (_, _) => SetState(ShopState.Initial);

            _cachedInventory = new HashSet<IInventoryItem>(_characterInventoryProvider.ItemInventory);
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

            if (++_tick % 8 == 0 && !_cachedInventory.SetEquals(_characterInventoryProvider.ItemInventory))
            {
                _sellItems = _shopDataProvider.TradeItems.Where(x => x.Sell > 0 && _characterInventoryProvider.ItemInventory.Any(inv => inv.ItemID == x.ID && inv.Amount > 0)).ToList();
                _cachedInventory = new HashSet<IInventoryItem>(_characterInventoryProvider.ItemInventory);

                if (_state == ShopState.Selling)
                    SetState(ShopState.Selling);
            }

            base.OnUpdateControl(gameTime);
        }

        private void SetState(ShopState state)
        {
            if (state == ShopState.None)
                return;

            if (state == ShopState.Buying && _buyItems.Count == 0)
            {
                var msg = _messageBoxFactory.CreateMessageBox(DialogResourceID.SHOP_NOTHING_IS_FOR_SALE);
                msg.ShowDialog();

                if (_state != ShopState.Initial)
                    SetState(ShopState.Initial);

                return;
            }
            else if (state == ShopState.Selling && _sellItems.Count == 0)
            {
                var msg = _messageBoxFactory.CreateMessageBox(DialogResourceID.SHOP_NOT_BUYING_YOUR_ITEMS);
                msg.ShowDialog();

                if (_state != ShopState.Initial)
                    SetState(ShopState.Initial);

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

                        AddItemToList(buyItem, sortList: false);
                        AddItemToList(sellItem, sortList: false);

                        if (_craftItems.Count > 0)
                        {
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

                            AddItemToList(craftItem, sortList: false);
                        }

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

                            items.Add(listItem);
                        }

                        SetItemList(items);
                        Buttons = ScrollingListDialogButtons.BackCancel;
                    }
                    break;

                case ShopState.Crafting:
                    {
                        var items = new List<ListDialogItem>();
                        foreach (var item in _craftItems)
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
            if (_state != ShopState.Buying && _state != ShopState.Selling)
                return;

            var listItemIndex = ((ListDialogItem)sender).Index + _scrollBar.ScrollOffset;
            var buying = _state == ShopState.Buying;

            var collection = (buying ? _buyItems : _sellItems);
            if (listItemIndex >= collection.Count)
                return;

            // todo: move some of this stuff into a controller class?
            var shopItem = collection[listItemIndex];
            var data = _eifFileProvider.EIFFile[shopItem.ID];

            var inventoryItem = _characterInventoryProvider.ItemInventory
                .SingleOrNone(x => buying ? x.ItemID == 1 : x.ItemID == shopItem.ID);

            // todo: move this stuff into a validator class?
            if (buying)
            {
                if (!_inventorySpaceValidator.ItemFits(data.Size))
                {
                    var msg = _messageBoxFactory.CreateMessageBox(EOResourceID.DIALOG_TRANSFER_NOT_ENOUGH_SPACE, EOResourceID.STATUS_LABEL_TYPE_WARNING);
                    msg.ShowDialog();
                    return;
                }

                var stats = _characterProvider.MainCharacter.Stats;
                if (data.Weight + stats[CharacterStat.Weight] > stats[CharacterStat.MaxWeight])
                {
                    var msg = _messageBoxFactory.CreateMessageBox(EOResourceID.DIALOG_TRANSFER_NOT_ENOUGH_WEIGHT, EOResourceID.STATUS_LABEL_TYPE_WARNING);
                    msg.ShowDialog();
                    return;
                }

                var hasEnoughGold = inventoryItem.Match(some: x => x.Amount >= shopItem.Buy, none: () => false);
                if (!hasEnoughGold)
                {
                    var msg = _messageBoxFactory.CreateMessageBox(DialogResourceID.WARNING_YOU_HAVE_NOT_ENOUGH, " gold.");
                    msg.ShowDialog();
                    return;
                }
            }
            else
            {
                var hasEnoughItem = inventoryItem.Match(some: x => x.Amount > 0, none: () => false);
                if (!hasEnoughItem)
                {
                    var msg = _messageBoxFactory.CreateMessageBox(DialogResourceID.SHOP_NOT_BUYING_YOUR_ITEMS);
                    msg.ShowDialog();
                    return;
                }
            }

            var needItemTransferDialog = (buying && shopItem.MaxBuy == 1) || (!buying && inventoryItem.Match(x => x.Amount != 1, () => false));

            if (needItemTransferDialog)
            {
                var itemTransferDialog = _itemTransferDialogFactory.CreateItemTransferDialog(data.Name,
                    ItemTransferDialog.TransferType.ShopTransfer,
                    buying ? shopItem.MaxBuy : inventoryItem.Match(x => x.Amount, () => 0),
                    buying ? EOResourceID.DIALOG_TRANSFER_BUY : EOResourceID.DIALOG_TRANSFER_SELL);
                itemTransferDialog.DialogClosing += (_, e) =>
                {
                    if (e.Result == XNADialogResult.OK)
                        ConfirmAndExecuteTrade(itemTransferDialog.SelectedAmount);
                };

                itemTransferDialog.ShowDialog();
            }
            else
            {
                ConfirmAndExecuteTrade(amount: 1);
            }

            void ConfirmAndExecuteTrade(int amount)
            {
                var message = $"{_localizedStringFinder.GetString(buying ? EOResourceID.DIALOG_WORD_BUY : EOResourceID.DIALOG_WORD_SELL)} {amount} {data.Name} " +
                    $"{_localizedStringFinder.GetString(EOResourceID.DIALOG_WORD_FOR)} {(buying ? shopItem.Buy : shopItem.Sell) * amount} gold?";
                var dlg = _messageBoxFactory.CreateMessageBox(message, _localizedStringFinder.GetString(buying ? EOResourceID.DIALOG_SHOP_BUY_ITEMS : EOResourceID.DIALOG_SHOP_SELL_ITEMS), EODialogButtons.OkCancel);
                dlg.DialogClosing += (_, e) =>
                {
                    if (e.Result == XNADialogResult.Cancel)
                        return;

                    if (buying)
                        _shopActions.BuyItem((short)shopItem.ID, amount);
                    else
                        _shopActions.SellItem((short)shopItem.ID, amount);
                };
                dlg.ShowDialog();
            }
        }

        private void CraftItem(object sender, EventArgs e)
        {
            if (_state != ShopState.Crafting)
                return;

            var listItemIndex = ((ListDialogItem)sender).Index + _scrollBar.ScrollOffset;

            if (listItemIndex >= _craftItems.Count)
                return;

            // todo: move some of this stuff into a controller class?
            var craftItem = _craftItems[listItemIndex];
            var data = _eifFileProvider.EIFFile[craftItem.ID];

            // todo: move this stuff into a validator class?
            foreach (var ingredient in craftItem.Ingredients)
            {
                if (!_characterInventoryProvider.ItemInventory.Any(x => x.ItemID == ingredient.ID && x.Amount >= ingredient.Amount))
                {
                    var message = BuildMessage(EOResourceID.DIALOG_SHOP_CRAFT_MISSING_INGREDIENTS);
                    var caption = BuildCaption(EOResourceID.DIALOG_SHOP_CRAFT_INGREDIENTS);
                    
                    var dlg = _messageBoxFactory.CreateMessageBox(message, caption, EODialogButtons.Cancel, EOMessageBoxStyle.LargeDialogSmallHeader);
                    dlg.ShowDialog();

                    return;
                }
            }

            if (!_inventorySpaceValidator.ItemFits(data.Size))
            {
                var msg = _messageBoxFactory.CreateMessageBox(EOResourceID.DIALOG_TRANSFER_NOT_ENOUGH_SPACE, EOResourceID.STATUS_LABEL_TYPE_WARNING);
                msg.ShowDialog();
                return;
            }

            var message2 = BuildMessage(EOResourceID.DIALOG_SHOP_CRAFT_PUT_INGREDIENTS_TOGETHER);
            var caption2 = BuildCaption(EOResourceID.DIALOG_SHOP_CRAFT_INGREDIENTS);

            var dlg2 = _messageBoxFactory.CreateMessageBox(message2, caption2, EODialogButtons.OkCancel, EOMessageBoxStyle.LargeDialogSmallHeader);
            dlg2.DialogClosing += (o, e) =>
            {
                if (e.Result == XNADialogResult.Cancel)
                    return;

                _shopActions.CraftItem((short)craftItem.ID);
            };
            dlg2.ShowDialog();

            string BuildMessage(EOResourceID resource)
            {
                var message = _localizedStringFinder.GetString(resource) + "\n\n";

                foreach (var ingred in craftItem.Ingredients)
                {
                    var ingredData = _eifFileProvider.EIFFile[ingred.ID];
                    message += $"+  {ingred.Amount}  {ingredData.Name}\n";
                }

                return message;
            }

            string BuildCaption(EOResourceID resource)
            {
                return $"{_localizedStringFinder.GetString(resource)} {_localizedStringFinder.GetString(EOResourceID.DIALOG_WORD_FOR)} {data.Name}";
            }
        }
    }
}
