using System.Collections.Generic;
using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Content;
using EndlessClient.Controllers;
using EndlessClient.Dialogs.Services;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Factories;
using EOLib.Domain.Character;
using EOLib.Domain.Interact.Barber;
using EOLib.Domain.Notifiers;
using EOLib.Graphics;
using EOLib.IO.Repositories;
using EOLib.Localization;

namespace EndlessClient.Dialogs.Factories
{
    [AutoMappedType]
    public class BarberDialogFactory : IBarberDialogFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly ICharacterRendererFactory _characterRendererFactory;
        private readonly IEODialogButtonService _dialogButtonService;
        private readonly ICharacterRepository _characterRepository;
        private readonly IEODialogIconService _dialogIconService;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IBarberActions _barberActions;
        private readonly ICharacterInventoryProvider _characterInventoryProvider;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly ISfxPlayer _sfxPlayer;

        public BarberDialogFactory(INativeGraphicsManager nativeGraphicsManager,
                                   ICharacterRendererFactory characterRendererFactory,
                                   IEODialogButtonService dialogButtonService,
                                   ICharacterRepository characterRepository,
                                   IEODialogIconService dialogIconService,
                                   ILocalizedStringFinder localizedStringFinder,
                                   IBarberActions barberActions,
                                   ICharacterInventoryProvider characterInventoryProvider,
                                   IEOMessageBoxFactory messageBoxFactory,
                                   IEIFFileProvider eifFileProvider,
                                   ISfxPlayer sfxPlayer)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _characterRendererFactory = characterRendererFactory;
            _dialogButtonService = dialogButtonService;
            _characterRepository = characterRepository;
            _dialogIconService = dialogIconService;
            _localizedStringFinder = localizedStringFinder;
            _barberActions = barberActions;
            _characterInventoryProvider = characterInventoryProvider;
            _messageBoxFactory = messageBoxFactory;
            _eifFileProvider = eifFileProvider;
            _sfxPlayer = sfxPlayer;
        }

        public BarberDialog Create()
        {
            return new BarberDialog(_nativeGraphicsManager,
                                    _characterRendererFactory,
                                    _dialogButtonService,
                                    _characterRepository,
                                    _dialogIconService,
                                    _localizedStringFinder,
                                    _barberActions,
                                    _characterInventoryProvider,
                                    _messageBoxFactory,
                                    _eifFileProvider,
                                    _sfxPlayer);
        }
    }

    public interface IBarberDialogFactory
    {
        BarberDialog Create();
    }
}