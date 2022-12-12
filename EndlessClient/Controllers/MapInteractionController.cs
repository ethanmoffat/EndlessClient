using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.ControlSets;
using EndlessClient.Dialogs;
using EndlessClient.Dialogs.Actions;
using EndlessClient.Dialogs.Factories;
using EndlessClient.HUD;
using EndlessClient.HUD.Controls;
using EndlessClient.HUD.Inventory;
using EndlessClient.HUD.Panels;
using EndlessClient.HUD.Spells;
using EndlessClient.Input;
using EndlessClient.Rendering;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Factories;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Interact;
using EOLib.Domain.Item;
using EOLib.Domain.Map;
using EOLib.Domain.Spells;
using EOLib.IO.Map;
using EOLib.Localization;
using Optional;
using Optional.Collections;
using System;
using System.Linq;

namespace EndlessClient.Controllers
{
    [AutoMappedType]
    public class MapInteractionController : IMapInteractionController
    {
        private readonly IMapActions _mapActions;
        private readonly ICharacterActions _characterActions;
        private readonly IInGameDialogActions _inGameDialogActions;
        private readonly IPaperdollActions _paperdollActions;
        private readonly IWalkValidationActions _walkValidationActions;
        private readonly IUnwalkableTileActions _unwalkableTileActions;
        private readonly ICharacterAnimationActions _characterAnimationActions;
        private readonly ISpellCastValidationActions _spellCastValidationActions;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IInventorySpaceValidator _inventorySpaceValidator;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly ICharacterRendererProvider _characterRendererProvider;
        private readonly IContextMenuRepository _contextMenuRepository;
        private readonly IUserInputTimeRepository _userInputTimeRepository;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IContextMenuRendererFactory _contextMenuRendererFactory;
        private readonly IActiveDialogProvider _activeDialogProvider;
        private readonly IUserInputRepository _userInputRepository;
        private readonly ISpellSlotDataRepository _spellSlotDataRepository;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly ISfxPlayer _sfxPlayer;

        public MapInteractionController(IMapActions mapActions,
                                        ICharacterActions characterActions,
                                        IInGameDialogActions inGameDialogActions,
                                        IPaperdollActions paperdollActions,
                                        IWalkValidationActions walkValidationActions,
                                        IUnwalkableTileActions unwalkableTileActions,
                                        ICharacterAnimationActions characterAnimationActions,
                                        ISpellCastValidationActions spellCastValidationActions,
                                        ICurrentMapStateProvider currentMapStateProvider,
                                        ICharacterProvider characterProvider,
                                        IStatusLabelSetter statusLabelSetter,
                                        IInventorySpaceValidator inventorySpaceValidator,
                                        IHudControlProvider hudControlProvider,
                                        ICharacterRendererProvider characterRendererProvider,
                                        IContextMenuRepository contextMenuRepository,
                                        IUserInputTimeRepository userInputTimeRepository,
                                        IEOMessageBoxFactory messageBoxFactory,
                                        IContextMenuRendererFactory contextMenuRendererFactory,
                                        IActiveDialogProvider activeDialogProvider,
                                        IUserInputRepository userInputRepository,
                                        ISpellSlotDataRepository spellSlotDataRepository,
                                        ICurrentMapProvider currentMapProvider,
                                        ISfxPlayer sfxPlayer)
        {
            _mapActions = mapActions;
            _characterActions = characterActions;
            _inGameDialogActions = inGameDialogActions;
            _paperdollActions = paperdollActions;
            _walkValidationActions = walkValidationActions;
            _unwalkableTileActions = unwalkableTileActions;
            _characterAnimationActions = characterAnimationActions;
            _spellCastValidationActions = spellCastValidationActions;
            _currentMapStateProvider = currentMapStateProvider;
            _characterProvider = characterProvider;
            _statusLabelSetter = statusLabelSetter;
            _inventorySpaceValidator = inventorySpaceValidator;
            _hudControlProvider = hudControlProvider;
            _characterRendererProvider = characterRendererProvider;
            _contextMenuRepository = contextMenuRepository;
            _userInputTimeRepository = userInputTimeRepository;
            _messageBoxFactory = messageBoxFactory;
            _contextMenuRendererFactory = contextMenuRendererFactory;
            _activeDialogProvider = activeDialogProvider;
            _userInputRepository = userInputRepository;
            _spellSlotDataRepository = spellSlotDataRepository;
            _currentMapProvider = currentMapProvider;
            _sfxPlayer = sfxPlayer;
        }

