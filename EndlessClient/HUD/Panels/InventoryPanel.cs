using EndlessClient.Controllers;
using EndlessClient.ControlSets;
using EndlessClient.Dialogs;
using EndlessClient.HUD.Controls;
using EndlessClient.HUD.Inventory;
using EndlessClient.Rendering.Map;
using EOLib;
using EOLib.Config;
using EOLib.Domain.Character;
using EOLib.Domain.Item;
using EOLib.Domain.Login;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Extensions;
using EOLib.IO.Map;
using EOLib.IO.Pub;
using EOLib.IO.Repositories;
using EOLib.Localization;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Optional;
using Optional.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using XNAControls;

namespace EndlessClient.HUD.Panels
{
    public class InventoryPanel : XNAPanel, IHudPanel
    {
        public const int InventoryRows = 4;
        public const int InventoryRowSlots = 14;

        private readonly IInventoryController _inventoryController;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IItemStringService _itemStringService;
        private readonly IItemNameColorService _itemNameColorService;
        private readonly IInventoryService _inventoryService;
        private readonly IInventorySlotRepository _inventorySlotRepository;
        private readonly IPlayerInfoProvider _playerInfoProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly ICharacterInventoryProvider _characterInventoryProvider;
        private readonly IPubFileProvider _pubFileProvider; // todo: this can probably become EIFFileProvider
        private readonly IHudControlProvider _hudControlProvider;
        private readonly IActiveDialogProvider _activeDialogProvider;

        private readonly Dictionary<int, int> _itemSlotMap;
        private readonly List<InventoryPanelItem> _childItems = new List<InventoryPanelItem>();

        private readonly IXNALabel _weightLabel;
        private readonly IXNAButton _drop, _junk, _paperdoll;
        //private readonly ScrollBar _scrollBar;

        private Option<CharacterStats> _cachedStats;
        private HashSet<IInventoryItem> _cachedInventory;

        public INativeGraphicsManager NativeGraphicsManager { get; }

        public InventoryPanel(INativeGraphicsManager nativeGraphicsManager,
                              IInventoryController inventoryController,
                              IStatusLabelSetter statusLabelSetter,
                              IItemStringService itemStringService,
                              IItemNameColorService itemNameColorService,
                              IInventoryService inventoryService,
                              IInventorySlotRepository inventorySlotRepository,
                              IPlayerInfoProvider playerInfoProvider,
                              ICharacterProvider characterProvider,
                              ICharacterInventoryProvider characterInventoryProvider,
                              IPubFileProvider pubFileProvider,
                              IHudControlProvider hudControlProvider,
                              IActiveDialogProvider activeDialogProvider)
        {
            NativeGraphicsManager = nativeGraphicsManager;
            _inventoryController = inventoryController;
            _statusLabelSetter = statusLabelSetter;
            _itemStringService = itemStringService;
            _itemNameColorService = itemNameColorService;
            _inventoryService = inventoryService;
            _inventorySlotRepository = inventorySlotRepository;
            _playerInfoProvider = playerInfoProvider;
            _characterProvider = characterProvider;
            _characterInventoryProvider = characterInventoryProvider;
            _pubFileProvider = pubFileProvider;
            _hudControlProvider = hudControlProvider;
            _activeDialogProvider = activeDialogProvider;
            _weightLabel = new XNALabel(Constants.FontSize08pt5)
            {
                DrawArea = new Rectangle(385, 37, 88, 18),
                ForeColor = ColorConstants.LightGrayText,
                TextAlign = LabelAlignment.MiddleCenter,
                Visible = true,
                AutoSize = false
            };

            _itemSlotMap = GetItemSlotMap(_playerInfoProvider.LoggedInAccountName, _characterProvider.MainCharacter.Name);

            var weirdOffsetSheet = NativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 27);

            _paperdoll = new XNAButton(weirdOffsetSheet, new Vector2(385, 9), new Rectangle(39, 385, 88, 19), new Rectangle(126, 385, 88, 19));
            _paperdoll.OnMouseEnter += MouseOverButton;
            _paperdoll.OnClick += (_, _) =>
            {
                if (NoItemsDragging())
                    _inventoryController.ShowPaperdollDialog();
            };

