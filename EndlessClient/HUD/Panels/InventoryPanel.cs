using EndlessClient.Dialogs.Actions;
using EndlessClient.HUD.Inventory;
using EOLib;
using EOLib.Config;
using EOLib.Domain.Character;
using EOLib.Domain.Item;
using EOLib.Domain.Login;
using EOLib.Graphics;
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
        public const int InventoryRowSlots = 14;

        // uses absolute coordinates
        private static readonly Rectangle InventoryGridArea = new Rectangle(110, 334, 377, 116);

        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IItemStringService _itemStringService;
        private readonly IInventoryService _inventoryService;
        private readonly IPlayerInfoProvider _playerInfoProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly ICharacterInventoryProvider _characterInventoryProvider;
        private readonly IEIFFileProvider _eifFileProvider;

        private readonly bool[,] _usedSlots = new bool[4, InventoryRowSlots];
        private readonly Dictionary<int, int> _itemSlotMap;
        private readonly List<InventoryPanelItem> _childItems = new List<InventoryPanelItem>();

        private readonly IXNALabel _weightLabel;
        private readonly IXNAButton _drop, _junk, _paperdoll;
        //private readonly ScrollBar _scrollBar;

        private Option<ICharacterStats> _cachedStats;
        private HashSet<IInventoryItem> _cachedInventory;

        public InventoryPanel(INativeGraphicsManager nativeGraphicsManager,
                              IInGameDialogActions inGameDialogActions,
                              IStatusLabelSetter statusLabelSetter,
                              IItemStringService itemStringService,
                              IInventoryService inventoryService,
                              IPlayerInfoProvider playerInfoProvider,
                              ICharacterProvider characterProvider,
                              ICharacterInventoryProvider characterInventoryProvider,
                              IEIFFileProvider eifFileProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _statusLabelSetter = statusLabelSetter;
            _itemStringService = itemStringService;
            _inventoryService = inventoryService;
            _playerInfoProvider = playerInfoProvider;
            _characterProvider = characterProvider;
            _characterInventoryProvider = characterInventoryProvider;
            _eifFileProvider = eifFileProvider;
            _weightLabel = new XNALabel(Constants.FontSize08pt5)
            {
                DrawArea = new Rectangle(385, 37, 88, 18),
                ForeColor = ColorConstants.LightGrayText,
                TextAlign = LabelAlignment.MiddleCenter,
                Visible = true,
                AutoSize = false
            };

            _itemSlotMap = GetItemSlotMap(_playerInfoProvider.LoggedInAccountName, _characterProvider.MainCharacter.Name);

            var weirdOffsetSheet = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 27);

            _paperdoll = new XNAButton(weirdOffsetSheet, new Vector2(385, 9), new Rectangle(39, 385, 88, 19), new Rectangle(126, 385, 88, 19));
            _paperdoll.OnMouseEnter += MouseOverButton;
            _paperdoll.OnClick += (_, _) => inGameDialogActions.ShowPaperdollDialog(characterProvider.MainCharacter, isMainCharacter: true);
            _drop = new XNAButton(weirdOffsetSheet, new Vector2(389, 68), new Rectangle(0, 15, 38, 37), new Rectangle(0, 52, 38, 37));
            _drop.OnMouseEnter += MouseOverButton;
            _junk = new XNAButton(weirdOffsetSheet, new Vector2(431, 68), new Rectangle(0, 89, 38, 37), new Rectangle(0, 126, 38, 37));
            _junk.OnMouseEnter += MouseOverButton;

            _cachedStats = Option.None<ICharacterStats>();
            _cachedInventory = new HashSet<IInventoryItem>();

            BackgroundImage = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 44);
            DrawArea = new Rectangle(102, 330, BackgroundImage.Width, BackgroundImage.Height);
        }

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
                        .MatchSome(s =>
                        {
                            _cachedStats = Option.Some(_characterProvider.MainCharacter.Stats);
                            _weightLabel.Text = $"{stats[CharacterStat.Weight]} / {stats[CharacterStat.MaxWeight]}";
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
                var updated = _characterInventoryProvider.ItemInventory.Except(added);

                foreach (var item in removed)
                {
                    var matchedItem = _childItems.SingleOrNone(x => x.InventoryItem.ItemID == item.ItemID);
                    matchedItem.MatchSome(childItem =>
                    {
                        childItem.SetControlUnparented();
                        childItem.Dispose();
                        _childItems.Remove(childItem);

                        var itemData = _eifFileProvider.EIFFile[item.ItemID];
                        _inventoryService.ClearSlots(_usedSlots, childItem.Slot, itemData.Size);
                    });
                }

                foreach (var item in updated)
                {
                    var matchedItem = _childItems.SingleOrNone(x => x.InventoryItem.ItemID == item.ItemID);
                    matchedItem.MatchSome(childItem =>
                    {
                        childItem.InventoryItem = item;
                    });
                }

                foreach (var item in added)
                {
                    var itemData = _eifFileProvider.EIFFile[item.ItemID];

                    var preferredSlot = _itemSlotMap.SingleOrNone(x => x.Value == item.ItemID).Map(x => x.Key);
                    var actualSlot = _inventoryService.GetNextOpenSlot(_usedSlots, itemData.Size, preferredSlot);

                    actualSlot.MatchSome(slot =>
                    {
                        _inventoryService.SetSlots(_usedSlots, slot, itemData.Size);

                        var newItem = new InventoryPanelItem(_nativeGraphicsManager, _itemStringService, _statusLabelSetter, this, slot, item, itemData);
                        newItem.Initialize();
                        newItem.SetParentControl(this);
                        newItem.DrawOrder = 102 - (slot % InventoryRowSlots) * 2;

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
                var inventory = new IniReader(Constants.InventoryFile);
                if (inventory.Load() && inventory.Sections.ContainsKey(_playerInfoProvider.LoggedInAccountName))
                {
                    var section = inventory.Sections[_playerInfoProvider.LoggedInAccountName];

                    foreach (var item in _childItems)
                        section[$"{_characterProvider.MainCharacter.Name}.{item.Slot}"] = $"{item.InventoryItem.ItemID}";
                }

                _paperdoll.OnMouseEnter -= MouseOverButton;
                _drop.OnMouseEnter -= MouseOverButton;
                _junk.OnMouseEnter -= MouseOverButton;
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
    }
}