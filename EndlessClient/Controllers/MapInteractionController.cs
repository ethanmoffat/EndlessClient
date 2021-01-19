using System;
using AutomaticTypeMapper;
using EndlessClient.HUD;
using EndlessClient.HUD.Inventory;
using EndlessClient.Rendering;
using EOLib.Domain.Item;
using EOLib.Domain.Map;
using EOLib.Extensions;
using EOLib.Localization;

namespace EndlessClient.Controllers
{
    [AutoMappedType]
    public class MapInteractionController : IMapInteractionController
    {
        private readonly IMapActions _mapActions;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IInventorySpaceValidator _inventorySpaceValidator;

        public MapInteractionController(IMapActions mapActions,
                                        ICurrentMapStateProvider currentMapStateProvider,
                                        IStatusLabelSetter statusLabelSetter,
                                        IInventorySpaceValidator inventorySpaceValidator)
        {
            _mapActions = mapActions;
            _currentMapStateProvider = currentMapStateProvider;
            _statusLabelSetter = statusLabelSetter;
            _inventorySpaceValidator = inventorySpaceValidator;
        }

        public void LeftClick(IMapCellState cellState, IMouseCursorRenderer mouseRenderer)
        {
            var item = cellState.Items.OptionalFirst();
            if (item.HasValue)
            {
                if (!_inventorySpaceValidator.ItemFits(item.Value))
                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, EOResourceID.STATUS_LABEL_ITEM_PICKUP_NO_SPACE_LEFT);
                else
                    HandlePickupResult(_mapActions.PickUpItem(item.Value), item.Value);
            }
            else if (cellState.NPC.HasValue) { /* TODO: spell cast */ }
            else if (cellState.Sign.HasValue) { /* TODO: sign interaction */ }
            else if (cellState.Chest.HasValue) { /* TODO: chest interaction */ }
            else if (cellState.Character.HasValue) { /* TODO: character spell cast */ }
            else
            {
                mouseRenderer.AnimateClick();
            }
        }

        public void RightClick(IMapCellState cellState)
        {
            if (!cellState.Character.HasValue)
                return;

            //todo: context menu
        }

        private void HandlePickupResult(ItemPickupResult pickupResult, IItem item)
        {
            switch (pickupResult)
            {
                case ItemPickupResult.DropProtection:
                    var message = EOResourceID.STATUS_LABEL_ITEM_PICKUP_PROTECTED;
                    var extra = string.Empty;
                    if (item.OwningPlayerID.HasValue)
                    {
                        var playerId = item.OwningPlayerID.Value;
                        message = EOResourceID.STATUS_LABEL_ITEM_PICKUP_PROTECTED_BY;
                        var characterRef = _currentMapStateProvider.Characters.OptionalSingle(x => x.ID == playerId);
                        extra = characterRef.HasValue ? characterRef.Value.Name : string.Empty;
                    }

                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, message, extra);

                    break;
                case ItemPickupResult.TooHeavy:
                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.DIALOG_ITS_TOO_HEAVY_WEIGHT);
                    break;
                case ItemPickupResult.TooFar:
                case ItemPickupResult.Ok: break;
                default: throw new ArgumentOutOfRangeException(nameof(pickupResult), pickupResult, null);
            }
        }
    }

    public interface IMapInteractionController
    {
        void LeftClick(IMapCellState cellState, IMouseCursorRenderer mouseRenderer);

        void RightClick(IMapCellState cellState);
    }
}
