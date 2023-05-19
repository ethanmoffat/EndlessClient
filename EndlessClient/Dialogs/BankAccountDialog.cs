using EndlessClient.ControlSets;
using EndlessClient.Dialogs.Factories;
using EndlessClient.Dialogs.Services;
using EndlessClient.HUD;
using EndlessClient.HUD.Controls;
using EndlessClient.HUD.Panels;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Bank;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using Optional;
using Optional.Collections;
using System;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class BankAccountDialog : ScrollingListDialog
    {
        private readonly IBankActions _bankActions;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IItemTransferDialogFactory _itemTransferDialogFactory;
        private readonly IBankDataProvider _bankDataProvider;
        private readonly ICharacterInventoryProvider _characterInventoryProvider;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly InventoryPanel _inventoryPanel;

        private int _cachedValue;
        private Option<int> _cachedUpgrades;

        public  BankAccountDialog(INativeGraphicsManager nativeGraphicsManager,
                                  IBankActions bankActions,
                                  IEODialogButtonService dialogButtonService,
                                  IEODialogIconService dialogIconService,
                                  ILocalizedStringFinder localizedStringFinder,
                                  IStatusLabelSetter statusLabelSetter,
                                  IEOMessageBoxFactory messageBoxFactory,
                                  IItemTransferDialogFactory itemTransferDialogFactory,
                                  IHudControlProvider hudControlProvider,
                                  IBankDataProvider bankDataProvider,
                                  ICharacterInventoryProvider characterInventoryProvider,
                                  IEIFFileProvider eifFileProvider)
            : base(nativeGraphicsManager, dialogButtonService, dialogType: DialogType.BankAccountDialog)
        {
            _bankActions = bankActions;
            _localizedStringFinder = localizedStringFinder;
            _statusLabelSetter = statusLabelSetter;
            _messageBoxFactory = messageBoxFactory;
            _itemTransferDialogFactory = itemTransferDialogFactory;
            _bankDataProvider = bankDataProvider;
            _characterInventoryProvider = characterInventoryProvider;
            _eifFileProvider = eifFileProvider;
            _inventoryPanel = hudControlProvider.GetComponent<InventoryPanel>(HudControlIdentifier.InventoryPanel);

            ListItemType = ListDialogItem.ListItemStyle.Large;
            Buttons = ScrollingListDialogButtons.Cancel;

            _titleText.Text = "0";
            _titleText.TextAlign = LabelAlignment.MiddleRight;
            _titleText.AutoSize = false;

            var currencyName = _eifFileProvider.EIFFile[1].Name;

            var depositItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 0)
            {
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.DIALOG_BANK_DEPOSIT),
                SubText = $"{_localizedStringFinder.GetString(EOResourceID.DIALOG_BANK_TRANSFER)} {currencyName} {_localizedStringFinder.GetString(EOResourceID.DIALOG_BANK_TO_ACCOUNT)}",
                IconGraphic = dialogIconService.IconSheet,
                IconGraphicSource = dialogIconService.GetDialogIconSource(DialogIcon.BankDeposit),
                OffsetY = 55,
                ShowIconBackGround = false,
            };
            depositItem.LeftClick += Deposit;
            depositItem.RightClick += Deposit;

            var withdrawItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 1)
            {
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.DIALOG_BANK_WITHDRAW),
                SubText = $"{_localizedStringFinder.GetString(EOResourceID.DIALOG_BANK_TRANSFER)} {currencyName} {_localizedStringFinder.GetString(EOResourceID.DIALOG_BANK_FROM_ACCOUNT)}",
                IconGraphic = dialogIconService.IconSheet,
                IconGraphicSource = dialogIconService.GetDialogIconSource(DialogIcon.BankWithdraw),
                OffsetY = 55,
                ShowIconBackGround = false,
            };
            withdrawItem.LeftClick += Withdraw;
            withdrawItem.RightClick += Withdraw;

            var upgradeItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large, 2)
            {
                PrimaryText = _localizedStringFinder.GetString(EOResourceID.DIALOG_BANK_LOCKER_UPGRADE),
                SubText = _localizedStringFinder.GetString(EOResourceID.DIALOG_BANK_MORE_SPACE),
                IconGraphic = dialogIconService.IconSheet,
                IconGraphicSource = dialogIconService.GetDialogIconSource(DialogIcon.BankLockerUpgrade),
                OffsetY = 55,
                ShowIconBackGround = false,
            };
            upgradeItem.LeftClick += Upgrade;
            upgradeItem.RightClick += Upgrade;

            AddItemToList(depositItem, sortList: false);
            AddItemToList(withdrawItem, sortList: false);
            AddItemToList(upgradeItem, sortList: false);

            DrawPosition += new Vector2(0, 50);
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (_bankDataProvider.AccountValue != _cachedValue)
            {
                _cachedValue = _bankDataProvider.AccountValue;
                Title = $"{_bankDataProvider.AccountValue}";
            }

            _cachedUpgrades.Match(
                some: c =>
                {
                    _bankDataProvider.LockerUpgrades.MatchSome(
                        upgrades =>
                        {
                            if (upgrades != c)
                            {
                                _cachedUpgrades = _bankDataProvider.LockerUpgrades;
                                _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, EOResourceID.STATUS_LABEL_LOCKER_SPACE_INCREASED);
                            }
                        });
                },
                none: () => _cachedUpgrades = _bankDataProvider.LockerUpgrades);

            base.OnUpdateControl(gameTime);
        }

        private void Deposit(object sender, EventArgs e)
        {
            if (!_inventoryPanel.NoItemsDragging())
                return;

            _characterInventoryProvider.ItemInventory.SingleOrNone(x => x.ItemID == 1)
                .Match(
                    some: characterGold =>
                    {
                        if (characterGold.Amount == 0)
                        {
                            var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.BANK_ACCOUNT_UNABLE_TO_DEPOSIT);
                            dlg.ShowDialog();

                            // todo: show the dialog message as a warning in the status label
                        }
                        else if (characterGold.Amount == 1)
                        {
                            _bankActions.Deposit(1);
                        }
                        else if (characterGold.Amount > 1)
                        {
                            var dlg = _itemTransferDialogFactory.CreateItemTransferDialog(_eifFileProvider.EIFFile[1].Name, ItemTransferDialog.TransferType.BankTransfer, characterGold.Amount, EOResourceID.DIALOG_TRANSFER_DEPOSIT);
                            dlg.DialogClosing += (_, e) =>
                            {
                                if (e.Result == XNADialogResult.OK)
                                    _bankActions.Deposit(dlg.SelectedAmount);
                            };
                            dlg.ShowDialog();
                        }
                    },
                    none: () =>
                    {
                        var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.BANK_ACCOUNT_UNABLE_TO_DEPOSIT);
                        dlg.ShowDialog();

                        // todo: show the dialog message as a warning in the status label
                    });
        }

        private void Withdraw(object sender, EventArgs e)
        {
            if (!_inventoryPanel.NoItemsDragging())
                return;

            if (_bankDataProvider.AccountValue == 0)
            {
                var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.BANK_ACCOUNT_UNABLE_TO_WITHDRAW);
                dlg.ShowDialog();

                // todo: show the dialog message as a warning in the status label
            }
            else if (_bankDataProvider.AccountValue == 1)
            {
                _bankActions.Withdraw(1);
            }
            else if (_bankDataProvider.AccountValue > 1)
            {
                var dlg = _itemTransferDialogFactory.CreateItemTransferDialog(_eifFileProvider.EIFFile[1].Name, ItemTransferDialog.TransferType.BankTransfer, _bankDataProvider.AccountValue, EOResourceID.DIALOG_TRANSFER_WITHDRAW);
                dlg.DialogClosing += (_, e) =>
                {
                    if (e.Result == XNADialogResult.OK)
                        _bankActions.Withdraw(dlg.SelectedAmount);
                };
                dlg.ShowDialog();
            }
        }

        private void Upgrade(object sender, EventArgs e)
        {
            if (!_inventoryPanel.NoItemsDragging())
                return;

            _bankDataProvider.LockerUpgrades.MatchSome(lockerUpgrades =>
            {
                if (lockerUpgrades == Constants.MaxLockerUpgrades)
                {
                    var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.LOCKER_UPGRADE_IMPOSSIBLE);
                    dlg.ShowDialog();
                    return;
                }

                int requiredGold = (lockerUpgrades + 1) * 1000;

                _characterInventoryProvider.ItemInventory.SingleOrNone(x => x.ItemID == 1)
                    .Match(
                        some: charaterGold =>
                        {
                            if (charaterGold.Amount < requiredGold)
                            {
                                var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.WARNING_YOU_HAVE_NOT_ENOUGH, $" {_eifFileProvider.EIFFile[1].Name}");
                                dlg.ShowDialog();
                            }
                            else
                            {
                                var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.LOCKER_UPGRADE_UNIT, $"{requiredGold} {_eifFileProvider.EIFFile[1].Name}?", EODialogButtons.OkCancel);
                                dlg.DialogClosing += (_, e) =>
                                {
                                    if (e.Result == XNADialogResult.OK)
                                        _bankActions.BuyStorageUpgrade();
                                };
                                dlg.ShowDialog();
                            }
                        },
                        () =>
                        {
                            var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.WARNING_YOU_HAVE_NOT_ENOUGH, $" {_eifFileProvider.EIFFile[1].Name}");
                            dlg.ShowDialog();
                        });
            });
        }
    }
}