        public void LeftClick(IMapCellState cellState, Option<IMouseCursorRenderer> mouseRenderer)
        {
            if (!InventoryPanel.NoItemsDragging() || _activeDialogProvider.ActiveDialogs.Any(x => x.HasValue))
                return;

            var optionalItem = cellState.Items.FirstOrNone();
            if (optionalItem.HasValue)
            {
                var item = optionalItem.ValueOr(MapItem.None);
                if (!_inventorySpaceValidator.ItemFits(item))
                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, EOResourceID.STATUS_LABEL_ITEM_PICKUP_NO_SPACE_LEFT);
                else
                    HandlePickupResult(_mapActions.PickUpItem(item), item);

                _userInputRepository.ClickHandled = true;
            }
            else if (cellState.Sign.HasValue)
            {
                var sign = cellState.Sign.ValueOr(Sign.None);
                var messageBox = _messageBoxFactory.CreateMessageBox(sign.Message, sign.Title);
                messageBox.ShowDialog();

                _userInputRepository.ClickHandled = true;
            }
            else if (_characterProvider.MainCharacter.RenderProperties.SitState != SitState.Standing)
            {
                _characterActions.ToggleSit();

                _userInputRepository.ClickHandled = true;
            }
            else if (InteractableTileSpec(cellState.TileSpec) && CharacterIsCloseEnough(cellState.Coordinate))
            {
                var unwalkableActions = _unwalkableTileActions.GetUnwalkableTileActions(cellState);

                foreach (var unwalkableAction in unwalkableActions)
                {
                    switch (cellState.TileSpec)
                    {
                        // todo: implement for other clickable tile specs (board, jukebox, etc)
                        case TileSpec.Chest:
                            if (unwalkableAction == UnwalkableTileAction.Chest)
                            {
                                _mapActions.OpenChest((byte)cellState.Coordinate.X, (byte)cellState.Coordinate.Y);
                                _inGameDialogActions.ShowChestDialog();

                                _userInputRepository.ClickHandled = true;
                            }
                            break;
                        case TileSpec.BankVault:
                            if (unwalkableAction == UnwalkableTileAction.Locker)
                            {
                                _mapActions.OpenLocker((byte)cellState.Coordinate.X, (byte)cellState.Coordinate.Y);
                                _inGameDialogActions.ShowLockerDialog();

                                _userInputRepository.ClickHandled = true;
                            }
                            break;
                    }
                }
            }
            else if (cellState.InBounds && !cellState.Character.HasValue && !cellState.NPC.HasValue
                && _walkValidationActions.IsCellStateWalkable(cellState) == WalkValidationResult.Walkable
                && !_characterProvider.MainCharacter.RenderProperties.IsActing(CharacterActionState.Attacking)
                && !_spellSlotDataRepository.SelectedSpellSlot.HasValue)
            {
                mouseRenderer.MatchSome(r => r.AnimateClick());
                _characterAnimationActions.StartWalking(Option.Some(cellState.Coordinate));

                _userInputRepository.ClickHandled = true;
                _userInputRepository.WalkClickHandled = true;
            }

            cellState.Warp.MatchSome(w =>
            {
                w.SomeWhen(d => d.DoorType != DoorSpec.NoDoor)
                    .MatchSome(d =>
                    {
                        if (_unwalkableTileActions.GetUnwalkableTileActions(cellState).Any(x => x == UnwalkableTileAction.Door))
                        {
                            _mapActions.OpenDoor(d);
                        }

                        _userInputRepository.ClickHandled = true;
                    });
            });

            _spellSlotDataRepository.SelectedSpellSlot = Option.None<int>();

            _userInputTimeRepository.LastInputTime = DateTime.Now;
        }

