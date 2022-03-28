using AutomaticTypeMapper;
using EndlessClient.Dialogs;
using EndlessClient.Dialogs.Actions;
using EndlessClient.HUD;
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
        private readonly IStatusLabelSetter _statusLabelSetter;

        public InventoryController(IItemActions itemActions,
                                   IInGameDialogActions inGameDialogActions,
                                   ICharacterProvider characterProvider,
                                   IPaperdollProvider paperdollProvider,
                                   IPubFileProvider pubFileProvider,
                                   IStatusLabelSetter statusLabelSetter)
        {
            _itemActions = itemActions;
            _inGameDialogActions = inGameDialogActions;
            _characterProvider = characterProvider;
            _paperdollProvider = paperdollProvider;
            _pubFileProvider = pubFileProvider;
            _statusLabelSetter = statusLabelSetter;
        }

        public void ShowPaperdollDialog()
        {
            // when called from inventory controller, paperdoll is for the main character
            _inGameDialogActions.ShowPaperdollDialog(_characterProvider.MainCharacter, isMainCharacter: true);
        }

        public void UseItem(EIFRecord record)
        {
            switch (record.Type)
            {
                //usable items
                case ItemType.Teleport:
                    //if (!OldWorld.Instance.ActiveMapRenderer.MapRef.Properties.CanScroll)
                    //{
                    //    EOGame.Instance.Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION, EOResourceID.STATUS_LABEL_NOTHING_HAPPENED);
                    //    break;
                    //}
                    //if (m_itemData.ScrollMap == OldWorld.Instance.MainPlayer.ActiveCharacter.CurrentMap &&
                    //    m_itemData.ScrollX == OldWorld.Instance.MainPlayer.ActiveCharacter.X &&
                    //    m_itemData.ScrollY == OldWorld.Instance.MainPlayer.ActiveCharacter.Y)
                    break; //already there - no need to scroll!
                           //useItem = true;
                    break;
                case ItemType.Heal:
                case ItemType.HairDye:
                case ItemType.Beer:
                    //useItem = true;
                    break;
                case ItemType.CureCurse:
                    //note: don't actually set the useItem bool here. Call API.UseItem if the dialog result is OK.
                    //if (c.PaperDoll.Select(id => OldWorld.Instance.EIF[id])
                    //               .Any(rec => rec.Special == ItemSpecial.Cursed))
                    //{
                    //    EOMessageBox.Show(DialogResourceID.ITEM_CURSE_REMOVE_PROMPT, EODialogButtons.OkCancel, EOMessageBoxStyle.SmallDialogSmallHeader,
                    //        (o, e) =>
                    //        {
                    //            //if (e.Result == XNADialogResult.OK && !m_api.UseItem((short)m_itemData.ID))
                    //            //{
                    //            //    ((EOGame)Game).DoShowLostConnectionDialogAndReturnToMainMenu();
                    //            //}
                    //        });
                    //}
                    break;
                case ItemType.EXPReward:
                    //useItem = true;
                    break;
                case ItemType.EffectPotion:
                    //useItem = true;
                    break;
                    //Not implemented server-side
                    //case ItemType.SkillReward:
                    //    break;
                    //case ItemType.StatReward:
                    //    break;
            }

            //if (useItem && !m_api.UseItem((short)m_itemData.ID))
            //    ((EOGame)Game).DoShowLostConnectionDialogAndReturnToMainMenu();
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
            /*
            if (((OldEOInventory)parent).IsOverDrop() || (OldWorld.Instance.ActiveMapRenderer.MouseOver
                //&& ChestDialog.Instance == null && EOPaperdollDialog.Instance == null && LockerDialog.Instance == null
                && BankAccountDialog.Instance == null && TradeDialog.Instance == null))
            {
                Point loc = OldWorld.Instance.ActiveMapRenderer.MouseOver ? OldWorld.Instance.ActiveMapRenderer.GridCoords :
                    new Point(OldWorld.Instance.MainPlayer.ActiveCharacter.X, OldWorld.Instance.MainPlayer.ActiveCharacter.Y);

                //in range if maximum coordinate difference is <= 2 away
                bool inRange = Math.Abs(Math.Max(OldWorld.Instance.MainPlayer.ActiveCharacter.X - loc.X, OldWorld.Instance.MainPlayer.ActiveCharacter.Y - loc.Y)) <= 2;

                if (m_itemData.Special == ItemSpecial.Lore)
                {
                    EOMessageBox.Show(DialogResourceID.ITEM_IS_LORE_ITEM, EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                }
                else if (OldWorld.Instance.JailMap == OldWorld.Instance.MainPlayer.ActiveCharacter.CurrentMap)
                {
                    EOMessageBox.Show(OldWorld.GetString(EOResourceID.JAIL_WARNING_CANNOT_DROP_ITEMS),
                        OldWorld.GetString(EOResourceID.STATUS_LABEL_TYPE_WARNING),
                        EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                }
                else if (m_inventory.Amount > 1 && inRange)
                {
                    ItemTransferDialog dlg = new ItemTransferDialog(m_itemData.Name, ItemTransferDialog.TransferType.DropItems,
                        m_inventory.Amount);
                    dlg.DialogClosing += (sender, args) =>
                    {
                        if (args.Result == XNADialogResult.OK)
                        {
                            //note: not sure of the actual limit. 10000 is arbitrary here
                            if (dlg.SelectedAmount > 10000 && m_inventory.ItemID == 1 && !safetyCommentHasBeenShown)
                                EOMessageBox.Show(DialogResourceID.DROP_MANY_GOLD_ON_GROUND, EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader,
                                    (o, e) => { safetyCommentHasBeenShown = true; });
                            else if (!m_api.DropItem(m_inventory.ItemID, dlg.SelectedAmount, (byte)loc.X, (byte)loc.Y))
                                ((EOGame)Game).DoShowLostConnectionDialogAndReturnToMainMenu();
                        }
                    };
                }
                else if (inRange)
                {
                    if (!m_api.DropItem(m_inventory.ItemID, 1, (byte)loc.X, (byte)loc.Y))
                        ((EOGame)Game).DoShowLostConnectionDialogAndReturnToMainMenu();
                }
                else //if (!inRange)
                {
                    EOGame.Instance.Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.STATUS_LABEL_ITEM_DROP_OUT_OF_RANGE);
                }
            }
            */
        }

        public void JunkItem(EIFRecord itemData, IInventoryItem inventoryItem)
        {
            /*
             if (((OldEOInventory)parent).IsOverJunk())
            {
                if (m_inventory.Amount > 1)
                {
                    ItemTransferDialog dlg = new ItemTransferDialog(m_itemData.Name, ItemTransferDialog.TransferType.JunkItems,
                        m_inventory.Amount, EOResourceID.DIALOG_TRANSFER_JUNK);
                    dlg.DialogClosing += (sender, args) =>
                    {
                        if (args.Result == XNADialogResult.OK && !m_api.JunkItem(m_inventory.ItemID, dlg.SelectedAmount))
                            ((EOGame)Game).DoShowLostConnectionDialogAndReturnToMainMenu();
                    };
                }
                else if (!m_api.JunkItem(m_inventory.ItemID, 1))
                    ((EOGame)Game).DoShowLostConnectionDialogAndReturnToMainMenu();
            }
             */
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
