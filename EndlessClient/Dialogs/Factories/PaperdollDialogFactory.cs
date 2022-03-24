using AutomaticTypeMapper;
using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EndlessClient.HUD;
using EndlessClient.HUD.Inventory;
using EOLib.Domain.Character;
using EOLib.Graphics;
using EOLib.IO.Repositories;

namespace EndlessClient.Dialogs.Factories
{
    [AutoMappedType]
    public class PaperdollDialogFactory : IPaperdollDialogFactory
    {
        private readonly IGameStateProvider _gameStateProvider;
        private readonly IEODialogButtonService _eoDialogButtonService;
        private readonly IInventorySpaceValidator _inventorySpaceValidator;
        private readonly IEOMessageBoxFactory _eoMessageBoxFactory;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IPaperdollProvider _paperdollProvider;
        private readonly IPubFileProvider _pubFileProvider;
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly ICharacterActions _characterActions;

        public PaperdollDialogFactory(IGameStateProvider gameStateProvider,
            INativeGraphicsManager nativeGraphicsManager,
            ICharacterActions characterActions,
            IPaperdollProvider paperdollProvider,
            IPubFileProvider pubFileProvider,
            IEODialogButtonService eoDialogButtonService,
            IInventorySpaceValidator inventorySpaceValidator,
            IEOMessageBoxFactory eoMessageBoxFactory,
            IStatusLabelSetter statusLabelSetter)
        {
            _paperdollProvider = paperdollProvider;
            _pubFileProvider = pubFileProvider;
            _nativeGraphicsManager = nativeGraphicsManager;
            _characterActions = characterActions;
            _gameStateProvider = gameStateProvider;
            _eoDialogButtonService = eoDialogButtonService;
            _inventorySpaceValidator = inventorySpaceValidator;
            _eoMessageBoxFactory = eoMessageBoxFactory;
            _statusLabelSetter = statusLabelSetter;
        }

        public PaperdollDialog Create(ICharacter character, bool isMainCharacter)
        {
            return new PaperdollDialog(_gameStateProvider,
                _nativeGraphicsManager,
                _characterActions,
                _paperdollProvider,
                _pubFileProvider,
                _eoDialogButtonService,
                _inventorySpaceValidator,
                _eoMessageBoxFactory,
                _statusLabelSetter,
                character,
                isMainCharacter);
        }
    }

    public interface IPaperdollDialogFactory
    {
        PaperdollDialog Create(ICharacter character, bool isMainCharacter);
    }
}
