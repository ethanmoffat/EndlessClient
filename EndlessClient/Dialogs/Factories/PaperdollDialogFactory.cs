using AutomaticTypeMapper;
using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EndlessClient.HUD;
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
        private readonly IEOMessageBoxFactory _eoMessageBoxFactory;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IPaperdollProvider _paperdollProvider;
        private readonly IPubFileProvider _pubFileProvider;
        private readonly INativeGraphicsManager _nativeGraphicsManager;

        public PaperdollDialogFactory(IGameStateProvider gameStateProvider,
            INativeGraphicsManager nativeGraphicsManager,
            IPaperdollProvider paperdollProvider,
            IPubFileProvider pubFileProvider,
            IEODialogButtonService eoDialogButtonService,
            IEOMessageBoxFactory eoMessageBoxFactory,
            IStatusLabelSetter statusLabelSetter)
        {
            _paperdollProvider = paperdollProvider;
            _pubFileProvider = pubFileProvider;
            _nativeGraphicsManager = nativeGraphicsManager;
            _gameStateProvider = gameStateProvider;
            _eoDialogButtonService = eoDialogButtonService;
            _eoMessageBoxFactory = eoMessageBoxFactory;
            _statusLabelSetter = statusLabelSetter;
        }

        public PaperdollDialog Create(ICharacter character, bool isMainCharacter)
        {
            return new PaperdollDialog(_gameStateProvider,
                _nativeGraphicsManager,
                _paperdollProvider,
                _pubFileProvider,
                _eoDialogButtonService,
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
