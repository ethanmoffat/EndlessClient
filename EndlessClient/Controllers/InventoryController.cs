using AutomaticTypeMapper;
using EndlessClient.ControlSets;
using EndlessClient.Dialogs;
using EndlessClient.Dialogs.Actions;
using EndlessClient.Dialogs.Factories;
using EndlessClient.HUD;
using EndlessClient.HUD.Controls;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Map;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Interact;
using EOLib.Domain.Item;
using EOLib.Domain.Map;
using EOLib.IO;
using EOLib.IO.Pub;
using EOLib.IO.Repositories;
using EOLib.Localization;
using System;
using System.Linq;
using XNAControls;

namespace EndlessClient.Controllers
{
    [AutoMappedType]
    public class InventoryController : IInventoryController
    {
        private readonly IItemActions _itemActions;
        private readonly IInGameDialogActions _inGameDialogActions;
        private readonly IPaperdollActions _paperdollActions;
        private readonly IChestActions _chestActions;
        private readonly ILockerActions _lockerActions;
        private readonly IItemEquipValidator _itemEquipValidator;
        private readonly IItemDropValidator _itemDropValidator;
        private readonly ICharacterProvider _characterProvider;
        private readonly IPaperdollProvider _paperdollProvider;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly IActiveDialogProvider _activeDialogProvider;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IItemTransferDialogFactory _itemTransferDialogFactory;
        private readonly IEOMessageBoxFactory _eoMessageBoxFactory;

        public InventoryController(IItemActions itemActions,
                                   IInGameDialogActions inGameDialogActions,
                                   IPaperdollActions paperdollActions,
                                   IChestActions chestActions,
                                   ILockerActions lockerActions,
                                   IItemEquipValidator itemEquipValidator,
                                   IItemDropValidator itemDropValidator,
                                   ICharacterProvider characterProvider,
                                   IPaperdollProvider paperdollProvider,
                                   IHudControlProvider hudControlProvider,
                                   ICurrentMapProvider currentMapProvider,
                                   IEIFFileProvider eifFileProvider,
                                   IActiveDialogProvider activeDialogProvider,
                                   IStatusLabelSetter statusLabelSetter,
                                   IItemTransferDialogFactory itemTransferDialogFactory,
                                   IEOMessageBoxFactory eoMessageBoxFactory)
        {
            _itemActions = itemActions;
            _inGameDialogActions = inGameDialogActions;
            _paperdollActions = paperdollActions;
            _chestActions = chestActions;
            _lockerActions = lockerActions;
            _itemEquipValidator = itemEquipValidator;
            _itemDropValidator = itemDropValidator;
            _characterProvider = characterProvider;
            _paperdollProvider = paperdollProvider;
            _hudControlProvider = hudControlProvider;
            _currentMapProvider = currentMapProvider;
            _eifFileProvider = eifFileProvider;
            _activeDialogProvider = activeDialogProvider;
            _statusLabelSetter = statusLabelSetter;
            _itemTransferDialogFactory = itemTransferDialogFactory;
            _eoMessageBoxFactory = eoMessageBoxFactory;
        }

        public void ShowPaperdollDialog()
        {
            // when called from inventory controller, paperdoll is for the main character
            _paperdollActions.RequestPaperdoll(_characterProvider.MainCharacter.ID);
            _inGameDialogActions.ShowPaperdollDialog(_characterProvider.MainCharacter, isMainCharacter: true);
        }

        public void UseItem(EIFRecord record)
        {
            var useItem = false;

            var character = _characterProvider.MainCharacter;

            switch (record.Type)
            {
                //usable items
                case ItemType.Teleport:
                    if (!_currentMapProvider.CurrentMap.Properties.CanScroll)
                    {
                        _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION, EOResourceID.STATUS_LABEL_NOTHING_HAPPENED);
                        break;
                    }

                    if (record.ScrollMap == character.MapID && record.ScrollX == character.RenderProperties.MapX && record.ScrollY == character.RenderProperties.MapY)
                        break;

                    useItem = true;
                    break;

                case ItemType.CureCurse:
                    var paperdollItems = _paperdollProvider.VisibleCharacterPaperdolls[_characterProvider.MainCharacter.ID].Paperdoll.Values;
                    if (paperdollItems.Where(id => id > 0).Select(id => _eifFileProvider.EIFFile[id].Special).Any(s => s == ItemSpecial.Cursed))
                    {
                        var msgBox = _eoMessageBoxFactory.CreateMessageBox(DialogResourceID.ITEM_CURSE_REMOVE_PROMPT, EODialogButtons.OkCancel, EOMessageBoxStyle.SmallDialogSmallHeader);
                        msgBox.DialogClosing += (o, e) =>
                        {
                            if (e.Result == XNADialogResult.OK)
                            {
                                _itemActions.UseItem((short)record.ID);
                            }
                        };
                        msgBox.ShowDialog();
                    }
                    break;

                case ItemType.Heal:
                case ItemType.HairDye:
                case ItemType.Beer:
                case ItemType.EffectPotion:
                case ItemType.EXPReward:
                    useItem = true;
                    break;

                // Not implemented server - side
                case ItemType.SkillReward:
                case ItemType.StatReward:
                    break;
            }

