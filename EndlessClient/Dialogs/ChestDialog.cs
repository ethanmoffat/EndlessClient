using EndlessClient.ControlSets;
using EndlessClient.Dialogs.Factories;
using EndlessClient.Dialogs.Services;
using EndlessClient.HUD;
using EndlessClient.HUD.Controls;
using EndlessClient.HUD.Inventory;
using EndlessClient.HUD.Panels;
using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Pub;
using EOLib.IO.Repositories;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace EndlessClient.Dialogs
{
    public class ChestDialog : ScrollingListDialog
    {
        private readonly IChestActions _chestActions;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IInventorySpaceValidator _inventorySpaceValidator;
        private readonly IMapItemGraphicProvider _mapItemGraphicProvider;
        private readonly IChestDataProvider _chestDataProvider;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly InventoryPanel _inventoryPanel;

        private HashSet<ChestItem> _cachedItems;

        public ChestDialog(INativeGraphicsManager nativeGraphicsManager,
                           IChestActions chestActions,
                           IEOMessageBoxFactory messageBoxFactory,
                           IEODialogButtonService dialogButtonService,
                           IStatusLabelSetter statusLabelSetter,
                           ILocalizedStringFinder localizedStringFinder,
                           IInventorySpaceValidator inventorySpaceValidator,
                           IMapItemGraphicProvider mapItemGraphicProvider,
                           IChestDataProvider chestDataProvider,
                           IHudControlProvider hudControlProvider,
                           IEIFFileProvider eifFileProvider,
                           ICharacterProvider characterProvider)
            : base(nativeGraphicsManager, dialogButtonService, dialogSize: ScrollingListDialogSize.LargeNoScroll)
        {
            _chestActions = chestActions;
            _messageBoxFactory = messageBoxFactory;
            _statusLabelSetter = statusLabelSetter;
            _localizedStringFinder = localizedStringFinder;
            _inventorySpaceValidator = inventorySpaceValidator;
            _mapItemGraphicProvider = mapItemGraphicProvider;
            _chestDataProvider = chestDataProvider;
            _hudControlProvider = hudControlProvider;
            _eifFileProvider = eifFileProvider;
            _characterProvider = characterProvider;

            ListItemType = ListDialogItem.ListItemStyle.Large;
            Buttons = ScrollingListDialogButtons.Cancel;

            _inventoryPanel = _hudControlProvider.GetComponent<InventoryPanel>(HudControlIdentifier.InventoryPanel);
            _cachedItems = new HashSet<ChestItem>();

            _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION,
                EOResourceID.STATUS_LABEL_CHEST_YOU_OPENED,
                " " + _localizedStringFinder.GetString(EOResourceID.STATUS_LABEL_DRAG_AND_DROP_ITEMS));
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (!_cachedItems.SetEquals(_chestDataProvider.Items))
            {
                _cachedItems = _chestDataProvider.Items.ToHashSet();
                RefreshItemList();
            }

            base.OnUpdateControl(gameTime);
        }

        private void RefreshItemList()
        {
            ClearItemList();

            foreach (var item in _cachedItems)
            {
                var itemData = _eifFileProvider.EIFFile[item.ItemID];
                string subText =
                    $"x {item.Amount}  " +
                    $"{(itemData.Type == ItemType.Armor ? "(" + _localizedStringFinder.GetString(EOResourceID.FEMALE - itemData.Gender) + ")" : "")}";
                var nextItem = new ListDialogItem(this, ListItemType)
                {
                    PrimaryText = itemData.Name,
                    SubText = subText,
                    IconGraphic = _mapItemGraphicProvider.GetItemGraphic(item.ItemID, item.Amount),
                    ShowIconBackGround = true,
                };
                nextItem.RightClick += (_, _) => TakeItem(item, itemData);

                AddItemToList(nextItem, sortList: false);
            }
        }

        private void TakeItem(ChestItem item, EIFRecord itemData)
        {
            if (!_inventorySpaceValidator.ItemFits(item.ItemID))
            {
                var dlg = _messageBoxFactory.CreateMessageBox(EOResourceID.STATUS_LABEL_ITEM_PICKUP_NO_SPACE_LEFT, EOResourceID.STATUS_LABEL_TYPE_WARNING);
                dlg.ShowDialog();

                _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, EOResourceID.STATUS_LABEL_ITEM_PICKUP_NO_SPACE_LEFT);
            }
            else if (itemData.Weight * item.Amount + _characterProvider.MainCharacter.Stats[CharacterStat.Weight] > _characterProvider.MainCharacter.Stats[CharacterStat.MaxWeight])
            {
                var dlg = _messageBoxFactory.CreateMessageBox(EOResourceID.DIALOG_ITS_TOO_HEAVY_WEIGHT, EOResourceID.STATUS_LABEL_TYPE_WARNING);
                dlg.ShowDialog();
            }
            else
            {
                _chestActions.TakeItemFromChest(item.ItemID);
            }
        }
    }
}