            _drop = new XNAButton(weirdOffsetSheet, new Vector2(389, 68), new Rectangle(0, 15, 38, 37), new Rectangle(0, 52, 38, 37));
            _drop.OnMouseEnter += MouseOverButton;

            _junk = new XNAButton(weirdOffsetSheet, new Vector2(431, 68), new Rectangle(0, 89, 38, 37), new Rectangle(0, 126, 38, 37));
            _junk.OnMouseEnter += MouseOverButton;

            _cachedStats = Option.None<CharacterStats>();
            _cachedInventory = new HashSet<IInventoryItem>();

            BackgroundImage = NativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 44);
            DrawArea = new Rectangle(102, 330, BackgroundImage.Width, BackgroundImage.Height);

            Game.Exiting += SaveInventoryFile;
        }

        public bool NoItemsDragging() => !_childItems.Any(x => x.IsDragging);

        public override void Initialize()
        {
            _weightLabel.Initialize();
            _weightLabel.SetParentControl(this);

            _paperdoll.Initialize();
            _paperdoll.SetParentControl(this);

            _drop.Initialize();
            _drop.SetParentControl(this);

            _junk.Initialize();
            _junk.SetParentControl(this);

            base.Initialize();
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            _cachedStats.Match(
                some: stats =>
                {
                    stats.SomeWhen(s => s != _characterProvider.MainCharacter.Stats)
                        .MatchSome(_ =>
                        {
                            var newStats = _characterProvider.MainCharacter.Stats;
                            _cachedStats = Option.Some(newStats);
                            _weightLabel.Text = $"{newStats[CharacterStat.Weight]} / {newStats[CharacterStat.MaxWeight]}";
                        });
                },
                none: () =>
                {
                    var stats = _characterProvider.MainCharacter.Stats;
                    _cachedStats = Option.Some(stats);
                    _weightLabel.Text = $"{stats[CharacterStat.Weight]} / {stats[CharacterStat.MaxWeight]}";
                });

            if (!_cachedInventory.SetEquals(_characterInventoryProvider.ItemInventory))
            {
                var added = _characterInventoryProvider.ItemInventory.Where(i => !_cachedInventory.Any(j => i.ItemID == j.ItemID));
                var removed = _cachedInventory.Where(i => !_characterInventoryProvider.ItemInventory.Any(j => i.ItemID == j.ItemID));
                var updated = _characterInventoryProvider.ItemInventory.Except(added)
                    .Where(i => _cachedInventory.Any(j => i.ItemID == j.ItemID && i.Amount != j.Amount));

                foreach (var item in removed)
                {
                    var matchedItem = _childItems.SingleOrNone(x => x.InventoryItem.ItemID == item.ItemID);
                    matchedItem.MatchSome(childItem =>
                    {
                        childItem.SetControlUnparented();
                        childItem.Dispose();
                        _childItems.Remove(childItem);

                        var itemData = _pubFileProvider.EIFFile[item.ItemID];
                        _inventoryService.ClearSlots(_inventorySlotRepository.FilledSlots, childItem.Slot, itemData.Size);
                    });
                }

                foreach (var item in updated)
                {
                    var itemData = _pubFileProvider.EIFFile[item.ItemID];

                    var matchedItem = _childItems.SingleOrNone(x => x.InventoryItem.ItemID == item.ItemID);
                    matchedItem.MatchSome(childItem =>
                    {
                        childItem.InventoryItem = item;
                        childItem.Text = _itemStringService.GetStringForInventoryDisplay(itemData, item.Amount);
                    });
                }

                foreach (var item in added)
                {
                    var itemData = _pubFileProvider.EIFFile[item.ItemID];

                    var preferredSlot = _itemSlotMap.SingleOrNone(x => x.Value == item.ItemID).Map(x => x.Key);
                    var actualSlot = _inventoryService.GetNextOpenSlot(_inventorySlotRepository.FilledSlots, itemData.Size, preferredSlot);

                    actualSlot.MatchSome(slot =>
                    {
                        _inventoryService.SetSlots(_inventorySlotRepository.FilledSlots, slot, itemData.Size);

                        var newItem = new InventoryPanelItem(_itemNameColorService, this, _activeDialogProvider, slot, item, itemData);
                        newItem.Initialize();
                        newItem.SetParentControl(this);
                        newItem.Text = _itemStringService.GetStringForInventoryDisplay(itemData, item.Amount);

                        newItem.OnMouseEnter += (_, _) => _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ITEM, newItem.Text);
                        newItem.DoubleClick += HandleItemDoubleClick;
                        newItem.DoneDragging += HandleItemDoneDragging;

                        // side-effect of calling newItem.SetParentControl(this) is that the draw order gets reset
                        // setting the slot manually here resets it so the item labels render appropriately
                        newItem.Slot = slot;

                        _childItems.Add(newItem);
                    });
                }

                _cachedInventory = _characterInventoryProvider.ItemInventory.ToHashSet();
            }

            base.OnUpdateControl(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _paperdoll.OnMouseEnter -= MouseOverButton;
                _drop.OnMouseEnter -= MouseOverButton;
                _junk.OnMouseEnter -= MouseOverButton;
                Game.Exiting -= SaveInventoryFile;

                // todo: IResettable should work but it doesn't
                _inventorySlotRepository.FilledSlots = new Matrix<bool>(InventoryRows, InventoryRowSlots, false);

                SaveInventoryFile(null, EventArgs.Empty);
            }

            base.Dispose(disposing);
        }

        private void MouseOverButton(object sender, EventArgs e)
        {
            var id = sender == _paperdoll
                ? EOResourceID.STATUS_LABEL_INVENTORY_SHOW_YOUR_PAPERDOLL
                : sender == _drop
                    ? EOResourceID.STATUS_LABEL_INVENTORY_DROP_BUTTON
                    : EOResourceID.STATUS_LABEL_INVENTORY_JUNK_BUTTON;
            _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_BUTTON, id);
        }

        private static Dictionary<int, int> GetItemSlotMap(string accountName, string characterName)
        {
            var map = new Dictionary<int, int>();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !File.Exists(Constants.InventoryFile))
            {
                using var inventoryKey = TryGetCharacterRegistryKey(accountName, characterName);
                if (inventoryKey != null)
                {
                    for (int i = 0; i < InventoryRowSlots * 4; ++i)
                    {
                        if (int.TryParse(inventoryKey.GetValue($"item{i}")?.ToString() ?? string.Empty, out var id))
                            map[i] = id;
                    }
                }
            }

            var inventory = new IniReader(Constants.InventoryFile);
            if (inventory.Load() && inventory.Sections.ContainsKey(accountName))
            {
                var section = inventory.Sections[accountName];
                foreach (var key in section.Keys.Where(x => x.Contains(characterName, StringComparison.OrdinalIgnoreCase)))
                {
                    if (!key.Contains("."))
                        continue;

                    var slot = key.Split(".")[1];
                    if (!int.TryParse(slot, out var slotIndex))
                        continue;

                    if (int.TryParse(section[key], out var id))
                        map[slotIndex] = id;
                }
            }

            return map;
        }

        [SupportedOSPlatform("Windows")]
        private static RegistryKey TryGetCharacterRegistryKey(string accountName, string characterName)
        {
            using RegistryKey currentUser = Registry.CurrentUser;

            var pathSegments = $"Software\\EndlessClient\\{accountName}\\{characterName}\\inventory".Split('\\');
            var currentPath = string.Empty;

            RegistryKey retKey = null;
            foreach (var segment in pathSegments)
            {
                retKey?.Dispose();

                currentPath = Path.Combine(currentPath, segment);
                retKey = currentUser.CreateSubKey(currentPath, RegistryKeyPermissionCheck.ReadSubTree);
            }

            return retKey;
        }

        private void SaveInventoryFile(object sender, EventArgs e)
        {
            var inventory = new IniReader(Constants.InventoryFile);

            var section = inventory.Load() && inventory.Sections.ContainsKey(_playerInfoProvider.LoggedInAccountName)
                ? inventory.Sections[_playerInfoProvider.LoggedInAccountName]
                : new SortedList<string, string>();

            var existing = section.Where(x => x.Key.Contains(_characterProvider.MainCharacter.Name)).Select(x => x.Key).ToList();
            foreach (var key in existing)
                section.Remove(key);

            foreach (var item in _childItems)
                section[$"{_characterProvider.MainCharacter.Name}.{item.Slot}"] = $"{item.InventoryItem.ItemID}";

            inventory.Sections[_playerInfoProvider.LoggedInAccountName] = section;

            inventory.Save();
        }

        private void HandleItemDoubleClick(object sender, EIFRecord itemData)
        {
            if (itemData.Type >= ItemType.Weapon && itemData.Type <= ItemType.Bracer)
            {
                _inventoryController.EquipItem(itemData);
            }
            else
            {
                _inventoryController.UseItem(itemData);
            }
        }

        private void HandleItemDoneDragging(object sender, InventoryPanelItem.ItemDragCompletedEventArgs e)
        {
            var item = sender as InventoryPanelItem;
            if (item == null)
                return;

            var oldSlot = item.Slot;

            if (_activeDialogProvider.ActiveDialogs.All(x => !x.HasValue))
            {
                // todo: if this is a chained drag, restoring the original slot could overlap with another item
                var mapRenderer = _hudControlProvider.GetComponent<IMapRenderer>(HudControlIdentifier.MapRenderer);
                if (mapRenderer.MouseOver)
                {
                    e.RestoreOriginalSlot = true;
                    _inventoryController.DropItem(item.Data, item.InventoryItem);
                    return;
                }
            }

            // todo: if this is a chained drag, restoring the original slot could overlap with another item
            if (_drop.MouseOver)
            {
                e.RestoreOriginalSlot = true;
                _inventoryController.DropItem(item.Data, item.InventoryItem);
                return;
            }
            else if (_junk.MouseOver)
            {
                e.RestoreOriginalSlot = true;
                _inventoryController.JunkItem(item.Data, item.InventoryItem);
                return;
            }

            var dialogDrop = false;
            _activeDialogProvider.PaperdollDialog.MatchSome(x =>
            {
                if (x.MouseOver && x.MouseOverPreviously && item.Data.GetEquipLocation() != EquipLocation.PAPERDOLL_MAX)
                {
                    dialogDrop = true;
                    _inventoryController.EquipItem(item.Data);
                }
            });
            _activeDialogProvider.ChestDialog.MatchSome(x =>
            {
                if (x.MouseOver && x.MouseOverPreviously)
                {
                    dialogDrop = true;
                    _inventoryController.DropItemInChest(item.Data, item.InventoryItem);
                }
            });
            _activeDialogProvider.LockerDialog.MatchSome(x =>
            {
                if (x.MouseOver && x.MouseOverPreviously)
                {
                    dialogDrop = true;
                    _inventoryController.DropItemInLocker(item.Data, item.InventoryItem);
                }
            });

            if (dialogDrop)
            {
                e.RestoreOriginalSlot = true;
                return;
            }

            var fitsInOldSlot = _inventoryService.FitsInSlot(_inventorySlotRepository.FilledSlots, oldSlot, e.Data.Size);
            var newSlot = item.GetCurrentSlotBasedOnPosition();

            // check overlapping items:
            //   1. If there's multiple items under it, snap it back to the original slot
            //   2. If there's only one item under it, start dragging that item
            //   3. If there's nothing under it, make sure it fits in the inventory, otherwise snap back to original slot

            var overlapped = GetOverlappingTakenSlots(newSlot, e.Data.Size, _childItems.Except(new[] { item }).Select(x => (x.Slot, x.Data.Size)))
                .ToList();

            if (overlapped.Count > 1)
            {
                e.RestoreOriginalSlot = true;

                if (!fitsInOldSlot)
                    e.ContinueDrag = true;
            }
            else if (overlapped.Count == 1)
            {
                _inventoryService.ClearSlots(_inventorySlotRepository.FilledSlots, oldSlot, e.Data.Size);
                _inventoryService.SetSlots(_inventorySlotRepository.FilledSlots, newSlot, e.Data.Size);

                // start a chained drag on another item (see below comment)
                _childItems.Single(x => x.Slot == overlapped[0]).StartDragging();
            }
            else if (oldSlot != newSlot)
            {
                if (!_inventoryService.FitsInSlot(_inventorySlotRepository.FilledSlots, oldSlot, newSlot, e.Data.Size))
                {
                    // if the original slot no longer fits (because this is a chained drag), don't stop dragging this item
                    if (!fitsInOldSlot)
                        e.ContinueDrag = true;
                    else
                        e.RestoreOriginalSlot = true;
                }
                else
                {
                    _inventoryService.ClearSlots(_inventorySlotRepository.FilledSlots, oldSlot, e.Data.Size);
                    _inventoryService.SetSlots(_inventorySlotRepository.FilledSlots, newSlot, e.Data.Size);
                }
            }

            #region Unimplemented drag action
            /*
            if (BankAccountDialog.Instance != null && BankAccountDialog.Instance.MouseOver && BankAccountDialog.Instance.MouseOverPreviously && m_inventory.ItemID == 1)
            {
                if (m_inventory.Amount == 0)
                {
                    EOMessageBox.Show(DialogResourceID.BANK_ACCOUNT_UNABLE_TO_DEPOSIT, EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                }
                else if (m_inventory.Amount > 1)
                {
                    ItemTransferDialog dlg = new ItemTransferDialog(m_itemData.Name, ItemTransferDialog.TransferType.BankTransfer,
                        m_inventory.Amount, EOResourceID.DIALOG_TRANSFER_DEPOSIT);
                    dlg.DialogClosing += (o, e) =>
                    {
                        if (e.Result == XNADialogResult.Cancel)
                            return;

                        if (!m_api.BankDeposit(dlg.SelectedAmount))
                            EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
                    };
                }
                else
                {
                    if (!m_api.BankDeposit(1))
                        EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
                }
            }
            else if (TradeDialog.Instance != null && TradeDialog.Instance.MouseOver && TradeDialog.Instance.MouseOverPreviously
                && !TradeDialog.Instance.MainPlayerAgrees)
            {
                if (m_itemData.Special == ItemSpecial.Lore)
                {
                    EOMessageBox.Show(DialogResourceID.ITEM_IS_LORE_ITEM);
                }
                else if (m_inventory.Amount > 1)
                {
                    ItemTransferDialog dlg = new ItemTransferDialog(m_itemData.Name, ItemTransferDialog.TransferType.TradeItems,
                        m_inventory.Amount, EOResourceID.DIALOG_TRANSFER_OFFER);
                    dlg.DialogClosing += (o, e) =>
                    {
                        if (e.Result != XNADialogResult.OK) return;

                        if (!m_api.TradeAddItem(m_inventory.ItemID, dlg.SelectedAmount))
                        {
                            TradeDialog.Instance.Close(XNADialogResult.NO_BUTTON_PRESSED);
                            ((EOGame)Game).DoShowLostConnectionDialogAndReturnToMainMenu();
                        }
                    };
                }
                else if (!m_api.TradeAddItem(m_inventory.ItemID, 1))
                {
                    TradeDialog.Instance.Close(XNADialogResult.NO_BUTTON_PRESSED);
                    ((EOGame)Game).DoShowLostConnectionDialogAndReturnToMainMenu();
                }
            }*/
            #endregion
        }

        private static IEnumerable<int> GetOverlappingTakenSlots(int newSlot, ItemSize size, IEnumerable<(int Slot, ItemSize Size)> items)
        {
            var slotX = newSlot % InventoryRowSlots;
            var slotY = newSlot / InventoryRowSlots;
            var slotItemDim = size.GetDimensions();

            var newSlotCoords = new List<(int X, int Y)>();
            for (int r = slotY; r < slotY + slotItemDim.Height; r++)
                for (int c = slotX; c < slotX + slotItemDim.Width; c++)
                    newSlotCoords.Add((c, r));

            foreach (var item in items)
            {
                var itemX = item.Slot % InventoryRowSlots;
                var itemY = item.Slot / InventoryRowSlots;
                var itemDim = item.Size.GetDimensions();

                var @break = false;
                for (int r = itemY; r < itemY + itemDim.Height; r++)
                {
                    if (@break)
                        break;

                    for (int c = itemX; c < itemX + itemDim.Width; c++)
                    {
                        if (newSlotCoords.Contains((c, r)))
                        {
                            yield return item.Slot;
                            @break = true;
                            break;
                        }
                    }
                }
            }
        }
    }
}