            if (useItem)
            {
                _itemActions.UseItem((short)record.ID);

                if (record.Type == ItemType.Beer)
                {
                    // The server does not send back the potency, it is all client-side
                    _hudControlProvider.GetComponent<IPeriodicEmoteHandler>(HudControlIdentifier.PeriodicEmoteHandler)
                        .SetDrunkTimeout(record.BeerPotency);
                }
            }
        }

        public void EquipItem(EIFRecord itemData)
        {
            var c = _characterProvider.MainCharacter;
            var (validationResult, detail, isAlternateEquipLocation) = _itemEquipValidator.ValidateItemEquip(c, itemData);

            switch (validationResult)
            {
                case ItemEquipResult.NotEquippable:
                    throw new ArgumentException("Item is not equippable", nameof(itemData));
                case ItemEquipResult.AlreadyEquipped:
                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, EOResourceID.STATUS_LABEL_ITEM_EQUIP_TYPE_ALREADY_EQUIPPED);
                    break;
                case ItemEquipResult.WrongGender:
                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, EOResourceID.STATUS_LABEL_ITEM_EQUIP_DOES_NOT_FIT_GENDER);
                    break;
                case ItemEquipResult.StatRequirementNotMet:
                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION,
                        EOResourceID.STATUS_LABEL_ITEM_EQUIP_THIS_ITEM_REQUIRES, detail);
                    break;
                case ItemEquipResult.ClassRequirementNotMet:
                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION,
                        EOResourceID.STATUS_LABEL_ITEM_EQUIP_CAN_ONLY_BE_USED_BY, detail);
                    break;
                case ItemEquipResult.Ok:
                    _itemActions.EquipItem((short)itemData.ID, isAlternateEquipLocation);
                    break;
            }
        }

        public void UnequipItem(EquipLocation equipLocation)
        {
            var locName = Enum.GetName(typeof(EquipLocation), equipLocation);
            var equipId = _paperdollProvider.VisibleCharacterPaperdolls[_characterProvider.MainCharacter.ID].Paperdoll[equipLocation];
            _itemActions.UnequipItem(equipId, alternateLocation: locName.Contains('2'));
        }

        public void DropItem(EIFRecord itemData, InventoryItem inventoryItem)
        {
            var mapRenderer = _hudControlProvider.GetComponent<IMapRenderer>(HudControlIdentifier.MapRenderer);
            if (_activeDialogProvider.ActiveDialogs.Any(x => x.HasValue) && mapRenderer.MouseOver)
                return;

            var rp = _characterProvider.MainCharacter.RenderProperties;
            var dropPoint = mapRenderer.MouseOver
                ? mapRenderer.GridCoordinates
                : new MapCoordinate(rp.MapX, rp.MapY);

            var validationResult = _itemDropValidator.ValidateItemDrop(_characterProvider.MainCharacter, inventoryItem, dropPoint);

            if (validationResult == ItemDropResult.Lore)
            {
                var msgBox = _eoMessageBoxFactory.CreateMessageBox(DialogResourceID.ITEM_IS_LORE_ITEM, EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                msgBox.ShowDialog();
            }
            else if (validationResult == ItemDropResult.Jail)
            {
                var msgBox = _eoMessageBoxFactory.CreateMessageBox(
                    EOResourceID.JAIL_WARNING_CANNOT_DROP_ITEMS,
                    EOResourceID.STATUS_LABEL_TYPE_WARNING,
                    EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                msgBox.ShowDialog();
            }
            else if (validationResult == ItemDropResult.Ok)
            {
                DoItemDrop(itemData, inventoryItem, a => _itemActions.DropItem(inventoryItem.ItemID, a, dropPoint));
            }
            else if (validationResult == ItemDropResult.TooFar)
            {
                _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.STATUS_LABEL_ITEM_DROP_OUT_OF_RANGE);
            }
        }

        public void DropItemInChest(EIFRecord itemData, InventoryItem inventoryItem)
        {
            var validationResult = _itemDropValidator.ValidateItemDrop(_characterProvider.MainCharacter, inventoryItem);

            if (validationResult == ItemDropResult.Lore)
            {
                var msgBox = _eoMessageBoxFactory.CreateMessageBox(DialogResourceID.ITEM_IS_LORE_ITEM);
                msgBox.ShowDialog();
            }
            else
            {
                DoItemDrop(itemData, inventoryItem, a => _chestActions.AddItemToChest(inventoryItem.WithAmount(a)));
            }
        }

        public void DropItemInLocker(EIFRecord itemData, InventoryItem inventoryItem)
        {
            if (inventoryItem.ItemID == 1)
            {
                var dlg = _eoMessageBoxFactory.CreateMessageBox(DialogResourceID.LOCKER_DEPOSIT_GOLD_ERROR);
                dlg.ShowDialog();
            }
            else
            {
                DoItemDrop(itemData, inventoryItem, a =>
                {
                    if (_lockerActions.GetNewItemAmount(inventoryItem.ItemID, a) > Constants.LockerMaxSingleItemAmount)
                    {
                        var dlg = _eoMessageBoxFactory.CreateMessageBox(DialogResourceID.LOCKER_FULL_SINGLE_ITEM_MAX);
                        dlg.ShowDialog();
                    }
                    else
                    {
                        _lockerActions.AddItemToLocker(inventoryItem.WithAmount(a));
                    }
                },
                ItemTransferDialog.TransferType.ShopTransfer,
                EOResourceID.DIALOG_TRANSFER_TRANSFER);
            }
        }

        public void JunkItem(EIFRecord itemData, InventoryItem inventoryItem)
        {
            if (inventoryItem.Amount > 1)
            {
                var transferDialog = _itemTransferDialogFactory.CreateItemTransferDialog(
                    itemData.Name,
                    ItemTransferDialog.TransferType.JunkItems,
                    inventoryItem.Amount,
                    EOResourceID.DIALOG_TRANSFER_JUNK);
                transferDialog.DialogClosing += (sender, e) =>
                {
                    if (e.Result == XNADialogResult.OK)
                    {
                        _itemActions.JunkItem(inventoryItem.ItemID, transferDialog.SelectedAmount);
                    }
                };
                transferDialog.ShowDialog();
            }
            else
            {
                _itemActions.JunkItem(inventoryItem.ItemID, 1);
            }
        }

        private void DoItemDrop(EIFRecord itemData, InventoryItem inventoryItem, Action<int> dropAction,
                                ItemTransferDialog.TransferType transferType = ItemTransferDialog.TransferType.DropItems,
                                EOResourceID message = EOResourceID.DIALOG_TRANSFER_DROP)
        {
            if (inventoryItem.Amount > 1)
            {
                var transferDialog = _itemTransferDialogFactory.CreateItemTransferDialog(
                    itemData.Name,
                    transferType,
                    inventoryItem.Amount,
                    message);
                transferDialog.DialogClosing += (sender, e) =>
                {
                    if (e.Result == XNADialogResult.OK)
                    {
                        if (inventoryItem.ItemID == 1 && transferDialog.SelectedAmount > 10000)
                        {
                            var warningMsg = _eoMessageBoxFactory.CreateMessageBox(DialogResourceID.DROP_MANY_GOLD_ON_GROUND, EODialogButtons.OkCancel);
                            warningMsg.DialogClosing += (_, warningArgs) =>
                            {
                                if (warningArgs.Result == XNADialogResult.OK)
                                    dropAction(transferDialog.SelectedAmount);
                            };
                            warningMsg.ShowDialog();
                        }
                        else
                        {
                            dropAction(transferDialog.SelectedAmount);
                        }
                    }
                };
                transferDialog.ShowDialog();
            }
            else
            {
                dropAction(1);
            }
        }
    }

    public interface IInventoryController
    {
        void ShowPaperdollDialog();

        void UseItem(EIFRecord record);

        void EquipItem(EIFRecord itemData);

        void UnequipItem(EquipLocation equipLocation);

        void DropItem(EIFRecord itemData, InventoryItem inventoryItem);

        void DropItemInChest(EIFRecord itemData, InventoryItem inventoryItem);

        void DropItemInLocker(EIFRecord itemData, InventoryItem inventoryItem);

        void JunkItem(EIFRecord itemData, InventoryItem inventoryItem);
    }
}
