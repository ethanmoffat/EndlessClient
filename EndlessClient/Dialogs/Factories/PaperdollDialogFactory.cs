using AutomaticTypeMapper;
using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
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
        private IPaperdollProvider _paperdollProvider;
        private IECFFileProvider _ecfFileProvider;
        private readonly INativeGraphicsManager _nativeGraphicsManager;

        public PaperdollDialogFactory(IGameStateProvider gameStateProvider,
            INativeGraphicsManager nativeGraphicsManager,
            IPaperdollProvider paperdollProvider,
            IECFFileProvider ecfFileProvider,
            IEODialogButtonService eoDialogButtonService)
        {
            _paperdollProvider = paperdollProvider;
            _ecfFileProvider = ecfFileProvider;
            _nativeGraphicsManager = nativeGraphicsManager;
            _gameStateProvider = gameStateProvider;
            _eoDialogButtonService = eoDialogButtonService;
        }

        public PaperdollDialog Create(ICharacter character)
        {
            return new PaperdollDialog(_gameStateProvider,
                _nativeGraphicsManager,
                _paperdollProvider,
                _ecfFileProvider,
                _eoDialogButtonService,
                character);
        }
    }

    public interface IPaperdollDialogFactory
    {
        PaperdollDialog Create(ICharacter character);
    }
}
