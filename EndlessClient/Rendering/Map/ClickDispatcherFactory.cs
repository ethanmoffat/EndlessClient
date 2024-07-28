using AutomaticTypeMapper;
using EndlessClient.Controllers;
using EndlessClient.ControlSets;
using EndlessClient.HUD.Spells;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.NPC;
using EOLib.Domain.Character;
using EOLib.Domain.Map;

namespace EndlessClient.Rendering.Map;

[AutoMappedType]
public class ClickDispatcherFactory : IClickDispatcherFactory
{
    private readonly IHudControlProvider _hudControlProvider;

    private readonly IClientWindowSizeProvider _clientWindowSizeProvider;
    private readonly ICurrentMapProvider _currentMapProvider;
    private readonly ICurrentMapStateProvider _currentMapStateProvider;
    private readonly ICharacterProvider _characterProvider;
    private readonly IMapCellStateProvider _mapCellStateProvider;
    private readonly ICharacterRendererProvider _characterRendererProvider;
    private readonly INPCRendererProvider _npcRendererProvider;
    private readonly ISpellSlotDataProvider _spellSlotDataProvider;
    private readonly IContextMenuRepository _contextMenuRepository;
    private readonly IMapObjectBoundsCalculator _mapObjectBoundsCalculator;
    private readonly IGridDrawCoordinateCalculator _gridDrawCoordinateCalculator;
    private readonly IMapInteractionController _mapInteractionController;
    private readonly INPCInteractionController _npcInteractionController;

    public ClickDispatcherFactory(IHudControlProvider hudControlProvider,
                                  IClientWindowSizeProvider clientWindowSizeProvider,
                                  ICurrentMapProvider currentMapProvider,
                                  ICurrentMapStateProvider currentMapStateProvider,
                                  ICharacterProvider characterProvider,
                                  IMapCellStateProvider mapCellStateProvider,
                                  ICharacterRendererProvider characterRendererProvider,
                                  INPCRendererProvider npcRendererProvider,
                                  ISpellSlotDataProvider spellSlotDataProvider,
                                  IContextMenuRepository contextMenuRepository,
                                  IMapObjectBoundsCalculator mapObjectBoundsCalculator,
                                  IGridDrawCoordinateCalculator gridDrawCoordinateCalculator,
                                  IMapInteractionController mapInteractionController,
                                  INPCInteractionController npcInteractionController)
    {
        _hudControlProvider = hudControlProvider;
        _clientWindowSizeProvider = clientWindowSizeProvider;
        _currentMapProvider = currentMapProvider;
        _currentMapStateProvider = currentMapStateProvider;
        _characterProvider = characterProvider;
        _mapCellStateProvider = mapCellStateProvider;
        _characterRendererProvider = characterRendererProvider;
        _npcRendererProvider = npcRendererProvider;
        _spellSlotDataProvider = spellSlotDataProvider;
        _contextMenuRepository = contextMenuRepository;
        _mapObjectBoundsCalculator = mapObjectBoundsCalculator;
        _gridDrawCoordinateCalculator = gridDrawCoordinateCalculator;
        _mapInteractionController = mapInteractionController;
        _npcInteractionController = npcInteractionController;
    }

    public IClickDispatcher Create()
    {
        return new ClickDispatcher(_hudControlProvider,
            _clientWindowSizeProvider,
            _currentMapProvider,
            _currentMapStateProvider,
            _characterProvider,
            _mapCellStateProvider,
            _characterRendererProvider,
            _npcRendererProvider,
            _spellSlotDataProvider,
            _contextMenuRepository,
            _mapObjectBoundsCalculator,
            _gridDrawCoordinateCalculator,
            _mapInteractionController,
            _npcInteractionController);
    }
}

public interface IClickDispatcherFactory
{
    IClickDispatcher Create();
}