        public void LeftClick(ISpellTargetable spellTarget)
        {
            if (_spellSlotDataRepository.SpellIsPrepared)
            {
                _spellSlotDataRepository.SelectedSpellInfo.MatchSome(si =>
                {
                    var result = _spellCastValidationActions.ValidateSpellCast(si.ID, spellTarget);
                    if (result == SpellCastValidationResult.Ok && _characterAnimationActions.PrepareMainCharacterSpell(si.ID, spellTarget))
                        _characterActions.PrepareCastSpell(si.ID);
                    else if (result == SpellCastValidationResult.CannotAttackNPC)
                        _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.YOU_CANNOT_ATTACK_THIS_NPC);
                    else if (result == SpellCastValidationResult.ExhaustedNoTp)
                        _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.ATTACK_YOU_ARE_EXHAUSTED_TP);
                    else if (result == SpellCastValidationResult.ExhaustedNoSp)
                        _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.ATTACK_YOU_ARE_EXHAUSTED_SP);
                });

                _spellSlotDataRepository.SpellIsPrepared = false;
                _spellSlotDataRepository.SelectedSpellSlot = Option.None<int>();

                _userInputRepository.ClickHandled = true;
            }

            _userInputTimeRepository.LastInputTime = DateTime.Now;
        }

        public void RightClick(Character character)
        {
            if (_activeDialogProvider.ActiveDialogs.Any(x => x.HasValue))
                return;

            if (character == _characterProvider.MainCharacter)
            {
                _paperdollActions.RequestPaperdoll(_characterProvider.MainCharacter.ID);
                _inGameDialogActions.ShowPaperdollDialog(_characterProvider.MainCharacter, isMainCharacter: true);
                _userInputTimeRepository.LastInputTime = DateTime.Now;

                _userInputRepository.ClickHandled = true;
            }
            else if (_characterRendererProvider.CharacterRenderers.ContainsKey(character.ID))
            {
                _contextMenuRepository.ContextMenu = _contextMenuRepository.ContextMenu.Match(
                    some: cmr =>
                    {
                        cmr.Dispose();
                        return Option.Some(_contextMenuRendererFactory.CreateContextMenuRenderer(_characterRendererProvider.CharacterRenderers[character.ID]));
                    },
                    none: () => Option.Some(_contextMenuRendererFactory.CreateContextMenuRenderer(_characterRendererProvider.CharacterRenderers[character.ID])));
                _contextMenuRepository.ContextMenu.MatchSome(r => r.Initialize());

                _userInputRepository.ClickHandled = true;
            }
        }

        private void PlayMainCharacterWalkSfx()
        {
            if (_characterProvider.MainCharacter.NoWall)
                _sfxPlayer.PlaySfx(SoundEffectID.NoWallWalk);
            else if (IsSteppingStone(_characterProvider.MainCharacter.RenderProperties))
                _sfxPlayer.PlaySfx(SoundEffectID.JumpStone);
        }

        private bool IsSteppingStone(CharacterRenderProperties renderProps)
        {
            return _currentMapProvider.CurrentMap.Tiles[renderProps.MapY, renderProps.MapX] == TileSpec.Jump
                || _currentMapProvider.CurrentMap.Tiles[renderProps.GetDestinationY(), renderProps.GetDestinationX()] == TileSpec.Jump;
        }

        private void HandlePickupResult(ItemPickupResult pickupResult, MapItem item)
        {
            switch (pickupResult)
            {
                case ItemPickupResult.DropProtection:
                    var message = EOResourceID.STATUS_LABEL_ITEM_PICKUP_PROTECTED;
                    var extra = string.Empty;

                    item.OwningPlayerID.MatchSome(playerId =>
                    {
                        message = EOResourceID.STATUS_LABEL_ITEM_PICKUP_PROTECTED_BY;
                        if (_currentMapStateProvider.Characters.ContainsKey(playerId))
                        {
                            extra = $" {_currentMapStateProvider.Characters[playerId].Name}";
                        }
                    });

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

        private InventoryPanel InventoryPanel => _hudControlProvider.GetComponent<InventoryPanel>(HudControlIdentifier.InventoryPanel);

        private static bool InteractableTileSpec(TileSpec tileSpec)
        {
            switch (tileSpec)
            {
                case TileSpec.Chest:
                case TileSpec.BankVault:
                case TileSpec.Board1:
                case TileSpec.Board2:
                case TileSpec.Board3:
                case TileSpec.Board4:
                case TileSpec.Board5:
                case TileSpec.Board6:
                case TileSpec.Board7:
                case TileSpec.Board8:
                case TileSpec.Jukebox:
                    return true;
                default:
                    return false;
            }
        }

        private bool CharacterIsCloseEnough(MapCoordinate coordinate)
        {
            var x = _characterProvider.MainCharacter.RenderProperties.MapX;
            var y = _characterProvider.MainCharacter.RenderProperties.MapY;

            var withinOneUnit = Math.Max(Math.Abs(x - coordinate.X), Math.Abs(y - coordinate.Y)) <= 1;
            var sameXOrY = x == coordinate.X || y == coordinate.Y;
            return withinOneUnit && sameXOrY;
        }
    }

    public interface IMapInteractionController
    {
        void LeftClick(IMapCellState cellState, Option<IMouseCursorRenderer> mouseRenderer);

        void LeftClick(ISpellTargetable spellTarget);

        void RightClick(Character character);
    }
}
