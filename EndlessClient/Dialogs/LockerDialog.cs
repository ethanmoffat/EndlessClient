using EndlessClient.ControlSets;
using EndlessClient.Dialogs.Factories;
using EndlessClient.Dialogs.Services;
using EndlessClient.HUD;
using EndlessClient.HUD.Controls;
using EndlessClient.HUD.Inventory;
using EndlessClient.HUD.Panels;
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
    public class LockerDialog : ScrollingListDialog
    {
        private readonly ILockerActions _lockerActions;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IInventorySpaceValidator _inventorySpaceValidator;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly ICharacterProvider _characterProvider;
        private readonly ILockerDataProvider _lockerDataProvider;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly InventoryPanel _inventoryPanel;

        private HashSet<InventoryItem> _cachedItems;

        public LockerDialog(INativeGraphicsManager nativeGraphicsManager,
                            ILockerActions lockerActions,
                            IEODialogButtonService dialogButtonService,
                            ILocalizedStringFinder localizedStringFinder,
                            IInventorySpaceValidator inventorySpaceValidator,
                            IStatusLabelSetter statusLabelSetter,
                            IEOMessageBoxFactory messageBoxFactory,
                            ICharacterProvider characterProvider,
                            ILockerDataProvider lockerDataProvider,
                            IHudControlProvider hudControlProvider,
                            IEIFFileProvider eifFileProvider)
            : base(nativeGraphicsManager, dialogButtonService, dialogSize: ScrollingListDialogSize.Large)
        {
            _lockerActions = lockerActions;
            _localizedStringFinder = localizedStringFinder;
            _inventorySpaceValidator = inventorySpaceValidator;
            _statusLabelSetter = statusLabelSetter;
            _messageBoxFactory = messageBoxFactory;
            _characterProvider = characterProvider;
            _lockerDataProvider = lockerDataProvider;
            _eifFileProvider = eifFileProvider;
            _inventoryPanel = hudControlProvider.GetComponent<InventoryPanel>(HudControlIdentifier.InventoryPanel);

            _cachedItems = new HashSet<InventoryItem>();

            Title = GetDialogTitle();
            Buttons = ScrollingListDialogButtons.Cancel;
            ListItemType = ListDialogItem.ListItemStyle.Large;
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (!_cachedItems.SetEquals(_lockerDataProvider.Items))
            {
                _cachedItems = _lockerDataProvider.Items.ToHashSet();
                UpdateItemList();
            }

            base.OnUpdateControl(gameTime);
        }

        private void UpdateItemList()
        {
            ClearItemList();

            Title = GetDialogTitle();

            foreach (var item in _cachedItems)
            {
                var itemData = _eifFileProvider.EIFFile[item.ItemID];

                var listItem = new ListDialogItem(this, ListDialogItem.ListItemStyle.Large)
                {
                    PrimaryText = itemData.Name,
                    SubText = $"x{item.Amount}  {(itemData.Type == ItemType.Armor ? $"({_localizedStringFinder.GetString(EOResourceID.FEMALE - itemData.Gender)})" : string.Empty)}",
                    IconGraphic = GraphicsManager.TextureFromResource(GFXTypes.Items, 2 * itemData.Graphic - 1, true),
                    OffsetY = 45
                };
                listItem.RightClick += (_, _) => TakeItem(itemData, item);

                AddItemToList(listItem, sortList: false);
            }
        }

        private void TakeItem(EIFRecord itemData, InventoryItem item)
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
                _lockerActions.TakeItemFromLocker(item.ItemID);
            }
        }

        private string GetDialogTitle()
        {
            return _characterProvider.MainCharacter.Name + "'s " + _localizedStringFinder.GetString(EOResourceID.DIALOG_TITLE_PRIVATE_LOCKER) + $" [{_lockerDataProvider.Items.Count}]";
        }
    }
}
