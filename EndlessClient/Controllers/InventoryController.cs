using AutomaticTypeMapper;
using EndlessClient.ControlSets;
using EndlessClient.Dialogs;
using EndlessClient.Dialogs.Actions;
using EndlessClient.Dialogs.Factories;
using EndlessClient.HUD;
using EndlessClient.HUD.Controls;
using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using EOLib.Domain.Item;
using EOLib.Domain.Map;
using EOLib.IO;
using EOLib.IO.Extensions;
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
        private readonly ICharacterProvider _characterProvider;
        private readonly IPaperdollProvider _paperdollProvider;
        private readonly IPubFileProvider _pubFileProvider;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly IActiveDialogProvider _activeDialogProvider;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IItemTransferDialogFactory _itemTransferDialogFactory;
        private readonly IEOMessageBoxFactory _eoMessageBoxFactory;

        public InventoryController(IItemActions itemActions,
                                   IInGameDialogActions inGameDialogActions,
                                   ICharacterProvider characterProvider,
                                   IPaperdollProvider paperdollProvider,
                                   IPubFileProvider pubFileProvider,
                                   IHudControlProvider hudControlProvider,
                                   ICurrentMapStateProvider currentMapStateProvider,
                                   ICurrentMapProvider currentMapProvider,
                                   IEIFFileProvider eifFileProvider,
                                   IActiveDialogProvider activeDialogProvider,
                                   IStatusLabelSetter statusLabelSetter,
                                   IItemTransferDialogFactory itemTransferDialogFactory,
                                   IEOMessageBoxFactory eoMessageBoxFactory)
        {
            _itemActions = itemActions;
            _inGameDialogActions = inGameDialogActions;
            _characterProvider = characterProvider;
            _paperdollProvider = paperdollProvider;
            _pubFileProvider = pubFileProvider;
            _hudControlProvider = hudControlProvider;
            _currentMapStateProvider = currentMapStateProvider;
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
                case ItemType.Heal:
                case ItemType.HairDye:
                case ItemType.Beer:
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
                case ItemType.EffectPotion:
                case ItemType.EXPReward: // todo: EXPReward has not been tested
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
            }
        }

        public void EquipItem(EIFRecord itemData)
        {
            if (itemData.Type < ItemType.Weapon || itemData.Type > ItemType.Bracer)
                throw new ArgumentException("Item is not equippable", nameof(itemData));

            // todo: move validation logic to validator class
            var c = _characterProvider.MainCharacter;
            if (!_paperdollProvider.VisibleCharacterPaperdolls.ContainsKey(c.ID))
            {
                // emulate client login bug: when the paperdoll doesn't exist, show an "already equipped" message
                // see: https://eoserv.net/bugs/view_bug/441
                _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, EOResourceID.STATUS_LABEL_ITEM_EQUIP_TYPE_ALREADY_EQUIPPED);
                return;
            }

            var isAlternateEquipLocation = false;

            var paperdoll = _paperdollProvider.VisibleCharacterPaperdolls[c.ID].Paperdoll;
            var equipLocation = itemData.GetEquipLocation();

            switch (itemData.Type)
            {
                case ItemType.Armlet:
                case ItemType.Bracer:
                case ItemType.Ring:
                    {
                        if (paperdoll[equipLocation] != 0)
                        {
                            isAlternateEquipLocation = true;
                            if (paperdoll[equipLocation + 1] != 0)
                            {
                                _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, EOResourceID.STATUS_LABEL_ITEM_EQUIP_TYPE_ALREADY_EQUIPPED);
                                return;
                            }
                        }
                    }
                    break;
                case ItemType.Armor:
                    {
                        if (c.RenderProperties.Gender != itemData.Gender)
                        {
                            _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, EOResourceID.STATUS_LABEL_ITEM_EQUIP_DOES_NOT_FIT_GENDER);
                            return;
                        }
                    }
                    break;
            }

            var reqs = new int[6];
            var reqNames = new[] { "STR", "INT", "WIS", "AGI", "CON", "CHA" };
            if ((reqs[0] = itemData.StrReq) > c.Stats[CharacterStat.Strength] || (reqs[1] = itemData.IntReq) > c.Stats[CharacterStat.Intelligence]
                || (reqs[2] = itemData.WisReq) > c.Stats[CharacterStat.Wisdom] || (reqs[3] = itemData.AgiReq) > c.Stats[CharacterStat.Agility]
                || (reqs[4] = itemData.ConReq) > c.Stats[CharacterStat.Constituion] || (reqs[5] = itemData.ChaReq) > c.Stats[CharacterStat.Charisma])
            {
                var req = reqs.Select((i, n) => new { Req = n, Ndx = i }).First(x => x.Req > 0);

                _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION,
                    EOResourceID.STATUS_LABEL_ITEM_EQUIP_THIS_ITEM_REQUIRES,
                    $" {reqs[req.Ndx]} {reqNames[req.Ndx]}");
                return;
            }

            if (itemData.ClassReq > 0 && itemData.ClassReq != c.ClassID)
            {
                _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION,
                    EOResourceID.STATUS_LABEL_ITEM_EQUIP_CAN_ONLY_BE_USED_BY,
                    _pubFileProvider.ECFFile[itemData.ClassReq].Name);
                return;
            }

            if (paperdoll[equipLocation] != 0 && !isAlternateEquipLocation)
            {
                _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, EOResourceID.STATUS_LABEL_ITEM_EQUIP_TYPE_ALREADY_EQUIPPED);
                return;
            }

            _itemActions.EquipItem((short)itemData.ID, isAlternateEquipLocation);
        }

        public void UnequipItem(EquipLocation equipLocation)
        {
            var locName = Enum.GetName(typeof(EquipLocation), equipLocation);
            var equipId = _paperdollProvider.VisibleCharacterPaperdolls[_characterProvider.MainCharacter.ID].Paperdoll[equipLocation];
            _itemActions.UnequipItem(equipId, alternateLocation: locName.Contains('2'));
        }

        public void DropItem(EIFRecord itemData, IInventoryItem inventoryItem)
        {
            var mapRenderer = _hudControlProvider.GetComponent<IMapRenderer>(HudControlIdentifier.MapRenderer);
            if (_activeDialogProvider.ActiveDialogs.Any() && mapRenderer.MouseOver)
                return;

            var rp = _characterProvider.MainCharacter.RenderProperties;
            var dropPoint = mapRenderer.MouseOver
                ? mapRenderer.GridCoordinates
                : new MapCoordinate(rp.MapX, rp.MapY);

            var inRange = Math.Max(Math.Abs(rp.MapX - dropPoint.X), Math.Abs(rp.MapY - dropPoint.Y)) <= 2;

            // todo: move validation logic to validator class
            if (itemData.Special == ItemSpecial.Lore)
            {
                var msgBox = _eoMessageBoxFactory.CreateMessageBox(DialogResourceID.ITEM_IS_LORE_ITEM, EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                msgBox.ShowDialog();
            }
            else if (_currentMapStateProvider.JailMapID == _currentMapStateProvider.CurrentMapID)
            {
                var msgBox = _eoMessageBoxFactory.CreateMessageBox(
                    EOResourceID.JAIL_WARNING_CANNOT_DROP_ITEMS,
                    EOResourceID.STATUS_LABEL_TYPE_WARNING,
                    EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                msgBox.ShowDialog();
            }
            else if (inRange)
            {
                if (inventoryItem.Amount > 1)
                {
                    var transferDialog = _itemTransferDialogFactory.CreateItemTransferDialog(
                        itemData.Name,
                        ItemTransferDialog.TransferType.DropItems,
                        inventoryItem.Amount,
                        EOResourceID.DIALOG_TRANSFER_DROP);
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
                                        _itemActions.DropItem(inventoryItem.ItemID, transferDialog.SelectedAmount, dropPoint);
                                };
                                warningMsg.ShowDialog();
                            }
                            else
                            {
                                _itemActions.DropItem(inventoryItem.ItemID, transferDialog.SelectedAmount, dropPoint);
                            }
                        }
                    };
                    transferDialog.ShowDialog();
                }
                else
                {
                    _itemActions.DropItem(inventoryItem.ItemID, 1, dropPoint);
                }
            }
            else
            {
                _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.STATUS_LABEL_ITEM_DROP_OUT_OF_RANGE);
            }
        }

        public void JunkItem(EIFRecord itemData, IInventoryItem inventoryItem)
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
    }

    public interface IInventoryController
    {
        void ShowPaperdollDialog();

        void UseItem(EIFRecord record);

        void EquipItem(EIFRecord itemData);

        void UnequipItem(EquipLocation equipLocation);

        void DropItem(EIFRecord itemData, IInventoryItem inventoryItem);

        void JunkItem(EIFRecord itemData, IInventoryItem inventoryItem);
    }
}
