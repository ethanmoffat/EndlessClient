using AutomaticTypeMapper;
using EndlessClient.Content;
using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EndlessClient.Input;
using EndlessClient.Rendering.Factories;
using EndlessClient.UIControls;
using EOLib.Graphics;

namespace EndlessClient.Dialogs.Factories
{
    [MappedType(BaseType = typeof(ICreateCharacterDialogFactory))]
    public class CreateCharacterDialogFactory : ICreateCharacterDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IGameStateProvider _gameStateProvider;
        private readonly ICharacterRendererFactory _characterRendererFactory;
        private readonly IContentProvider _contentProvider;
        private readonly IEOMessageBoxFactory _eoMessageBoxFactory;
        private readonly IEODialogButtonService _dialogButtonService;
        private readonly IXnaControlSoundMapper _xnaControlSoundMapper;

        public CreateCharacterDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                            IGameStateProvider gameStateProvider,
                                            ICharacterRendererFactory characterRendererFactory,
                                            IContentProvider contentProvider,
                                            IEOMessageBoxFactory eoMessageBoxFactory,
                                            IEODialogButtonService dialogButtonService,
                                            IXnaControlSoundMapper xnaControlSoundMapper)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _gameStateProvider = gameStateProvider;
            _characterRendererFactory = characterRendererFactory;
            _contentProvider = contentProvider;
            _eoMessageBoxFactory = eoMessageBoxFactory;
            _dialogButtonService = dialogButtonService;
            _xnaControlSoundMapper = xnaControlSoundMapper;
        }

        public CreateCharacterDialog BuildCreateCharacterDialog()
        {
            return new CreateCharacterDialog(_nativeGraphicsManager,
                                             _gameStateProvider,
                                             _characterRendererFactory,
                                             _contentProvider,
                                             _eoMessageBoxFactory,
                                             _dialogButtonService,
                                             _xnaControlSoundMapper);
        }
    }

    public interface ICreateCharacterDialogFactory
    {
        CreateCharacterDialog BuildCreateCharacterDialog();
    }
}
